using System;
using System.Collections.Generic;
using Skrypt.Engine;
using Skrypt.Tokenization;

namespace Skrypt.Parsing
{
    /// <summary>
    ///     The general parser class.
    ///     Contains all methods to parse higher-level code, e.g code that contains statements AND expressions
    /// </summary>
    public class GeneralParser
    {
        public delegate Node ParseArgumentsMethod(List<List<Token>> args, List<Token> tokens);
        public delegate Node ParseMethod(List<Token> tokens);

        public static List<string> Keywords = new List<string> {
            "if",
            "while",
            "for",
            "fn",
            "class",
            "using",
            "break",
            "continue",
            "const",
            "return",
            "include"
        };

        public static List<string> NotPermittedInExpression = new List<string> {
            "if",
            "while",
            "for",
            "class",
            "using",
            "const",
            "include"
        };

        public static List<string> Modifiers = new List<string> {
            "public",
            "private",
            "const",
            "in",
            "strong"
        };

        private readonly SkryptEngine _engine;

        public GeneralParser(SkryptEngine e) {
            _engine = e;
        }

        /// <summary>
        ///     Gets all tokens surrouned by the open and close tokens.
        /// </summary>
        public List<Token> GetSurroundedTokens(string open, string close, int start, List<Token> tokens) {
            var index = start;
            var i = index + 1;
            var skip = _engine.ExpressionParser.SkipFromTo(open, close, tokens, index);
            var end = skip.End;
            index = skip.End;

            return tokens.GetRange(i, end - i);
        }

        /// <summary>
        ///     Parses tokens surrounded by the 'open' and 'close' tokens using the given parse method.
        /// </summary>
        public ParseResult ParseSurrounded(string open, string close, int start, List<Token> tokens, ParseMethod parseMethod) {
            var surroundedTokens = GetSurroundedTokens(open, close, start, tokens);

            var node = parseMethod(surroundedTokens);

            return new ParseResult {Node = node, Delta = surroundedTokens.Count + 1};
        }

        /// <summary>
        ///     Parses any expression surrounded by the 'open' and 'close' tokens.
        /// </summary>
        public ParseResult ParseSurroundedExpressions(string open, string close, int start, List<Token> tokens) {
            var surroundedTokens = GetSurroundedTokens(open, close, start, tokens);

            var node = new Node();
            var expressions = new List<List<Token>>();

            // Get a token list for each expression
            _engine.ExpressionParser.SetArguments(expressions, surroundedTokens);

            foreach (var expression in expressions) {
                // Multiple expressions per argument (e.g 'a, b + 2 c + 3, d'), is not allowed. 
                // Check for any EndOfExpression tokens within an argument to see if there are multiple.
                for (int i = 0; i < expression.Count - 1; i++) {
                    var next = expression[i + 1];

                    if (next.Type == TokenTypes.EndOfExpression) {
                        _engine.ThrowError("Syntax error, ',' expected.", expression[i]);
                    }
                }

                var argNode = _engine.ExpressionParser.ParseExpression(node, expression);
                node.Add(argNode);
            }

            return new ParseResult {Node = node, Delta = surroundedTokens.Count + 1};
        }

