using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SierraLib.Analytics.Implementation
{
    /// <summary>
    /// A handler which may be invoked whenever a method which is decorated
    /// by the a trigger for the <see cref="MethodInvokeAspect"/> is invoked.
    /// </summary>
    public interface IMethodInvokeTrigger
    {
        void OnEntry(MethodBase method, object[] parameters);

        void OnExit(MethodBase method, object[] parameters, object result);

        void OnException(MethodBase method, object[] parameters, Exception exception);
    }

    [Aspect(Scope.Global)]
    public class MethodInvokeAspect
    {
        [Advice(Kind.Around, Targets = Target.Method)]
        public object HandleMethod([Argument(Source.Metadata)]MethodBase method, [Argument(Source.Arguments)] object[] parameters, [Argument(Source.Target)] Func<object[], object> target, [Argument(Source.Triggers)] Attribute[] triggers)
        {
            if (method.GetCustomAttributes<DontTrackAttribute>(true).Any())
            {
                return target(parameters);
            }

            var invokeHandlers = triggers.OfType<IMethodInvokeTrigger>();

            try
            {
                foreach (var handler in invokeHandlers)
                {
                    handler.OnEntry(method, parameters);
                }

                var result = target(parameters);

                foreach (var handler in invokeHandlers)
                {
                    handler.OnExit(method, parameters, result);
                }

                return result;
            }
            catch (Exception ex)
            {
                foreach (var handler in invokeHandlers)
                {
                    handler.OnException(method, parameters, ex);
                }

                throw;
            }
        }
    }
}
