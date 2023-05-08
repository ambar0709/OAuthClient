using System.Collections.Specialized;
using System.Web;
using Extoms.OAuth.Infrastructure;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Extoms.OAuth.Models;
using Extoms.OAuth.Configuration;

namespace Extoms.OAuth.Client
{
    public abstract class OAuth2Client : IClient
    {
        private const string AccessTokenKey = "access_token";
        private const string RefreshTokenKey = "refresh_token";
        private const string ExpiresKey = "expires_in";
        private const string TokenTypeKey = "token_type";
        private const string ScopeKey = "scope";

        internal readonly IRequestFactory _factory;

        public IClientConfiguration Configuration { get; private set; }

        public string State { get; private set; }

        public string AccessToken { get; private set; }

        public string RefreshToken { get; private set; }

        public string TokenType { get; private set; }

        public string Scope { get; private set; }

        public DateTime ExpiresAt { get; private set; }

        public string UserInfo { get; private set; }

        private string GrantType { get; set; }

        protected abstract Endpoint AccessCodeServiceEndpoint { get; }

        protected abstract Endpoint AccessTokenServiceEndpoint { get; }

        protected abstract Endpoint UserInfoServiceEndpoint { get; }

        protected abstract Endpoint RevokeTokenServiceEndpoint { get; }

        public OAuth2Client(IRequestFactory factory, IClientConfiguration configuration)
        {
            _factory = factory;
            Configuration = configuration;
        }

        public virtual Task<string> GetLoginLinkUriAsync(string state = null, CancellationToken cancellationToken = default)
        {
            var client = _factory.CreateClient(this.AccessCodeServiceEndpoint);
            var request = _factory.CreateRequest(this.AccessCodeServiceEndpoint);
            this.State = state;
            this.BeforeGetLoginLink(new BeforeAfterRequestArgs
            {
                Client = client,
                Request = request,
                Configuration = Configuration
            });

            return Task.FromResult(client.BuildUri(request).ToString());
        }

        public async Task<string> GetTokenAsync(NameValueCollection parameters, CancellationToken cancellationToken = default)
        {
            GrantType = "authorization_code";
            CheckErrorAndSetState(parameters);
            await QueryAccessTokenAsync(parameters, cancellationToken);
            return AccessToken;
        }

        public async Task RevokeTokenAsync(string accessToken = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new Exception("Access token not provided");
            }

            var client = _factory.CreateClient(this.RevokeTokenServiceEndpoint);

            NameValueCollection parameters = new()
            {
                { "access_token", accessToken }
            };

            var args = new BeforeAfterRequestArgs
            {
                Client = client,
                Parameters = parameters,
                Configuration = Configuration
            };

            BeforeRevokeToken(args);
            await client.ExecuteAndVerifyAsync(args.Request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetCurrentTokenAsync(string refreshToken = null, string accessToken = null, DateTime expiresAt = default, bool forceUpdate = false, CancellationToken cancellationToken = default)
        {
            if (!forceUpdate && expiresAt != default && DateTime.UtcNow < expiresAt && !string.IsNullOrEmpty(accessToken))
            {
                AccessToken = accessToken;
                return accessToken;
            }

            NameValueCollection parameters = new();
            if (!string.IsNullOrEmpty(refreshToken))
            {
                parameters.Add("refresh_token", refreshToken);
            }
            else if (!string.IsNullOrEmpty(RefreshToken))
            {
                parameters.Add("refresh_token", RefreshToken);
            }

            if (parameters.Count > 0)
            {
                GrantType = "refresh_token";
                await QueryAccessTokenAsync(parameters, cancellationToken).ConfigureAwait(false);
                return AccessToken;
            }

            throw new Exception("Token never fetched and refresh token not provided.");
        }

        public async Task<UserInfo> GetUserInfoAsync(NameValueCollection parameters, CancellationToken cancellationToken = default)
        {
            GrantType = "authorization_code";
            CheckErrorAndSetState(parameters);
            await QueryAccessTokenAsync(parameters, cancellationToken).ConfigureAwait(false);
            return await GetUserInfoAsync(cancellationToken).ConfigureAwait(false);
        }

        private string ParseTokenResponse(string content, string key)
        {
            if (string.IsNullOrEmpty(content) || String.IsNullOrEmpty(key))
                return null;

            try
            {
                // response can be sent in JSON format
                var token = JObject.Parse(content).SelectToken(key);
                return token?.ToString();
            }
            catch (JsonReaderException)
            {
                // or it can be in "query string" format (param1=val1&param2=val2)
                var collection = HttpUtility.ParseQueryString(content);
                return collection[key];
            }
        }

        private async Task QueryAccessTokenAsync(NameValueCollection parameters, CancellationToken cancellationToken = default)
        {
            var client = _factory.CreateClient(this.AccessTokenServiceEndpoint);
            var request = _factory.CreateRequest(this.AccessTokenServiceEndpoint, Method.Post);

            BeforeGetAccessToken(new BeforeAfterRequestArgs
            {
                Client = client,
                Request = request,
                Parameters = parameters,
                Configuration = Configuration
            });

            var response = await client.ExecuteAndVerifyAsync(request, cancellationToken).ConfigureAwait(false);

            AfterGetAccessToken(new BeforeAfterRequestArgs
            {
                Response = response,
                Parameters = parameters
            });

            AccessToken = ParseTokenResponse(response.Content, AccessTokenKey);
            if (string.IsNullOrEmpty(AccessToken))
            {
                throw new UnexpectedResponseException(AccessTokenKey);
            }

            if (GrantType != "refresh_token")
            {
                RefreshToken = ParseTokenResponse(response.Content, RefreshTokenKey);
            }

            TokenType = ParseTokenResponse(response.Content, TokenTypeKey);

            Scope = ParseTokenResponse(response.Content, ScopeKey);

            if (Int32.TryParse(ParseTokenResponse(response.Content, ExpiresKey), out int expiresIn))
            {
                ExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn);
            }
        }

