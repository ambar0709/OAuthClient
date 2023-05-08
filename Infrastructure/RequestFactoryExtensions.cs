using OAuthClient.Models;
using RestSharp;

namespace OAuthClient.Infrastructure
{
    public static class RequestFactoryExtensions
    {
        public static RestClient CreateClient(this IRequestFactory factory, Endpoint endpoint)
        {
            return factory.CreateClient(new Uri(endpoint.BaseUri));
        }

        public static RestRequest CreateRequest(this IRequestFactory factory, Endpoint endpoint)
        {
            return CreateRequest(factory, endpoint, Method.Get);
        }

        public static RestRequest CreateRequest(this IRequestFactory factory, Endpoint endpoint, Method method)
        {
            var request = factory.CreateRequest();
            request.Resource = endpoint.Resource;
            request.Method = method;
            return request;
        }
    }
}