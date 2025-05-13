using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using MVCFramework.Commons;
using MVCFramework.Serializer.Commons;

namespace MVCFramework.Middleware.Authentication
{
    
    public class MVCBasicAuthenticationMiddleware : IMVCMiddleware
    {
        private readonly IMVCAuthenticationHandler _authenticationHandler;
        private readonly string _realm;

        private const string CONTENT_HTML_FORMAT = "<html><body><h1>{0}</h1><p>{1}</p></body></html>";
        private const string CONTENT_401_NOT_AUTHORIZED = "401: Not authorized";
        private const string CONTENT_403_FORBIDDEN = "403: Forbidden";

        public MVCBasicAuthenticationMiddleware(
            IMVCAuthenticationHandler authenticationHandler,
            string realm = "DelphiMVCFramework REALM")
        {
            _authenticationHandler = authenticationHandler;
            _realm = realm;
        }

        public void OnBeforeRouting(WebContext context, ref bool handled)
        {
            
            handled = false;
        }

        public void OnBeforeControllerAction(WebContext context, string controllerQualifiedClassName, string actionName, ref bool handled)
        {
            
            _authenticationHandler.OnRequest(context, controllerQualifiedClassName, actionName, out bool authRequired);
            if (!authRequired)
            {
                handled = false;
                return;
            }

            
            context.LoggedUser.LoadFromSession(context.Session);
            bool isValid = context.LoggedUser.IsValid;

            
            if (!isValid)
            {
                string authHeader = context.Request.Headers["Authorization"];
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                {
                    SendWWWAuthenticate(context, ref handled);
                    return;
                }

                
                string token = authHeader.Substring("Basic ".Length).Trim();
                
                authHeader = MVCSerializerHelper.DecodeString(token);
                string[] authPieces = authHeader.Split(':');
                if (authPieces.Length != 2)
                {
                    SendWWWAuthenticate(context, ref handled);
                    return;
                }

                var rolesList = new List<string>();
                SessionData sessionData = new SessionData();
                _authenticationHandler.OnAuthentication(context, authPieces[0], authPieces[1], rolesList, out isValid, sessionData);
                if (isValid)
                {
                    context.LoggedUser.Roles.AddRange(rolesList);
                    context.LoggedUser.UserName = authPieces[0];
                    context.LoggedUser.LoggedSince = DateTime.Now;
                    context.LoggedUser.Realm = _realm;
                    context.LoggedUser.SaveToSession(context.Session);
                    foreach (var pair in sessionData)
                    {
                        context.Session[pair.Key] = pair.Value;
                    }
                }
            }

            
            bool isAuthorized = false;
            if (isValid)
            {
                _authenticationHandler.OnAuthorization(
                    context,
                    context.LoggedUser.Roles,
                    controllerQualifiedClassName,
                    actionName,
                    out isAuthorized);
            }

            if (isAuthorized)
            {
                handled = false;
            }
            else
            {
                if (isValid)
                    Send403Forbidden(context, ref handled);
                else
                    SendWWWAuthenticate(context, ref handled);
            }
        }

        public void OnAfterControllerAction(WebContext context, string controllerQualifiedClassName, string actionName, bool handled)
        {
            // No post-action processing in basic authentication.
        }

        public void OnAfterRouting(WebContext context, bool handled)
        {
            // Nothing to do after routing.
        }

        #region Helper Methods

        private void SendWWWAuthenticate(WebContext context, ref bool handled)
        {
            context.LoggedUser.Clear();
            if (context.Request.ClientPreferHTML)
            {
                context.Response.ContentType = MediaType.TextHtml;
                context.Response.RawWebResponse.Content = string.Format(
                    CONTENT_HTML_FORMAT,
                    CONTENT_401_NOT_AUTHORIZED,
                    context.Config[ConfigKey.ServerName]);
            }
            else
            {
                context.Response.ContentType = MediaType.TextPlain;
                context.Response.RawWebResponse.Content = CONTENT_401_NOT_AUTHORIZED + Environment.NewLine +
                    context.Config[ConfigKey.ServerName];
            }
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.SetCustomHeader("WWW-Authenticate", $"Basic realm={Quote(_realm)}");
            context.SessionStop(false);
            handled = true;
        }

        private void Send403Forbidden(WebContext context, ref bool handled)
        {
            context.LoggedUser.Clear();
            if (context.Request.ClientPreferHTML)
            {
                context.Response.ContentType = MediaType.TextHtml;
                context.Response.RawWebResponse.Content = string.Format(
                    CONTENT_HTML_FORMAT,
                    CONTENT_403_FORBIDDEN,
                    context.Config[ConfigKey.ServerName]);
            }
            else if (context.Request.ContentMediaType.StartsWith(MediaType.ApplicationJson, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.ContentType = MediaType.ApplicationJson;
                context.Response.RawWebResponse.Content = "{\"status\":\"error\", \"message\":\"" +
                    CONTENT_403_FORBIDDEN.Replace("\"", "\\\"") + "\"}";
            }
            else
            {
                context.Response.ContentType = MediaType.TextPlain;
                context.Response.RawWebResponse.Content = CONTENT_403_FORBIDDEN + Environment.NewLine +
                    context.Config[ConfigKey.ServerName];
            }
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.ReasonString = context.Config[ConfigKey.ServerName];
            handled = true;
        }

        private string Quote(string s) => $"\"{s}\"";

        #endregion
    }

    
    public class MVCCustomAuthenticationMiddleware : IMVCMiddleware
    {
        private readonly IMVCAuthenticationHandler _authenticationHandler;
        private readonly string _loginUrl;

