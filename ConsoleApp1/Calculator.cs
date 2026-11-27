using System.Globalization;
using System.Text.RegularExpressions;

namespace ConsoleApp1;

public enum AngleMode
{
    Degrees,
    Radians
}

public class CalculatorException(string message) : Exception(message);

public partial class Calculator
{
    static readonly Dictionary<string, double> Constants = new(StringComparer.OrdinalIgnoreCase)
    {
        ["pi"] = Math.PI,
        ["e"] = Math.E,
        ["tau"] = Math.Tau
    };

    static readonly HashSet<string> Functions = new(StringComparer.OrdinalIgnoreCase)
    {
        "sin", "cos", "tan", "asin", "acos", "atan", "atan2", "sinh", "cosh", "tanh",
        "sqrt", "cbrt", "ln", "log", "log2", "exp", "abs", "floor", "ceil", "round", "trunc",
        "fact", "pow", "mod", "min", "max", "ncr", "npr", "deg", "rad"
    };

    readonly Dictionary<string, double> _variables = new(StringComparer.OrdinalIgnoreCase);

    public AngleMode AngleMode { get; set; } = AngleMode.Degrees;

    public double Ans { get; private set; }

    public IReadOnlyDictionary<string, double> Variables => _variables;

    public void ClearVariables()
    {
        _variables.Clear();
        Ans = 0;
    }

    [GeneratedRegex(@"^\s*([A-Za-z_][A-Za-z0-9_]*)\s*=(?!=)(.*)$")]
    private static partial Regex AssignmentRegex();

    public double Evaluate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new CalculatorException("수식을 입력하세요.");

        string? target = null;
        var expression = input;
        var match = AssignmentRegex().Match(input);
        if (match.Success)
        {
            target = match.Groups[1].Value;
            expression = match.Groups[2].Value;
            if (Constants.ContainsKey(target) || Functions.Contains(target) ||
                target.Equals("ans", StringComparison.OrdinalIgnoreCase))
                throw new CalculatorException($"'{target}'은(는) 예약된 이름이라 변수로 쓸 수 없습니다.");
        }

