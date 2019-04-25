using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SierraLib.Analytics.Fody
{
    public class AttributeFinder<TAttribute>
        where TAttribute : Attribute
    {
        public AttributeFinder(MethodDefinition method)
        {
            var customAttributes = method.CustomAttributes;
            Attributes = customAttributes
                .Where(a => InheritsType(a.AttributeType.Resolve(), typeof(TAttribute)));
        }

        public IEnumerable<CustomAttribute> Attributes { get; private set; }

        private bool InheritsType(TypeDefinition type, Type baseType)
        {
            if (type == null) return false;

            if (type.FullName == baseType.FullName) return true;
            
            if (type.BaseType == null) return false;

            return InheritsType(type.BaseType.Resolve(), baseType);
        }
    }
}
