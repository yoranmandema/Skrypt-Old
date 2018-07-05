using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;
using Skrypt.Engine;
using Skrypt.Library;

namespace Skrypt.Parsing {
    public class skipInfo {
        public int start;
        public int end;
        public int delta;
    }

    /// <summary>
    /// The expression parser class.
    /// Contains all methods to parse expressions, and helper methods
    /// </summary>
    public class ExpressionParser {
        SkryptEngine engine;

        public ExpressionParser(SkryptEngine e) {
            engine = e;
        }

        /// <summary>
        /// Class representing an operator group
        /// </summary>
        class OperatorGroup {
            public List<Operator> Operators = new List<Operator>();
            public int Members;
            public bool LeftAssociate = true;
            public bool IsPostfix = false;

            public OperatorGroup(Operator[] ops, bool LA = true, int mems = 2, bool PF = false) {
                Operators = ops.ToList<Operator>();
                Members = mems;
                LeftAssociate = LA;
                IsPostfix = PF;
            }
        }

        // Create list of operator groups with the right precedence order
        static List<OperatorGroup> OperatorPrecedence = new List<OperatorGroup> {
            new OperatorGroup(new[] {new Op_Return()}, false, 1),
            new OperatorGroup(new[] {new Op_Assign()}, false),
            new OperatorGroup(new[] {new Op_Access()}, false, 2, false),
            new OperatorGroup(new[] {new Op_Or()}),
            new OperatorGroup(new[] {new Op_And()}),
            new OperatorGroup(new Operator[] {new Op_NotEqual(),new Op_Equal()}),
            new OperatorGroup(new Operator[] {new Op_Lesser(),new Op_Greater(), new Op_LesserEqual(), new Op_GreaterEqual()}),
            new OperatorGroup(new Operator[] {new Op_Add(), new Op_Subtract()}),
            new OperatorGroup(new Operator[] {new Op_Multiply(), new Op_Divide(), new Op_Modulo()}),
            new OperatorGroup(new[] {new Op_Power()}),
            new OperatorGroup(new Operator[] { new Op_Negate(), new Op_Not() }, false, 1),
            new OperatorGroup(new Operator[] { new Op_PostInc(), new Op_PostDec() }, false, 1, true),
        };

        // (debug) Serializes a list of tokens into a string
        public static string TokenString (List<Token> Tokens) {
            string sb = "";

            foreach (Token token in Tokens) {
                sb += token.Value;
            }

            return sb;
        }

