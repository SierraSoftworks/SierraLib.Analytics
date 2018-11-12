using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SierraLib.Analytics.Fody
{
    public class ModuleWeaver
    {
        public ModuleWeaver()
        {
            LogInfo = m => { };
        }

        public Action<string> LogInfo { get; set; }
        public ModuleDefinition ModuleDefinition { get; set; }

        public void Execute()
        {
            foreach (var type in ModuleDefinition.Types.Where(t => t.HasMethods))
            {
                foreach (var method in type.Methods)
                {
                    var attributes = new AttributeFinder(method);
                    foreach (var handler in attributes.Handlers)
                    {
                        // TODO: Rewrite instructions for OnEntry, OnExit, OnException
                    }
                }
            }
        }
    }
}
