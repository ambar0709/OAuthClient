using Extoms.OAuth.Infrastructure;
using Extoms.OAuth.Models;
using Newtonsoft.Json.Linq;
using RestSharp;
using Extoms.OAuth.Configuration;

namespace Extoms.OAuth.Client
{
    public class AzureADOAuthClient : OAuth2Client, IClient
    {
        private const string Prompt = "select_account";

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureADOAuthClient"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="configuration">The configuration.</param>
        public AzureADOAuthClient(IRequestFactory factory, IClientConfiguration configuration) : base(factory, configuration) { }

        /// <summary>
        /// Defines URI of service which issues access code.
        /// </summary>
        protected override Endpoint AccessCodeServiceEndpoint
        {
            get
            {
                return new Endpoint
                {
                    BaseUri = "https://login.microsoftonline.com/common/oauth2/v2.0",
                    Resource = "/authorize"
                };
            }
        }

        /// <summary>
        /// Defines URI of service which issues access token.
        /// </summary>
        protected override Endpoint AccessTokenServiceEndpoint
        {
            get
            {
                return new Endpoint
                {
                    BaseUri = "https://login.microsoftonline.com/common/oauth2/v2.0",
                    Resource = "/token"
                };
            }
        }

        /// <summary>
        /// Defines URI of service which allows to obtain information about user which is currently logged in.
        /// </summary>
        protected override Endpoint UserInfoServiceEndpoint
        {
            get
            {
                return new Endpoint
                {
                    BaseUri = "https://graph.microsoft.com/v1.0",
                    Resource = "/me"
                };
            }
        }

        /// <summary>
        /// Gets the revoke token service endpoint.
        /// </summary>
        /// <value>
        /// The revoke token service endpoint.
        /// </value>
        protected override Endpoint RevokeTokenServiceEndpoint
        {
            get
            {
                return new Endpoint
                {
                    BaseUri = "https://login.microsoftonline.com/",
                    Resource = "common/oauth2/v2.0/logout"
                };
            }
        }

        /// <summary>
        /// Befores the get login link.
        /// </summary>
        /// <param name="args">The arguments.</param>
        protected override void BeforeGetLoginLink(BeforeAfterRequestArgs args)
        {
            base.BeforeGetLoginLink(args);
            args.Request.AddObject(new
            {
                prompt = Prompt
            });
        }

        /// <summary>
        /// Parses the user information.
        /// </summary>
        /// <param name="userInfo">The user information.</param>
        /// <returns>
        /// User Details,
        /// </returns>
        protected override UserInfo ParseUserInfo(string userInfo)
        {
            var data = JObject.Parse(userInfo);
            return new UserInfo()
            {
                FirstName = data["givenName"].Value<string>(),
                LastName = data["surname"].Value<string>(),
                Email = data["mail"].Value<string>() ?? data["userPrincipalName"].Value<string>(),
            };
        }
    }
}
