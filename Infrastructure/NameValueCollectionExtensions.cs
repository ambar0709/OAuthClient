using System.Collections.Specialized;
using Extoms.OAuth.Client;

namespace Extoms.OAuth.Infrastructure
{
    public static class NameValueCollectionExtensions
    {
        public static string GetOrThrowUnexpectedResponse(this NameValueCollection collection, string key)
        {
            var value = collection[key];
            if (value.IsEmpty())
            {
                throw new UnexpectedResponseException(key);
            }
            return value;
        }
    }
}