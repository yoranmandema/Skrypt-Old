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
            public bool IsPostfix = false;

            public OpereratorPrecedence(string[] ops, bool LA = true, int mems = 2, bool PF = false) {
                Operators = ops.ToList<string>();
                Members = mems;
                LeftAssociate = LA;
                IsPostfix = PF;
            }
        }

        static List<OpereratorPrecedence> OperatorPrecedence = new List<OpereratorPrecedence> {
            new OpereratorPrecedence(new[] {"."}, true, 2, true),
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
                        int i = 0;
                        bool CanLoop = Tokens.Count > 0;
                        int parDepth = 0;
                        int firstPar = -1;

                        while (CanLoop) {                      
                            Token token = Tokens[i];
                            Token previousToken = i >= 1 ? Tokens[i-1] : null;

                            if (token.Value == "(") {
                                if (previousToken != null) {
                                    if (previousToken.Type == "Identifier") {
                                        Node node = ParseCall(Tokens, ref i);

                                        if (i == Tokens.Count) {
                                            branch.Add(node);
                                            return;
                                        }
                                    }
                                }

                                if (firstPar == -1)
                                    firstPar = i;
                            
                                parDepth++;
                            } else if (token.Value == ")") {
                                parDepth--;

                                if (i == Tokens.Count - 1 && firstPar == 0) {
                                    isInPars = true;
                                    return;
                                }
                            } else if (token.Value == "[") {
                                if (previousToken != null) {
                                    if (previousToken.Type == "Identifier" || previousToken.Type == "StringLiteral") {
                                        Node node = ParseIndexing(Tokens, ref i);

                                        if (i == Tokens.Count) {
                                            branch.Add(node);
                                            return;
                                        }
                                    }
                                } else {
                                    Node node = ParseArrayLiteral(Tokens, ref i);

                                    if (i == Tokens.Count) {
                                        branch.Add(node);
                                        return;
                                    }
                                }
                            } else if (token.Value == Operator && parDepth == 0) {
                                leftBuffer = Tokens.GetRange(0, i);
                                rightBuffer = Tokens.GetRange(i + 1, Tokens.Count - i - 1);

                                Node NewNode = new Node();
                                NewNode.Body = token.Value;
                                NewNode.TokenType = token.Type;

                                if (OP.Members > 1) {
                                    Node LeftNode = parse(NewNode, leftBuffer);
                                    NewNode.Add(LeftNode);
                                }

                                Node RightNode = parse(NewNode, rightBuffer);
                                NewNode.Add(RightNode);

                                branch.Add(NewNode);
                                return;
                            }

                            CanLoop = OP.LeftAssociate ? i < Tokens.Count - 1 : ((Tokens.Count - 1) - i) > 0 ;
                            i++;
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

        static public void SetArguments(List<List<Token>> Arguments, List<Token> Tokens) {
            int depth = 0;
            int indexDepth = 0;
            int i = 0;
            int startArg = 0;
            List<Token> Buffer = new List<Token>();

            for (i = 0; i < Tokens.Count; i++) {
                Token token = Tokens[i];

                if (token.Value == "(") {
                    depth++;
                }
                else if (token.Value == ")") {
                    depth--;
                }

                if (token.Value == "[") {
                    indexDepth++;
                }
                else if (token.Value == "]") {
                    indexDepth--;
                }

                if ((token.Value == "," || i == Tokens.Count - 1) && depth == 0 && indexDepth == 0) {
                    Buffer = Tokens.GetRange(startArg, (i == Tokens.Count - 1 ? i + 1 : i) - startArg);
                    startArg = i + 1;
                    Arguments.Add(Buffer);
                }
            }
        }

        static void SkipArguments(ref int Index, ref int endArguments, string upScope, string downScope, List<Token> Tokens) {
            int depth = 1;

            while (depth != 0) {              
                if (Tokens[Index].Value == upScope) {
                    depth++;
                }
                else if (Tokens[Index].Value == downScope) {
                    depth--;

                    if (depth == 0) {
                        endArguments = Index;
                    }
                }

                Index++;
            }
        }

        static public Node ParseCall(List<Token> Tokens, ref int Index) {
            Node node = new Node { Body = Tokens[Index-1].Value, TokenType = "Call" };

            Index += 1;

            int i = Index;
            int endArguments = i;

            SkipArguments(ref Index, ref endArguments, "(", ")", Tokens);

            List<List<Token>> Arguments = new List<List<Token>>();
            SetArguments(Arguments, Tokens.GetRange(i, endArguments - i));

            foreach (List<Token> Argument in Arguments) {
                Node argNode = parse(node, Argument);
                node.Add(argNode);
            }

            return node;
        }

        static public Node ParseIndexing(List<Token> Tokens, ref int Index) {
            Node node = new Node { Body = Tokens[Index - 1].Value, TokenType = "Index" };

            Index++;

            int i = Index;
            int endArguments = i;

            SkipArguments(ref Index, ref endArguments, "[", "]", Tokens);

            Node argNode = parse(node, Tokens.GetRange(i, endArguments - i));
            node.Add(argNode);

            return node;
        }

        static public Node ParseArrayLiteral (List<Token> Tokens, ref int Index) {
            Node node = new Node { Body = "Array", TokenType = "ArrayLiteral" };

            Index++;
            int i = Index;
            int endArguments = i;

            SkipArguments(ref Index, ref endArguments, "[", "]", Tokens);

            List<List<Token>> Arguments = new List<List<Token>>();
            SetArguments(Arguments, Tokens.GetRange(i, endArguments - i));

            foreach (List<Token> Argument in Arguments) {
                Node argNode = parse(node, Argument);
                node.Add(argNode);
            }

            return node;
        }

        static public Node Parse(List<Token> Tokens, ref int Index) {
            Node node = new Node();
            int i = Index;

            while (Tokens[Index].Value != ";") {
                Index++;
            }

            node.Add(parse(node, Tokens.GetRange(i, Index - i)));

            return node.SubNodes[0];
        }
    }
}
