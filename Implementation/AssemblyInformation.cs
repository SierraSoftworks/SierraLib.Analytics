using System;
using System.Reflection;

namespace SierraLib
{
    public static class AssemblyInformation
    {
        public static Version GetAssemblyFileVersion(Assembly assembly)
        {
            if (assembly == null)
                return new Version();

            AssemblyFileVersionAttribute attribute = GetCustomAttribute<AssemblyFileVersionAttribute>(assembly);
            if (attribute == null)
                return new Version();
            else
                return new Version(attribute.Version);
        }

        public static Version GetAssemblyVersion()
        {
            return GetAssemblyVersion(Assembly.GetCallingAssembly());
        }

        public static Version GetAssemblyVersion(Assembly assembly)
        {
            if (assembly == null)
                return new Version();
            else
                return assembly.GetName().Version;
        }

        public static string GetAssemblyCopyright(Assembly assembly)
        {
            if (assembly == null)
                return string.Empty;

            AssemblyCopyrightAttribute attribute = GetCustomAttribute<AssemblyCopyrightAttribute>(assembly);
            if (attribute == null)
                return string.Empty;
            else
                return attribute.Copyright;
        }

        public static string GetAssemblyDescription(Assembly assembly)
        {
            if (assembly == null)
                return string.Empty;

            AssemblyDescriptionAttribute attribute = GetCustomAttribute<AssemblyDescriptionAttribute>(assembly);
            if (attribute == null)
                return string.Empty;
            else
                return attribute.Description;
        }

        public static string GetAssemblyTitle()
        {
            return GetAssemblyTitle(Assembly.GetCallingAssembly());
        }

        public static string GetAssemblyTitle(Assembly assembly)
        {
            if (assembly == null)
                return string.Empty;

            AssemblyTitleAttribute attribute = GetCustomAttribute<AssemblyTitleAttribute>(assembly);
            if (attribute == null)
                return string.Empty;
            else
                return attribute.Title;
        }

        public static string GetAssemblyCompany(Assembly assembly)
        {
            if (assembly == null)
                return string.Empty;

            AssemblyCompanyAttribute attribute = GetCustomAttribute<AssemblyCompanyAttribute>(assembly);
            if (attribute == null)
                return string.Empty;
            else
                return attribute.Company;
        }

        internal static T GetCustomAttribute<T>(Assembly assembly) where T : Attribute
        {
            if (assembly == null)
                return null;

            T[] customAttributes = (assembly.GetCustomAttributes(typeof(T), false)) as T[];
            if ((customAttributes == null) || (customAttributes.Length == 0))
                return null;
            else
                return customAttributes[0];
        }

        internal static T[] GetCustomAttributes<T>(MemberInfo memberInfo) where T : Attribute
        {
            if (memberInfo == null)
                return null;
            else
                return (memberInfo.GetCustomAttributes(typeof(T), true)) as T[];
        }
    }
}