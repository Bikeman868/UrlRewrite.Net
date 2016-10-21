using System.IO;
using System.Xml.Linq;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Operations;

namespace UrlRewrite.Operations
{
    public class AbsoluteUrlOperation : IAbsoluteUrlOperation
    {
        public IAbsoluteUrlOperation Initialize()
        {
            return this;
        }

        public string Execute(string value)
        {
            if (ReferenceEquals(value, null) || value.Length == 0) return "/";
            if (value[0] == '/' || value.Contains("://")) return value;
            return "/" + value;
        }

        public string ToString(IRequestInfo requestInfo)
        {
            return "ToAbsoluteUrl()";
        }

        public override string ToString()
        {
            return "ToAbsoluteUrl()";
        }

        public void Describe(TextWriter writer, string indent, string indentText)
        {
            writer.Write(indent);
            writer.WriteLine("convert to absolute URL");
        }
    }
}