        /// <summary>
        /// Parses a list of tokens into an expression recursively
        /// </summary>
        public Node ParseExpression (Node branch, List<Token> Tokens) {
            if (Tokens.Count == 1) {
                // return resulting end node
                return new Node {
                    Body = Tokens[0].Value,
                    //Value = null,
                    TokenType = "" + Tokens[0].Type,
                    Token = Tokens[0],
                };
            }

            // Create left and right token buffers
            List<Token> leftBuffer = new List<Token>();
            List<Token> rightBuffer = new List<Token>();

            bool isInPars = false;
            bool isMethodCall = false;
            bool isIndexing = false;
            bool isArrayLiteral = false;
            bool isFunctionLiteral = false;
            bool isChain = false;

            int accessEnd = 0;

            // Do logic in delegate so we can easily exit out of it when we need to
            Action loop = () => {
                foreach (OperatorGroup OP in OperatorPrecedence) {
                    foreach (Operator Operator in OP.Operators) {
                        int i = 0;
                        bool CanLoop = Tokens.Count > 0;

                        while (CanLoop) {
                            //Console.WriteLine("Accessing from: " + TokenString(Tokens));
                            //Console.WriteLine("sub: " + TokenString(Tokens.GetRange(i, Tokens.Count - i)));
                            skipInfo s = skipChain(Tokens, i);

                            if (s.delta > 0) {
                                i = s.end - 1;
                                //Console.WriteLine(s.delta);
                                //Console.WriteLine(i);
                                //Console.WriteLine(s.start);
                                //Console.WriteLine(Tokens.Count - 1);
                                //Console.WriteLine(Tokens[i]);
                                if (s.start == 0 && i == Tokens.Count - 1 && s.delta != 0) {
                                    //Console.WriteLine("Whole expression is accessing");
                                    isChain = true;
                                    return;
                                }
                            }

                            Token token = Tokens[i];
                            Token previousToken = i >= 1 ? Tokens[i - 1] : null;

                            if (token.Value == "func") {
                                skipInfo skip = engine.expectValue("(", Tokens, i);
                                i += skip.delta;

                                int start = i;
                                skip = engine.expressionParser.SkipFromTo("(", ")", Tokens, i);
                                i += skip.delta;

                                skip = engine.expressionParser.SkipFromTo("{", "}", Tokens, i);
                                i += skip.delta;

                                if (start == 1 && skip.end == Tokens.Count - 1) {
                                    isFunctionLiteral = true;
                                    return;
                                }
                            }

                            if (GeneralParser.Keywords.Contains(Tokens[i].Value)) {
                                engine.throwError("Unexpected keyword '" + Tokens[i].Value + "' found", Tokens[i], 2);
                            }

                            if (token.Value == "(") {
                                if (previousToken != null) {
                                    // Previous token was identifier; possible method call
                                    if (previousToken.Type == TokenTypes.Identifier) {
                                        skipInfo skip = engine.expressionParser.SkipFromTo("(", ")", Tokens, i);
                                        i += skip.delta;

                                        if (skip.start == 1 && skip.end == Tokens.Count - 1) {
                                            isMethodCall = true;
                                            Console.WriteLine("Method call tokens: " + TokenString(Tokens));
                                            return;
                                        }
                                    }
                                }
                            }
                            if (token.Value == "(") {
                                skipInfo skip = engine.expressionParser.SkipFromTo("(", ")", Tokens, i);
                                i += skip.delta;

                                if (skip.start == 0 && skip.end == Tokens.Count - 1) {
                                    isInPars = true;
                                    return;
                                }
                            }
                            if (token.Value == "[") {
                                if (previousToken != null) {
                                    // Previous token was identifier or string; possible indexing
                                    if (previousToken.Type == TokenTypes.Identifier || previousToken.Type == TokenTypes.StringLiteral) {
                                        skipInfo skip = engine.expressionParser.SkipFromTo("[", "]", Tokens, i);
                                        i += skip.delta;

                                        if (skip.start == 1 && skip.end == Tokens.Count - 1) {
                                            isIndexing = true;
                                            return;
                                        }
                                    }
                                }
                            }
                            if (token.Value == "[") {
                                skipInfo skip = engine.expressionParser.SkipFromTo("[", "]", Tokens, i);
                                i += skip.delta;

                                if (skip.start == 0 && skip.end == Tokens.Count - 1) {
                                    isArrayLiteral = true;
                                    return;
                                }
                            }
                            if (token.Value == Operator.Operation) {
                                // Fill left and right buffers
                                leftBuffer = Tokens.GetRange(0, i);
                                rightBuffer = Tokens.GetRange(i + 1, Tokens.Count - i - 1);

                                bool HasRequiredLeftTokens = leftBuffer.Count > 0;
                                bool HasRequiredRightTokens = rightBuffer.Count > 0;

                                if (OP.Members == 1) {
                                    if (OP.IsPostfix) {
                                        HasRequiredRightTokens = true;
                                    }
                                    else {
                                        HasRequiredLeftTokens = true;
                                    }
                                }

                                if (HasRequiredLeftTokens && HasRequiredRightTokens) {
                                    // Create operation node with type and body
                                    Node NewNode = new Node();
                                    NewNode.Body = Operator.OperationName;
                                    NewNode.TokenType = "" + token.Type;
                                    NewNode.Token = token;

                                    if (OP.Members == 1) {
                                        // Parse unary and do postfix logic

                                        Node LeftNode = !OP.IsPostfix ? null : ParseExpression(NewNode, leftBuffer);
                                        NewNode.Add(LeftNode);

                                        Node RightNode = OP.IsPostfix ? null : ParseExpression(NewNode, rightBuffer);
                                        NewNode.Add(RightNode);
                                    } else {
                                        // Parse operators that need 2 sides

                                        Node LeftNode = ParseExpression(NewNode, leftBuffer);
                                        NewNode.Add(LeftNode);

                                        Node RightNode = ParseExpression(NewNode, rightBuffer);
                                        NewNode.Add(RightNode);
                                    }

                                    branch.Add(NewNode);
                                    return;
                                } else {
                                    engine.throwError("Missing member of operation!", token, 10);
                                }
                            }

                            // Check if we're still in bounds
                            CanLoop = OP.LeftAssociate ? i < Tokens.Count - 1 : ((Tokens.Count - 1) - i) > 0 ;
                            i++;
                        }
                    }
                }
            };
            loop();

            if (isChain) {
                return ParseChain(Tokens);
            }

            // Parse expression within parenthesis if it's completely surrounded
            if (isInPars) {
                return ParseExpression(branch, Tokens.GetRange(1, Tokens.Count - 2));
            }

            // Parse method call
            if (isMethodCall) {
                ParseResult result = ParseCall(Tokens, accessEnd);
                return result.node;
            }

            // Parse indexing
            if (isIndexing) {
                ParseResult result = ParseIndexing(Tokens);
                return result.node;
            }

            // Parse indexing
            if (isArrayLiteral) {
                ParseResult result = ParseArrayLiteral(Tokens);
                return result.node;
            }

            // Parse function literal
            if (isFunctionLiteral) {
                ParseResult result = engine.methodParser.ParseFunctionLiteral(Tokens.GetRange(1, Tokens.Count - 1));
                return result.node;
            }

            return null;
        }

