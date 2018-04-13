using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SymbolicCS.Util;

namespace SymbolicCS.Parsing
{
    class Tokenizer
    {
        private Tokenizer()
        {
            _dict = new Dictionary<TokenType, Regex>()
            {
                [TokenType.Numeric] = new Regex(@"[0-9]+(\.[0-9]+)?", RegexOptions.Compiled),
                [TokenType.Variable] = new Regex(@"\b[a-z]\b", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                [TokenType.Add] = new Regex(@"\+", RegexOptions.Compiled),
                [TokenType.Sub] = new Regex(@"\-", RegexOptions.Compiled),
                [TokenType.Mul] = new Regex(@"\*", RegexOptions.Compiled),
                [TokenType.Div] = new Regex(@"\/", RegexOptions.Compiled),
                [TokenType.Pow] = new Regex(@"\^", RegexOptions.Compiled),
                [TokenType.Exp] = new Regex(@"exp", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                [TokenType.Log] = new Regex(@"log", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                [TokenType.Sin] = new Regex(@"sin", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                [TokenType.Cos] = new Regex(@"cos", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                [TokenType.LeftParen] = new Regex(@"\(", RegexOptions.Compiled),
                [TokenType.RightParen] = new Regex(@"\)", RegexOptions.Compiled),
            };
        }

        public IList<Token> Tokenize(string s)
        {
            var list = new SortedList<int, Token>();
            foreach (var kv in _dict)
            {
                var rx = kv.Value;
                foreach (Match match in rx.Matches(s))
                    list.Add(match.Index, new Token(match.Value, match.Index, kv.Key));
            }

            return list.Values;

        }

        public static Tokenizer GetInstance() => _instance ?? (_instance = new Tokenizer());

        private readonly IDictionary<TokenType, Regex> _dict;
        private static Tokenizer _instance;

        internal struct Token
        {
            public string Identifier { get; }
            public int Position { get; }
            public TokenType Type { get; }
            public int Precedence { get; }
            public Associativity Associativity { get; }

            public Token(string identifier, int position, TokenType type)
            {
                Identifier = identifier.ToLower();
                Position = position;
                Type = type;
                Precedence = type.GetPrecedence();
                Associativity = type.GetAssociativity();
            }

            public Token(Token copyToken, TokenType type) : this(copyToken.Identifier, copyToken.Position, type) { }

            public override string ToString() => $"{Type}('{Identifier}')";
        }
    }


    [Flags]
    internal enum TokenType
    {
        None = 0,
        Numeric = 1,
        Variable = 2,
        Add = 4, Sub = 8, Mul = 16, Div = 32, Pow = 64, Neg = 128,
        Operator = Add | Sub | Mul | Div | Pow | Neg,
        Log = 256, Sin = 512, Cos = 1024, Exp = 2048,
        Function = Exp | Log | Sin | Cos,
        Operation = Operator | Function,
        LeftParen = 4096, RightParen = 1 << 13,
    }

    internal enum Associativity
    {
        None,
        Right,
        Left
    }
}