using System;

namespace UrlRewrite.Utilities
{
    internal class UrlRewriteException : Exception
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
