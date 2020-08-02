namespace AOPConcepts.Proxy
{
    public abstract class ProxyHandler<T>
    {
        protected internal abstract void Init(Proxy<T> p);
    }
}