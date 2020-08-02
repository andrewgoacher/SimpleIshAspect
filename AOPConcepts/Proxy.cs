using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AOPConcepts
{
    public abstract class ProxyHandler<T>
    {
        protected internal abstract void Init(Proxy<T> p);
    }

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

        private void POnResult(object? sender, InvokationResult e)
        {
            Console.WriteLine($"Method: {e.Method.Name}: Result ({e.Result})");
        }
    }

    public class InvokationResult
    {
        public MethodInfo Method { get; set; }
        public object Result { get; set; }
    }

    public class TaskException
    {
        public MethodInfo Method { get; set; }
        public Exception Exception { get; set; }
    }

    public class Proxy<T> : DispatchProxy
    {
        private T DecoratedClass { get; set; }

        public event EventHandler<MethodInfo> BeforeExecution;
        public event EventHandler<MethodInfo> AfterExecution;
        public event EventHandler<InvokationResult> Result;
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
                                    Result?.Invoke(this, new InvokationResult
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
                Result?.Invoke(this, new InvokationResult
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

        // public static T Create(T decorated, Action<string> logInfo, Action<string> logError,
        //     Func<object, string> serializeFunction, TaskScheduler loggingScheduler = null)
        // {
        //     object proxy = Create<T, Proxy<T>>();
        //     ((Proxy<T>) proxy).SetParameters(decorated, logInfo, logError, serializeFunction, loggingScheduler);
        //     return (T) proxy;
        // }

        // private void SetParameters(T decorated, Action<string> logInfo, Action<string> logError,
        //     Func<object, string> serializeFunction, TaskScheduler loggingScheduler)
        // {
        //     if (decorated == null)
        //     {
        //         throw new ArgumentNullException(nameof(decorated));
        //     }
        //
        //     _decorated = decorated;
        //     _logInfo = logInfo;
        //     _logError = logError;
        //     _serializeFunction = serializeFunction;
        //     _loggingScheduler = loggingScheduler ?? TaskScheduler.FromCurrentSynchronizationContext();
        // }

        // private string GetStringValue(object obj)
        // {
        //     if (obj == null)
        //     {
        //         return "null";
        //     }
        //
        //     if (obj.GetType().GetTypeInfo().IsPrimitive || obj.GetType().GetTypeInfo().IsEnum || obj is string)
        //     {
        //         return obj.ToString();
        //     }
        //
        //     try
        //     {
        //         return _serializeFunction?.Invoke(obj) ?? obj.ToString();
        //     }
        //     catch
        //     {
        //         return obj.ToString();
        //     }
        // }

        // private void LogException(Exception exception, MethodInfo methodInfo = null)
        // {
        // try
        // {
        //     var errorMessage = new StringBuilder();
        //     errorMessage.AppendLine($"Class {_decorated.GetType().FullName}");
        //     errorMessage.AppendLine($"Method {methodInfo?.Name} threw exception");
        //     errorMessage.AppendLine(exception.GetDescription());
        //     _logError?.Invoke(errorMessage.ToString());
        // }
        // catch (Exception)
        // {
        //     // ignored
        //     //Method should return original exception
        // }
        // }

        // private void LogAfter(MethodInfo methodInfo, object[] args, object result)
        // {
        //     var afterMessage = new StringBuilder();
        //     afterMessage.AppendLine($"Class {_decorated.GetType().FullName}");
        //     afterMessage.AppendLine($"Method {methodInfo.Name} executed");
        //     afterMessage.AppendLine("Output:");
        //     afterMessage.AppendLine(GetStringValue(result));
        //     var parameters = methodInfo.GetParameters();
        //     if (parameters.Any())
        //     {
        //         afterMessage.AppendLine("Parameters:");
        //         for (var i = 0; i < parameters.Length; i++)
        //         {
        //             var parameter = parameters[i];
        //             var arg = args[i];
        //             afterMessage.AppendLine($"{parameter.Name}:{GetStringValue(arg)}");
        //         }
        //     }
        //
        //     _logInfo?.Invoke(afterMessage.ToString());
        // }

        // private void LogBefore(MethodInfo methodInfo, object[] args)
        // {
        //     var beforeMessage = new StringBuilder();
        //     beforeMessage.AppendLine($"Class {_decorated.GetType().FullName}");
        //     beforeMessage.AppendLine($"Method {methodInfo.Name} executing");
        //     var parameters = methodInfo.GetParameters();
        //     if (parameters.Any())
        //     {
        //         beforeMessage.AppendLine("Parameters:");
        //         for (var i = 0; i < parameters.Length; i++)
        //         {
        //             var parameter = parameters[i];
        //             var arg = args[i];
        //             beforeMessage.AppendLine($"{parameter.Name}:{GetStringValue(arg)}");
        //         }
        //     }
        //
        //     _logInfo?.Invoke(beforeMessage.ToString());
        // }
    }
}