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

        public IOperation Initialize(XElement configuration)
        {
            return this;
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
