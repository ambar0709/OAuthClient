namespace Extoms.OAuth.Infrastructure
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }
    }
}
