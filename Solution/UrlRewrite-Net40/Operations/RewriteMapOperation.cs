using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Operations;
using UrlRewrite.Utilities;

namespace UrlRewrite.Operations
{
    internal class RewriteMapOperation : IRewriteMapOperation
    {
        public string Name { get; private set; }

        private string _defaultValue;
        private IDictionary<string, string> _map;

        public string Execute(string value)
        {
            if (ReferenceEquals(value, null))
                return _defaultValue;

            string result;
            return _map.TryGetValue(value.ToLower(), out result) ? result : _defaultValue;
        }

        public IRewriteMapOperation Initialize(XElement element)
        {
            _map = new Dictionary<string, string>();
            Name = string.Empty;
            _defaultValue = string.Empty;

            if (element.HasAttributes)
            {
                foreach (var attribute in element.Attributes())
                {
                    switch(attribute.Name.LocalName.ToLower())
                    {
                        case "name":
                            Name = attribute.Value;
                            break;
                        case "defaultvalue":
                            _defaultValue = attribute.Value;
                            break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(Name))
                throw new UrlRewriteException("Rewrite maps must have a name attribute");

            if (Name.Contains(":"))
                throw new UrlRewriteException("Rewrite map names can not contain : charaters");

            if (element.HasElements)
            {
                foreach (var child in element.Elements())
                {
                    if (child.Name.LocalName.ToLower() == "add")
                    {
                        var key = string.Empty;
                        var value = string.Empty;
                        foreach(var attribute in child.Attributes())
                        {
                            switch (attribute.Name.LocalName.ToLower())
                            {
                                case "key":
                                    key = attribute.Value;
                                    break;
                                case "value":
                                    value = attribute.Value;
                                    break;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(key))
                        {
                            _map.Add(key.ToLower(), value);
                        }
                    }
                }
            }
         
            return this;
        }

        public string ToString(IRequestInfo requestInfo)
        {
            return Name + "()";
        }

        public override string ToString()
        {
            return Name + "()";
        }

        public void Describe(TextWriter writer, string indent, string indentText)
        {
            writer.Write(indent);
            writer.WriteLine("lookup rewrite map " + Name);
        }
    }
}
