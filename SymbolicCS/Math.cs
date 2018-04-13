using System;
using System.Diagnostics;
using SymbolicCS.Parsing;
using SymbolicCS.Util;
using static System.Math; // Used in HandleOp(IFuncExpression op)

namespace SymbolicCS
{
    static class Math
    {
        // Set of rules to simplify expression
        public static IExpression Simplify(this IExpression x) // 1+2+3+x+4+5,   1+(2*4)*-(5/3)^2^(1-0)
        {
            switch (x)
            {
                case Add a when a.Left is Num n1 && a.Right is Num n2: // Add(Num, Num) -> Num
                    return new Num(n1.Value + n2.Value);
                case Sub s when s.Left is Num n1 && s.Right is Num n2: // Sub(Num, Num) -> Num
                    return new Num(n1.Value - n2.Value);
                case Mul m when m.Left is Num n1 && m.Right is Num n2: // Mul(Num, Num) -> Num
                    return new Num(n1.Value * n2.Value);
                case Div d when d.Left is Num n1 && d.Right is Num n2: // Div(Num, Num) -> Num
                    return new Num(n1.Value / n2.Value);
                case Pow p when p.Left is Num n1 && p.Right is Num n2: // Pow(Num, Num) -> Num
                    return new Num(Pow(n1.Value,n2.Value));
                case Neg n when n.Value is Num n2 && n2.Value == 0:    // Neg(0) -> 0
                    return n2;
                case Neg n when n.Value is Num n2:                     // Neg(Num(?)) -> Num(-?)
                    return new Num(-n2.Value);
                case Neg n when n.Value is Neg n2:                     // Neg(Neg(?)) -> ?
                    return n2.Value.Simplify();
                case Add a when a.Right is Num n && n.Value == 0:      // Add(?, 0) -> ?
                    return a.Left.Simplify();
                case Add a when a.Left is Num n && n.Value == 0:       // Add(0, ?) -> ?
                    return a.Right.Simplify();
                case Add a when a.Left is Num n:                       // Add(Num, ?) -> Add(? Num)
                    return new Add(a.Right, n).Simplify();              
                case Add a when a.Right is Neg n:                      // Add(?1 Neg(?2)) -> Sub(?1, ?2)
                    return new Sub(a.Left, n.Value).Simplify();        
                case Add a when a.Left is Neg n:                       // Add(Neg(?1), ?2) -> Sub(?2, ?1)
                    return new Sub(a.Right, n.Value).Simplify();
                case Sub s when s.Right is Num n && n.Value == 0:      // Sub(?, 0) -> ?
                    return s.Left.Simplify();
                case Sub s when s.Left is Num n && n.Value == 0:       // Sub(0, ?) -> Neg(?)
                    return new Neg(s.Right).Simplify();
                case Mul m when m.Right is Num n && n.Value == 1:      // Mul(?, 1) -> ? 
                    return m.Left.Simplify();
                case Mul m when m.Left is Num n && n.Value == 1:       // Mul(1, ?) -> ? 
                    return m.Right.Simplify();
                case Mul m when m.Right is Num n && n.Value == 0:      // Mul(?, 0) -> 0 
                    return new Num(0);
                case Mul m when m.Left is Num n && n.Value == 0:       // Mul(0, ?) -> 0 
                    return new Num(0);
                case Mul m when m.Right is Num n:                      // Mul(?, Num) -> Mul(Num, ?)
                    return new Mul(n, m.Left).Simplify();
                case Mul m when m.Left is Div d && d.Left is Num n:    // Mul(Div(Num, ?1), ?2) -> Mul(Num, Div(?2, ?1)) 
                    return new Mul(n, new Div(m.Right, d.Right)).Simplify();
                case Mul m when m.Right is Div d && d.Left is Num n:   // Mul(?1, Div(Num, ?2)) -> ? Mul(Num, Div(?1, ?2)) 
                    return new Mul(n, new Div(m.Left, d.Right)).Simplify();
                case Mul m when m.Left is Neg n:                       // Mul(Neg(?1), ?2) -> Neg(Mul(?1, ?2))
                    return new Neg(new Mul(n.Value, m.Right)).Simplify();
                case Mul m when m.Right is Neg n:                      // Mul(?1, Neg(?2) -> Neg(Mul(?1, ?2))
                    return new Neg(new Mul(m.Left, n.Value)).Simplify();
                case Div d when d.Left is Num n && n.Value == 0:       // Div(0, ?) -> 0
                    return new Num(0).Simplify();
                case Div d when d.Right is Num n && n.Value == 1:      // Div(?, 1) -> ?
                    return d.Left.Simplify();
                case Div d when d.Left is Neg n:                       // Div(Neg(?1), ?2) -> Neg(Div(?1, ?2))
                    return new Neg(new Div(n.Value, d.Right)).Simplify();
                case Div d when d.Right is Neg n:                      // Div(?1, Neg(?2)) -> Neg(Div(?1, ?2))
                    return new Neg(new Div(d.Left, n.Value)).Simplify();
                case Pow p when p.Left is Num n && n.Value == 0:       // Pow(0, ?) ->  0
                    return new Num(0);
                case Pow p when p.Left is Num n && n.Value == 1:       // Pow(1, ?) ->  1
                    return new Num(1);
                case Pow p when p.Right is Num n && n.Value == 0:      // Pow(?, 0) ->  1
                    return new Num(1);
                case Pow p when p.Right is Num n && n.Value == 1:      // Pow(?, 1) ->  ?
                    return p.Left.Simplify();
                case IFuncExpression op:
                    return HandleOp(op);
                case IBinaryExpression op:
                    return HandleOp(op);
                case IUnaryExpression op:
                    return HandleOp(op);
                default:
                    return x;
            }
          
        }

