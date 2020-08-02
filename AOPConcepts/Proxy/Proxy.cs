using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AOPConcepts.Proxy
{
      public class Proxy<T> : DispatchProxy
    {
        private T DecoratedClass { get; set; }

        public event EventHandler<MethodInfo> BeforeExecution;
        public event EventHandler<MethodInfo> AfterExecution;
        public event EventHandler<InvocationResult> Result;
        public event EventHandler<TaskException> TaskException; 

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            object result = null;
            
            try
            {
                BeforeExecution?.Invoke(this, targetMethod);

                result = targetMethod.Invoke(DecoratedClass, args);

                if (targetMethod.ReturnType != typeof(void))
                {
                    HandleResult(targetMethod, result);
                }
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    throw ex.InnerException ?? ex;
                }

            }
            AfterExecution?.Invoke(this, targetMethod);

            return result;
        }

        private void HandleResult(MethodInfo info, object result)
        {
            var resultTask = result as Task;
            if (resultTask != null)
            {
                resultTask.ContinueWith(task =>
                    {
                        if (task.Exception != null)
                        {
                            TaskException?.Invoke(this, new TaskException
                            {
                                Method = info,
                                Exception = task.Exception.InnerException ?? task.Exception
                            });
                        }
                        else
                        {
                            object taskResult = null;
                            if (task.GetType().GetTypeInfo().IsGenericType &&
                                task.GetType().GetGenericTypeDefinition() == typeof(Task<>))
                            {
                                var property = task.GetType().GetTypeInfo().GetProperties()
                                    .FirstOrDefault(p => p.Name == "Result");
                                if (property != null)
                                {
                                    taskResult = property.GetValue(task);
                                    Result?.Invoke(this, new InvocationResult
                                    {
                                        Method = info,
                                        Result = taskResult
                                    });
                                }
                            }
                        }
                    });
            }
            else
            {
                Result?.Invoke(this, new InvocationResult
                {
                    Method = info,
                    Result = result
                });
            }
        }

        public static T ProxyOf<T>(T decorated, params ProxyHandler<T>[] handlers)
        {
            if (decorated == null)
            {
                throw new ArgumentNullException(nameof(decorated));
            }

            object wrappedObject = Create<T, Proxy<T>>();
            Proxy<T> proxy = (Proxy<T>) wrappedObject;
            proxy.DecoratedClass = decorated;

            foreach (var handler in handlers)
            {
                handler.Init(proxy);
            }
            
            return (T) wrappedObject;
        }
    }
}