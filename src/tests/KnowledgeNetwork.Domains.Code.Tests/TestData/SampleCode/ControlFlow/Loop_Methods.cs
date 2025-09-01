namespace KnowledgeNetwork.Domains.Code.Tests.TestData.SampleCode.ControlFlow;

public class LoopMethods
{
    public void SimpleForLoop()
    {
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(i);
        }
    }

    public void SimpleWhileLoop(int count)
    {
        int i = 0;
        while (i < count)
        {
            Console.WriteLine(i);
            i++;
        }
    }

    public void SimpleDoWhileLoop(int count)
    {
        int i = 0;
        do
        {
            Console.WriteLine(i);
            i++;
        } while (i < count);
    }

    public void ForeachLoop()
    {
        var numbers = new[] { 1, 2, 3, 4, 5 };
        foreach (var number in numbers)
        {
            Console.WriteLine(number);
        }
    }

    public void NestedLoops()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Console.WriteLine($"{i},{j}");
            }
        }
    }

    public void LoopWithBreak()
    {
        for (int i = 0; i < 100; i++)
        {
            if (i == 10)
            {
                break;
            }

            Console.WriteLine(i);
        }
    }

    public void LoopWithContinue()
    {
        for (int i = 0; i < 10; i++)
        {
            if (i % 2 == 0)
            {
                continue;
            }

            Console.WriteLine(i);
        }
    }
}