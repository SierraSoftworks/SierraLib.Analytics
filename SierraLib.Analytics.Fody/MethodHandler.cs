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

        }

        public MethodDefinition OnEntry { get; private set; }
        public MethodDefinition OnExit { get; private set; }
        public MethodDefinition OnException { get; private set; }
    }
}
