using System;
using System.Diagnostics;
using SymbolicCS.Parsing;
using SymbolicCS.Util;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace SymbolicCS
{
    static class Maths
    {
        // Set of rules to simplify expression
        public static IExpression Simplify(this IExpression x) // 1+2+3+x+4+5,   1+(2*4)*-(5/3)^2^(1-0)
        {
            if (!(x is Num) && !(x is Var))
            {
                x.Print(o => Debug.Write(o));
                x.DisplayTree(o => Debug.Write(o));
            }
            else return x;

            switch (x)
            {
                case Add a when a.Left is Num n1 && a.Right is Num n2: // Add(Num, Num) -> Num
                    return Num(n1.Value + n2.Value);
                case Sub s when s.Left is Num n1 && s.Right is Num n2: // Sub(Num, Num) -> Num
                    return Num(n1.Value - n2.Value);
                case Mul m when m.Left is Num n1 && m.Right is Num n2: // Mul(Num, Num) -> Num
                    return Num(n1.Value * n2.Value);
                case Div d when d.Left is Num n1 && d.Right is Num n2: // Div(Num, Num) -> Num
                    return Num(n1.Value / n2.Value);
                case Pow p when p.Left is Num n1 && p.Right is Num n2: // Pow(Num, Num) -> Num
                    return Num(Math.Pow(n1.Value,n2.Value));
                case Neg n when n.Value is Num n2 && n2.Value == 0:    // Neg(0) -> 0
                    return n2;
                case Neg n when n.Value is Num n2:                     // Neg(Num(?)) -> Num(-?)
                    return Num(-n2.Value);
                case Neg n when n.Value is Neg n2:                     // Neg(Neg(?)) -> ?
                    return n2.Value.Simplify();
                case Add a when a.Left is Num n:                       // Add(Num, ?) -> Add(? Num)
                    return Add(a.Right, n).Simplify();
                case Add a when a.Right is Num n && n.Value == 0:      // Add(0, ?) -> ?
                    return a.Left.Simplify();
                case Add s when s.Right is Num n && n.Value < 0:       // Sub(?1, -?2) -> Add(?1, ?2)
                    return Sub(s.Left, new Num(-n.Value)).Simplify();
                case Add a when a.Right is Neg n:                      // Add(?1 Neg(?2)) -> Sub(?1, ?2)
                    return Sub(a.Left, n.Value).Simplify();        
                case Add a when a.Left is Neg n:                       // Add(Neg(?1), ?2) -> Sub(?2, ?1)
                    return Sub(a.Right, n.Value).Simplify();
                case Sub s when s.Right is Num n && n.Value == 0:      // Sub(?, 0) -> ?
                    return s.Left.Simplify();
                case Sub s when s.Left is Num n && n.Value == 0:       // Sub(0, ?) -> Neg(?)
                    return Neg(s.Right).Simplify();
                case Sub s when s.Right is Num n && n.Value < 0:       // Sub(?1, -?2) -> Add(?1, ?2)
                    return Add(s.Left, new Num(-n.Value)).Simplify();
                case Mul m when m.Left is Num n:                       // Mul(Num, ?) -> Mul(?, Num)
                    return Mul(m.Right, n).Simplify();
                case Mul m when m.Right is Num n && n.Value == 1:      // Mul(?, 1) -> ? 
                    return m.Left.Simplify();
                case Mul m when m.Right is Num n && n.Value == 0:      // Mul(?, 0) -> 0 
                    return Num(0);
                case Mul m when m.Left is Div d && d.Left is Num n:    // Mul(Div(Num, ?1), ?2) -> Mul(Num, Div(?2, ?1)) 
                    return Mul(n, Div(m.Right, d.Right)).Simplify();
                case Mul m when m.Right is Div d && d.Left is Num n:   // Mul(?1, Div(Num, ?2)) -> ? Mul(Num, Div(?1, ?2)) 
                    return Mul(n, Div(m.Left, d.Right)).Simplify();
                case Mul m when m.Left is Neg n:                       // Mul(Neg(?1), ?2) -> Neg(Mul(?1, ?2))
                    return Neg(Mul(n.Value, m.Right)).Simplify();
                case Mul m when m.Right is Neg n:                      // Mul(?1, Neg(?2) -> Neg(Mul(?1, ?2))
                    return Neg(Mul(m.Left, n.Value)).Simplify();
                case Div d when d.Left is Num n && n.Value == 0:       // Div(0, ?) -> 0
                    return Num(0).Simplify();
                case Div d when d.Right is Num n && n.Value == 1:      // Div(?, 1) -> ?
                    return d.Left.Simplify();
                case Div d when d.Left is Neg n:                       // Div(Neg(?1), ?2) -> Neg(Div(?1, ?2))
                    return Neg(Div(n.Value, d.Right)).Simplify();
                case Div d when d.Right is Neg n:                      // Div(?1, Neg(?2)) -> Neg(Div(?1, ?2))
                    return Neg(Div(d.Left, n.Value)).Simplify();
                case Pow p when p.Left is Num n && n.Value == 0:       // Pow(0, ?) ->  0
                    return Num(0);
                case Pow p when p.Left is Num n && n.Value == 1:       // Pow(1, ?) ->  1
                    return Num(1);
                case Pow p when p.Right is Num n && n.Value == 0:      // Pow(?, 0) ->  1
                    return Num(1);
                case Pow p when p.Right is Num n && n.Value == 1:      // Pow(?, 1) ->  ?
                    return p.Left.Simplify();
#region **Expremental Rules**
                case Add a when a.Left is Add a2 && a2.Left is Var v:  // Add(Add(Var, ?1), ?2) -> Add(Var, Add(?1, ?2))
                    return Add(Add(a.Right, a2.Right), v).Simplify();
                case Mul a when a.Left is Mul a2 && a2.Left is Var v:  // Mul(Mul(Var, ?1), ?2) -> Mul(Var, Mul(?1, ?2))
                    return Mul(new Mul(a.Right, a2.Right), v).Simplify();
                case Add a when a.Left is Sub a2 && !(a2.Left is Num && a2.Right is Num):                      // Add(Sub(?1, ?2), ?3) -> Sub(Sub(?1, ?3), ?2)
                    return new Sub(new Add(a2.Left, a.Right), a2.Right).Simplify();
                case Sub a when a.Left is Sub a2:                      // Sub(Sub(?1, ?2), ?3) -> Sub(?1, Add(?2, ?3))
                    return new Sub(a2.Left, new Add(a2.Right, a.Right)).Simplify();
                case Sub s when s.Left is Add a && !(a.Left is Num && a.Right is Num):
                    return new Sub(a.Left, new Sub(s.Right, a.Right)).Simplify();
                //   case Sub s when s.Right is Sub a:
                //        return new Sub(a.Left, new Sub(s.Right, a.Right)).Simplify();
#endregion
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
            // if op.Left or op.Right can been simplified then try and simplify further
            if (e1s != e1 || e2s != e2)
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

            // No furher simplification possible so just return op.
            return op; 
        }



        private static IExpression HandleOp(IFuncExpression op)
        {
            var e1 = op.Value;
            var e1s = e1.Simplify();
            // If e1s is a Num then preform System.Math.[op] on value else continue simplifying.
            if (e1s != e1)
                switch (op) 
                {
                    case Exp _:
                        return e1s is Num n1 
                            ? new Num(Math.Exp(n1.Value)) 
                            : new Exp(e1s).Simplify();
                    case Log _:
                        return e1s is Num n2 
                            ? new Num(Math.Log(n2.Value)) 
                            : new Log(e1s).Simplify();
                    case Sin _:
                        return e1s is Num n3 
                            ? new Num(Math.Sin(n3.Value)) 
                            : new Sin(e1s).Simplify();
                    case Cos _:
                        return e1s is Num n4 
                            ? new Num(Math.Cos(n4.Value)) 
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
                    case Neg _:
                        return new Neg(e1s).Simplify();
                }


            return op;
        }

        #region HelperMethods;
        public static IExpression Add(IExpression e1, IExpression e2) => new Add(e1, e2);
        public static IExpression Sub(IExpression e1, IExpression e2) => new Sub(e1, e2);
        public static IExpression Mul(IExpression e1, IExpression e2) => new Mul(e1, e2);
        public static IExpression Div(IExpression e1, IExpression e2) => new Div(e1, e2);
        public static IExpression Pow(IExpression e1, IExpression e2) => new Pow(e1, e2);
        public static IExpression Neg(IExpression e)=> new Neg(e);
        public static IExpression Exp(IExpression e)=> new Exp(e);
        public static IExpression Log(IExpression e)=> new Log(e);
        public static IExpression Sin(IExpression e)=> new Sin(e);
        public static IExpression Cos(IExpression e)=> new Cos(e);
        public static IExpression Num(double d) => new Num(d);
        #endregion

    }
}
