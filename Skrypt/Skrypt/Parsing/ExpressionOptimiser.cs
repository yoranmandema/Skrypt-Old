using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Engine;
using Skrypt.Tokenization;
using Skrypt.Library;

namespace Skrypt.Parsing {
    internal class ExpressionOptimiser {
        public static Node OptimiseExpressionNode (Node node, SkryptEngine engine) {
            if (!node.Nodes.Any()) {
                return node;
            }

            var isBinaryExpression = node.Nodes.Count == 2;
            var onlyHasLiterals = true;
            var token = node.Token;
            var newNode = node.Copy();

            for (int i = 0; i < node.Nodes.Count; i++) {
                newNode.Nodes[i] = OptimiseExpressionNode(node.Nodes[i], engine);
            }

            foreach (var n in newNode.Nodes) {
                if ((n.Type & TokenTypes.Literal) == 0) {
                    onlyHasLiterals = false;
                    break;
                }
            }

            if (onlyHasLiterals) {
                var retVal = engine.Executor.ExecuteExpression(newNode, engine.GlobalScope);

                if (retVal.GetType() == typeof(Skrypt.Library.Native.System.Numeric)) {
                    newNode = new NumericNode {
                        Value = (Skrypt.Library.Native.System.Numeric)retVal,
                        Token = node.Token
                    };
                } else if (retVal.GetType() == typeof(Skrypt.Library.Native.System.String)) {
                    newNode = new StringNode {
                        Value = (Skrypt.Library.Native.System.String)retVal,
                        Token = node.Token
                    };
                }
                else if (retVal.GetType() == typeof(Skrypt.Library.Native.System.Boolean)) {
                    newNode = new BooleanNode {
                        Value = (Skrypt.Library.Native.System.Boolean)retVal,
                        Token = node.Token
                    };
                }
                //else if (retVal.GetType() == typeof(Skrypt.Library.Native.System.Array)) {
                //    newNode = new ArrayNode {
                //        Value = (Skrypt.Library.Native.System.Array)retVal,
                //        Token = node.Token
                //    };
                //}
            }

            return newNode;
        }
    }
}
