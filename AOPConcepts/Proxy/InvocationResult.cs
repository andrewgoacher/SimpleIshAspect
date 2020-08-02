using System.Reflection;

namespace AOPConcepts.Proxy
{
    public class InvocationResult
    {
        public MethodInfo Method { get; set; }
        public object Result { get; set; }
    }
}