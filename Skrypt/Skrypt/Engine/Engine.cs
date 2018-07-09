﻿using System;
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
        public int delta = -1;
        public Node node;
    }

    public class SkryptEngine
    {
        public Analizer analizer;
        public ClassParser classParser;
        private string Code = "";
        public Executor executor;
        public ExpressionParser expressionParser;
        public GeneralParser generalParser;
        public ScopeContext GlobalScope = new ScopeContext();
        public List<Node> MethodNodes = new List<Node>();
        public MethodParser methodParser;
        public List<SkryptMethod> Methods = new List<SkryptMethod>();
        public StandardMethods standardMethods;
        public StatementParser statementParser;
        public Tokenizer tokenizer;

        private List<Token> Tokens;

        //List<SkryptClass> Classes = new List<SkryptClass>();

        public SkryptEngine()
        {
            tokenizer = new Tokenizer(this);
            statementParser = new StatementParser(this);
            expressionParser = new ExpressionParser(this);
            generalParser = new GeneralParser(this);
            methodParser = new MethodParser(this);
            classParser = new ClassParser(this);
            analizer = new Analizer(this);
            executor = new Executor(this);
            standardMethods = new StandardMethods(this);

            standardMethods.AddMethodsToEngine();


            var SystemObject = ObjectGenerator.MakeObjectFromClass(typeof(Library.Native.System), this);

            foreach (var property in SystemObject.Properties)
                GlobalScope.AddVariable(property.Name, property.Value, true);

            // Tokens that are found using a token rule with type defined as 'null' won't get added to the token list.
            // This means you can ignore certain characters, like whitespace in this case, that way.
            tokenizer.AddRule(
                new Regex(@"\s"),
                TokenTypes.None
            );

            tokenizer.AddRule(
                new Regex(@"\d+(\.\d+)?"),
                TokenTypes.NumericLiteral
            );

            tokenizer.AddRule(
                new Regex(@"[_a-zA-Z]+[_a-zA-Z0-9]*"),
                TokenTypes.Identifier
            );

            tokenizer.AddRule(
                new Regex(@"class|func|if|elseif|else|while"),
                TokenTypes.Keyword
            );

            tokenizer.AddRule(
                new Regex("true|false"),
                TokenTypes.BooleanLiteral
            );

            tokenizer.AddRule(
                new Regex("null"),
                TokenTypes.NullLiteral
            );

            tokenizer.AddRule(
                new Regex(@"[;]"),
                TokenTypes.EndOfExpression
            );

            tokenizer.AddRule(
                new Regex(
                    @"(return)|(&&)|(\|\|)|(\|\|\|)|(==)|(!=)|(>=)|(<=)|(<<)|(>>)|(>>>)|(\+\+)|(--)|[~=:<>+\-*/%^&|!\[\]\(\)\.\,{}]"),
                TokenTypes.Punctuator
            );

            tokenizer.AddRule(
                new Regex(@""".*?(?<!\\)"""),
                TokenTypes.StringLiteral
            );

            // Multi line comment
            tokenizer.AddRule(
                new Regex(@"\/\*(.|\n)*\*\/"),
                TokenTypes.None
            );

            // Single line comment
            tokenizer.AddRule(
                new Regex(@"\/\/.*\n"),
                TokenTypes.None
            );
        }

        public Dictionary<string, SkryptObject> Types { get; set; } = new Dictionary<string, SkryptObject>();

        /// <summary>
        ///     Calculates the line and column of a given index
        /// </summary>
        public string getLineAndRowStringFromIndex(int index)
        {
            var lines = 1;
            var row = 1;
            var i = 0;

            while (i < index)
            {
                if (Code[i] == '\n')
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
        public skipInfo expectValue(string Value, List<Token> Tokens, int startingPoint = 0)
        {
            var start = startingPoint;
            var index = startingPoint;
            var msg = "Token '" + Value + "' expected after " + Tokens[index].Value + " keyword";

            if (index == Tokens.Count - 1) throwError(msg, Tokens[index]);

            if (Tokens[index + 1].Value == Value)
                index++;
            else
                throwError(msg, Tokens[index]);

            var delta = index - startingPoint;

            return new skipInfo {start = start, end = index, delta = delta};
        }

        /// <summary>
        ///     Skips token if next token has the given value. Throws exception when not found.
        /// </summary>
        public skipInfo expectType(TokenTypes Type, List<Token> Tokens, int startingPoint = 0)
        {
            var start = startingPoint;
            var index = startingPoint;
            var msg = "Token with type '" + Type + "' expected after " + Tokens[index].Value + " keyword";

            if (index == Tokens.Count - 1) throwError(msg, Tokens[index]);

            if (Tokens[index + 1].Type == Type)
                index++;
            else
                throwError(msg, Tokens[index]);

            var delta = index - startingPoint;

            return new skipInfo {start = start, end = index, delta = delta};
        }

        /// <summary>
        ///     Throws an error with line and colom indicator
        /// </summary>
        public void throwError(string message, Token token = null, int urgency = -1)
        {
            var lineRow = token != null ? " (" + getLineAndRowStringFromIndex(token.Start) + ")" : "";

            throw new SkryptException(message + lineRow, urgency);
        }

        public Node Parse(string code)
        {
            Code = code;

            var stopwatch = Stopwatch.StartNew();

            // Tokenize code
            Tokens = tokenizer.Tokenize(code);
            if (Tokens == null) return null;

            // Pre-process tokens so their values are correct
            TokenProcessor.ProcessTokens(Tokens);

            stopwatch.Stop();
            double T_Token = stopwatch.ElapsedMilliseconds;

            // Generate the program node
            stopwatch = Stopwatch.StartNew();
            var ProgramNode = generalParser.Parse(Tokens);
            stopwatch.Stop();
            double T_Parse = stopwatch.ElapsedMilliseconds;

            // Debug program node
            Console.WriteLine("Program:\n" + ProgramNode);

            //ScopeContext AnalizeScope = new ScopeContext();
            //analizer.Analize(ProgramNode, AnalizeScope);

            stopwatch = Stopwatch.StartNew();
            GlobalScope = executor.ExecuteBlock(ProgramNode, GlobalScope);
            stopwatch.Stop();
            double T_Execute = stopwatch.ElapsedMilliseconds;

            Console.WriteLine("Execution: {0}ms, Parsing: {1}ms, Tokenization: {2}ms", T_Execute, T_Parse, T_Token);

            return ProgramNode;
        }
    }
}