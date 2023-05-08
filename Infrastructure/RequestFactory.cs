using RestSharp;

namespace Extoms.OAuth.Infrastructure
{
    /// <summary>
    /// Default implementation of <see cref="IRequestFactory"/>.
    /// </summary>
    public class RequestFactory : IRequestFactory
    {
        /// <summary>
        /// Returns new REST client instance.
        /// </summary>
        public RestClient CreateClient(Uri baseUri)
        {
            return new RestClient(baseUri);
        }

        /// <summary>
        /// Returns new REST request instance.
        /// </summary>
        public RestRequest CreateRequest()
        {
            return new RestRequest();
        }
    }
}