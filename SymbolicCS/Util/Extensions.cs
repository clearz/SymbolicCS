using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using AsciiTree;
using SymbolicCS.Parsing;

namespace SymbolicCS.Util
{
    internal static class Extensions
    {

        public static int GetPrecedence(this TokenType t)
        {
            switch (t)
            {
                case TokenType.Exp:
                case TokenType.Log:
                case TokenType.Sin:
                case TokenType.Cos:
                    return 0;
                case TokenType.Add:
                case TokenType.Sub:
                    return 1;
                case TokenType.Mul:
                case TokenType.Div:
                    return 2;
                case TokenType.Pow:
                case TokenType.Neg:
                    return 3;
                default:
                    return -1;
            }
        }

        public static Associativity GetAssociativity(this TokenType t)
        {
            switch (t)
            {
                case TokenType.Add:
                case TokenType.Sub:
                case TokenType.Mul:
                case TokenType.Div:
                    return Associativity.Left;
                case TokenType.Exp:
                case TokenType.Log:
                case TokenType.Sin:
                case TokenType.Cos:
                case TokenType.Pow:
                    return Associativity.Right;
                default:
                    return Associativity.None;
            }
        }

        public static void Print(this IExpression e, Action<object> output = null)
        {
            output = output ?? Console.Write;
            
            output(InnerPrint(e));
            output(Environment.NewLine);

            object InnerPrint(IExpression ex)
            {
                switch (ex)
                {
                    case Num n:
                        return n.Value;
                    case Var v:
                        return v.Name;
                    case IBinaryExpression b:
                        return $"{b.GetType().Name}({InnerPrint(b.Left)}, {InnerPrint(b.Right)})";
                    case IFuncExpression f:
                        return $"{f.GetType().Name}({InnerPrint(f.Value)})";
                    case IUnaryExpression u:
                        return $"{u.GetType().Name}({InnerPrint(u.Value)})";
                    default:
                        return e.GetType().Name;
                }
            }
        }
    }
}