        private static IExpression HandleOp(IBinaryExpression op)
        {
            var e1 = op.Left;
            var e2 = op.Right;
            var e1s = e1.Simplify();
            var e2s = e2.Simplify();
            if (e1s != e1 || e2s != e2) // if op.Left or op.Right can been simplified then try and simplify further
                switch (op)
                {
                    case Add _:
                        return new Add(e1s, e2s).Simplify();
                    case Sub _:
                        return new Sub(e1s, e2s).Simplify();
                    case Mul _:
                        return new Mul(e1s, e2s).Simplify();
                    case Div _:
                        return new Div(e1s, e2s).Simplify();
                    case Pow _:
                        return new Pow(e1s, e2s).Simplify();    
                }


            return op; // No furher simplification possible so just return op.
        }



        private static IExpression HandleOp(IFuncExpression op)
        {
            var e1 = op.Value;
            var e1s = e1.Simplify();
            if (e1s != e1)
                switch (op) // If e1s is a number then preform System.Math.[op] on value else continue simplifying.
                {
                    case Exp _:
                        return e1s is Num n1 
                            ? new Num(Exp(n1.Value)) 
                            : new Exp(e1s).Simplify();
                    case Log _:
                        return e1s is Num n2 
                            ? new Num(Log(n2.Value)) 
                            : new Log(e1s).Simplify();
                    case Sin _:
                        return e1s is Num n3 
                            ? new Num(Sin(n3.Value)) 
                            : new Sin(e1s).Simplify();
                    case Cos _:
                        return e1s is Num n4 
                            ? new Num(Cos(n4.Value)) 
                            : new Cos(e1s).Simplify();
                }


            return op;
        }

        private static IExpression HandleOp(IUnaryExpression op)
        {
            var e1 = op.Value;
            var e1s = e1.Simplify();
            if (e1s != e1)
                switch (op)
                {
                    case Neg a:
                        return new Neg(e1s).Simplify();
                }


            return op;
        }
    }
}
