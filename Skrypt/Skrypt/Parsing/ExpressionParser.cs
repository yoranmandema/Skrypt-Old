using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;

namespace Skrypt.Parsing {
    static class ExpressionParser {
        class OpereratorPrecedence {
            public List<string> Operators = new List<string>();
            public int Members;
            public bool LeftAssociate = true;

            public OpereratorPrecedence(string[] ops, bool LA = true, int mems = 2) {
                Operators = ops.ToList<string>();
                Members = mems;
                LeftAssociate = LA;
            }
        }

        static List<OpereratorPrecedence> OperatorPrecedence = new List<OpereratorPrecedence> {
            new OpereratorPrecedence(new[] {"="}, false),
            new OpereratorPrecedence(new[] {"+","-"}),
            new OpereratorPrecedence(new[] {"*","/","%"}),
            new OpereratorPrecedence(new[] {"^"}),
            new OpereratorPrecedence(new[] { "-", "!" }, false, 1),
        };

        static string TokenString (List<Token> Tokens) {
            string sb = "";

            foreach (Token token in Tokens) {
                sb += token.Value;
            }

            return sb;
        }

        static Node parse (Node branch, List<Token> Tokens) {
            List<Token> leftBuffer = new List<Token>();
            List<Token> rightBuffer = new List<Token>();

            bool isInPars = false; 

            Action loop = () => {
                foreach (OpereratorPrecedence OP in OperatorPrecedence) {
                    foreach (string Operator in OP.Operators) {
                        int i = OP.LeftAssociate ? 0 : Math.Max(Tokens.Count - 1,0);
                        bool CanLoop = Tokens.Count > 0;
                        int parDepth = 0;
                        int firstPar = -1;

                        while (CanLoop) {                      
                            Token token = Tokens[i];

                            if (token.Value == "(") {
                                if (firstPar == -1)
                                    firstPar = i;

                                parDepth++;
                            } else if (token.Value == ")") {
                                parDepth--;

                                if (i == Tokens.Count - 1 && firstPar == 0) {
                                    isInPars = true;
                                }
                            } if (token.Value == Operator && parDepth == 0) {
                                leftBuffer = Tokens.GetRange(0, i);
                                rightBuffer = Tokens.GetRange(i + 1, Tokens.Count - i - 1);

                                Node NewNode = new Node();
                                NewNode.Body = token.Value;
                                NewNode.TokenType = token.Type;

                                if (OP.Members > 1) {
                                    Node LeftNode = parse(NewNode, leftBuffer);
                                    if (LeftNode != null)
                                        NewNode.Add(LeftNode);
                                }

                                Node RightNode = parse(NewNode, rightBuffer);
                                if (RightNode != null) 
                                    NewNode.Add(RightNode);

                                branch.Add(NewNode);
                                return;
                            }

                            CanLoop = OP.LeftAssociate ? i < Tokens.Count - 1 : i > 0;
                            i += OP.LeftAssociate ? 1 : -1;
                        }
                    }
                }
            };
            loop();

            if (isInPars) {
                return parse(branch, Tokens.GetRange(1, Tokens.Count - 2));
            }

            if (Tokens.Count != 1) {
                return null;
            }

            return new Node {
                Body = Tokens[0].Value,
                Value = null,
                TokenType = Tokens[0].Type
            };
        }

        static public Node Parse(List<Token> Tokens, ref int Index) {
            Node node = new Node { Body = "Expression" };
            int i = Index;

            while (Tokens[Index].Value != ";") {
                Index++;
            }

            parse(node, Tokens.GetRange(i, Index - i));

            return node;
        }
    }
}