        private void CheckErrorAndSetState(NameValueCollection parameters)
        {
            const string errorFieldName = "error";
            var error = parameters[errorFieldName];
            if (!error.IsEmpty())
            {
                throw new UnexpectedResponseException(errorFieldName);
            }

            State = parameters["state"];
        }

        protected virtual void BeforeGetLoginLink(BeforeAfterRequestArgs args)
        {
            args.Request.AddObject(new
            {
                response_type = "code",
                client_id = Configuration.ClientId,
                redirect_uri = Configuration.RedirectUri,
                state = this.State
            });

            if (!string.IsNullOrEmpty(Configuration.Scope))
            {
                args.Request.AddParameter("scope", Configuration.Scope);
            }
        }

        protected virtual void BeforeGetAccessToken(BeforeAfterRequestArgs args)
        {
            if (GrantType == "refresh_token")
            {
                args.Request.AddObject(new
                {
                    refresh_token = args.Parameters.GetOrThrowUnexpectedResponse("refresh_token"),
                    client_id = Configuration.ClientId,
                    client_secret = Configuration.ClientSecret,
                    grant_type = GrantType
                });
            }
            else
            {
                args.Request.AddObject(new
                {
                    code = args.Parameters.GetOrThrowUnexpectedResponse("code"),
                    client_id = Configuration.ClientId,
                    client_secret = Configuration.ClientSecret,
                    redirect_uri = Configuration.RedirectUri,
                    grant_type = GrantType
                });
            }
        }

        protected virtual async Task<UserInfo> GetUserInfoAsync(CancellationToken cancellationToken = default)
        {
            var client = _factory.CreateClient(UserInfoServiceEndpoint);
            var request = _factory.CreateRequest(UserInfoServiceEndpoint);

            BeforeGetUserInfo(new BeforeAfterRequestArgs
            {
                Client = client,
                Request = request,
                Configuration = Configuration
            });

            var response = await client.ExecuteAndVerifyAsync(request, cancellationToken).ConfigureAwait(false);

            return this.ParseUserInfo(response.Content);
        }

        protected virtual void AfterGetAccessToken(BeforeAfterRequestArgs args)
        {
        }

        protected virtual void BeforeGetUserInfo(BeforeAfterRequestArgs args)
        {
            args.Request.AddHeader("Authorization", AccessToken);
        }

        protected abstract UserInfo ParseUserInfo(string userInfo);

        protected virtual void BeforeRevokeToken(BeforeAfterRequestArgs args)
        {
            args.Request = _factory.CreateRequest(this.RevokeTokenServiceEndpoint);
            args.Request.AddObject(new
            {
                access_token = args.Parameters.GetOrThrowUnexpectedResponse("access_token")
            });
        }
    }
}