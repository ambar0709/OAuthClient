using OAuthClient.Configuration;
using OAuthClient.Infrastructure;
using Newtonsoft.Json.Linq;
using RestSharp;
using OAuthClient.Models;

namespace OAuthClient.Client
{
    /// <summary>
    /// Class Zoom OAuth Client.
    /// </summary>
    /// <seealso cref="OAuthClient.Client.OAuth2Client" />
    public class ZoomOAuthClient : OAuth2Client, IClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZoomOAuthClient"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="configuration">The configuration.</param>
        public ZoomOAuthClient(IRequestFactory factory, IClientConfiguration configuration) : base(factory, configuration) { }

        /// <summary>
        /// Gets the access code service endpoint.
        /// </summary>
        /// <value>
        /// The access code service endpoint.
        /// </value>
        protected override Endpoint AccessCodeServiceEndpoint
        {
            get
            {
                return new Endpoint
                {
                    BaseUri = "https://zoom.us/",
                    Resource = "oauth/authorize"
                };
            }
        }

        /// <summary>
        /// Gets the access token service endpoint.
        /// </summary>
        /// <value>
        /// The access token service endpoint.
        /// </value>
        protected override Endpoint AccessTokenServiceEndpoint
        {
            get
            {
                return new Endpoint
                {
                    BaseUri = "https://zoom.us/",
                    Resource = "oauth/token"
                };
            }
        }

        /// <summary>
        /// Gets the user information service endpoint.
        /// </summary>
        /// <value>
        /// The user information service endpoint.
        /// </value>
        protected override Endpoint UserInfoServiceEndpoint
        {
            get
            {
                return new Endpoint
                {
                    BaseUri = "https://api.zoom.us/",
                    Resource = "v2/users/me"
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
                    BaseUri = "https://zoom.us/",
                    Resource = "oauth/revoke"
                };
            }
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
        /// <returns></returns>
        protected override UserInfo ParseUserInfo(string userInfo)
        {
            var data = JObject.Parse(userInfo);
            return new UserInfo()
            {
                FirstName = data["first_name"].Value<string>(),
                LastName = data["last_name"].Value<string>(),
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
                token = args.Parameters.GetOrThrowUnexpectedResponse("access_token"),
                client_id = Configuration.ClientId,
                client_secret = Configuration.ClientSecret,
            });
        }
    }
}
