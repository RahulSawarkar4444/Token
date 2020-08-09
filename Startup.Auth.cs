using System;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Owin;
//using OAuthWebApiApp.Providers;
using Microsoft.Owin.Security;

using System.Web.Http;

namespace WebApplication1.App_Start
{
    public partial class Startup
    {
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static string PublicClientId { get; private set; }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {


            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "ExternalCookie",
                AuthenticationMode = AuthenticationMode.Passive,
                CookieName = "aspouthcookie",
                ExpireTimeSpan = TimeSpan.FromMinutes(5)
            });


            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Configure the application for OAuth based flow
            PublicClientId = "self";
            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                Provider = new ApplicationOAuthProvider(PublicClientId),
                RefreshTokenProvider = new RefreshTokenProvider(),
                //AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
                AuthorizeEndpointPath = new PathString("/User/ExternalLogin"),

                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(50),
                // In production mode set AllowInsecureHttp = false
                AllowInsecureHttp = true
            };

        }
    }
}