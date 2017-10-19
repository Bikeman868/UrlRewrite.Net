using System.IO;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Operations;

namespace UrlRewrite.Operations
{
    internal class UpperCaseOperation : IUpperCaseOperation
    {
        public IUpperCaseOperation Initialize()
        {
            return this;
        }

        public string Execute(string value)
        {
            return ReferenceEquals(value, null) ? string.Empty : value.ToUpper();
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
