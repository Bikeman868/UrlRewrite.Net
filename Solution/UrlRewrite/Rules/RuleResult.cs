using System.Collections.Generic;
using UrlRewrite.Interfaces;
using UrlRewrite.Utilities;

namespace UrlRewrite.Rules
{
    internal class RuleResult: IRuleResult
    {
        public List<IAction> Actions { get; set; }
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
