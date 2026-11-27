using ConsoleApp1;

namespace ConsoleApp1.Tests;

public class CalculatorTests
{
    static double Eval(string expr, AngleMode mode = AngleMode.Degrees)
        => new Calculator { AngleMode = mode }.Evaluate(expr);

    static void AssertError(string expr, AngleMode mode = AngleMode.Degrees)
        => Assert.Throws<CalculatorException>(() => Eval(expr, mode));

    [Theory]
    [InlineData("1+2", 3)]
    [InlineData("10-4", 6)]
    [InlineData("6*7", 42)]
    [InlineData("8/2", 4)]
    [InlineData("1/3*3", 1)]
    [InlineData("0.1+0.2", 0.3)]
    [InlineData("  2  +  3  ", 5)]
    [InlineData("-5*-2", 10)]
    [InlineData("--3", 3)]
    [InlineData("1.5e3", 1500)]
    [InlineData("2E-2", 0.02)]
    [InlineData(".5+.5", 1)]
    public void BasicArithmetic(string expr, double expected)
        => Assert.Equal(expected, Eval(expr), 10);

    [Theory]
    [InlineData("2+3*4", 14)]
    [InlineData("(2+3)*4", 20)]
    [InlineData("2^3^2", 512)]
    [InlineData("-2^2", -4)]
    [InlineData("2^-1", 0.5)]
    [InlineData("10%3", 1)]
    [InlineData("10-2-3", 5)]
    [InlineData("100/10/5", 2)]
    [InlineData("((((1+1))))", 2)]
    [InlineData("2*(3+4)^2", 98)]
    public void PrecedenceAndAssociativity(string expr, double expected)
        => Assert.Equal(expected, Eval(expr), 10);

    [Theory]
    [InlineData("sin(30)", 0.5)]
    [InlineData("sin(90)", 1)]
    [InlineData("sin(180)", 0)]
    [InlineData("cos(60)", 0.5)]
    [InlineData("cos(90)", 0)]
    [InlineData("tan(45)", 1)]
    [InlineData("asin(1)", 90)]
    [InlineData("acos(0)", 90)]
    [InlineData("atan(1)", 45)]
    [InlineData("atan2(1,1)", 45)]
    [InlineData("sinh(0)", 0)]
    [InlineData("cosh(0)", 1)]
    [InlineData("tanh(0)", 0)]
    public void TrigonometryInDegrees(string expr, double expected)
        => Assert.Equal(expected, Eval(expr), 10);

    [Theory]
    [InlineData("sin(pi/2)", 1)]
    [InlineData("cos(pi)", -1)]
    [InlineData("tan(pi/4)", 1)]
    [InlineData("asin(1)", Math.PI / 2)]
    [InlineData("atan(1)", Math.PI / 4)]
    public void TrigonometryInRadians(string expr, double expected)
        => Assert.Equal(expected, Eval(expr, AngleMode.Radians), 10);

    [Theory]
    [InlineData("sqrt(16)", 4)]
    [InlineData("cbrt(27)", 3)]
    [InlineData("cbrt(-8)", -2)]
    [InlineData("exp(0)", 1)]
    [InlineData("exp(1)", Math.E)]
    [InlineData("ln(e)", 1)]
    [InlineData("log(1000)", 3)]
    [InlineData("log(8,2)", 3)]
    [InlineData("log2(1024)", 10)]
    [InlineData("pow(2,10)", 1024)]
    [InlineData("2^0.5", 1.4142135623730951)]
    public void PowersRootsLogs(string expr, double expected)
        => Assert.Equal(expected, Eval(expr), 10);

    [Theory]
    [InlineData("abs(-3.5)", 3.5)]
    [InlineData("floor(2.7)", 2)]
    [InlineData("floor(-2.1)", -3)]
    [InlineData("ceil(2.1)", 3)]
    [InlineData("trunc(-2.9)", -2)]
    [InlineData("round(2.5)", 3)]
    [InlineData("round(3.14159,2)", 3.14)]
    [InlineData("mod(10,4)", 2)]
    [InlineData("min(3,7)", 3)]
    [InlineData("max(3,7)", 7)]
    [InlineData("deg(pi)", 180)]
    [InlineData("rad(180)", Math.PI)]
    public void MiscFunctions(string expr, double expected)
        => Assert.Equal(expected, Eval(expr), 10);

    [Theory]
    [InlineData("0!", 1)]
    [InlineData("5!", 120)]
    [InlineData("fact(10)", 3628800)]
    [InlineData("3!!", 720)]
    [InlineData("2*3!", 12)]
    [InlineData("ncr(10,3)", 120)]
    [InlineData("ncr(5,0)", 1)]
    [InlineData("ncr(5,5)", 1)]
    [InlineData("npr(5,2)", 20)]
    [InlineData("fact(170)", 7.257415615307994e306)]
    public void CombinatoricsAndFactorial(string expr, double expected)
        => Assert.Equal(1.0, Eval(expr) / expected, 10);

    [Theory]
    [InlineData("pi", Math.PI)]
    [InlineData("PI", Math.PI)]
    [InlineData("e", Math.E)]
    [InlineData("tau", Math.Tau)]
    [InlineData("2pi", 2 * Math.PI)]
    [InlineData("2(3+4)", 14)]
    [InlineData("sin(30)cos(60)", 0.25)]
    [InlineData("SIN(30)", 0.5)]
    [InlineData("sin (30)", 0.5)]
    public void ConstantsAndImplicitMultiplication(string expr, double expected)
        => Assert.Equal(expected, Eval(expr), 10);

