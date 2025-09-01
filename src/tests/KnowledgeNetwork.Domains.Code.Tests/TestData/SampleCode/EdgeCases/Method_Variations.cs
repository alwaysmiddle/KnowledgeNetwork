namespace KnowledgeNetwork.Domains.Code.Tests.TestData.SampleCode.EdgeCases;

public class MethodVariations
{
    public void VoidMethod() { }

    public void VoidMethodWithSingleStatement() => Console.WriteLine("Expression body");

    public int SimpleReturn() => 42;

    public (int, string) TupleReturn() => (42, "Hello");

    public ref int RefReturn(ref int value) => ref value;

    public void MethodWithOutParameter(out int result)
    {
        result = 42;
    }

    public void MethodWithRefParameter(ref int value)
    {
        value *= 2;
    }

    public void MethodWithInParameter(in int value)
    {
        Console.WriteLine(value);
    }

    public void MethodWithParams(params int[] values)
    {
        foreach (var value in values)
        {
            Console.WriteLine(value);
        }
    }

    public void MethodWithOptionalParameters(int required, int optional = 10, string name = "default")
    {
        Console.WriteLine($"Required: {required}, Optional: {optional}, Name: {name}");
    }

    public void MethodWithNamedParameters()
    {
        MethodWithOptionalParameters(required: 5, name: "test", optional: 20);
    }

    public T GenericMethod<T>(T input) where T : class
    {
        return input;
    }

    public T GenericMethodWithConstraints<T>(T input) 
        where T : class, new()
    {
        return new T();
    }

    public void GenericMethodWithMultipleConstraints<T, U>(T first, U second)
        where T : class, IDisposable
        where U : struct
    {
        first?.Dispose();
        Console.WriteLine(second);
    }

    public static void StaticMethod()
    {
        Console.WriteLine("Static method");
    }

    public virtual void VirtualMethod()
    {
        Console.WriteLine("Virtual method");
    }

    public virtual void VirtualMethodWithoutImplementation()
    {
        // Virtual method that can be overridden
        throw new NotImplementedException("Must be implemented by derived class");
    }

    protected virtual void ProtectedMethod()
    {
        Console.WriteLine("Protected method");
    }

    private void PrivateMethod()
    {
        Console.WriteLine("Private method");
    }

    internal void InternalMethod()
    {
        Console.WriteLine("Internal method");
    }

    public override string ToString()
    {
        return "MethodVariations";
    }

    public void RecursiveMethod(int count)
    {
        if (count <= 0)
            return;
        
        Console.WriteLine($"Count: {count}");
        RecursiveMethod(count - 1);
    }

    public void MutualRecursionA(int count)
    {
        if (count <= 0)
            return;
        
        Console.WriteLine($"A: {count}");
        MutualRecursionB(count - 1);
    }

    public void MutualRecursionB(int count)
    {
        if (count <= 0)
            return;
        
        Console.WriteLine($"B: {count}");
        MutualRecursionA(count - 1);
    }

    public void MethodCallingLocalFunction()
    {
        LocalFunction(5);
        
        void LocalFunction(int x)
        {
            Console.WriteLine($"Local function: {x}");
            
            void NestedLocalFunction()
            {
                Console.WriteLine("Nested local function");
            }
            
            NestedLocalFunction();
        }
    }

    public void LambdaVariations()
    {
        Action simpleAction = () => Console.WriteLine("Simple lambda");
        Func<int, int> squareFunc = x => x * x;
        Func<int, int, int> addFunc = (x, y) => x + y;
        
        Func<int, bool> complexLambda = x =>
        {
            var squared = x * x;
            return squared > 10 && squared < 100;
        };
        
        simpleAction();
        Console.WriteLine(squareFunc(5));
        Console.WriteLine(addFunc(3, 4));
        Console.WriteLine(complexLambda(5));
    }

    // Constructor variations
    public MethodVariations()
    {
        Console.WriteLine("Default constructor");
    }

    public MethodVariations(int value) : this()
    {
        Console.WriteLine($"Constructor with value: {value}");
    }

    public MethodVariations(string name, int value) : this(value)
    {
        Console.WriteLine($"Constructor with name and value: {name}, {value}");
    }

    // Finalizer
    ~MethodVariations()
    {
        Console.WriteLine("Finalizer called");
    }

    // Operator overload
    public static MethodVariations operator +(MethodVariations left, MethodVariations right)
    {
        return new MethodVariations();
    }

    // Property with complex getter/setter
    private int _complexProperty;
    public int ComplexProperty
    {
        get
        {
            if (_complexProperty < 0)
                return 0;
            return _complexProperty;
        }
        set
        {
            if (value < 0)
                throw new ArgumentException("Value cannot be negative");
            _complexProperty = value;
        }
    }

    // Auto-property
    public string AutoProperty { get; set; } = "default";

    // Read-only property
    public int ReadOnlyProperty { get; }

    // Init-only property
    public string InitOnlyProperty { get; init; } = "init";

    // Indexer
    private readonly Dictionary<string, int> _data = new();
    
    public int this[string key]
    {
        get => _data.TryGetValue(key, out var value) ? value : 0;
        set => _data[key] = value;
    }
}

public abstract class AbstractMethodVariations
{
    public abstract void AbstractMethod();
}

public interface IMethodInterface
{
    void InterfaceMethod();
    void InterfaceMethodWithDefaultImplementation()
    {
        Console.WriteLine("Default implementation");
    }
}