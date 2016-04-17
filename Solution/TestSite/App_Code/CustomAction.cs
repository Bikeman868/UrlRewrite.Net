using System.IO;
using System.Xml.Linq;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Rules;

namespace TestSite
{
    public class CustomAction: IAction
    {
        public IAction Initialize(XElement configuration)
        {
            return this;
        }

        public void PerformAction(IRequestInfo request, IRuleResult ruleResult, out bool stopProcessing, out bool endRequest)
        {
            stopProcessing = false;
            endRequest = false;
        }

        public string ToString(IRequestInfo requestInfo)
        {
            return ToString();
        }

        public override string ToString()
        {
            return "Application defined custom action";
        }

        public void Describe(TextWriter writer, string indent, string indentText)
        {
            writer.WriteLine(indent + ToString());
        }
    }
}