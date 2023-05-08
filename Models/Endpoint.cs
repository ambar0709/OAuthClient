namespace OAuthClient.Models
{
    /// <summary>
    /// Defines endpoint URI (service address).
    /// </summary>
    public class Endpoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Endpoint"/> class.
        /// </summary>
        public Endpoint() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Endpoint"/> class.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="resource">The resource.</param>
        public Endpoint(string baseUri, string resource)
        {
            BaseUri = baseUri;
            Resource = resource;
        }

        /// <summary>
        /// Base URI (service host address).
        /// </summary>
        public string BaseUri { get; set; }

        /// <summary>
        /// Resource URI (service method).
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Complete URI of endpoint (base URI combined with resource URI).
        /// </summary>
        public string Uri { get { return BaseUri + Resource; } }
    }
}