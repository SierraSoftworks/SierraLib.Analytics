using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SierraLib.Analytics.Fody
{
    public class AttributeFinder
    {
        public AttributeFinder(MethodDefinition method)
        {
            var customAttributes = method.CustomAttributes;
            Handlers = customAttributes
                .Where(a => InheritsType(a.AttributeType.Resolve(), typeof(SierraLib.Analytics.Implementation.MethodWrapperAttribute)))
                .Select(a => new MethodHandler(a));
        }

        public IEnumerable<MethodHandler> Handlers { get; private set; }

        private bool InheritsType(TypeDefinition type, Type baseType)
        {
            if (type == null) return false;

            if (type.FullName == baseType.FullName) return true;
            
            if (type.BaseType == null) return false;

            return InheritsType(type.BaseType.Resolve(), baseType);
        }
    }
}
