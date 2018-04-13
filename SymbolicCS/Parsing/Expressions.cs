namespace SymbolicCS.Parsing
{
    public interface IExpression { }
    public interface IBinaryExpression : IExpression { IExpression Left { get; } IExpression Right { get; } }
    public interface IFuncExpression : IExpression { IExpression Value { get; } }
    public interface IUnaryExpression : IExpression { IExpression Value { get; } }
    class Var : IExpression { public Var(char n){ Name = n; } public char Name { get; } }
    class Num : IExpression { public double Value { get; } public Num(double v) { Value = v; } }
    class Neg : IUnaryExpression { public IExpression Value { get; } public Neg(IExpression ex) { Value = ex; } }
    class Add : IBinaryExpression { public IExpression Left { get; } public IExpression Right { get; } public Add(IExpression ex1, IExpression ex2) { Left = ex1; Right = ex2; } }
    class Sub : IBinaryExpression { public IExpression Left { get; } public IExpression Right { get; } public Sub(IExpression ex1, IExpression ex2) { Left = ex1; Right = ex2; } }
    class Mul : IBinaryExpression { public IExpression Left { get; } public IExpression Right { get; } public Mul(IExpression ex1, IExpression ex2) { Left = ex1; Right = ex2; } }
    class Div : IBinaryExpression { public IExpression Left { get; } public IExpression Right { get; } public Div(IExpression ex1, IExpression ex2) { Left = ex1; Right = ex2; } }
    class Pow : IBinaryExpression { public IExpression Left { get; } public IExpression Right { get; } public Pow(IExpression ex1, IExpression ex2) { Left = ex1; Right = ex2; } }
    class Log : IFuncExpression { public IExpression Value { get; } public Log(IExpression ex) { Value = ex ;} }
    class Exp : IFuncExpression { public IExpression Value { get; } public Exp(IExpression ex) { Value = ex; } }
    class Sin : IFuncExpression { public IExpression Value { get; } public Sin(IExpression ex) { Value = ex; } }
    class Cos : IFuncExpression { public IExpression Value { get; } public Cos(IExpression ex) { Value = ex; } }
    class Der : IFuncExpression { public IExpression Value { get; } public Der(IExpression ex) { Value = ex; } }
}
