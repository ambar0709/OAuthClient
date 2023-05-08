using OAuthClient.Configuration;
using OAuthClient.Infrastructure;
using OAuthClient.Models;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace OAuthClient.Client
{
    /// <summary>
    /// Google OAuth2 Client Implementation
    /// </summary>
    /// <seealso cref="OAuthClient.Client.OAuth2Client" />
    public class GoogleOAuth2Client : OAuth2Client, IClient
    {
        private const string AccessType = "offline";
        private const string Prompt = "consent";
        private const string IncludeGrantedScope = "true";

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleOAuth2Client"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="configuration">The configuration.</param>
        public GoogleOAuth2Client(IRequestFactory factory, IClientConfiguration configuration) : base(factory, configuration) { }

        /// <summary>
        /// Defines URI of service which issues access code.
        /// </summary>
        protected override Endpoint AccessCodeServiceEndpoint
        {
            get
            {
                return new Endpoint
                {
                    BaseUri = "https://accounts.google.com",
                    Resource = "/o/oauth2/v2/auth"
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
                    BaseUri = "https://oauth2.googleapis.com",
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
                    BaseUri = "https://openidconnect.googleapis.com",
                    Resource = "/v1/userinfo"
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
                    BaseUri = "https://oauth2.googleapis.com/",
                    Resource = "revoke"
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
                access_type = AccessType,
                include_granted_scopes = IncludeGrantedScope,
                prompt = Prompt
            });
        }

        /// <summary>
        /// Called just before issuing request to service when everything is ready.
        /// Allows to add extra parameters to request or do any other needed preparations.
        /// </summary>
        protected override void BeforeGetUserInfo(BeforeAfterRequestArgs args)
        {
            args.Request.AddHeader("Authorization", $"Bearer {AccessToken}");
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
                FirstName = data["given_name"].Value<string>(),
                LastName = data["family_name"].Value<string>(),
                Email = data["email"].Value<string>(),
            };
        }

        /// <summary>
        /// Called just before issuing request to service revoking token
        /// </summary>
        /// <param name="args">The arguments.</param>
        protected override void BeforeRevokeToken(BeforeAfterRequestArgs args)
        {
            args.Request = _factory.CreateRequest(this.RevokeTokenServiceEndpoint, RestSharp.Method.Post);
            args.Request.AddObject(new
            {
                token = args.Parameters.GetOrThrowUnexpectedResponse("access_token")
            });
        }
    }
}
