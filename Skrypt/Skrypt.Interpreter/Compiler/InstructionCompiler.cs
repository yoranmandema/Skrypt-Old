using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Parsing;

namespace Skrypt.Interpreter.Compiler {
    class InstructionCompiler {
        public static CompileScope CurrentScope { get; set; }

        public static List<Instruction> Compile(Node node) {
            CurrentScope = new CompileScope();
            var instructions = new List<Instruction>();

            CompileBranch(node, ref instructions);

            return instructions;
        }

        public static void CompileBranch(Node node, ref List<Instruction> instructions) {
            var type = node.GetType().Name;

            switch (type) {
                case nameof(BlockNode):
                    foreach (var n in node.Nodes) {
                        CompileBranch(n, ref instructions);
                    }
                    break;
                case nameof(OperationNode):
                    CompileOperation(node, ref instructions);
                    break;
                case nameof(NumericNode):
                    CompileNumeric(node, ref instructions);
                    break;
                case nameof(IdentifierNode):
                    CompileIdentifier(node, ref instructions);
                    break;
            }
        }

        public static void CompileOperation(Node node, ref List<Instruction> instructions) {
            var operationCode = (OperationCode)Enum.Parse(typeof(OperationCode), node.Body);

            instructions.Add(new Instruction {
                OpCode = operationCode
            });

            foreach (var n in node.Nodes) {
                CompileBranch(n, ref instructions);
            }
        }

        public static void CompileNumeric(Node node, ref List<Instruction> instructions) {
            instructions.Add(new Instruction {
                OpCode = OperationCode.number,
                Value = ((NumericNode)node).Value
            });
        }

        public static void CompileIdentifier(Node node, ref List<Instruction> instructions) {
            instructions.Add(new Instruction {
                OpCode = OperationCode.stloc,
                Value = CurrentScope.GetStlocFromIdentifier(((IdentifierNode)node).Body)
            });
        }
    }
}
