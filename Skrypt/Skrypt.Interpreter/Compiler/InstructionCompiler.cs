using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Parsing;

namespace Skrypt.Interpreter.Compiler {
    class PrecompiledAdress {
        public int Address;
        public int Delta;
    }

    class InstructionCompiler {
        public static CompileScope CurrentScope { get; set; }
        public static int ScopeDepth { get; set; }

        public static List<Instruction> Compile(Node node) {
            CurrentScope = new CompileScope();
            var instructions = new List<Instruction>();

            CompileGeneral(node, ref instructions);

            for (int i = 0; i < instructions.Count; i++) {
                if (instructions[i].Value.GetType() == typeof(PrecompiledAdress)) {
                    instructions[i] = new Instruction {
                        OpCode = instructions[i].OpCode,
                        Value = i + ((PrecompiledAdress)instructions[i].Value).Delta + 1
                    };
                }
            }

            return instructions;
        }

        public static void CompileGeneral(Node node, ref List<Instruction> instructions) {
            var type = node.GetType().Name;

            switch (type) {
                case nameof(BlockNode):
                    foreach (var n in node.Nodes) {
                        CompileGeneral(n, ref instructions);
                    }
                    break;

                case nameof(ImportNode):
                    CompileUsing(node, ref instructions);
                    break;

                case nameof(CallNode):
                    CompileCall(node, ref instructions);
                    break;

                case nameof(IfNode):
                    CompileIf((IfNode)node, ref instructions);
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

                case nameof(BooleanNode):
                    CompileBoolean(node, ref instructions);
                    break;

                case nameof(NullNode):
                    CompileNull(node, ref instructions);
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
                CompileGeneral(node.Nodes[i], ref instructions);
            }

            var operationCode = (OperationCode)Enum.Parse(typeof(OperationCode), node.Body);

            instructions.Add(new Instruction {
                OpCode = operationCode
            });
        }

        public static void CompileAssignment (Node node, ref List<Instruction> instructions) {
            CompileGeneral(node.Nodes[1], ref instructions);

            instructions.Add(new Instruction {
                OpCode = OperationCode.stloc,
                Value = CurrentScope.GetIndexFromIdentifier(node.Nodes[0].Body)
            });
        }

        public static void CompileAccess(Node node, ref List<Instruction> instructions) {
            CompileGeneral(node.Nodes[0], ref instructions);

            instructions.Add(new Instruction {
                OpCode = OperationCode.access,
                Value = node.Nodes[1].Body
            });
        }

        public static void CompileIf(IfNode node, ref List<Instruction> instructions) {
            var moveAddresses = new List<PrecompiledAdress>();
             
            var ifInstructions = CompileBranch(node);
            var ifMoveAdress = (PrecompiledAdress)ifInstructions.Last().Value;

            instructions = instructions.Concat(ifInstructions).ToList();

            ifMoveAdress.Address = instructions.Count;
            moveAddresses.Add(ifMoveAdress);

            foreach (var elseifNode in node.ElseIfNodes) {
                var elseifInstructions = CompileBranch(elseifNode);
                var elseifMoveAdress = (PrecompiledAdress)elseifInstructions.Last().Value;

                instructions = instructions.Concat(elseifInstructions).ToList();

                elseifMoveAdress.Address = instructions.Count;
                moveAddresses.Add(elseifMoveAdress);
            }

            if (node.ElseNode != null) CompileGeneral(node.ElseNode, ref instructions);

            foreach (var adress in moveAddresses) {
                adress.Delta = instructions.Count - adress.Address;
            }
        }

        public static List<Instruction> CompileBranch (IBranchNode node) {
            var instructions = new List<Instruction>();
            var branchAddress = new PrecompiledAdress();
            var innerInstructions = new List<Instruction>();

            CompileGeneral(node.Block, ref innerInstructions);
            innerInstructions.Add(new Instruction {
                OpCode = OperationCode.move,
                Value = new PrecompiledAdress()
            });

            branchAddress.Delta = innerInstructions.Count;

            CompileGeneral(node.Condition, ref instructions);
            instructions.Add(new Instruction {
                OpCode = OperationCode.brfalse,
                Value = branchAddress
            });

            instructions = instructions.Concat(innerInstructions).ToList();

            return instructions;
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

        public static void CompileBoolean(Node node, ref List<Instruction> instructions) {
            instructions.Add(new Instruction {
                OpCode = OperationCode.ldbool,
                Value = ((BooleanNode)node).Value
            });
        }

        public static void CompileNull(Node node, ref List<Instruction> instructions) {
            instructions.Add(new Instruction {
                OpCode = OperationCode.ldnull,
                Value = null
            });
        }

        public static void CompileIdentifier(Node node, ref List<Instruction> instructions) {
            instructions.Add(new Instruction {
                OpCode = OperationCode.ldloc,
                Value = CurrentScope.GetIndexFromIdentifier(((IdentifierNode)node).Body)
            });
        }

        public static void CompileUsing(Node node, ref List<Instruction> instructions) {
            CompileGeneral(((ImportNode)node).Getter, ref instructions);

            instructions.Add(new Instruction {
                OpCode = OperationCode.import
            });
        }

        public static void CompileCall(Node node, ref List<Instruction> instructions) {
            for (int i = ((CallNode)node).Arguments.Count; i --> 0;) {
                CompileGeneral(((CallNode)node).Arguments[i], ref instructions);
            }

            CompileGeneral(((CallNode)node).Getter, ref instructions);

            instructions.Add(new Instruction {
                OpCode = OperationCode.call
            });
        }
    }
}
