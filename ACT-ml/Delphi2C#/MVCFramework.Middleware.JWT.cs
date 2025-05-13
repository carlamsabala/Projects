using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq; 
using MVCFramework;         
using MVCFramework.Commons;  
using MVCFramework.JWT;      
using MVCFramework.HMAC;     

namespace MVCFramework.Middleware.JWT
{
    
    public static class MVCJWTDefaults
    {
        public const string AUTHORIZATION_HEADER = "Authorization";
        public const string USERNAME_HEADER = "jwtusername";
        public const string PASSWORD_HEADER = "jwtpassword";
        public const string AUTH_SCHEMA = "Bearer";
        public const string AUTHORIZATION_ACCESS_TOKEN = "access_token";
    }

    
    public delegate void JWTClaimsSetup(JWT jwt);

    
    public class MVCJWTAuthenticationMiddleware : IMVCMiddleware
    {
        private readonly IMVCAuthenticationHandler _authenticationHandler;
        private readonly JWTCheckableClaims _claimsToCheck;
        private readonly JWTClaimsSetup _setupJWTClaims;
        private readonly string _secret;
        private readonly uint _leewaySeconds;
        private readonly string _loginURLSegment;
        private readonly string _authorizationHeaderName;
        private readonly string _authorizationAccessToken;
        private readonly string _userNameHeaderName;
        private readonly string _passwordHeaderName;
        private readonly string _hmacAlgorithm;
        private readonly bool _useHttpOnly;
        private DateTime _tokenHttpOnlyExpires;
        private readonly string _logoffURLSegment;

        #region Constructors

        public MVCJWTAuthenticationMiddleware(
            IMVCAuthenticationHandler authenticationHandler,
            JWTClaimsSetup configClaims,
            string secret = "D3lph1MVCFram3w0rk",
            string loginURLSegment = "/login",
            JWTCheckableClaims claimsToCheck = JWTCheckableClaims.None,
            uint leewaySeconds = 300,
            string hmacAlgorithm = HMAC_HS512)
        {
            _authenticationHandler = authenticationHandler;
            _setupJWTClaims = configClaims;
            _claimsToCheck = claimsToCheck;
            _secret = secret;
            _loginURLSegment = loginURLSegment;
            _leewaySeconds = leewaySeconds;
            _authorizationHeaderName = MVCJWTDefaults.AUTHORIZATION_HEADER;
            _authorizationAccessToken = MVCJWTDefaults.AUTHORIZATION_ACCESS_TOKEN;
            _userNameHeaderName = MVCJWTDefaults.USERNAME_HEADER;
            _passwordHeaderName = MVCJWTDefaults.PASSWORD_HEADER;
            _hmacAlgorithm = hmacAlgorithm;
            _useHttpOnly = false;
            _tokenHttpOnlyExpires = DateTime.Now;
        }

        public MVCJWTAuthenticationMiddleware(
            IMVCAuthenticationHandler authenticationHandler,
            JWTClaimsSetup configClaims,
            bool useHttpOnly,
            string logoffURLSegment = "/logoff",
            string secret = "D3lph1MVCFram3w0rk",
            string loginURLSegment = "/login",
            JWTCheckableClaims claimsToCheck = JWTCheckableClaims.None,
            uint leewaySeconds = 300,
            string hmacAlgorithm = HMAC_HS512)
            : this(authenticationHandler, configClaims, secret, loginURLSegment, claimsToCheck, leewaySeconds, hmacAlgorithm)
        {
            _useHttpOnly = useHttpOnly;
            _logoffURLSegment = logoffURLSegment;
        }

        #endregion

        #region Helper Methods

        
        protected bool NeedsToBeExtended(JWT jwt)
        {
            double secondsToExpire = (jwt.Claims.ExpirationTime - DateTime.Now).TotalSeconds;
            return secondsToExpire <= jwt.LiveValidityWindowInSeconds;
        }

        
        protected void ExtendExpirationTime(JWT jwt)
        {
            
            jwt.Claims.ExpirationTime = Max(jwt.Claims.ExpirationTime, DateTime.Now)
                .AddSeconds(jwt.LeewaySeconds + jwt.LiveValidityWindowInSeconds);
            if (_useHttpOnly)
            {
                _tokenHttpOnlyExpires = jwt.Claims.ExpirationTime;
            }
        }

