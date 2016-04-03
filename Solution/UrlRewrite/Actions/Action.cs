using System.Text;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    internal class Action
    {
        protected bool _stopProcessing;
        protected bool _endRequest;

        public virtual void Initialize(XElement configuration)
        {
        }

    }
}
