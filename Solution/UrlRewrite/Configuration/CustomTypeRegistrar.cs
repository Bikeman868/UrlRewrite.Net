using System;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Configuration
{
    internal class CustomTypeRegistrar : ICustomTypeRegistrar
    {
        public void RegisterOperation(Type type, string name)
        {
        }

        public void RegisterAction(Type type, string name)
        {
        }

        public void RegisterCondition(Type type, string name)
        {
        }

        public IOperation ConstructOperation(string name, XElement configuration)
        {
            return null;
        }

        public IAction ConstructAction(string name, XElement configuration)
        {
            return null;
        }

        public ICondition ConstructCondition(string name, XElement configuration, IValueGetter valueGetter)
        {
            return null;
        }
    }
}
