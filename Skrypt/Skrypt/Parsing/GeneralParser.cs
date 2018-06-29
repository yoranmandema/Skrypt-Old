using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;
using Skrypt.Engine;

namespace Skrypt.Parsing {
    /// <summary>
    /// The general parser class.
    /// Contains all methods to parse higher-level code, e.g code that contains statements AND expressions
    /// </summary>
    public class GeneralParser {
        SkryptEngine engine;

        public GeneralParser(SkryptEngine e) {
            engine = e;
        }

        class TokenPattern {
            public string[] values;
            public int dependingPattern = -1;
            public string type;
            public string kind;

            public TokenPattern (string[] v, int d, string t, string k) {
                values = v;
                dependingPattern = d;
                type = t;
                kind = k;
            }

            public override string ToString() {
                return "values: " + string.Join(",", values) +
                    ", depending: " + dependingPattern +
                    ", type: " + type +
                    ", kind: " + kind;
            }
        }

        static TokenPattern getPattern (string part) {
            string[] values = part.Split('|');
            string type = "";
            string kind = "";
            int enclosedIndex = -1;

            string[] enclosedSplit = values[values.Length - 1].Split(':');

            if (enclosedSplit.Length == 2) {
                values[values.Length - 1] = enclosedSplit[0];
                enclosedIndex = int.Parse(enclosedSplit[1]);
            }

            string[] typeSplit = values[values.Length - 1].Split('?');

            if (typeSplit.Length == 2) {
                values[values.Length - 1] = typeSplit[0];
                type = typeSplit[1];
            }

            string[] kindSplit = values[values.Length - 1].Split('#');

            if (kindSplit.Length == 2) {
                values[values.Length - 1] = kindSplit[0];
                kind = kindSplit[1];
            }

            return new TokenPattern(values, enclosedIndex, type, kind);
        }

        public ParseResult FromPattern (List<Token> Tokens, string[] pattern) {
            Node node = new Node ();
            Node lastNode = null;
            int index = 0;

            for (int i = 0; i < pattern.Length; i++) {
                Console.WriteLine("Index {0} length {1}",i,pattern.Length);
                string part = pattern[i];

                TokenPattern partPattern = getPattern(part);

                Console.WriteLine(partPattern);

                if (i < pattern.Length -1) {
                    TokenPattern next = getPattern(pattern[i + 1]);
                    string nextValue = next.values[0];
                    Console.WriteLine("Check next");

                    if (!(nextValue == "expression" || nextValue == "block") && !(partPattern.values[0] == "expression" || partPattern.values[0] == "block")) {
                        int succeeded = 0;

                        foreach (string Value in next.values) {
                            try {
                                engine.expectValue(Value, Tokens, index);
                                succeeded++;
                            } catch (Exception e) {

                            }
                        }
                    }
                }

                Console.WriteLine("Success");

                if (partPattern.dependingPattern > -1) {
                    TokenPattern dependingPattern = getPattern(pattern[partPattern.dependingPattern]);
                    TokenPattern enclosedPattern = getPattern(pattern[partPattern.dependingPattern - 1]);

                    string opening = partPattern.values[0];
                    string closing = dependingPattern.values[0];

                    skipInfo skip = engine.expressionParser.SkipFromTo(opening, closing, Tokens, index);
                    List<Token> SubTokens = Tokens.GetRange(skip.start + 1, skip.delta - 1);

                    Console.WriteLine(ExpressionParser.TokenString(SubTokens));
                    Console.WriteLine(Tokens[index + skip.delta]);

                    i = partPattern.dependingPattern;

                    switch (enclosedPattern.values[0]) {
                        case "expression":
                            lastNode = new Node {
                                Body = enclosedPattern.type.Length > 0 ? enclosedPattern.type : null,
                                TokenType = enclosedPattern.kind.Length > 0 ? enclosedPattern.kind : null
                            };

                            lastNode.Add(engine.expressionParser.Parse(SubTokens).node);

                            node.Add(lastNode);

                            index = skip.end + 1;
                            break;
                       case "block":
                            lastNode = engine.generalParser.Parse(SubTokens);

                            node.Add(lastNode);
                            index = skip.end + 1;
                            break;
                    }
                } else if (partPattern.values.Any(Tokens[index].Value.Contains) || (Tokens[index].Type == partPattern.type)) {
                    if (index == 0) {
                        node.Body = Tokens[index].Value;
                        node.TokenType = partPattern.kind.Length > 0 ? partPattern.kind : null;
                        index++;
                    }
                    else {
                        lastNode = new Node {
                            Body = Tokens[index].Value,
                            TokenType = partPattern.kind.Length > 0 ? partPattern.kind : null
                        };

                        node.Add(lastNode);
                        index++;
                    }
                } else if (partPattern.values[0] == "expression") {
                    List<Token> SubTokens = Tokens.GetRange(index, Tokens.Count - index);

                    Console.WriteLine(ExpressionParser.TokenString(SubTokens));

                    lastNode = new Node {
                        Body = partPattern.type.Length > 0 ? partPattern.type : null,
                        TokenType = partPattern.kind.Length > 0 ? partPattern.kind : null
                    };

                    ParseResult result = engine.expressionParser.Parse(SubTokens);

                    lastNode.Add(result.node);

                    node.Add(lastNode);

                    index += result.delta;
                }
            }

            return new ParseResult { node = node, delta = index };
        }

