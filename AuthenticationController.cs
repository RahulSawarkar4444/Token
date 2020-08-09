using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace WebApplication1
{
    public class AuthenticationController : ApiController
    {
        #region Private Members

        /// <summary>
        /// Unity container object.
        /// </summary>
        private IUnityContainer container;

        /// <summary>
        /// The authy manager
        /// </summary>
        private AuthyManager authyManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the AuthenticationController class
        /// </summary>
        /// <param name="container">the unity container object.</param>
        public AuthenticationController(IUnityContainer container)
        {
            this.container = container;
            this.authyManager = new AuthyManager();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This function is responsible for authenticate user.
        /// </summary>
        /// <param name="authenticationParameter">The authentication search parameter.</param>
        /// <returns>Returns user details.</returns>
        [HttpPost]
        public JObject Authenticate(Authenticate authenticationParameter)
        {
            string ipAddress, deviceType, browserName;
            try
            {
                bool isUserAllowed = false;
                if (ConfigurationManager.AppSettings["IsActive"].ToString() == "1")
                {
                    isUserAllowed = true;
                }
                else
                {
                    if (authenticationParameter.Email == ConfigurationManager.AppSettings["EmergencyLogin"].ToString())
                    {
                        isUserAllowed = true;
                    }
                    else
                    {
                        throw new Exception(ConfigurationManager.AppSettings["EndMsg"].ToString());
                    }
                }

                if (isUserAllowed)
                {
                    ipAddress = HttpContext.Current.Request.UserHostAddress;
                    deviceType = HttpContext.Current.Request.Browser.IsMobileDevice ? DeviceType.Mobile.GetDescription() : DeviceType.Desktop.GetDescription();
                    browserName = HttpContext.Current.Request.UserAgent;
                    string tenantCode = Guid.Empty.ToString();
                    tenantCode = ((IEnumerable<string>)Request.Headers.GetValues(00000000)).FirstOrDefault();
                    User user = new AuthenticationManager(this.container).UserAuthenticate(authenticationParameter.Email, authenticationParameter.Password, ipAddress, deviceType, browserName, tenantCode, authenticationParameter.IsForcefullyLogout);

                    //Generate Token
                    string baseAddress = ConfigurationManager.AppSettings["BaseURL"].ToString();
                    using (var client = new HttpClient())
                    {
                        var form = new Dictionary<string, string>
                       {
                        {"grant_type", "password"},
                        {"username", authenticationParameter.Email},
                        {"password", authenticationParameter.Password},
                        {"ClientId", tenantCode},
                        {"User", JsonConvert.SerializeObject(user).ToString()},
                    };
                        var tokenResponse = client.PostAsync(baseAddress + "Token", new FormUrlEncodedContent(form)).Result;
                        var token = tokenResponse.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<JObject>(token.Result);
                    }
                }
            }
            catch
            {
                throw;
            }
            return null;
        }

        #endregion
    }
}