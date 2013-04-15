using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SierraLib.Analytics.Implementation
{
    public static class TrackingModuleHelpers
    {
        public static void AddParameterExclusiveOrThrow(this IRestRequest request, string name, object value)
        {
            if (!request.Parameters.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                request.AddParameter(name, value);
            else
                throw new InvalidOperationException(string.Format("Cannot add more than one instance of the {0} parameter.", name));

        }

        public static void AddParameterExclusive(this IRestRequest request, string name, object value)
        {
            if (!request.Parameters.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                request.AddParameter(name, value);
        }

        public static string Truncate(this string value, int maxLength)
        {
            value = Regex.Replace(value, @"(\s)\s+", "$1").Trim();
            if (value.Length > maxLength)
                return value.Substring(0, maxLength);
            return value;
        }

        public static void RequiresParameter(ITrackingModule module, IRestRequest request, string name)
        {
            if (!request.Parameters.Any(x => x.Name == name))
                throw new InvalidOperationException(string.Format("Cannot use {0} without the {1} parameter set.", module.GetType().Name, name));
        }

        public static void RequiresParameter(ITrackingModule module, IRestRequest request, string name, object value)
        {
            if (!request.Parameters.Any(x => x.Name == name && x.Value == value))
                throw new InvalidOperationException(string.Format("Cannot use {0} without the {1} parameter set to {2}", module.GetType().Name, name, value));
        }
    }
}
