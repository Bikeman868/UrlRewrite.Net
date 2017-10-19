using System.IO;
using System.Web;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Operations;

namespace UrlRewrite.Operations
{
    internal class UrlEncodeOperation : IUrlEncodeOperation
    {
        public IUrlEncodeOperation Initialize()
        {
            return this;
        }

        public string Execute(string value)
        {
            return ReferenceEquals(value, null) ? string.Empty : HttpUtility.UrlEncode(value);
        }

        public string ToString(IRequestInfo requestInfo)
        {
            return "UrlEncode()";
        }

        public override string ToString()
        {
            return "UrlEncode()";
        }

        public void Describe(TextWriter writer, string indent, string indentText)
        {
            writer.Write(indent);
            writer.WriteLine("url encode");
        }
    }
}
