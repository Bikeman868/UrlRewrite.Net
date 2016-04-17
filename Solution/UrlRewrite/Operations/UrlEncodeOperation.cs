using System.IO;
using System.Web;
using System.Xml.Linq;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Operations;

namespace UrlRewrite.Operations
{
    public class UrlEncodeOperation: IOperation
    {
        public string Execute(string value)
        {
            return ReferenceEquals(value, null) ? string.Empty : HttpUtility.UrlEncode(value);
        }

        public IOperation Initialize(XElement configuration)
        {
            return this;
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