        public MVCCustomAuthenticationMiddleware(
            IMVCAuthenticationHandler authenticationHandler,
            string loginUrl = "/system/users/logged")
        {
            _authenticationHandler = authenticationHandler;
            _loginUrl = loginUrl.ToLower();
        }

        public void OnBeforeRouting(WebContext context, ref bool handled)
        {
            if (context.Request.PathInfo.ToLower() == _loginUrl)
            {
                handled = false;
                if (context.Request.HTTPMethod.Equals("POST", StringComparison.OrdinalIgnoreCase) &&
                    context.Request.ContentType.StartsWith(MediaType.ApplicationJson, StringComparison.OrdinalIgnoreCase))
                {
                    DoLogin(context, ref handled);
                }
                else if (context.Request.HTTPMethod.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
                {
                    DoLogout(context, ref handled);
                }
            }
        }

        public virtual void OnBeforeControllerAction(WebContext context, string controllerQualifiedClassName, string actionName, ref bool handled)
        {
            _authenticationHandler.OnRequest(context, controllerQualifiedClassName, actionName, out bool authRequired);
            if (!authRequired)
            {
                handled = false;
                return;
            }
            context.LoggedUser.LoadFromSession(context.Session);
            bool isValid = context.LoggedUser.IsValid;
            if (!isValid)
            {
                context.SessionStop(false);
                SendResponse(context, ref handled);
                return;
            }
            bool isAuthorized = false;
            _authenticationHandler.OnAuthorization(
                context,
                context.LoggedUser.Roles,
                controllerQualifiedClassName,
                actionName,
                out isAuthorized);
            if (isAuthorized)
            {
                handled = false;
            }
            else
            {
                if (isValid)
                    SendResponse(context, ref handled, HttpStatusCode.Forbidden);
                else
                    SendResponse(context, ref handled, HttpStatusCode.Unauthorized);
            }
        }

        public void OnAfterControllerAction(WebContext context, string controllerQualifiedClassName, string actionName, bool handled)
        {
            // Implement post-action processing if needed.
        }

        public void OnAfterRouting(WebContext context, bool handled)
        {
            // Nothing to do.
        }

        #region Protected Helpers

        protected void SendResponse(WebContext context, ref bool handled, HttpStatusCode status = HttpStatusCode.Unauthorized)
        {
            context.LoggedUser.Clear();
            context.Response.SetCustomHeader("X-LOGIN-URL", _loginUrl);
            context.Response.SetCustomHeader("X-LOGIN-METHOD", "POST");
            context.Response.StatusCode = (int)status;
            if (context.Request.ClientPreferHTML)
            {
                context.Response.ContentType = MediaType.TextHtml;
                context.Response.RawWebResponse.Content = string.Format(
                    MVCBasicAuthenticationMiddleware.CONTENT_HTML_FORMAT,
                    ((int)status).ToString(),
                    context.Config[ConfigKey.ServerName]);
            }
            else
            {
                bool isPositive = (((int)status) / 100) == 2;
                string msg = isPositive ? "OK" : "KO";
                context.Response.ContentType = MediaType.ApplicationJson;
                context.Response.RawWebResponse.Content = $"{{\"status\":\"{msg}\", \"message\":\"{(int)status}\"}}";
            }
            handled = true;
        }

        protected void DoLogin(WebContext context, ref bool handled)
        {
            context.SessionStop(false);
            context.LoggedUser.Clear();
            if (!context.Request.HasBody)
            {
                handled = true;
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = MediaType.ApplicationJson;
                context.Response.RawWebResponse.Content =
                    "{\"status\":\"error\", \"message\":\"username and password are mandatory in the body request as json object\"}";
                return;
            }

            
            JsonDocument doc = JsonDocument.Parse(context.Request.Body);
            JsonElement root = doc.RootElement;
            string userName = root.TryGetProperty("username", out JsonElement userProp)
                ? userProp.GetString() ?? string.Empty : string.Empty;
            string password = root.TryGetProperty("password", out JsonElement passProp)
                ? passProp.GetString() ?? string.Empty : string.Empty;

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                handled = true;
                SendResponse(context, ref handled);
                return;
            }

            var rolesList = new List<string>();
            SessionData sessionData = new SessionData();
            bool isValid = false;
            _authenticationHandler.OnAuthentication(context, userName, password, rolesList, out isValid, sessionData);
            if (!isValid)
            {
                SendResponse(context, ref handled);
                return;
            }

            context.LoggedUser.Roles.AddRange(rolesList);
            context.LoggedUser.UserName = userName;
            context.LoggedUser.LoggedSince = DateTime.Now;
            context.LoggedUser.Realm = "custom";
            context.LoggedUser.SaveToSession(context.Session);

            foreach (var pair in sessionData)
            {
                context.Session[pair.Key] = pair.Value;
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.SetCustomHeader("X-LOGOUT-URL", _loginUrl);
            context.Response.SetCustomHeader("X-LOGOUT-METHOD", "DELETE");
            context.Response.ContentType = MediaType.ApplicationJson;
            context.Response.RawWebResponse.Content = "{\"status\":\"OK\"}";
            handled = true;
        }

        protected void DoLogout(WebContext context, ref bool handled)
        {
            context.SessionStop(false);
            SendResponse(context, ref handled, HttpStatusCode.OK);
        }

        #endregion
    }
}
