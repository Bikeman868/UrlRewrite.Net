using System;
using Ninject;
using Ninject.Modules;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Conditions;
using UrlRewrite.Interfaces.Rules;
using UrlRewrite.Interfaces.Utilities;

namespace UrlRewrite.Utilities
{
    public class NinjectFactory: IFactory
    {
        private readonly IKernel _ninject;

        public NinjectFactory()
        {
            _ninject = new StandardKernel(new NinjectUrlRewriteModule(this));
        }

        T IFactory.Create<T>()
        {
            return (T)(((IFactory)this).Create(typeof(T)));
        }

        object IFactory.Create(Type type)
        {
            var result = _ninject.TryGet(type);
            if (ReferenceEquals(result, null))
            {
                var constructor = type.GetConstructor(Type.EmptyTypes);
                result = constructor == null ? null : constructor.Invoke(null);
                if (ReferenceEquals(result, null))
                    throw new UrlRewriteException(
                        "Failed to construct instance of " 
                        + type.FullName 
                        + " because it is not registered in IoC and has no default public constructor");
            }
            return result;
        }
    }
}
