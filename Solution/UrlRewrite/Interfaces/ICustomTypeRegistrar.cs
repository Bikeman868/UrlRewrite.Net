using System;
using System.Xml.Linq;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Conditions;
using UrlRewrite.Interfaces.Operations;

namespace UrlRewrite.Interfaces
{
    public interface ICustomTypeRegistrar
    {
        void RegisterOperation(Type type, string name);
        void RegisterAction(Type type, string name);
        void RegisterCondition(Type type, string name);

        IOperation ConstructOperation(string name, XElement configuration);
        IAction ConstructAction(string name, XElement configuration);
        ICondition ConstructCondition(string name, XElement configuration, IValueGetter valueGetter);
    }
}
