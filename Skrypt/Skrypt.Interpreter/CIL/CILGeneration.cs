using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Diagnostics.SymbolStore;
using System.Diagnostics;
using Skrypt.Parsing;

namespace Skrypt.Interpreter.CIL {
    public class CILGenerator {
        private static int sequencePoint = 2;
        private static ILGenerator ILGenerator;
        private static ISymbolDocumentWriter document;

        public static MethodInfo CompileToMethod (Node program, string name = "SkryptAssembly") {
            // create a dynamic assembly and module

            var assemblyName = new AssemblyName {
                Name = name
            };

            var assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);

            // Mark generated code as debuggable.

            // See http://blogs.msdn.com/rmbyers/archive/2005/06/26/432922.aspx for explanation.

            var daType = typeof(DebuggableAttribute);

            ConstructorInfo daCtor = daType.GetConstructor(new Type[] { typeof(DebuggableAttribute.DebuggingModes) });

            var daBuilder = new CustomAttributeBuilder(daCtor, new object[] {
                DebuggableAttribute.DebuggingModes.DisableOptimizations |
                DebuggableAttribute.DebuggingModes.Default
            });

            assemblyBuilder.SetCustomAttribute(daBuilder);

            ModuleBuilder module = assemblyBuilder.DefineDynamicModule($"{name}.exe", true); // <-- pass 'true' to track debug info.

            // Tell Emit about the source file that we want to associate this with.

            document = module.DefineDocument(@"Source.txt", Guid.Empty, Guid.Empty, Guid.Empty);

            // create a new type to hold our Main method

            TypeBuilder typeBuilder = module.DefineType("Program", TypeAttributes.Public | TypeAttributes.Class);

            // create the Main(string[] args) method

            MethodBuilder methodbuilder = typeBuilder.DefineMethod("Main", MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, typeof(void), new Type[] { typeof(string[]) });

            // generate the IL for the Main method

            ILGenerator = methodbuilder.GetILGenerator();

            // Create a local variable of type 'string', and call it 'xyz'

            LocalBuilder localXYZ = ILGenerator.DeclareLocal(typeof(string));

            localXYZ.SetLocalSymInfo("xyz"); // Provide name for the debugger. 

            // Emit sequence point before the IL instructions. This is start line, start col, end line, end column, 

            // Line 2: xyz = "hello";

            MarkSequencePoint();

            ILGenerator.Emit(OpCodes.Ldstr, "Hello world!");

            ILGenerator.Emit(OpCodes.Stloc, localXYZ);

            // Line 3: Write(xyz);

            MethodInfo infoWriteLine = typeof(System.Console).GetMethod("WriteLine", new Type[] { typeof(string) });

            MarkSequencePoint();

            ILGenerator.Emit(OpCodes.Ldloc, localXYZ);

            ILGenerator.EmitCall(OpCodes.Call, infoWriteLine, null);

            // Line 4: return;

            MarkSequencePoint();

            ILGenerator.Emit(OpCodes.Ret);

            Type type = typeBuilder.CreateType();

            // This now calls the newly generated method. We can step into this and debug our emitted code!!

            return type.GetMethod("Main");
        }

        static void MarkSequencePoint () {
            ILGenerator.MarkSequencePoint(document, sequencePoint, 1, sequencePoint, 100);
            sequencePoint++;
        }
    }
}
