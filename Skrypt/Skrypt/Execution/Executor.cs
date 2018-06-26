using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Library.SkryptClasses;
using System.Reflection;
using Skrypt.Library;
using Skrypt.Engine;
using Skrypt.Parsing;

namespace Skrypt.Execution {
    class Executor {
        SkryptEngine engine;

        public Executor(SkryptEngine e) {
            engine = e;
        }

        public SkryptObject ExecuteExpression (Node node, ScopeContext scope) {
            ScopeContext scopeContext = new ScopeContext(scope);

            Operator op = Operator.AllOperators.Find(o => o.OperationName == node.Body || o.Operation == node.Body);

            if (op != null) {
                int Members = node.SubNodes.Count;

                if (Members != op.Members) {
                    engine.throwError("Missing member of operation!", node.Token);
                }

                if (op.OperationName == "assign") {
                    if (node.SubNodes[0].TokenType != "Identifier") {
                        engine.throwError("Can't assign non-variable", node.SubNodes[0].Token);
                    }

                    SkryptObject r = scopeContext.Variables[node.SubNodes[0].Body] = ExecuteExpression(node.SubNodes[1], scopeContext);
                    return r;
                }

                if (op.Members == 2) {
                    SkryptObject Left = ExecuteExpression(node.SubNodes[0], scopeContext);
                    SkryptObject Right = ExecuteExpression(node.SubNodes[1], scopeContext);

                    if (Left != null && Right != null) {
                        Type t1 = Left.GetType();
                        Type t2 = Right.GetType();

                        MethodInfo methodInfo1 = null;
                        MethodInfo methodInfo2 = null;
                        var Methods1 = t1.GetMethodsBySig("_" + op.OperationName, t1, t2);
                        var Methods2 = t2.GetMethodsBySig("_" + op.OperationName, t1, t2);

                        if (Methods1.Count() > 0) methodInfo1 = Methods1.ElementAt(0);
                        if (Methods2.Count() > 0) methodInfo2 = Methods2.ElementAt(0);

                        if (methodInfo1 != null) {
                            try {
                                object result = methodInfo1.Invoke(null, new object[] { Left, Right });

                                return (SkryptObject)result;
                            }
                            catch (Exception e) { }
                        }
                        else if (methodInfo2 != null) {
                            try {
                                object result = methodInfo2.Invoke(null, new object[] { Left, Right });
                                return (SkryptObject)result;
                            }
                            catch (Exception e) { }
                        }

                        engine.throwError("No such operation as " + Left.Name + " " + op.Operation + " " + Right.Name, node.SubNodes[0].Token);
                    }
                }
                else if (op.Members == 1) {
                    SkryptObject Left = ExecuteExpression(node.SubNodes[0], scopeContext);

                    if (Left != null) {
                        Type t1 = Left.GetType();

                        MethodInfo methodInfo1 = null;
                        var Methods1 = t1.GetMethodsBySig("_" + op.OperationName, t1);

                        if (Methods1.Count() > 0) methodInfo1 = Methods1.ElementAt(0);

                        if (methodInfo1 != null) {
                            try {
                                object result = methodInfo1.Invoke(null, new object[] { Left });
                                return (SkryptObject)result;
                            }
                            catch (Exception e) { }
                        }

                        engine.throwError("No such operation as " + op.Operation + " " + Left.Name, node.SubNodes[0].Token);
                    }
                }
            }
            else if (node.SubNodes.Count == 0) {
                switch (node.TokenType) {
                    case "NumericLiteral":
                        return new Numeric { value = Double.Parse(node.Body) };
                    case "StringLiteral":
                        return new SkryptString { value = node.Body };
                    case "BooleanLiteral":
                        return new SkryptBoolean { value = node.Body == "true" ? true : false };
                }
            }
            else if (node.TokenType == "ArrayLiteral") {
                SkryptArray array = new SkryptArray();

                foreach (Node subNode in node.SubNodes) {
                    array.value.Add(ExecuteExpression(subNode, scopeContext));
                }

                return array;
            }

            if (node.TokenType == "Identifier") {
                if (scopeContext.Variables.ContainsKey(node.Body)) {
                    return scopeContext.Variables[node.Body];
                }
                else {
                    engine.throwError("Variable '" + node.Body + "' does not exist in the current context!", node.Token);
                }
            }

            return null;
        }
    }
}
