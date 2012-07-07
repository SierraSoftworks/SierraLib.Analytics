using System;
using System.Collections.Generic;
using System.Text;

namespace SierraLib.Analytics.GoogleAnalytics
{
    public class CustomVariable
    {
        public enum Scopes : int
        {
            Visitor = 1,
            Session = 2, 
            Page = 3
        }

        const int MaxVariableLength = 64;


        public Scopes Scope
        {
            get;
            private set;
        }

        public string Name
        { get; private set; }

        public string Value
        { get; private set; }

        public int Index
        { get; private set; }


        public CustomVariable(int index, string name, string value, Scopes scope = Scopes.Page)
        {
            //Test values first
            if (index < 1 || index > 5)
                throw new ArgumentOutOfRangeException("index", "Index should be between 1 and 5, inclusive");

            if (name == null || name.Trim().Length == 0)
                throw new ArgumentNullException("name");

            if (value == null || value.Trim().Length == 0)
                throw new ArgumentNullException("value");

#if THROW_ALL || THROW_LENGTH
            if (name.Replace("+","%20").Length > MaxVariableLength)
                throw new ArgumentNullException("The value for Name must be less than " + MaxVariableLength + " characters long", "name");

            if (value.Replace("+", "%20").Length > MaxVariableLength)
                throw new ArgumentNullException("The value for Value must be less than " + MaxVariableLength + " characters long", "value");
            Name = name;
            Value = value;
#else
            if (name.Replace("+", "%20").Length > MaxVariableLength)
                Name = name.Replace("+", "%20").Substring(0, MaxVariableLength);
            else
                Name = name;

            if (value.Replace("+", "%20").Length > MaxVariableLength)
                Value = value.Replace("+", "%20").Substring(0, MaxVariableLength);
            else
                Value = value;
#endif



            Scope = scope;
            Index = index;
        }
    }
}
