using System.Collections.Generic;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Utilities
{
    public class PropertyBag : IPropertyBag
    {
        private readonly IDictionary<string, object> _properties;
        private readonly IPropertyBag _parent;

        public PropertyBag (IPropertyBag parent = null)
        {
            _parent = parent;
            _properties = new Dictionary<string, object>();
        }

        public IPropertyBag CreateChild()
        {
            return new PropertyBag(this);
        }

        public T Get<T>(string name)
        {
            object result;
            if (_properties.TryGetValue(GetKey<T>(name), out result)) return (T)result;
            return _parent == null ? default(T) : _parent.Get<T>(name);
        }

        public void Set<T>(T value, string name)
        {
            _properties[GetKey<T>(name)] = value;
        }

        public string this[string name]
        {
            get { return Get<string>(name); }
            set { Set(value, name); }
        }

        private string GetKey<T>(string name)
        {
            if (name == null)
                name = typeof(T).FullName;
            return name.ToLower();
        }
    }
}