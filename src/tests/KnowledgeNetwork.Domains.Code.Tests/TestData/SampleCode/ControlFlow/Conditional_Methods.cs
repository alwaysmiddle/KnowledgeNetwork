namespace KnowledgeNetwork.Domains.Code.Tests.TestData.SampleCode.ControlFlow;

public class ConditionalMethods
{
    public void SimpleIfMethod(int x)
    {
        if (x > 0)
        {
            Console.WriteLine("Positive");
        }
    }
    
    public void IfElseMethod(int x)
    {
        Console.WriteLine(x > 0 ? "Positive" : "Not positive");
    }
    
    public void NestedIfMethod(int x, int y)
    {
        if (x > 0)
        {
            Console.WriteLine(y > 0 ? "Both positive" : "X positive, Y not");
        }
        else
        {
            Console.WriteLine("X not positive");
        }
    }
    
    public string TernaryMethod(int x)
    {
        return x > 0 ? "Positive" : "Not positive";
    }
    
    public void SwitchMethod(int value)
    {
        switch (value)
        {
            case 1:
                Console.WriteLine("One");
                break;
            case 2:
                Console.WriteLine("Two");
                break;
            default:
                Console.WriteLine("Other");
                break;
        }
    }
}
