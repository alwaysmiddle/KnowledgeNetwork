namespace KnowledgeNetwork.Domains.Code.Tests.TestData.SampleCode.EdgeCases;

public class ComplexExpressions
{
    public void ComplexConditionalExpression(int a, int b, int c)
    {
        if ((a > 0 && b < 10) || (c == 0 && a != b))
        {
            Console.WriteLine("Complex condition met");
        }
    }

    public void ChainedMethodCalls()
    {
        var result = "Hello World"
            .ToUpper()
            .Replace("WORLD", "UNIVERSE")
            .Substring(0, 5)
            .Trim();
    }

    public void LinqExpression()
    {
        var numbers = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var result = numbers
            .Where(x => x % 2 == 0)
            .Select(x => x * x)
            .Where(x => x > 10)
            .ToList();
    }

    public void NullConditionalOperators(string? text)
    {
        var length = text?.Length ?? 0;
        var upperText = text?.ToUpper()?.Trim();
        Console.WriteLine($"Length: {length}, Upper: {upperText ?? "null"}");
    }

    public void PatternMatching(object obj)
    {
        var result = obj switch
        {
            string s when s.Length > 5 => $"Long string: {s}",
            string s => $"Short string: {s}",
            int i when i > 100 => $"Large number: {i}",
            int i => $"Small number: {i}",
            null => "null value",
            _ => $"Unknown type: {obj.GetType().Name}"
        };
        Console.WriteLine(result);
    }

    public void RecordPatternMatching(object obj)
    {
        if (obj is Point { X: > 0, Y: > 0 } positivePoint)
        {
            Console.WriteLine($"Positive point: {positivePoint}");
        }
        else if (obj is Point point)
        {
            Console.WriteLine($"Other point: {point}");
        }
    }

    public void LocalFunctionWithClosure()
    {
        int outerVariable = 10;
        
        int LocalFunction(int parameter)
        {
            return parameter + outerVariable; // Captures outer variable
        }

        var result = LocalFunction(5);
        Console.WriteLine(result);
    }

    public void ComplexLambdaExpression()
    {
        var numbers = Enumerable.Range(1, 100);
        
        var complexResult = numbers
            .Where(x => 
            {
                var squared = x * x;
                var isEven = squared % 2 == 0;
                return isEven && squared > 50;
            })
            .Select(x => new { Number = x, Square = x * x })
            .ToList();
    }

    public void PointerLikeOperation()
    {
        int value = 42;
        // Simulating pointer-like behavior without unsafe code
        ref int valueRef = ref value;
        Console.WriteLine($"Value via reference: {valueRef}");
    }

    public void YieldReturn()
    {
        foreach (var item in GetNumbers())
        {
            Console.WriteLine(item);
        }
    }

    private IEnumerable<int> GetNumbers()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return i * i;
        }
    }
}

public record Point(int X, int Y);