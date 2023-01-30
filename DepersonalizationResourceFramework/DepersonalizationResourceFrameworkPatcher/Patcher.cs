using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DepersonalizationResourceFrameworkPatcher
{
    public static class Patcher
    {
        // List of assemblies to patch
        public static IEnumerable<string> TargetDLLs => GetDLLs();

        // Patches the assemblies
        public static void Patch(ref AssemblyDefinition assembly)
        {
            switch (assembly.Name.Name)
            {
                case "Assembly-CSharp":
                    {
                        foreach(TypeDefinition type in assembly.MainModule.Types)
                        {
                            if (type.Name == "RoleModel")
                            {
                                MethodDefinition method = new MethodDefinition("Awake", MethodAttributes.Private, assembly.MainModule.ImportReference(typeof(void)));
                                method.Body=new MethodBody(method);
                                method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                                type.Methods.Add(method);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public static IEnumerable<string> GetDLLs()
        {
            yield return "Assembly-CSharp.dll";
        }
    }
}
