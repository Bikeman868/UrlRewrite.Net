using System.IO;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Operations
{
    public class UpperCaseOperation: IOperation
    {
        public string Execute(string value)
        {
            return ReferenceEquals(value, null) ? string.Empty : value.ToUpper();
        }

        public void Initialize(XElement configuration)
        {
        }

        public string ToString(IRequestInfo requestInfo)
        {
            return "ToUpper()";
        }

        public override string ToString()
        {
            return "ToUpper()";
        }

        public void Describe(TextWriter writer, string indent, string indentText)
        {
            writer.Write(indent);
            writer.WriteLine("convert to upper case");
        }
    }
}
