using Mono.Cecil;
using Mono.Cecil.Cil;
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
                    var attributes = new AttributeFinder<Implementation.MethodWrapperAttribute>(method);
                    if (new AttributeFinder<Implementation.MethodDontWrapAttribute>(method).Attributes.Any())
                        continue;

                    var processor = method.Body.GetILProcessor();

                    var onEntryPlaceholder = Instruction.Create(OpCodes.Nop);
                    var onExitPlaceholder = Instruction.Create(OpCodes.Nop);
                    var catchPlaceholder = Instruction.Create(OpCodes.Nop);

                    foreach (var handler in attributes.Attributes.Select(a => new MethodHandler(a)))
                    {
                        // TODO: Rewrite instructions for OnEntry, OnExit, OnException

                        processor.InsertBefore(method.Body.Instructions.First(), onEntryPlaceholder);
                        processor.InsertAfter(method.Body.Instructions.Last(), onExitPlaceholder);


                    }

                    
                }
            }
        }

        private void InjectOnEntry(MethodDefinition method, ILProcessor processor, MethodHandler handler, Instruction placeholder)
        {
            var current = placeholder;

            foreach (var instruction in GenerateOnEntryIL(method, handler))
            {
                processor.InsertAfter(current, instruction);
                current = instruction;
            }
        }

        private IEnumerable<Instruction> GenerateOnEntryIL(MethodDefinition method, MethodHandler handler)
        {
            method.GetElementMethod()
        }
    }
}