        private DateTime Max(DateTime a, DateTime b)
        {
            return (a > b) ? a : b;
        }

        
        protected void InternalRender(JObject jsonObj, string contentType, string contentEncoding, WebContext context, bool instanceOwner = true)
        {
            string jValue = jsonObj.ToString();

            if (_useHttpOnly)
            {
                
                context.Response.Cookies.Add(new Cookie("token", jsonObj["token"]?.ToString())
                {
                    Expires = _tokenHttpOnlyExpires,
                    Path = "/",
                    HttpOnly = true
                });
            }

            context.Response.RawWebResponse.ContentType = $"{contentType}; charset={contentEncoding}";
            Encoding encoding = Encoding.GetEncoding(contentEncoding);
            byte[] bytes = encoding.GetBytes(jValue);
            context.Response.SetContentStream(new MemoryStream(bytes), $"{contentType}; charset={contentEncoding}");
        }

        #endregion

        #region IMVCMiddleware Members

        public void OnBeforeRouting(WebContext context, ref bool handled)
        {
            // No routing logic needed for JWT middleware.
        }

        public void OnBeforeControllerAction(WebContext context, string controllerQualifiedClassName, string actionName, ref bool handled)
        {
            
            _authenticationHandler.OnRequest(context, controllerQualifiedClassName, actionName, out bool authRequired);
            if (!authRequired)
            {
                handled = false;
                
                string authHeader = context.Request.Headers[_authorizationHeaderName];
                if (!string.IsNullOrEmpty(authHeader))
                {
                    JWT jwtTemp = new JWT(_secret, _leewaySeconds);
                    try
                    {
                        jwtTemp.RegClaimsToChecks = _claimsToCheck;
                        string token = "";
                        if (authHeader.StartsWith(MVCJWTDefaults.AUTH_SCHEMA, StringComparison.OrdinalIgnoreCase))
                        {
                            token = authHeader.Substring(MVCJWTDefaults.AUTH_SCHEMA.Length).Trim();
                            token = WebUtility.UrlDecode(token);
                        }
                        if (jwtTemp.LoadToken(token, out string errorMsg))
                        {
                            context.LoggedUser.UserName = jwtTemp.CustomClaims["username"];
                            context.LoggedUser.Roles.AddRange(jwtTemp.CustomClaims["roles"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                            context.LoggedUser.LoggedSince = jwtTemp.Claims.IssuedAt;
                            context.LoggedUser.CustomData = jwtTemp.CustomClaims.AsCustomData;
                        }
                    }
                    finally
                    {
                        jwtTemp.Dispose();
                    }
                }
                return;
            }

            
            using (JWT jwt = new JWT(_secret, _leewaySeconds))
            {
                jwt.RegClaimsToChecks = _claimsToCheck;
                string authHeader = context.Request.Headers[_authorizationHeaderName];
                string authToken = "";
                if (!string.IsNullOrEmpty(authHeader))
                {
                    if (authHeader.StartsWith(MVCJWTDefaults.AUTH_SCHEMA, StringComparison.OrdinalIgnoreCase))
                    {
                        authToken = authHeader.Substring(MVCJWTDefaults.AUTH_SCHEMA.Length).Trim();
                        authToken = WebUtility.UrlDecode(authToken);
                    }
                }
                else
                {
                    
                    string queryToken = context.Request.Params[_authorizationAccessToken];
                    if (!string.IsNullOrEmpty(queryToken))
                    {
                        authToken = queryToken.Trim();
                        authToken = WebUtility.UrlDecode(authToken);
                    }
                    else if (_useHttpOnly)
                    {
                        
                        string cookieToken = context.Request.Cookie("token");
                        if (!string.IsNullOrEmpty(cookieToken))
                        {
                            authToken = cookieToken.Trim();
                            authToken = WebUtility.UrlDecode(authToken);
                        }
                    }
                }
                if (string.IsNullOrEmpty(authToken))
                    throw new MVCJWTException(HTTP_STATUS.Unauthorized, "Authorization Required");

                if (!jwt.LoadToken(authToken, out string errorMsg))
                    throw new MVCJWTException(HTTP_STATUS.Unauthorized, errorMsg);

                if (string.IsNullOrEmpty(jwt.CustomClaims["username"]))
                    throw new MVCJWTException(HTTP_STATUS.Unauthorized, "Invalid Token, Authorization Required");

                context.LoggedUser.UserName = jwt.CustomClaims["username"];
                context.LoggedUser.Roles.AddRange(jwt.CustomClaims["roles"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                context.LoggedUser.LoggedSince = jwt.Claims.IssuedAt;
                context.LoggedUser.CustomData = jwt.CustomClaims.AsCustomData;

                if (_authenticationHandler != null)
                {
                    _authenticationHandler.OnAuthorization(context, context.LoggedUser.Roles, controllerQualifiedClassName, actionName, out bool isAuthorized);
                    if (!isAuthorized)
                        throw new MVCJWTException(HTTP_STATUS.Forbidden, "Authorization Forbidden");
                }

                
                if (jwt.LiveValidityWindowInSeconds > 0 && NeedsToBeExtended(jwt))
                {
                    ExtendExpirationTime(jwt);
                    context.Response.SetCustomHeader(_authorizationHeaderName, "Bearer " + jwt.GetToken());
                }
                handled = false;
            }
        }

        public void OnAfterControllerAction(WebContext context, string controllerQualifiedClassName, string actionName, bool handled)
        {
            // Implement if any after-action logic is needed.
        }

        public void OnAfterRouting(WebContext context, bool handled)
        {
            // Implement if any after-routing logic is needed.
        }

        #endregion

        #region Exposed Properties

        public string AuthorizationHeaderName => _authorizationHeaderName;
        public string UserNameHeaderName => _userNameHeaderName;
        public string PasswordHeaderName => _passwordHeaderName;

        #endregion
    }

    
    public class MVCJWTBlackListMiddleware : IMVCMiddleware
    {
        private readonly Action<WebContext, string, ref bool> _onAcceptToken;
        private readonly Action<WebContext, string> _onNewJWTToBlackList;
        private readonly string _blackListRequestURLSegment;

        public MVCJWTBlackListMiddleware(
            Action<WebContext, string, ref bool> onAcceptToken,
            Action<WebContext, string> onNewJWTToBlackList,
            string blackListRequestURLSegment = "/logout")
        {
            _onAcceptToken = onAcceptToken ?? throw new ArgumentNullException(nameof(onAcceptToken));
            _onNewJWTToBlackList = onNewJWTToBlackList;
            _blackListRequestURLSegment = blackListRequestURLSegment;
            if (string.IsNullOrWhiteSpace(_blackListRequestURLSegment))
                throw new ArgumentException("BlackListRequestURLSegment cannot be empty", nameof(blackListRequestURLSegment));
        }

        public void OnBeforeRouting(WebContext context, ref bool handled)
        {
            string authHeader = context.Request.Headers[MVCJWTDefaults.AUTHORIZATION_HEADER];
            string authToken = "";
            if (!string.IsNullOrEmpty(authHeader))
            {
                if (authHeader.StartsWith(MVCJWTDefaults.AUTH_SCHEMA, StringComparison.OrdinalIgnoreCase))
                {
                    authToken = authHeader.Substring(MVCJWTDefaults.AUTH_SCHEMA.Length).Trim();
                    authToken = WebUtility.UrlDecode(authToken);
                }
            }

            if (string.Equals(context.Request.PathInfo, _blackListRequestURLSegment, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(authToken))
                    throw new Exception("JWTToken required - cannot blacklist an unknown token");
                _onNewJWTToBlackList(context, authToken);
                context.Response.StatusCode = HTTP_STATUS.NoContent;
                handled = true;
            }
            else
            {
                if (string.IsNullOrEmpty(authToken))
                {
                    handled = false;
                }
                else
                {
                    bool accepted = true;
                    _onAcceptToken(context, authToken, ref accepted);
                    if (!accepted)
                        throw new MVCJWTException(HTTP_STATUS.Forbidden, "JWT not accepted");
                }
            }
        }

        public void OnBeforeControllerAction(WebContext context, string controllerQualifiedClassName, string actionName, ref bool handled)
        {
            // Implement as needed.
        }

        public void OnAfterControllerAction(WebContext context, string controllerQualifiedClassName, string actionName, bool handled)
        {
            // Implement as needed.
        }

        public void OnAfterRouting(WebContext context, bool handled)
        {
            // Implement as needed.
        }
    }
}
