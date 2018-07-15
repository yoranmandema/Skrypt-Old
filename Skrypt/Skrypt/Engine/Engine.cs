using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Skrypt.Analysis;
using Skrypt.Execution;
using Skrypt.Library;
using Skrypt.Library.Native;
using Skrypt.Library.Reflection;
using Skrypt.Parsing;
using Skrypt.Tokenization;
// Using this so we can check how fast everything is happening

namespace Skrypt.Engine
{
    public class ParseResult
    {
        //public Exception error;
        public int Delta = -1;
        public Node Node;
    }

    public class SkryptEngine
    {
        public Analizer Analizer;
        public ClassParser ClassParser;
        private string _code = "";
        public Executor Executor;
        public ExpressionParser ExpressionParser;
        public GeneralParser GeneralParser;
        public ScopeContext GlobalScope = new ScopeContext();
        public List<Node> MethodNodes = new List<Node>();
        public MethodParser MethodParser;
        public List<SkryptMethod> Methods = new List<SkryptMethod>();
        public StandardMethods StandardMethods;
        public StatementParser StatementParser;
        public Tokenizer Tokenizer;
        public TokenProcessor TokenProcessor;

        private List<Token> _tokens;

        //List<SkryptClass> Classes = new List<SkryptClass>();

        public SkryptEngine()
        {
            Tokenizer = new Tokenizer(this);
            TokenProcessor = new TokenProcessor(this);
            StatementParser = new StatementParser(this);
            ExpressionParser = new ExpressionParser(this);
            GeneralParser = new GeneralParser(this);
            MethodParser = new MethodParser(this);
            ClassParser = new ClassParser(this);
            Analizer = new Analizer(this);
            Executor = new Executor(this);
            StandardMethods = new StandardMethods(this);

            StandardMethods.AddMethodsToEngine();


            var systemObject = ObjectGenerator.MakeObjectFromClass(typeof(Library.Native.System), this);

            //foreach (var property in systemObject.Properties)
            //    GlobalScope.AddVariable(property.Name, property.Value, true);

            GlobalScope.AddVariable(systemObject.Name, systemObject, true);

            // Tokens that are found using a token rule with type defined as 'null' won't get added to the token list.
            // This means you can ignore certain characters, like whitespace in this case, that way.
            Tokenizer.AddRule(
                new Regex(@"\s"),
                TokenTypes.None
            );

            Tokenizer.AddRule(
                new Regex(@"\d+(\.\d+)?([eE][-+]?\d+)?"),
                TokenTypes.NumericLiteral
            );

            Tokenizer.AddRule(
                new Regex(@"0x([A-Fa-f\d])+"),
                TokenTypes.HexadecimalLiteral
            );

            Tokenizer.AddRule(
                new Regex(@"0b([01])+"),
                TokenTypes.BinaryLiteral
            );

            Tokenizer.AddRule(
                new Regex(@"[_a-zA-Z]+[_a-zA-Z0-9]*"),
                TokenTypes.Identifier
            );

            Tokenizer.AddRule(
                new Regex(@"class|func|if|elseif|else|while"),
                TokenTypes.Keyword
            );

            Tokenizer.AddRule(
                new Regex("true|false"),
                TokenTypes.BooleanLiteral
            );

            Tokenizer.AddRule(
                new Regex("null"),
                TokenTypes.NullLiteral
            );

            Tokenizer.AddRule(
                new Regex(@"[;]"),
                TokenTypes.EndOfExpression
            );

            //Tokenizer.AddRule(
            //    new Regex(@"\n"),
            //    TokenTypes.NewLine
            //);

            Tokenizer.AddRule(
                new Regex(
                    @"(using)|(return)|(continue)|(break)|(&&)|(\|\|\|)|(\|\|)|(==)|(!=)|(>=)|(<=)|(<<)|(>>>)|(>>)|(\+\+)|(--)|[~=:<>+\-*/%^&|!\[\]\(\)\.\,{}\?\:]"),
                TokenTypes.Punctuator
            );

            Tokenizer.AddRule(
                new Regex(@""".*?(?<!\\)"""),
                TokenTypes.StringLiteral
            );

            // Multi line comment
            Tokenizer.AddRule(
                new Regex(@"\/\*(.|\n)*\*\/"),
                TokenTypes.None
            );

            // Single line comment
            Tokenizer.AddRule(
                new Regex(@"\/\/.*\n"),
                TokenTypes.None
            );
        }

