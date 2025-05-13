using System;
using MVCFramework;
using MVCFramework.Commons;
using AppControllerU;
using AuthenticationU;
using MVCFramework.Middleware.JWT;
using MVCFramework.JWT;
using MVCFramework.HMAC;
using MVCFramework.Middleware.StaticFiles;

namespace WebModuleUnit1
{
    public class WebModule1 : WebModule
    {
        private TMVCEngine MVC;

        public void WebModuleCreate(object sender)
        {
            Action<TJWT> lClaimsSetup = (JWT) =>
            {
                JWT.Claims.Issuer = "Delphi MVC Framework JWT Middleware Sample";
                JWT.Claims.NotBefore = DateTime.Now.AddMinutes(-5);
                JWT.Claims.IssuedAt = DateTime.Now;
                JWT.Claims.ExpirationTime = DateTime.Now.AddSeconds(30);
                JWT.CustomClaims["mycustomvalue"] = "THIS IS A CUSTOM CLAIM!";
                JWT.LiveValidityWindowInSeconds = 10;
            };

            MVC = new TMVCEngine(this, config =>
            {
                config[TMVCConfigKey.DefaultContentType] = "text/html";
            });
            MVC.AddController(typeof(TApp1MainController))
               .AddController(typeof(TAdminController))
               .AddMiddleware(new TMVCJWTAuthenticationMiddleware(
                   new TAuthenticationSample(),
                   lClaimsSetup,
                   "mys3cr37",
                   "/login",
                   new TJWTCheckableClaim[] { TJWTCheckableClaim.ExpirationTime, TJWTCheckableClaim.NotBefore, TJWTCheckableClaim.IssuedAt },
                   0,
                   HMAC_HS512
               ));
        }
    }
}
