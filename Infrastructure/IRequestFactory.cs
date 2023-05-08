using RestSharp;

namespace OAuthClient.Infrastructure
{
    /// <summary>
    /// Intended for REST client/request creation.
    /// </summary>
    public interface IRequestFactory
    {
        /// <summary>
        /// Returns new REST client instance.
        /// </summary>
        RestClient CreateClient(Uri baseUri);
        
        /// <summary>
        /// Returns new REST request instance.
        /// </summary>
        RestRequest CreateRequest();
    }
}