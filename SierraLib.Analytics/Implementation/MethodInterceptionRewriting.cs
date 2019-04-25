using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SierraLib.Analytics.Implementation
{
    public interface IMethodDecorator
    {
        void OnEntry(MethodBase method, object[] parameters);
        void OnExit(MethodBase method, object[] parameters, object result);
        void OnException(MethodBase method, Exception exception, object[] parameters);
    }
    
    //public class MethodWrapperAmender<T> : Amendment<T, T>
    //{
    //    public MethodWrapperAmender()
    //    {
    //        Methods.Where(ShouldAmend)
    //               .Before(MethodWrapperAmender<T>.OnBegin)
    //               .After(MethodWrapperAmender<T>.OnAfter)
    //               .Catch<Exception>((MethodEnumeration.CatchMethodAction<Exception>)MethodWrapperAmender<T>.OnException);
    //    }
        
    //    private bool ShouldAmend(Afterthought.Amendment.Method method)
    //    {
    //        return method.MethodInfo.GetCustomAttributes(typeof(MethodWrapperAttribute), true).Any()
    //            && !method.MethodInfo.GetCustomAttributes(typeof(MethodDontWrapAttribute), true).Any();
    //    }


    //    public static void OnBegin(T instance, string name, object[] parameters)
    //    {
    //        var methodInfo = instance.GetType().GetMethod(name, parameters.Select(x => x.GetType()).ToArray());
    //        var handlers = methodInfo.GetCustomAttributes<MethodWrapperAttribute>(true);
    //        foreach (var handler in handlers)
    //            handler.OnEntry(methodInfo, parameters);
    //    }

    //    public static object OnAfter(T instance, string name, object[] parameters, object result)
    //    {
    //        var methodInfo = instance.GetType().GetMethod(name, parameters.Select(x => x.GetType()).ToArray());
    //        var handlers = methodInfo.GetCustomAttributes<MethodWrapperAttribute>(true);
    //        foreach (var handler in handlers)
    //            handler.OnExit(methodInfo, parameters, result);
    //        return result;
    //    }

    //    public static void OnException(T instance, string name, Exception exception, object[] parameters)
    //    {
    //        var methodInfo = instance.GetType().GetMethod(name, parameters.Select(x => x.GetType()).ToArray());
    //        var handlers = methodInfo.GetCustomAttributes<MethodWrapperAttribute>(true);
    //        foreach (var handler in handlers)
    //            handler.OnException(methodInfo, exception, parameters);
    //    }
    //}

    public class MethodDontWrapAttribute : Attribute
    {
    
    }

    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    public abstract class MethodWrapperAttribute : Attribute, IMethodDecorator
    {
        public virtual void OnEntry(MethodBase method, object[] parameters)
        {
            //Don't do anything by default
        }

        public virtual void OnExit(MethodBase method, object[] parameters, object result)
        {
            //Don't do anything by default
        }

        public virtual void OnException(MethodBase method, Exception exception, object[] parameters)
        {
            //Don't do anything by default
        }
    }
}
