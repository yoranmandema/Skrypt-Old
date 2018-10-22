using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using Skrypt.Parsing;
using Skrypt.Tokenization;

namespace Skrypt.ILGen {
    internal static class BasicGenerator {
        private delegate TReturn Method<TReturn> ();

        struct Operation {
            public OpCode Op;
            public string Value;
        }

        static List<Operation> Code = new List<Operation>();
        static List<string> Variables = new List<string>();

        public static Delegate GenerateMethodFromNode (Node programNode) {
            AppDomain domain = AppDomain.CurrentDomain;
            AssemblyName asmName = new AssemblyName("SkryptProgram");
            AssemblyBuilder assembly = domain.DefineDynamicAssembly(asmName,  AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder module = assembly.DefineDynamicModule(asmName.Name, asmName.Name + ".dll");

            DynamicMethod method = new DynamicMethod("Main", typeof(long), new Type[] {}, module);

            var il = method.GetILGenerator();

            Emit(il, OpCodes.Nop);

            EmitBlockNode(il, programNode);

            Emit(il, OpCodes.Ret);

            PrintCode();

            Method<long> del = (Method<long>)method.CreateDelegate(typeof(Method<long>));

            Console.WriteLine(del());

            return del;
        }

        static void PrintCode () {
            int count = 0;

            foreach (var op in Code) {
                var str = "IL_";
                str += count.ToString("X4");
                str += ": ";
                str += op.Op.ToString();
                str += " ";
                str += op.Value;

                Console.WriteLine(str);

                count++;
            }
        }
        
        static void EmitLdlocOp (ILGenerator il, string name) {
            var index = Variables.IndexOf(name);
            var opCode = default(OpCode);

            switch (index) {
                case 0:
                    opCode = OpCodes.Ldloc_0;
                    break;
                case 1:
                    opCode = OpCodes.Ldloc_1;
                    break;
                case 2:
                    opCode = OpCodes.Ldloc_2;
                    break;
                case 3:
                    opCode = OpCodes.Ldloc_3;
                    break;
                default:
                    opCode = OpCodes.Ldloc_S;
                    break;
            }

            if (opCode == OpCodes.Ldloc_S) {
                il.Emit(opCode, index);
                Code.Add(new Operation { Op = opCode, Value = index.ToString() });
            } else {
                il.Emit(opCode, index);
                Code.Add(new Operation { Op = opCode });
            }
        }

        static void EmitStlocOp(ILGenerator il, string name) {
            var index = 0;
            var opCode = default(OpCode);

            if (Variables.Exists(n => n == name)) {
                index = Variables.IndexOf(name);
            } else {
                index = Variables.Count;
                Variables.Add(name);
            }

            switch (index) {
                case 0:
                    opCode = OpCodes.Stloc_0;
                    break;
                case 1:
                    opCode = OpCodes.Stloc_1;
                    break;
                case 2:
                    opCode = OpCodes.Stloc_2;
                    break;
                case 3:
                    opCode = OpCodes.Stloc_3;
                    break;
                default:
                    opCode = OpCodes.Stloc_S;
                    break;
            }

            if (opCode == OpCodes.Stloc_S) {
                il.Emit(opCode, index);
                Code.Add(new Operation { Op = opCode, Value = index.ToString() });
            }
            else {
                il.Emit(opCode, index);
                Code.Add(new Operation { Op = opCode });
            }
        }

        static void Emit (ILGenerator il, OpCode op) {
            il.Emit(op);
            Code.Add(new Operation {Op = op});
        }

        static void EmitNumber (ILGenerator il, OpCode op, double n) {
            il.Emit(op, n);
            Code.Add(new Operation { Op = op, Value = n.ToString()});
        }

        static void EmitString (ILGenerator il, OpCode op, string s) {
            il.Emit(op, s);
            Code.Add(new Operation { Op = op, Value = s});
        }

        static void EmitBlockNode (ILGenerator il, Node node) {
            foreach (var n in node.Nodes) {
                EmitNode(il,n);
            }
        }

        static void EmitNode (ILGenerator il, Node node) {
            switch (node.Type) {
                case TokenTypes.NumericLiteral:
                    EmitNumericOpCode(il, (NumericNode)node);
                    break;
                case TokenTypes.StringLiteral:
                    EmitStringOpCode(il, (StringNode)node);
                    break;
                case TokenTypes.BooleanLiteral:
                    EmitBoolOpCode(il, (BooleanNode)node);
                    break;
                case TokenTypes.Punctuator:
                    EmitOperation(il, node);
                    break;
                case TokenTypes.Identifier:
                    EmitLdlocOp(il, node.Body);
                    break;
            }
        }

        static void EmitOperation (ILGenerator il, Node node) {
            if (node.Body == "assign") {
                EmitNode(il, node.Nodes[1]);

                if (node.Nodes[0].Type == TokenTypes.Identifier) {
                    EmitStlocOp(il, node.Nodes[0].Body);
                }

                return;
            }

            for (int i = node.Nodes.Count; i-- > 0;) {
                EmitNode(il, node.Nodes[i]);
            }

            switch (node.Body) {
                case "add":
                    Emit(il, OpCodes.Add);
                    break;
                case "subtract":
                    Emit(il, OpCodes.Sub);
                    break;
                case "multiply":
                    Emit(il, OpCodes.Mul);
                    break;
                case "divide":
                    Emit(il, OpCodes.Div);
                    break;
                case "modulo":
                    Emit(il, OpCodes.Rem);
                    break;
            }
        }

        static void EmitNumericOpCode (ILGenerator il, NumericNode node) {
            EmitNumber(il, OpCodes.Ldc_R8, node.Value);
        }

        static void EmitStringOpCode(ILGenerator il, StringNode node) {
            EmitString(il, OpCodes.Ldstr, node.Value);
        }

        static void EmitBoolOpCode(ILGenerator il, BooleanNode node) {
            if (node.Value == true) {
                Emit(il, OpCodes.Ldc_I4_1);
            } else {
                Emit(il, OpCodes.Ldc_I4_0);
            }
        }
    }
}