        /// <summary>
        /// Sets parses individual arguments as expressions
        /// </summary>
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

        /// <summary>
        /// Skip tokens that are surrounded by 'upScope' and 'downScope'
        /// </summary>
        public skipInfo SkipFromTo(string upScope, string downScope, List<Token> Tokens, int startingPoint = 0) {
            int start = startingPoint;
            int depth = 0;
            int index = startingPoint;
            int end = 0;
            Token firstToken = null;

            while (true) {
                if (Tokens[index].Value == upScope) {
                    depth++;

                    if (firstToken == null) {
                        firstToken = Tokens[index];
                    }
                }
                else if (Tokens[index].Value == downScope) {
                    depth--;

                    if (depth == 0) {
                        end = index;
                        int delta = index - start;

                        return new skipInfo {start=start, end=end, delta=delta};
                    }
                }

                index++;

                if (index == Tokens.Count) {
                    if (depth > 0) {
                        engine.throwError("Closing token '" + downScope + "' not found", firstToken);
                    } else if (depth < 0) {
                        engine.throwError("Opening token '" + upScope + "' not found", Tokens[index]);
                    }
                }
            }
        }

        /// <summary>
        /// Skip tokens until we hit a token with the given value
        /// </summary>
        public void SkipUntil (ref int Index, Token Comparator, List<Token> Tokens) {
            Token startToken = Tokens[Index];

            while (!Tokens[Index].LazyEqual(Comparator)) {
                Index++;

                if (Index == Tokens.Count - 1) {
                    engine.throwError("Token '" + Comparator + "' expected", startToken);
                }
            }
        }   

        public skipInfo SkipAccessing (List<Token> Tokens, int startingPoint = 0) {
            int start = startingPoint;
            int index = startingPoint;
            int end = 0;
            int state = 1;

            Token nextToken = Tokens[index];

            if (nextToken.Type == TokenTypes.Identifier) {
                state = 1;
            } else if (nextToken.Value == ".") {
                state = 0;
            }

            while (true) {
                if (state == 1) {
                    if (nextToken.Type == TokenTypes.Identifier) {
                        state = 0;
                    } else {
                        engine.throwError("Identifier expected!", nextToken);
                    }
                } else if (state == 0) {
                    if (nextToken.Value == ".") {
                        state = 1;
                    } else {
                        break;
                    }
                }

                index++;
                nextToken = Tokens[index];
            }

            end = index;
            int delta = index - start;

            return new skipInfo { start = start, end = end, delta = delta };
        }

        public skipInfo SkipAccess (List<Token> Tokens, int startingPoint = 0) {
            int start = startingPoint;
            int index = startingPoint;
            int end = 0;
            int state = 0;
            Token token = Tokens[index];
            
            if (token.Type == TokenTypes.Identifier) {
                state = 0;
            }

            if (token.Value == ".") {
                state = 1;
            }

            while (true) {
                if (token.Type == TokenTypes.Identifier && state == 0) {
                    state = 1;
                } else if (token.Value == "." && state == 1) {
                    state = 0;
                } else {
                    break;
                }

                index++;

                if (index == Tokens.Count)
                    break;

                token = Tokens[index];
            }

            if (index > 0) {
                if (Tokens[index - 1].Value == ".") {
                    engine.throwError("Identifier expected!", Tokens[index - 1]);
                }
            }

            //Console.WriteLine("Last: " + Tokens[index-1]);

            end = index;
            int delta = index - start;

            return new skipInfo { start = start, end = end, delta = delta };
        }