    [Theory]
    [InlineData("1/0")]
    [InlineData("5%0")]
    [InlineData("mod(5,0)")]
    [InlineData("0^-1")]
    [InlineData("(-8)^0.5")]
    [InlineData("sqrt(-1)")]
    [InlineData("ln(0)")]
    [InlineData("ln(-1)")]
    [InlineData("log(0)")]
    [InlineData("log2(-4)")]
    [InlineData("log(8,1)")]
    [InlineData("log(8,-2)")]
    [InlineData("asin(2)")]
    [InlineData("acos(-1.5)")]
    [InlineData("tan(90)")]
    [InlineData("tan(270)")]
    [InlineData("(-1)!")]
    [InlineData("2.5!")]
    [InlineData("fact(171)")]
    [InlineData("ncr(3,5)")]
    [InlineData("ncr(3.5,1)")]
    [InlineData("npr(-1,0)")]
    [InlineData("ncr(1000000,2)")]
    [InlineData("1e308*10")]
    [InlineData("1e999")]
    [InlineData("exp(1000)")]
    [InlineData("10^400")]
    [InlineData("sinh(1000)")]
    public void DomainAndOverflowErrors(string expr) => AssertError(expr);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1+")]
    [InlineData("*3")]
    [InlineData("(1+2")]
    [InlineData("1+2)")]
    [InlineData("()")]
    [InlineData("1 2")]
    [InlineData("1..2")]
    [InlineData("1.2.3")]
    [InlineData("abc")]
    [InlineData("foo(1)")]
    [InlineData("sin()")]
    [InlineData("sin(1,2)")]
    [InlineData("pow(1)")]
    [InlineData("pow(1,2,3)")]
    [InlineData("log(1,2,3)")]
    [InlineData("round(1.5,2.5)")]
    [InlineData("round(1.5,20)")]
    [InlineData("sin(30")]
    [InlineData("3 $ 4")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("1,5+1")]
    [InlineData("pi(2)")]
    [InlineData("2^")]
    [InlineData("!")]
    public void SyntaxErrors(string expr) => AssertError(expr);

    [Fact]
    public void AnsStoresLastResult()
    {
        var calc = new Calculator();
        Assert.Equal(0, calc.Evaluate("ans"));
        calc.Evaluate("6*7");
        Assert.Equal(42, calc.Evaluate("ans"));
        Assert.Equal(84, calc.Evaluate("ans*2"));
    }

    [Fact]
    public void FailedEvaluationKeepsPreviousAns()
    {
        var calc = new Calculator();
        calc.Evaluate("5");
        Assert.Throws<CalculatorException>(() => calc.Evaluate("1/0"));
        Assert.Equal(5, calc.Ans);
    }

    [Fact]
    public void VariablesCanBeAssignedAndUsed()
    {
        var calc = new Calculator();
        Assert.Equal(12, calc.Evaluate("x = 3*4"));
        Assert.Equal(144, calc.Evaluate("x^2"));
        Assert.Equal(12, calc.Evaluate("X"));
        Assert.Equal(24, calc.Evaluate("y=x*2"));
        Assert.Equal(36, calc.Evaluate("x+y"));
        Assert.Equal(2, calc.Variables.Count);
    }

    [Fact]
    public void FailedAssignmentDoesNotCreateVariable()
    {
        var calc = new Calculator();
        Assert.Throws<CalculatorException>(() => calc.Evaluate("z = 1/0"));
        Assert.Throws<CalculatorException>(() => calc.Evaluate("z"));
    }

    [Theory]
    [InlineData("pi = 3")]
    [InlineData("e = 1")]
    [InlineData("sin = 1")]
    [InlineData("ans = 1")]
    public void ReservedNamesCannotBeAssigned(string expr)
        => AssertError(expr);

    [Fact]
    public void UnknownVariableIsAnError() => AssertError("undefined_var + 1");

    [Fact]
    public void ClearVariablesResetsState()
    {
        var calc = new Calculator();
        calc.Evaluate("a = 5");
        calc.ClearVariables();
        Assert.Empty(calc.Variables);
        Assert.Equal(0, calc.Ans);
        Assert.Throws<CalculatorException>(() => calc.Evaluate("a"));
    }

    [Fact]
    public void AngleModeSwitchChangesTrigResults()
    {
        var calc = new Calculator();
        Assert.Equal(0.5, calc.Evaluate("sin(30)"), 10);
        calc.AngleMode = AngleMode.Radians;
        Assert.Equal(Math.Sin(30), calc.Evaluate("sin(30)"), 10);
    }

    [Theory]
    [InlineData(0, "0")]
    [InlineData(-0.0, "0")]
    [InlineData(3, "3")]
    [InlineData(-42, "-42")]
    [InlineData(0.3, "0.3")]
    [InlineData(0.1 + 0.2, "0.3")]
    [InlineData(1099511627776, "1099511627776")]
    [InlineData(1e20, "1E+20")]
    [InlineData(1.5e-7, "1.5E-07")]
    [InlineData(1.0 / 3, "0.33333333333333")]
    public void FormatsNumbers(double value, string expected)
        => Assert.Equal(expected, Calculator.Format(value));

    [Fact]
    public void LargeButValidExpressionsWork()
    {
        Assert.Equal(1.0, Eval("1e150*1e150") / 1e300, 10);
        Assert.Equal(0, Eval("1e-300*1e-300"));
        var deep = new string('(', 200) + "1" + new string(')', 200);
        Assert.Equal(1, Eval(deep));
    }
}
