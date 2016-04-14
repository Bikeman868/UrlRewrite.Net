using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Configuration
{
    internal class CustomTypeRegistrar : ICustomTypeRegistrar
    {
        private readonly IFactory _factory;
        private readonly IDictionary<string, Type> _conditions;
        private readonly IDictionary<string, Type> _operations;
        private readonly IDictionary<string, Type> _actions;

        public CustomTypeRegistrar(IFactory factory)
        {
            _factory = factory;
            _conditions = new Dictionary<string, Type>();
            _operations = new Dictionary<string, Type>();
            _actions = new Dictionary<string, Type>();
        }

        public void RegisterOperation(Type type, string name)
        {
            _operations[name.ToLower()] = type;
        }

        public void RegisterAction(Type type, string name)
        {
            _actions[name.ToLower()] = type;
        }

        public void RegisterCondition(Type type, string name)
        {
            _conditions[name.ToLower()] = type;
        }

        public IOperation ConstructOperation(string name, XElement configuration)
        {
            Type type;

            if (_operations.TryGetValue(name.ToLower(), out type))
                return _factory.Create(type) as IOperation;

            return null;
        }

        public IAction ConstructAction(string name, XElement configuration)
        {
            Type type;

            if (_actions.TryGetValue(name.ToLower(), out type))
                return _factory.Create(type) as IAction;

            return null;
        }

        public ICondition ConstructCondition(string name, XElement configuration, IValueGetter valueGetter)
        {
            Type type;

            if (_conditions.TryGetValue(name.ToLower(), out type))
                return _factory.Create(type) as ICondition;

            return null;
        }
    }
}
