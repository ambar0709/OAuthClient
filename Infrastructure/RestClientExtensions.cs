using System.Net;
using OAuthClient.Models;
using RestSharp;

namespace OAuthClient.Infrastructure
{
    public static class RestClientExtensions
    {
        static RestResponse VerifyResponse(RestResponse response)
        {
            if (response.Content.IsEmpty() || (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created))
            {
                throw new UnexpectedResponseException(response);
            }

            return response;
        }

        public static async Task<RestResponse> ExecuteAndVerifyAsync(this RestClient client, RestRequest request, CancellationToken cancellationToken = default)
        {
            return VerifyResponse(await client.ExecuteAsync(request, cancellationToken).ConfigureAwait(false));
        }
    }
}