        public Dictionary<string, SkryptObject> Types { get; set; } = new Dictionary<string, SkryptObject>();

        public void AddClass(Type type) {
            var generated = ObjectGenerator.MakeObjectFromClass(type, this);
            GlobalScope.AddVariable(generated.Name, generated, true);
        }

        /// <summary>
        ///     Calculates the line and column of a given index
        /// </summary>
        public string GetLineAndRowStringFromIndex(int index)
        {
            var lines = 1;
            var row = 1;
            var i = 0;

            while (i < index)
            {
                if (_code[i] == '\n')
                {
                    lines++;
                    row = 1;
                }
                else
                {
                    row++;
                }

                i++;
            }

            return "line: " + lines + ", col: " + row;
        }

        /// <summary>
        ///     Skips token if next token has the given value. Throws exception when not found.
        /// </summary>
        public SkipInfo ExpectValue(string value, List<Token> tokens, int startingPoint = 0)
        {
            var start = startingPoint;
            var index = startingPoint;
            var msg = "Token '" + value + "' expected after " + tokens[index].Value + " keyword";

            if (index == tokens.Count - 1) ThrowError(msg, tokens[index]);

            if (tokens[index + 1].Value == value)
                index++;
            else
                ThrowError(msg, tokens[index]);

            var delta = index - startingPoint;

            return new SkipInfo {Start = start, End = index, Delta = delta};
        }

        /// <summary>
        ///     Skips token if next token has the given value. Throws exception when not found.
        /// </summary>
        public SkipInfo ExpectType(TokenTypes type, List<Token> tokens, int startingPoint = 0)
        {
            var start = startingPoint;
            var index = startingPoint;
            var msg = "Token with type '" + type + "' expected after " + tokens[index].Value + " keyword";

            if (index == tokens.Count - 1) ThrowError(msg, tokens[index]);

            if (tokens[index + 1].Type == type)
                index++;
            else
                ThrowError(msg, tokens[index]);

            var delta = index - startingPoint;

            return new SkipInfo {Start = start, End = index, Delta = delta};
        }

        /// <summary>
        ///     Throws an error with line and colom indicator
        /// </summary>
        public void ThrowError(string message, Token token = null)
        {
            var lineRow = token != null ? " (" + GetLineAndRowStringFromIndex(token.Start) + ")" : "";

            throw new SkryptException(message + lineRow);
        }

        public Node Parse(string code)
        {
            _code = code;

            var stopwatch = Stopwatch.StartNew();

            // Tokenize code
            _tokens = Tokenizer.Tokenize(code);
            if (_tokens == null) return null;

            // Pre-process tokens so their values are correct
            TokenProcessor.ProcessTokens(_tokens);

            stopwatch.Stop();
            double token = stopwatch.ElapsedMilliseconds;

            //foreach (var t in _tokens) Console.WriteLine(t);

            // Generate the program node
            stopwatch = Stopwatch.StartNew();
            var programNode = GeneralParser.Parse(_tokens);
            stopwatch.Stop();
            double parse = stopwatch.ElapsedMilliseconds;

            // Debug program node
            Console.WriteLine("Program:\n" + programNode);

            //ScopeContext AnalizeScope = new ScopeContext();
            //analizer.Analize(ProgramNode, AnalizeScope);

            stopwatch = Stopwatch.StartNew();
            GlobalScope = Executor.ExecuteBlock(programNode, GlobalScope);
            stopwatch.Stop();
            double execute = stopwatch.ElapsedMilliseconds;

            Console.WriteLine($"Execution: {execute}ms, Parsing: {parse}ms, Tokenization: {token}ms, Total: {execute + parse + token}ms");

            int instances = 1000;
            stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < instances; i++) {
                GlobalScope = Executor.ExecuteBlock(programNode, GlobalScope);
            }
            stopwatch.Stop();
            Console.WriteLine($"Average ({instances} instances): {stopwatch.ElapsedMilliseconds / instances}ms");

            return programNode;
        }
    }
}