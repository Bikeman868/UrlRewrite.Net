using System;

namespace UrlRewrite.Interfaces
{
    public interface IFactory
    {
        /// <summary>
        /// Constructs an instance of specified type using dependency injection
        /// </summary>
        /// <typeparam name="T">The type of object to construct</typeparam>
        T Create<T>() where T: class;

        /// <summary>
        /// Constructs an instance of specified type using dependency injection
        /// </summary>
        /// <param name="type">The type of object to construct</param>
        object Create(Type type);
    }
}
