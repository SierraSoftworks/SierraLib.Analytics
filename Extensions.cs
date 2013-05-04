using SierraLib.Analytics.Implementation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace SierraLib.Analytics
{
    static class Extensions
    {
        public static bool IsNullOrEmpty(this string s)
        {
            return s == null || s.Length == 0;
        }

        public static bool IsNullOrWhitespace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> collection, T element)
        {
            using (var e = collection.GetEnumerator())
                while (e.MoveNext()) yield return e.Current;
            yield return element;
        }

        public static T GetCustomAttribute<T>(this MethodBase method, bool inherit = false)
        {
            return (T)method.GetCustomAttributes(typeof(T), inherit)[0];
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this MethodBase method, bool inherit = false)
        {
            return method.GetCustomAttributes(typeof(T), inherit).Cast<T>();
        }

        public static TrackingEngine GetTrackingEngine(this MethodBase method)
        {
            var a = method.GetCustomAttribute<TrackingEngineAttributeBase>(true);
            if (a == null)
                throw new InvalidOperationException("TrackingEngine not set for this method or any of its ancestors");
            return a.Engine;
        }

        public static T GetCustomAttribute<T>(this MemberInfo method, bool inherit = false)
        {
            return method.GetCustomAttributes<T>(inherit).FirstOrDefault();
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo method, bool inherit = false)
        {
            if (inherit)
                return method.GetCustomAttributes(true).Concat(method.DeclaringType.GetCustomAttributes(true)).OfType<T>();
            else
                return method.GetCustomAttributes(true).OfType<T>();
        }

        public static TrackingEngine GetTrackingEngine(this MemberInfo method)
        {
            var a = method.GetCustomAttribute<TrackingEngineAttributeBase>(true);
            if (a == null)
                throw new InvalidOperationException("TrackingEngine not set for this method or any of its ancestors");
            return a.Engine;
        }
        
        /// <summary>
        /// Converts an expression into a <see cref="MemberInfo"/>.
        /// </summary>
        /// <param name="expression">The expression to convert.</param>
        /// <returns>The member info.</returns>
        public static MemberInfo GetMemberInfo(this Expression expression)
        {
            var lambda = (LambdaExpression)expression;

            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else if(lambda.Body is MethodCallExpression)
            {
                return ((MethodCallExpression)lambda.Body).Method;
            }
            else
            {
                memberExpression = (MemberExpression)lambda.Body;
            }

            return memberExpression.Member;
        }

        public static byte[] Serialize(this PreparedTrackingRequest request)
        {
            var formatter = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                formatter.Serialize(ms, request);
                return ms.ToArray();
            }
        }

        public static PreparedTrackingRequest ToPreparedTrackingRequest(this byte[] serialized)
        {
            var formatter = new BinaryFormatter();
            using(var ms = new MemoryStream(serialized))            
                return (PreparedTrackingRequest)formatter.Deserialize(ms);            
        }

    }
}
