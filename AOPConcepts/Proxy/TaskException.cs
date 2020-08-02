using System;
using System.Reflection;

namespace AOPConcepts.Proxy
{
    public class TaskException
    {
        public MethodInfo Method { get; set; }
        public Exception Exception { get; set; }
    }
}