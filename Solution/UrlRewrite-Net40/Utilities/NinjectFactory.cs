using System;
using Ioc.Modules;
using Ninject;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Utilities
{
    public class NinjectFactory: IFactory
    {
        private readonly IKernel _ninject;

        public NinjectFactory()
        {
            var packageLocator = new PackageLocator().ProbeBinFolderAssemblies();
            _ninject = new StandardKernel(new Ioc.Modules.Ninject.Module(packageLocator));
            _ninject.Rebind<IFactory>().ToConstant(this);
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
