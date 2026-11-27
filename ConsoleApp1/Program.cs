using System.Text;
using ConsoleApp1;

Console.OutputEncoding = Encoding.UTF8;

var calculator = new Calculator();

PrintHelp();

while (true)
{
    Console.Write($"[{(calculator.AngleMode == AngleMode.Degrees ? "DEG" : "RAD")}] > ");
    var line = Console.ReadLine();
    if (line is null) break;

    var command = line.Trim().ToLowerInvariant();
    switch (command)
    {
        case "":
            continue;
        case "q" or "quit" or "exit":
            return;
        case "help" or "?":
            PrintHelp();
            continue;
        case "deg":
            calculator.AngleMode = AngleMode.Degrees;
            Console.WriteLine("각도 모드: 도(DEG)");
            continue;
        case "rad":
            calculator.AngleMode = AngleMode.Radians;
            Console.WriteLine("각도 모드: 라디안(RAD)");
            continue;
        case "vars":
            if (calculator.Variables.Count == 0) Console.WriteLine("저장된 변수가 없습니다.");
            foreach (var (name, value) in calculator.Variables)
                Console.WriteLine($"{name} = {Calculator.Format(value)}");
            continue;
        case "clear":
            calculator.ClearVariables();
            Console.WriteLine("변수와 ans를 초기화했습니다.");
            continue;
    }

    try
    {
        Console.WriteLine($"= {Calculator.Format(calculator.Evaluate(line))}");
    }
    catch (CalculatorException ex)
    {
        Console.WriteLine($"오류: {ex.Message}");
    }
}

static void PrintHelp()
{
    Console.WriteLine("""
        === 공학용 계산기 ===
        연산자   : + - * / % ^(거듭제곱) !(팩토리얼) ( )
        상수     : pi, e, tau, ans(직전 결과)
        삼각함수 : sin cos tan asin acos atan atan2(y,x) sinh cosh tanh
        지수/로그: sqrt cbrt exp ln log(x) log(x,밑) log2 pow(a,b)
        기타     : abs floor ceil round(x[,자릿수]) trunc mod(a,b) min max
                   fact(n) ncr(n,r) npr(n,r) deg(라디안->도) rad(도->라디안)
        변수     : x = 3*4   (사용: x^2)
        명령     : deg, rad(각도 모드), vars, clear, help, q(종료)
        예시     : sin(30), 2^10, sqrt(2)*pi, 5!, ncr(10,3), log(8,2)
        """);
}
