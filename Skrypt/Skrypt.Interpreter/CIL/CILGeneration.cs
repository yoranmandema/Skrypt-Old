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
using Sigil;

namespace Skrypt.Interpreter.CIL {
    public class CILGenerator {
        public MethodInfo OutputMethod;
        private Emit<Action> emit;

        public Action CompileToAction(Node program) {
            emit = Emit<Action>.NewDynamicMethod();

            try {
                CompileGeneral(program);

                emit.Return();
            }
            catch (SigilVerificationException e) {
                Console.WriteLine(e.GetDebugInfo());

                throw e;
            }

            var del = emit.CreateDelegate();

            return del;
        }

        public void CompileGeneral(Node node) {
            var type = node.GetType().Name;

            switch (type) {
                case nameof(BlockNode):
                    foreach (var n in node.Nodes) {
                        CompileGeneral(n);
                    }
                    break;
                case nameof(NumericNode):
                    CompileNumeric((NumericNode)node);
                    break;
                case nameof(StringNode):
                    CompileString((StringNode)node);
                    break;
                case nameof(BooleanNode):
                    CompileBool((BooleanNode)node);
                    break;
                case nameof(NullNode):
                    CompileNull();
                    break;
                case nameof(OperationNode):
                    CompileOperation((OperationNode)node);
                    break;
            }
        }

        public void CompileNumeric(NumericNode node) {
            emit.LoadConstant(node.Value);
            emit.Box(typeof(double));
        }

        public void CompileString(StringNode node) {
            emit.LoadConstant(node.Value);
            emit.Box(typeof(string));
        }

        public void CompileBool(BooleanNode node) {
            emit.LoadConstant(node.Value);
            emit.Box(typeof(bool));
        }

        public void CompileNull() {
            emit.LoadNull();
        }

        public void CompileOperation(OperationNode node) {
            if (node.Body == "assign") {
                CompileAssignment(node);

                return;
            }

            for (int i = node.Nodes.Count; i-- > 0;) {
                CompileGeneral(node.Nodes[i]);
            }

            switch (node.Body) {
                case "add":
                    emit.Add();
                    break;
                case "subtract":
                    emit.Subtract();
                    break;
                case "multiply":
                    emit.Multiply();
                    break;
                case "divide":
                    emit.Divide();
                    break;
                case "modulo":
                    emit.Remainder();
                    break;
            }
        }

        public void CompileAssignment(Node node) {
            CompileGeneral(node.Nodes[1]);

            var local = emit.DeclareLocal(typeof(object), node.Body);
            emit.StoreLocal(local);
            emit.LoadLocal(local);
            emit.Call(typeof(System.Console).GetMethod("WriteLine", new Type[] { typeof(object) }));
        }
    }
}