        /// <summary>
        ///     Parses one part of a program (branch statement, expression, function definition etc).
        /// </summary>
        private ParseResult TryParse(List<Token> tokens) {
            var i = 0;
            var t = tokens[i];
            var usedModifiers = new List<string>();
            var appliedModifiers = Modifier.None;

            // Curly brackets have to be preceeded by either a branch statement, class definition or function definition
            if ((tokens[0].Type == TokenTypes.Punctuator) && (tokens[0].Value == "{" || tokens[0].Value == "}")) {
                _engine.ThrowError("Statement expected.", tokens[0]);
            }
            
            // Check for modifiers.
            while (Modifiers.Contains(t.Value)) {
                if (usedModifiers.Contains(t.Value)) {
                    _engine.ThrowError("Object can't be marked with multiple of the same modifiers", t);
                }

                if (usedModifiers.Contains("public") && t.Equals("private", TokenTypes.Keyword)) {
                    _engine.ThrowError("Object can't be marked as both private and public", t);
                }

                if (usedModifiers.Contains("private") && t.Equals("public", TokenTypes.Keyword)) {
                    _engine.ThrowError("Object can't be marked as both private and public", t);
                }

                usedModifiers.Add(t.Value);

                switch (t.Value) {
                    case "public":
                        appliedModifiers |= Modifier.Public;
                        break;
                    case "private":
                        appliedModifiers |= Modifier.Private;
                        break;
                    case "in":
                        appliedModifiers |= Modifier.Instance;
                        break;
                    case "const":
                        appliedModifiers |= Modifier.Const;
                        break;
                    case "strong":
                        appliedModifiers |= Modifier.Strong;
                        break;
                }

                i++;

                if (i < tokens.Count - 1) {
                    t = tokens[i];
                } else {
                    break;
                }
            }

            // All tokens proceeding the modifier tokens.
            var parseTokens = tokens.GetRange(i, tokens.Count - i);

            var result = default(ParseResult);

            if (appliedModifiers != Modifier.None) {
                // Modifier tokens have to be proceeded by a variable definition.
                if (parseTokens.Count == 0) {
                    _engine.ThrowError("Syntax error, variable definition expected.", tokens[0]);
                }

                if ((appliedModifiers & Modifier.Public) == 0) {
                    appliedModifiers |= Modifier.Private;
                }
            }

            // Parse branch statement.
            if (parseTokens[0].Equals("if", TokenTypes.Keyword) || parseTokens[0].Equals("while", TokenTypes.Keyword) || parseTokens[0].Equals("for", TokenTypes.Keyword)) {
                result = _engine.StatementParser.Parse(parseTokens);
            }
            // Parse function definiton (or a function literal if applicable).
            else if (parseTokens[0].Equals("fn", TokenTypes.Keyword)) {
                var isLiteral = false;

                // If the fn keyword is not proceeded by an identifier, it means its a function literal.
                if (parseTokens.Count > 2) {
                    if (parseTokens[1].Type != TokenTypes.Identifier) {
                        isLiteral = true;
                    }
                }

                if (isLiteral) {
                    result = _engine.ExpressionParser.Parse(parseTokens);
                }
                else {
                    result = _engine.MethodParser.Parse(parseTokens);
                }
            }
            // Parse class definition.
            else if (parseTokens[0].Equals("class", TokenTypes.Keyword)) {
                result = _engine.ClassParser.Parse(parseTokens);

                if ((appliedModifiers & Modifier.Instance) != 0) {
                    ((ClassNode)result.Node).BodyNode.Nodes.ForEach(x => {
                        if ((x.Modifiers & Modifier.Instance) != 0) _engine.ThrowError("Syntax error, unexpected 'in' modifier.", x.Token);
                    });
                }
            }
            // Parse using statement.
            else if (parseTokens[0].Equals("using", TokenTypes.Punctuator)) {
                result = _engine.ExpressionParser.ParseUsing(parseTokens);
            }
            // Parse include statement.
            else if (parseTokens[0].Equals("include", TokenTypes.Keyword)) {
                result = _engine.StatementParser.ParseInclude(parseTokens);
            }
            // Parse as expression.
            else {
                result = _engine.ExpressionParser.Parse(parseTokens);
            }

            result.Node.Modifiers = appliedModifiers;

            _engine.ModifierChecker.CheckModifiers(result.Node);

            result.Delta += usedModifiers.Count;

            return result;
        }

        /// <summary>
        ///     Parses a set of tokens into a program node.
        /// </summary>
        public Node Parse(List<Token> tokens) {
            _engine.State = EngineState.Parsing;

            // Create main node
            var node = new BlockNode();

            if (tokens.Count == 0) return node;

            var i = 0;

            while (i <= tokens.Count - 1) {
                var bit = TryParse(tokens.GetRange(i, tokens.Count - i));
                i += bit.Delta;

                // Move function definition nodes to first place.
                if (bit.Node.Type == TokenTypes.MethodDeclaration) {
                    node.AddAsFirst(bit.Node);
                }
                //else if (bit.Node.Type == TokenTypes.Include) {
                //    foreach (var n in bit.Node.Nodes) {
                //        node.Add(n);
                //    }
                //}
                else {
                    node.Add(bit.Node);
                }
            }

            node.Token = new Token {
                Start = tokens[0].Start,
                End = tokens[tokens.Count - 1].End
            };

            return node;
        }
    }
}