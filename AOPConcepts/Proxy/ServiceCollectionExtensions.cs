using System;
using Microsoft.Extensions.DependencyInjection;

namespace AOPConcepts.Proxy
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection WrapSingleton<T, U>(this IServiceCollection collection,
            params ProxyHandler<T>[] handlers)
            where T : class
            where U : class, T
        {
            collection.AddSingleton<T>(provider =>
            {
                var instance = ActivatorUtilities.CreateInstance<U>(provider);
                var proxy = Proxy<U>.ProxyOf(instance, handlers);
                return proxy;
            });
            return collection;
        }
    }
}