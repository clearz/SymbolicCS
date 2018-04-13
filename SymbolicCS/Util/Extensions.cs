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

        // Generate a text based representation of the expression tree
        public static void DisplayTree(this IExpression ans, Action<object> output = null)
        {
            output = output ?? Console.Write;
            var generator = new TreeGenerator(); // -(-(3*Log((-5 + -3*2)-6*-(-Sin(-5/3) + Cos(-3/2))---6/4*a)))
            var root = BuildTree(ans);
            string text = generator.CreateTree(root);
            output(text);


            TextNode BuildTree(IExpression expr)
            {
                var lbl = GetLabel(expr);
                if (expr is IBinaryExpression be)
                    return new TextNode(lbl, BuildTree(be.Left), BuildTree(be.Right));
                if (expr is Neg nx)
                    return new TextNode(lbl, left: BuildTree(nx.Value));
                if (expr is IFuncExpression fx)
                    return new TextNode(lbl, left: BuildTree(fx.Value));

                return new TextNode(lbl);

            }

            // Get a string representation of an IExpression
            string GetLabel(IExpression e)
            {
                switch (e)
                {
                    case Num n:
                        return n.Value.ToString(CultureInfo.CurrentCulture);
                    case Var v:
                        return v.Name.ToString();
                    default:
                        return e.GetType().Name;
                }
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
                        return System.Math.Round(n.Value, 2);
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