        public skipInfo skipChain (List<Token> Tokens, int startingPoint) {
            int start = startingPoint;
            int index = startingPoint;
            int end = 0;

            Token token = Tokens[index];

            while (true) {
                //Console.WriteLine(token);

                if (token.Value == "(") {
                    skipInfo skip = engine.expressionParser.SkipFromTo("(", ")", Tokens, index);
                    index = skip.end + 1;

                    //Console.WriteLine("Skip amount: " + skip.delta);
                    //Console.WriteLine("Par Skipped: " + TokenString(Tokens.GetRange(skip.start, index - skip.start)));
                }
                else if (token.Value == "[") {
                    skipInfo skip = engine.expressionParser.SkipFromTo("[", "]", Tokens, index);
                    index = skip.end + 1;
                    //Console.WriteLine("Bracket Skipped: " + TokenString(Tokens.GetRange(skip.start, index - skip.start)));
                } else if (token.Value == "." || token.Type == TokenTypes.Identifier) {
                    skipInfo skip = engine.expressionParser.SkipAccess(Tokens, index);
                    index = skip.end;
                    //Console.WriteLine("Skip amount: " + skip.delta);
                    //Console.WriteLine("Access Skipped: " + TokenString(Tokens.GetRange(skip.start, index - skip.start)));
                } else {
                    break;
                }

                if (index == Tokens.Count)
                    break;

                token = Tokens[index];
            }

            end = index;
            int delta = index - start;

            //Console.WriteLine("Skipped: " + delta);
            //Console.WriteLine("Skipped: " + TokenString(Tokens.GetRange(start, delta)));

            return new skipInfo { start = start, end = end, delta = delta };
        }

        public Node ParseChain (List<Token> Tokens) {
            Node node = new Node();

            //Console.WriteLine("Count: " + Tokens.Count);

            if (Tokens.Count == 1) {
                return ParseExpression(node, Tokens);
            }

            List<Token> Reverse = Tokens.GetRange(0,Tokens.Count);
            Reverse.Reverse();

            // Get last part of the chain tokens
            //Console.WriteLine("First: " + Reverse[0]);

            if (Reverse[0].Value == "]") {
                skipInfo skip = SkipFromTo("]", "[", Reverse, 0);

                if (skip.end + 1 >= Reverse.Count) {
                    engine.throwError("Indexing operator needs left hand value!", Reverse[skip.end]);
                }
                else if (Reverse[skip.end + 1].Value == ".") {
                    engine.throwError("Indexing operator needs left hand value!", Reverse[skip.end]);
                }

                Node getterNode = new Node();
                getterNode.Add(ParseChain(Tokens.GetRange(0, Tokens.Count - (skip.end + 1))));
                getterNode.Body = "Getter";
                getterNode.TokenType = "Getter";

                //Console.WriteLine("Getter: " + TokenString(Tokens.GetRange(0, Tokens.Count - (skip.end + 1))));

                node.Add(getterNode);

                List<Token> ExpressionTokens = Reverse.GetRange(1, skip.end - 1);
                ExpressionTokens.Reverse();
                //Console.WriteLine("Expression: " + TokenString(ExpressionTokens));
                node.Add(ParseExpression(node, ExpressionTokens));

                node.Body = "Index";
                node.TokenType = "Index";

                //Console.WriteLine(node);
            } else if (Reverse[0].Value == ")") {
                skipInfo skip = SkipFromTo(")", "(", Reverse, 0);

                if (skip.end + 1 >= Reverse.Count) {
                    engine.throwError("Call operator needs left hand value!", Reverse[skip.end]);
                }
                else if (Reverse[skip.end + 1].Value == ".") {
                    engine.throwError("Call operator needs left hand value!", Reverse[skip.end]);
                }

                Node getterNode = new Node();
                getterNode.Add(ParseChain(Tokens.GetRange(0, Tokens.Count - (skip.end + 1))));
                getterNode.Body = "Getter";
                getterNode.TokenType = "Getter";

                //Console.WriteLine("Getter: " + TokenString(Tokens.GetRange(0, Tokens.Count - (skip.end + 1))));

                node.Add(getterNode);

                List<Token> ArgumentTokens = Reverse.GetRange(0, skip.end + 1);
                ArgumentTokens.Reverse();
                //Console.WriteLine("Arguments: " + TokenString(ArgumentTokens));

                ParseResult result = engine.generalParser.parseSurroundedExpressions("(", ")", 0, ArgumentTokens);
                Node argumentsNode = result.node;
                argumentsNode.Body = "Arguments";
                argumentsNode.TokenType = "Arguments";
                node.Add(argumentsNode);

                node.Body = "Call";
                node.TokenType = "Call";

                //Console.WriteLine(node);
            } else {
                Console.WriteLine("access");

                node.Body = "access";
                node.TokenType = "" + TokenTypes.Punctuator;

                node.Add(ParseExpression(node,new List<Token> { Reverse[0] }));
                Console.WriteLine(node);
                node.Add(ParseChain(Tokens.GetRange(0, Tokens.Count - 2)));

                //Console.WriteLine(node);
            }

            return node;
        }