        var result = new Parser(expression, this).Parse();
        Ans = result;
        if (target is not null)
            _variables[target] = result;
        return result;
    }

    public static string Format(double value)
    {
        if (value == 0) return "0";
        if (Math.Abs(value) < 1e15 && value == Math.Floor(value))
            return value.ToString("F0", CultureInfo.InvariantCulture);
        return value.ToString("G14", CultureInfo.InvariantCulture);
    }

    static double Check(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new CalculatorException("계산할 수 없습니다. (정의되지 않음 또는 범위 초과)");
        return value;
    }

    static double Snap(double value) => Math.Abs(value) < 1e-14 ? 0 : value;

    double ToRadians(double x) => AngleMode == AngleMode.Degrees ? x * Math.PI / 180 : x;

    double FromRadians(double x) => AngleMode == AngleMode.Degrees ? x * 180 / Math.PI : x;

    double Lookup(string name)
    {
        if (name.Equals("ans", StringComparison.OrdinalIgnoreCase)) return Ans;
        if (Constants.TryGetValue(name, out var constant)) return constant;
        if (_variables.TryGetValue(name, out var variable)) return variable;
        throw new CalculatorException($"알 수 없는 이름: {name}");
    }

    static double Factorial(double x)
    {
        if (x < 0 || x != Math.Floor(x))
            throw new CalculatorException("팩토리얼은 0 이상의 정수에만 사용할 수 있습니다.");
        if (x > 170)
            throw new CalculatorException("팩토리얼 결과가 너무 큽니다. (최대 170!)");
        double result = 1;
        for (var i = 2; i <= (int)x; i++) result *= i;
        return result;
    }

    static void RequireIntegers(double n, double r)
    {
        if (n != Math.Floor(n) || r != Math.Floor(r) || n < 0 || r < 0 || r > n)
            throw new CalculatorException("n, r은 0 <= r <= n 인 정수여야 합니다.");
        if (n > 100000)
            throw new CalculatorException("n이 너무 큽니다. (최대 100000)");
    }

    double Call(string name, List<double> a)
    {
        var key = name.ToLowerInvariant();
        if (!Functions.Contains(key))
            throw new CalculatorException($"알 수 없는 함수: {name}");

        int expected = key switch
        {
            "atan2" or "pow" or "mod" or "min" or "max" or "ncr" or "npr" => 2,
            "log" or "round" => -1,
            _ => 1
        };
        if (expected == -1)
        {
            if (a.Count is < 1 or > 2)
                throw new CalculatorException($"{name}() 인수는 1개 또는 2개여야 합니다.");
        }
        else if (a.Count != expected)
        {
            throw new CalculatorException($"{name}() 인수는 {expected}개여야 합니다.");
        }

        var x = a[0];
        switch (key)
        {
            case "sin": return Snap(Math.Sin(ToRadians(x)));
            case "cos": return Snap(Math.Cos(ToRadians(x)));
            case "tan":
                {
                    var r = ToRadians(x);
                    if (Math.Abs(Math.Cos(r)) < 1e-15)
                        throw new CalculatorException("tan 값이 정의되지 않습니다.");
                    return Snap(Math.Tan(r));
                }
            case "asin":
            case "acos":
                if (x is < -1 or > 1)
                    throw new CalculatorException($"{name}()의 입력은 -1 ~ 1 범위여야 합니다.");
                return Snap(FromRadians(key == "asin" ? Math.Asin(x) : Math.Acos(x)));
            case "atan": return Snap(FromRadians(Math.Atan(x)));
            case "atan2": return Snap(FromRadians(Math.Atan2(x, a[1])));
            case "sinh": return Check(Math.Sinh(x));
            case "cosh": return Check(Math.Cosh(x));
            case "tanh": return Math.Tanh(x);
            case "sqrt":
                if (x < 0) throw new CalculatorException("음수의 제곱근은 계산할 수 없습니다.");
                return Math.Sqrt(x);
            case "cbrt": return Math.Cbrt(x);
            case "ln":
                if (x <= 0) throw new CalculatorException("ln()의 입력은 0보다 커야 합니다.");
                return Math.Log(x);
            case "log":
                {
                    if (x <= 0) throw new CalculatorException("log()의 입력은 0보다 커야 합니다.");
                    if (a.Count == 1) return Snap(Math.Log10(x));
                    var b = a[1];
                    if (b <= 0 || b == 1) throw new CalculatorException("log()의 밑은 0보다 크고 1이 아니어야 합니다.");
                    return Snap(Math.Log(x) / Math.Log(b));
                }
            case "log2":
                if (x <= 0) throw new CalculatorException("log2()의 입력은 0보다 커야 합니다.");
                return Math.Log2(x);
            case "exp": return Check(Math.Exp(x));
            case "abs": return Math.Abs(x);
            case "floor": return Math.Floor(x);
            case "ceil": return Math.Ceiling(x);
            case "trunc": return Math.Truncate(x);
            case "round":
                {
                    if (a.Count == 1) return Math.Round(x, MidpointRounding.AwayFromZero);
                    var digits = a[1];
                    if (digits != Math.Floor(digits) || digits is < 0 or > 15)
                        throw new CalculatorException("round()의 자릿수는 0~15 정수여야 합니다.");
                    return Math.Round(x, (int)digits, MidpointRounding.AwayFromZero);
                }
            case "fact": return Factorial(x);
            case "pow": return Check(Math.Pow(x, a[1]));
            case "mod":
                if (a[1] == 0) throw new CalculatorException("0으로 나눌 수 없습니다.");
                return x % a[1];
            case "min": return Math.Min(x, a[1]);
            case "max": return Math.Max(x, a[1]);
            case "ncr":
                {
                    RequireIntegers(x, a[1]);
                    var r = Math.Min(a[1], x - a[1]);
                    double result = 1;
                    for (var i = 1; i <= r; i++) result = result * (x - r + i) / i;
                    return Check(Math.Round(result));
                }
            case "npr":
                {
                    RequireIntegers(x, a[1]);
                    double result = 1;
                    for (var i = 0; i < a[1]; i++) result *= x - i;
                    return Check(result);
                }
            case "deg": return x * 180 / Math.PI;
            case "rad": return x * Math.PI / 180;
            default: throw new CalculatorException($"알 수 없는 함수: {name}");
        }
    }

    sealed class Parser(string text, Calculator calc)
    {
        int _pos;

        public double Parse()
        {
            var value = Expression();
            if (Peek() != '\0')
                throw new CalculatorException($"예상치 못한 문자: '{text[_pos]}'");
            return Check(value);
        }

        char Peek()
        {
            while (_pos < text.Length && char.IsWhiteSpace(text[_pos])) _pos++;
            return _pos < text.Length ? text[_pos] : '\0';
        }

        bool Match(char ch)
        {
            if (Peek() != ch) return false;
            _pos++;
            return true;
        }

        double Expression()
        {
            var value = Term();
            while (true)
            {
                if (Match('+')) value = Check(value + Term());
                else if (Match('-')) value = Check(value - Term());
                else return value;
            }
        }

        double Term()
        {
            var value = Unary();
            while (true)
            {
                if (Match('*'))
                {
                    value = Check(value * Unary());
                }
                else if (Match('/'))
                {
                    var divisor = Unary();
                    if (divisor == 0) throw new CalculatorException("0으로 나눌 수 없습니다.");
                    value = Check(value / divisor);
                }
                else if (Match('%'))
                {
                    var divisor = Unary();
                    if (divisor == 0) throw new CalculatorException("0으로 나눌 수 없습니다.");
                    value %= divisor;
                }
                else if (Peek() == '(' || char.IsAsciiLetter(Peek()) || Peek() == '_')
                {
                    value = Check(value * Unary());
                }
                else return value;
            }
        }

        double Unary()
        {
            if (Match('-')) return -Unary();
            if (Match('+')) return Unary();
            return Power();
        }

        double Power()
        {
            var baseValue = Postfix();
            if (!Match('^')) return baseValue;
            var exponent = Unary();
            return Check(Math.Pow(baseValue, exponent));
        }

        double Postfix()
        {
            var value = Primary();
            while (Match('!')) value = Factorial(value);
            return value;
        }

        double Primary()
        {
            var ch = Peek();
            if (ch == '\0')
                throw new CalculatorException("수식이 불완전합니다.");

            if (ch == '(')
            {
                _pos++;
                var inner = Expression();
                if (!Match(')')) throw new CalculatorException("')'가 필요합니다.");
                return inner;
            }

            if (char.IsAsciiDigit(ch) || ch == '.')
                return Number();

            if (char.IsAsciiLetter(ch) || ch == '_')
                return Identifier();

            throw new CalculatorException($"예상치 못한 문자: '{ch}'");
        }

        double Number()
        {
            var start = _pos;
            while (_pos < text.Length && char.IsAsciiDigit(text[_pos])) _pos++;
            if (_pos < text.Length && text[_pos] == '.')
            {
                _pos++;
                while (_pos < text.Length && char.IsAsciiDigit(text[_pos])) _pos++;
            }

            if (_pos < text.Length && (text[_pos] == 'e' || text[_pos] == 'E'))
            {
                var look = _pos + 1;
                if (look < text.Length && (text[look] == '+' || text[look] == '-')) look++;
                if (look < text.Length && char.IsAsciiDigit(text[look]))
                {
                    _pos = look;
                    while (_pos < text.Length && char.IsAsciiDigit(text[_pos])) _pos++;
                }
            }

            var token = text[start.._pos];
            if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                throw new CalculatorException($"올바르지 않은 숫자: {token}");
            return Check(value);
        }

        double Identifier()
        {
            var start = _pos;
            while (_pos < text.Length && (char.IsAsciiLetterOrDigit(text[_pos]) || text[_pos] == '_')) _pos++;
            var name = text[start.._pos];

            if (Peek() == '(')
            {
                _pos++;
                var args = new List<double>();
                if (!Match(')'))
                {
                    do { args.Add(Expression()); } while (Match(','));
                    if (!Match(')')) throw new CalculatorException("')'가 필요합니다.");
                }
                return Check(calc.Call(name, args));
            }

            return calc.Lookup(name);
        }
    }
}
