using UrlRewrite.Interfaces.Rules;
using UrlRewrite.Interfaces.Utilities;
using UrlRewrite.Utilities;

namespace UrlRewrite.Rules
{
    internal class RuleResult : IRuleResult
    {
        public bool StopProcessing { get; set; }
        public bool EndRequest { get; set; }
        public bool IsDynamic { get; set; }

        private IPropertyBag _properties;
        public IPropertyBag Properties
        {
            get 
            {
                if (_properties == null) _properties = new PropertyBag();
                return _properties;
            }
        }

    }
}