        /// <summary>
        /// Parses a method call with arguments
        /// </summary>
        public ParseResult ParseCall(List<Token> Tokens, int accessEnd) {
            int index = 0;
            string name = Tokens[index].Value;
            Node node = new Node();
            node.Body = "Call";
            node.TokenType = "Call";

            List<Token> AccessTokens = Tokens.GetRange(0, accessEnd);
            Console.WriteLine("Call: " + TokenString(AccessTokens));
            Node getterNode = new Node();
            getterNode.Add(ParseExpression(getterNode, AccessTokens));
            getterNode.Body = "Getter";
            getterNode.TokenType = "Getter";
            node.Add(getterNode);

            ParseResult result = engine.generalParser.parseSurroundedExpressions("(", ")", accessEnd, Tokens);
            Node argumentsNode = result.node;
            argumentsNode.Body = "Arguments";
            argumentsNode.TokenType = "Arguments";
            node.Add(argumentsNode);

            index = Tokens.Count - 1;

            //Console.WriteLine(node);

            //skipInfo skip = engine.expectValue("(", Tokens);
            //index += skip.delta;

            //ParseResult result = engine.generalParser.parseSurroundedExpressions("(", ")", accessEnd, Tokens);
            //Node node = result.node;
            //node.TokenType = "Call";
            //node.Body = name;
            //node.Token = Tokens[0];
            //index += result.delta;

            return new ParseResult {node=node,delta=index};
        }

        /// <summary>
        /// Parses an index operation
        /// </summary>
        public ParseResult ParseIndexing(List<Token> Tokens) {
            Node node = new Node { Body = Tokens[0].Value, TokenType = "Index" };
            int index = 1;

            ParseResult result = engine.generalParser.parseSurrounded("[", "]", index, Tokens, engine.expressionParser.ParseClean);
            Node argNode = result.node;
            index += result.delta;
            node.Add(argNode);
            node.Token = Tokens[0];

            return new ParseResult { node = node, delta = index };
        }

        /// <summary>
        /// Parses an array literal
        /// </summary>
        public ParseResult ParseArrayLiteral (List<Token> Tokens) {
            int index = 0;

            ParseResult result = engine.generalParser.parseSurroundedExpressions("[", "]", 0, Tokens);
            Node node = result.node;
            node.TokenType = "ArrayLiteral";
            node.Body = "Array";
            index += result.delta;

            return new ParseResult { node = node, delta = index };
        }

        /// <summary>
        /// Parses an expression node without any parenting node
        /// </summary>
        public Node ParseClean (List<Token> Tokens) {
            Node node = new Node();
            node.Add(ParseExpression(node, Tokens));

            // Only return the first subnode, so we don't create a messy AST
            Node returnNode = null;
            if (node.SubNodes.Count > 0) {
                returnNode = node.SubNodes[0];
            }

            return returnNode;
        }

        /// <summary>
        /// Parses a list of tokens into an expression node
        /// </summary>
        public ParseResult Parse(List<Token> Tokens) {
            Node node = new Node();
            int delta = 0;
            int pScope = 0;
            int bScope = 0;
            int cScope = 0;

            // Skip until we hit the end of an expression
            while (true) {
                switch (Tokens[delta].Value) {
                    case "(":
                        pScope++;
                        break;
                    case ")":
                        pScope--;
                        break;
                    case "[":
                        bScope++;
                        break;
                    case "]":
                        bScope--;
                        break;
                    case "{":
                        cScope++;
                        break;
                    case "}":
                        cScope--;
                        break;
                }

                //if (Tokens[delta].Value == ";" && pScope == 0 && bScope == 0 && cScope == 0) {
                //    break;
                //}

                if (pScope == 0 && bScope == 0 && cScope == 0) {
                    // Definite end of expression
                    if (Tokens[delta].Value == ";") {
                        break;
                    }
                }

                delta++;

                if (delta == Tokens.Count) {
                    break;
                }
            }

            Node returnNode = ParseClean(Tokens.GetRange(0, delta));

            delta++;

            return new ParseResult {node = returnNode, delta = delta};
        }
    }
}
