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
                case nameof(ImportNode):
                    CompileUsing(node, ref instructions);
                    break;
                case nameof(CallNode):
                    CompileCall(node, ref instructions);
                    break;
                case nameof(OperationNode):
                    CompileOperation(node, ref instructions);
                    break;
                case nameof(NumericNode):
                    CompileNumeric(node, ref instructions);
                    break;
                case nameof(StringNode):
                    CompileString(node, ref instructions);
                    break;
                case nameof(IdentifierNode):
                    CompileIdentifier(node, ref instructions);
                    break;
            }
        }

        public static void CompileOperation(Node node, ref List<Instruction> instructions) {
            if (node.Body == "assign") {
                CompileAssignment(node, ref instructions);

                return;
            } else if (node.Body == "access") {
                CompileAccess(node, ref instructions);

                return;
            }

            for (int i = node.Nodes.Count; i --> 0;) {
                CompileBranch(node.Nodes[i], ref instructions);
            }

            var operationCode = (OperationCode)Enum.Parse(typeof(OperationCode), node.Body);

            instructions.Add(new Instruction {
                OpCode = operationCode
            });
        }

        public static void CompileAssignment (Node node, ref List<Instruction> instructions) {
            CompileBranch(node.Nodes[1], ref instructions);

            instructions.Add(new Instruction {
                OpCode = OperationCode.stloc,
                Value = CurrentScope.GetIndexFromIdentifier(node.Nodes[0].Body)
            });
        }

        public static void CompileAccess(Node node, ref List<Instruction> instructions) {
            CompileBranch(node.Nodes[0], ref instructions);

            instructions.Add(new Instruction {
                OpCode = OperationCode.access,
                Value = node.Nodes[1].Body
            });
        }

        public static void CompileNumeric(Node node, ref List<Instruction> instructions) {
            instructions.Add(new Instruction {
                OpCode = OperationCode.ldnum,
                Value = ((NumericNode)node).Value
            });
        }

        public static void CompileString(Node node, ref List<Instruction> instructions) {
            instructions.Add(new Instruction {
                OpCode = OperationCode.ldstr,
                Value = ((StringNode)node).Value
            });
        }

        public static void CompileIdentifier(Node node, ref List<Instruction> instructions) {
            instructions.Add(new Instruction {
                OpCode = OperationCode.ldloc,
                Value = CurrentScope.GetIndexFromIdentifier(((IdentifierNode)node).Body)
            });
        }

        public static void CompileUsing(Node node, ref List<Instruction> instructions) {
            CompileBranch(((ImportNode)node).Getter, ref instructions);

            instructions.Add(new Instruction {
                OpCode = OperationCode.import
            });
        }

        public static void CompileCall(Node node, ref List<Instruction> instructions) {
            CompileBranch(((CallNode)node).Getter, ref instructions);

            instructions.Add(new Instruction {
                OpCode = OperationCode.fnloc
            });

            for (int i = ((CallNode)node).Arguments.Count; i --> 0;) {
                CompileBranch(((CallNode)node).Arguments[i], ref instructions);
            }

            instructions.Add(new Instruction {
                OpCode = OperationCode.call
            });
        }
    }
}
