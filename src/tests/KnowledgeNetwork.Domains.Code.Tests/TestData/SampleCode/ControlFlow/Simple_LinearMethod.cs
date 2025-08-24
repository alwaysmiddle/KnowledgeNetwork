namespace KnowledgeNetwork.Domains.Code.Tests.TestData.SampleCode.ControlFlow;

public class SimpleLinearMethods
{
    public void SimpleMethod()
    {
        int x = 10;
        Console.WriteLine(x);
    }

    public int SimpleReturnMethod()
    {
        int result = 42;
        return result;
    }

    public void EmptyMethod()
    {
    }

    public int ExpressionBodiedMethod() => 42;

    public string ExpressionBodiedStringMethod() => "Hello World";
}