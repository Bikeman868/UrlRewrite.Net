using System;

namespace UrlRewrite.Utilities
{
    public class IocRegistration
    {
        public IocRegistration Init<TInterface, TClass>(IocLifetime lifetime) 
            where TInterface: class
            where TClass: class, TInterface
        {
            Interfacetype = typeof (TInterface);
            ConcreteType = typeof (TClass);
            Lifetime = lifetime;

            return this;
        }

        public Type Interfacetype { get; set; }
        public Type ConcreteType { get; set; }
        public IocLifetime Lifetime { get; set; }
    }
}
