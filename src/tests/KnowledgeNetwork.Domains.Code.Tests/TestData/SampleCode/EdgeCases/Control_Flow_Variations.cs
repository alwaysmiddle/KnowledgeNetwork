namespace KnowledgeNetwork.Domains.Code.Tests.TestData.SampleCode.EdgeCases;

public class ControlFlowVariations
{
    public void GotoStatement()
    {
        int i = 0;
        
        start:
        Console.WriteLine($"Iteration {i}");
        i++;
        
        if (i < 3)
            goto start;
        
        Console.WriteLine("Done with goto");
    }

    public void SwitchWithGoto(int value)
    {
        switch (value)
        {
            case 1:
                Console.WriteLine("One");
                goto case 3;
            case 2:
                Console.WriteLine("Two");
                break;
            case 3:
                Console.WriteLine("Three (or from One)");
                break;
            default:
                Console.WriteLine("Default");
                break;
        }
    }

    public void MultipleReturnPaths(int value)
    {
        if (value < 0)
            return;
        
        if (value == 0)
        {
            Console.WriteLine("Zero");
            return;
        }
        
        if (value > 100)
        {
            Console.WriteLine("Large");
            return;
        }
        
        Console.WriteLine("Normal");
    }

    public void ComplexLoopWithMultipleExits()
    {
        for (int i = 0; i < 100; i++)
        {
            if (i == 5)
                continue;
            
            if (i == 10)
                break;
            
            if (i % 7 == 0)
            {
                Console.WriteLine($"Lucky 7 multiple: {i}");
                if (i > 20)
                    break;
            }
            
            Console.WriteLine(i);
        }
    }

    public void NestedSwitchStatements(int outer, int inner)
    {
        switch (outer)
        {
            case 1:
                switch (inner)
                {
                    case 1:
                        Console.WriteLine("1,1");
                        break;
                    case 2:
                        Console.WriteLine("1,2");
                        break;
                }
                break;
            case 2:
                switch (inner)
                {
                    case 1:
                        Console.WriteLine("2,1");
                        break;
                    default:
                        Console.WriteLine("2,other");
                        break;
                }
                break;
        }
    }

    public void WhileWithBreakAndContinue()
    {
        int count = 0;
        while (count < 20)
        {
            count++;
            
            if (count % 3 == 0)
                continue;
            
            if (count > 15)
                break;
            
            Console.WriteLine(count);
        }
    }

    public void DoWhileWithEarlyExit()
    {
        int i = 0;
        do
        {
            i++;
            if (i == 5)
                break;
            Console.WriteLine(i);
        } while (i < 10);
    }

    public void ForeachWithComplexLogic()
    {
        var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        
        foreach (var item in items)
        {
            if (item < 3)
                continue;
            
            if (item > 7)
                break;
            
            if (item % 2 == 0)
            {
                Console.WriteLine($"Even: {item}");
            }
            else
            {
                Console.WriteLine($"Odd: {item}");
            }
        }
    }

    public bool ComplexConditionalWithEarlyReturn(int a, int b, int c)
    {
        if (a < 0)
            return false;
        
        if (b == 0)
        {
            if (c > 10)
                return true;
            else
                return false;
        }
        
        var result = (a * b) + c;
        
        if (result > 100)
            return true;
        
        return result % 2 == 0;
    }
}