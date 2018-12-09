using System.Collections.Generic;
using Skrypt.Engine;
using Skrypt.Tokenization;
using System;
using System.IO;

namespace Skrypt.Parsing
{
    /// <summary>
    ///     The statement parser class.
    ///     Contains all methods to parse if, elseif, else and while statements
    /// </summary>
    public class StatementParser
    {
        private readonly SkryptEngine _engine;

        public StatementParser(SkryptEngine e) {
            _engine = e;
        }

        /// <summary>
        ///     Parses a statement ([if,while] (expression) {block}) into a node
        /// </summary>
        public ParseResult ParseStatement(List<Token> tokens) {
            var index = 0;
            var skip = default(SkipInfo);
            var result = default(ParseResult);

            // Create main statement node
            var node = new Node {Body = tokens[index].Value, Type = TokenTypes.Statement};

            skip = _engine.ExpectValue("(", tokens);
            index += skip.Delta;

            result = _engine.GeneralParser.ParseSurrounded("(", ")", index, tokens, _engine.ExpressionParser.ParseClean);
            var conditionNode = result.Node;
            index += result.Delta;

            var conditionParentNode = new Node {
                Body = "Condition",
                Type = TokenTypes.Condition
            };

            if (conditionNode == null) {
                _engine.ThrowError("Condition can't be empty!", tokens[index - 1]);
            }

            conditionParentNode.Add(conditionNode);

            skip = _engine.ExpectValue("{", tokens, index);
            index += skip.Delta;

            result = _engine.GeneralParser.ParseSurrounded("{", "}", index, tokens, _engine.GeneralParser.Parse);
            var blockNode = result.Node;
            index += result.Delta + 1;

            // Add condition and block nodes to main node
            node.Add(conditionNode);
            node.Add(blockNode);

            return new ParseResult {Node = node, Delta = index};
        }

        public ParseResult ParseForStatement(List<Token> tokens) {
            var index = 0;
            var skip = default(SkipInfo);
            var result = default(ParseResult);
            var node = new Node { Body = "for", Type = TokenTypes.Statement };

            skip = _engine.ExpectValue("(", tokens);
            index += skip.Delta;
            skip = _engine.ExpressionParser.SkipFromTo("(",")",tokens,index);
            index += skip.Delta;

            var conditionTokens = tokens.GetRange(skip.Start + 1, skip.Delta - 1);
            var conditionResult = default(ParseResult);
            int i;

            for (i = 0; i < conditionTokens.Count; i++) {
                conditionResult = _engine.ExpressionParser.Parse(conditionTokens.GetRange(i, conditionTokens.Count - i));
                i += conditionResult.Delta - 1;

                node.Add(conditionResult.Node);
            }

            if (node.Nodes.Count != 3) {
                if (node.Nodes.Count > 3) {
                    _engine.ThrowError("For loop has too many statements", conditionTokens[i - 2]);
                } else {
                    switch (node.Nodes.Count) {
                        case 0:
                            _engine.ThrowError("Missing initializer", conditionTokens[i - 2]);
                            break;
                        case 1:
                            _engine.ThrowError("Missing condition", conditionTokens[i - 2]);
                            break;
                        case 2:
                            _engine.ThrowError("Missing modifier", conditionTokens[i - 2]);
                            break;
                    }
                }
            }

            skip = _engine.ExpectValue("{", tokens, index);
            index += skip.Delta;

            result = _engine.GeneralParser.ParseSurrounded("{", "}", index, tokens, _engine.GeneralParser.Parse);
            var blockNode = result.Node;
            index += result.Delta + 1;

            node.Add(blockNode);

            return new ParseResult { Node = node, Delta = index };
        }

        /// <summary>
        ///     Parses an else statement (else {block})
        /// </summary>
        public ParseResult ParseElseStatement(List<Token> tokens) {
            var index = 0;
            var skip = default(SkipInfo);

            skip = _engine.ExpectValue("{", tokens);
            index += skip.Delta;

            var result = _engine.GeneralParser.ParseSurrounded("{", "}", index, tokens, _engine.GeneralParser.Parse);
            var node = result.Node;
            node.Body = "else";
            index += result.Delta + 1;

            return new ParseResult {Node = node, Delta = index};
        }

        /// <summary>
        ///     Parses an if statement, including elseif and else statements
        /// </summary>
        public ParseResult ParseIfStatement(List<Token> tokens)
        {
            var index = 0;

            // Create main statement node
            var preResult = ParseStatement(tokens);
            index += preResult.Delta;

            var result = new IfNode {
                Condition = preResult.Node.Nodes[0],
                Block = preResult.Node.Nodes[1]
            };

            // Only parse statements elseif/else if there's any tokens after if statement
            if (index < tokens.Count - 1) {
                while (tokens[index].Equals("elseif", TokenTypes.Keyword)) {
                    var elseIfResult = ParseStatement(tokens.GetRange(index, tokens.Count - index));

                    result.ElseIfNodes.Add(new ElseIfNode {
                        Condition = elseIfResult.Node.Nodes[0],
                        Block = elseIfResult.Node.Nodes[1]
                    });

                    index += elseIfResult.Delta;

                    if (index == tokens.Count) break;
                }

                if (tokens[index].Equals("else", TokenTypes.Keyword)) {
                    var elseResult = ParseElseStatement(tokens.GetRange(index, tokens.Count - index));

                    result.ElseNode = elseResult.Node;
                    index += elseResult.Delta;
                }
            }   

            return new ParseResult {Node = result, Delta = index};
        }

        /// <summary>
        ///     Parses an include statement.
        /// </summary>
        public ParseResult ParseInclude(List<Token> tokens) {
            var skip = _engine.ExpectType(TokenTypes.StringLiteral, tokens);

            if (string.IsNullOrEmpty(_engine.Root)) {
                _engine.ThrowError("Engine path invalid!", tokens[1]);
            }

            var desiredFile = tokens[1].Value.EndsWith(".skt") ? tokens[1].Value : tokens[1].Value + ".skt";
            var path = Path.GetFullPath(Path.Combine(_engine.Root, desiredFile));
            var fileName = SkryptEngine.MakeRelativePath(_engine.Root, path);
            var fileStream = File.Open(path, FileMode.Open);

            string includedCode;
            using (var sr = new StreamReader(fileStream)) {
                includedCode = sr.ReadToEnd();
            }

            _engine.Files[fileName] = includedCode;

            var oldParsingFile = _engine.CurrentParsingFile;
            _engine.CurrentParsingFile = path;

            var includeTokens = _engine.Tokenizer.Tokenize(includedCode);

            _engine.TokenProcessor.ProcessTokens(includeTokens);

            var programNode = _engine.GeneralParser.Parse(includeTokens);
            var node = new IncludeNode {
                Nodes = programNode.Nodes,
                Path = fileName
            };

            _engine.CurrentParsingFile = oldParsingFile;

            return new ParseResult { Node = node, Delta = 3 };
        }

        /// <summary>
        ///     Parses any statement and returns node
        /// </summary
        public ParseResult Parse(List<Token> tokens)
        {
            switch (tokens[0].Value)
            {
                case "while":
                    return ParseStatement(tokens);
                case "for":
                    return ParseForStatement(tokens);
                case "if":
                    return ParseIfStatement(tokens);
                case "else":
                    _engine.ThrowError("else statement must come directly after if or elseif statement", tokens[0]);
                    break;
                case "elseif":
                    _engine.ThrowError("elseif statement must come directly after if or elseif statement", tokens[0]);
                    break;
            }

            // No statement found
            return null;
        }
    }
}