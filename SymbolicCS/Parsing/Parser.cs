using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SymbolicCS.Util;
using static SymbolicCS.Parsing.Associativity;

namespace SymbolicCS.Parsing
{
    using Token = Tokenizer.Token;

    internal class Parser
    {
        public IExpression Parse(string input)
        {
            var list = CreateAST(input);
            return list;
        }

        // Create Syntax Tree
        private IExpression CreateAST(string input)
        {
            var stack = new Stack<IExpression>();
            var tokens = CreateRPN(input);
            Debug.WriteLine($"RPN: {string.Join(" -> ", tokens.Select(t => t+""))}\n");
            foreach (var token in tokens)
            {
                switch (token.Type)
                {
                    case TokenType.Numeric:
                        stack.Push(new Num(double.Parse(token.Identifier)));
                        break;
                    case TokenType.Variable:
                        stack.Push(new Var(token.Identifier[0]));
                        break;
                    case TokenType.Add:
                        var p1 = stack.Pop();
                        stack.Push(new Add(stack.Pop(), p1));
                        break;
                    case TokenType.Sub:
                        var p2 = stack.Pop();
                        stack.Push(new Sub(stack.Pop(), p2));
                        break;
                    case TokenType.Mul:
                        var p3 = stack.Pop();
                        stack.Push(new Mul(stack.Pop(), p3));
                        break;
                    case TokenType.Div:
                        var p4 = stack.Pop();
                        stack.Push(new Div(stack.Pop(), p4));
                        break;
                    case TokenType.Pow:
                        var p5 = stack.Pop();
                        stack.Push(new Pow(stack.Pop(), p5));
                        break;
                    case TokenType.Exp:
                        stack.Push(new Exp(stack.Pop()));
                        break;
                    case TokenType.Neg:
                        stack.Push(new Neg(stack.Pop()));
                        break;
                    case TokenType.Log:
                        stack.Push(new Log(stack.Pop()));
                        break;
                    case TokenType.Sin:
                        stack.Push(new Sin(stack.Pop()));
                        break;
                    case TokenType.Cos:
                        stack.Push(new Cos(stack.Pop()));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return stack.Pop();
        }

        // Create Reverse Polish Notation using Shunting Yard
        private Queue<Token> CreateRPN(string input)
        {
            var tokenizer = Tokenizer.GetInstance();
            var list = new Queue<Token>();
            var stack = new Stack<Token>();
            TokenType prev = 0;
            var tokens = tokenizer.Tokenize(input);
            Debug.WriteLine("\nIN : " + string.Join(" -> ", tokens.Select(t => t+"")));
            foreach (var token in tokens)
            {
                var curTok = token;
                try
                {
                    switch (curTok.Type)
                    {
                        case TokenType.Numeric:
                        case TokenType.Variable:
                            list.Enqueue(curTok);
                            continue;
                        case TokenType.Sub:
                            if (prev == 0 || prev == TokenType.LeftParen || (prev & TokenType.Operation) != 0)
                                curTok = new Token(curTok, TokenType.Neg);
                            break;
                        case TokenType.Add:
                        case TokenType.Mul:
                        case TokenType.Div:
                        case TokenType.Pow:
                            break;
                        case TokenType.Exp:
                        case TokenType.Log:
                        case TokenType.Sin:
                        case TokenType.Cos:
                        case TokenType.LeftParen:
                            stack.Push(curTok);
                            continue;
                        case TokenType.RightParen:
                            while (stack.Count > 0 && stack.Peek().Type != TokenType.LeftParen)
                                list.Enqueue(stack.Pop());
                            stack.Pop();
                            continue;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    while (stack.Count > 0)
                    {
                        var stackOp = stack.Peek();

                        if ((stackOp.Type & TokenType.Function) > 0
                          || stackOp.Precedence > curTok.Precedence
                          || (stackOp.Associativity == Left && stackOp.Precedence == curTok.Precedence))
                        {
                            Debug.Assert(stackOp.Type != TokenType.LeftParen);
                            list.Enqueue(stack.Pop());
                        }
                        else break;
                    }
                    stack.Push(curTok);
                }
                finally
                {
                    prev = curTok.Type;
                }
            }
            while (stack.Count > 0)
                list.Enqueue(stack.Pop());

            return list;
        }


        private Parser() {}
        public static Parser GetInstance() => _instance ?? (_instance = new Parser());
        private static Parser _instance;

    }

}