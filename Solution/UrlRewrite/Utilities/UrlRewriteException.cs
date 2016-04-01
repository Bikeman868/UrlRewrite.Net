using System;

namespace UrlRewrite.Utilities
{
    public class UrlRewriteException: Exception
    {
        public UrlRewriteException(string message) : base(message)
        {
        }

        public UrlRewriteException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
