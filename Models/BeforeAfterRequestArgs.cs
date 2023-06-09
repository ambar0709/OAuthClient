using System.Collections.Specialized;
using Extoms.OAuth.Configuration;
using RestSharp;

namespace Extoms.OAuth.Models
{
    public class BeforeAfterRequestArgs
    {
        /// <summary>
        /// Client instance.
        /// </summary>
        public RestClient Client { get; set; }

        /// <summary>
        /// Request instance.
        /// </summary>
        public RestRequest Request { get; set; }

        /// <summary>
        /// Response instance.
        /// </summary>
        public RestResponse Response { get; set; }

        /// <summary>
        /// Values received from service.
        /// </summary>
        public NameValueCollection Parameters { get; set; }

        /// <summary>
        /// Client configuration.
        /// </summary>
        public IClientConfiguration Configuration { get; set; }
    }
}