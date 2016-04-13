using System.IO;
using System.Web;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Operations
{
    public class UrlDecodeOperation: IOperation
    {
        public string Execute(string value)
        {
            return ReferenceEquals(value, null) ? string.Empty : HttpUtility.UrlDecode(value);
        }

        public IOperation Initialize(XElement configuration)
        {
            return this;
        }

        public string ToString(IRequestInfo requestInfo)
        {
            return "UrlDecode()";
        }

        public override string ToString()
        {
            return "UrlDecode()";
        }

        public void Describe(TextWriter writer, string indent, string indentText)
        {
            writer.Write(indent);
            writer.WriteLine("url decode");
        }
    }
}
