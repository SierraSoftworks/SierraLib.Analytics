using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SierraLib.Analytics.Fody
{
    public class MethodHandler
    {
        public MethodHandler(CustomAttribute attribute)
        {
            var methods = attribute.AttributeType.Resolve().Methods;

            OnEntry = methods.Where(method => method.Name == nameof(OnEntry)).First();
            OnExit = methods.Where(method => method.Name == nameof(OnExit)).First();
            OnException = methods.Where(method => method.Name == nameof(OnException)).First();
            this.Attribute = attribute;
        }

        public MethodDefinition OnEntry { get; }

        public MethodDefinition OnExit { get; }

        public MethodDefinition OnException { get; }
        public CustomAttribute Attribute { get; }
    }
}
