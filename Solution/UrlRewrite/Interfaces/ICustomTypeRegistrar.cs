using System;
using System.Xml.Linq;

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
