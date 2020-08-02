using System;
using System.Reflection;
using AOPConcepts.Proxy;

namespace AOPConcepts
{
    public class LogHandler<T> : ProxyHandler<T>
    {
        protected internal override void Init(Proxy<T> p)
        {
            p.Result += POnResult;
            p.AfterExecution += POnAfterExecution;
            p.BeforeExecution += POnBeforeExecution;
            p.TaskException += POnTaskException;
        }

        private void POnTaskException(object? sender, TaskException e)
        {
            Console.WriteLine("Task Exception");
        }

        private void POnBeforeExecution(object? sender, MethodInfo e)
        {
            Console.WriteLine($"Method: {e.Name} Begin");
        }

        private void POnAfterExecution(object? sender, MethodInfo e)
        {
            Console.WriteLine($"Method: {e.Name} End");
        }

        private void POnResult(object? sender, InvocationResult e)
        {
            Console.WriteLine($"Method: {e.Method.Name}: Result ({e.Result})");
        }
    }

}