        public delegate Node parseMethod(List<Token> Tokens);
        public delegate Node parseArgumentsMethod(List<List<Token>> Args, List<Token> Tokens);

        List<Token> GetSurroundedTokens (string open, string close, int start, List<Token> Tokens) {
            int index = start;

            int i = index + 1;
            skipInfo skip = engine.expressionParser.SkipFromTo(open, close, Tokens, index);
            int end = skip.end;
            index = skip.end;

            return Tokens.GetRange(i, end - i);
        }

        /// <summary>
        /// Parses tokens surrounded by 'open' and 'close' tokens using the given parse method
        /// </summary>
        public ParseResult parseSurrounded(string open, string close, int start, List<Token> Tokens, parseMethod parseMethod) {
            List<Token> SurroundedTokens = GetSurroundedTokens(open, close, start, Tokens);

            Console.WriteLine("General: " + ExpressionParser.TokenString(Tokens));

            Node node = parseMethod(SurroundedTokens);

            return new ParseResult { node = node, delta = SurroundedTokens.Count + 1 };
        }

        Exception GetExceptionBasedOnUrgency (Exception e, ref int highestErrorUrgency) {
            Exception error = null;

            if (e.GetType() == typeof(SkryptException)) {
                SkryptException cast = (SkryptException)e;

                if (cast.urgency >= highestErrorUrgency) {
                    Console.WriteLine(cast.urgency);

                    error = e;
                    highestErrorUrgency = cast.urgency;
                }
            }

            return error;
        }

        /// <summary>
        /// Parses tokens surrounded by 'open' and 'close' tokens using the given parse method
        /// </summary>
        public ParseResult parseSurroundedExpressions (string open, string close, int start, List<Token> Tokens) {
            List<Token> SurroundedTokens = GetSurroundedTokens(open, close, start, Tokens);

            Node node = new Node ();
            List<List<Token>> Arguments = new List<List<Token>>();
            ExpressionParser.SetArguments(Arguments, SurroundedTokens);

            foreach (List<Token> Argument in Arguments) {
                Node argNode = engine.expressionParser.ParseClean(Argument);
                node.Add(argNode);
            }

            return new ParseResult { node = node, delta = SurroundedTokens.Count + 1 };
        }

        ParseResult TryParse (List<Token> Tokens) {

            int highestErrorUrgency = -1;
            Exception error = null;
            ParseResult ExpressionResult = null;
            ParseResult StatementResult = null;
            ParseResult MethodResult = null;

            try {
                ParseResult result = engine.expressionParser.Parse(Tokens);

                ExpressionResult = result;
            }
            catch (Exception e) {
                Exception test = GetExceptionBasedOnUrgency(e, ref highestErrorUrgency);

                if (test != null)
                    error = test;
            }

            try {
                ParseResult result = engine.statementParser.Parse(Tokens);

                StatementResult = result;
            }
            catch (Exception e) {
                Exception test = GetExceptionBasedOnUrgency(e, ref highestErrorUrgency);

                if (test != null)
                    error = test;
            }

            try {
                ParseResult result = engine.methodParser.Parse(Tokens);

                MethodResult = result;
            }
            catch (Exception e) {
                Exception test = GetExceptionBasedOnUrgency(e, ref highestErrorUrgency);

                if (test != null)
                    error = test;
            }

            Console.WriteLine(highestErrorUrgency + " , " + error.Message);

            if (highestErrorUrgency > -1) {
                engine.throwError(error.Message);
                return null;
            }

            if (StatementResult != null) {
                return StatementResult;
            } else if (ExpressionResult != null) {
                return ExpressionResult;
            } else if (MethodResult != null) {
                return MethodResult;
            } else {
                //throw error;
                engine.throwError(error.Message, Tokens[0]);
                return null;
            }
        }

        public Node Parse(List<Token> Tokens) {
            // Create main node
            Node Node = new Node { Body = "Block", TokenType = "Block" };

            if (Tokens.Count == 0) {
                return Node;
            }

            int i = 0;

            while (i < Tokens.Count - 1) {
                var Test = TryParse(Tokens.GetRange(i,Tokens.Count - i));
                i += Test.delta;

                Node.Add(Test.node);
            }

            return Node;
        }
    }
}
