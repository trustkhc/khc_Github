using System.Globalization;

Console.WriteLine("4칙 연산 계산기 (종료: q)");

while (true)
{
    Console.Write("첫 번째 숫자: ");
    var input1 = Console.ReadLine();
    if (input1 is null || input1.Trim().ToLower() == "q") break;

    Console.Write("연산자 (+, -, *, /): ");
    var op = Console.ReadLine()?.Trim();

    Console.Write("두 번째 숫자: ");
    var input2 = Console.ReadLine();
    if (input2 is null || input2.Trim().ToLower() == "q") break;

    const NumberStyles style = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
    if (!decimal.TryParse(input1.Trim(), style, CultureInfo.InvariantCulture, out var a) ||
        !decimal.TryParse(input2.Trim(), style, CultureInfo.InvariantCulture, out var b))
    {
        Console.WriteLine("올바른 숫자를 입력하세요.\n");
        continue;
    }

    try
    {
        switch (op)
        {
            case "+": Console.WriteLine($"결과: {a + b}\n"); break;
            case "-": Console.WriteLine($"결과: {a - b}\n"); break;
            case "*": Console.WriteLine($"결과: {a * b}\n"); break;
            case "/":
                if (b == 0) Console.WriteLine("0으로 나눌 수 없습니다.\n");
                else Console.WriteLine($"결과: {a / b}\n");
                break;
            default:
                Console.WriteLine("지원하지 않는 연산자입니다.\n");
                break;
        }
    }
    catch (OverflowException)
    {
        Console.WriteLine("계산 결과가 너무 큽니다.\n");
    }
}
