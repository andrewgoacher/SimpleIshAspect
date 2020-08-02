using Microsoft.Extensions.DependencyInjection;

namespace AOPConcepts.Proxy
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection WrapSingleton<T, U>(this IServiceCollection collection, U impl,
            params ProxyHandler<T>[] handlers)
            where U : T
            where T : class
        {
            var wrapped = Proxy<T>.ProxyOf(impl, handlers);
            collection.AddSingleton<T>(wrapped);
            return collection;
        }
    }
}