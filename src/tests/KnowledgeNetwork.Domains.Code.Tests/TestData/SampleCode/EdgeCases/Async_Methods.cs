namespace KnowledgeNetwork.Domains.Code.Tests.TestData.SampleCode.EdgeCases;

public class AsyncMethods
{
    public async Task SimpleAsyncMethod()
    {
        await Task.Delay(100);
        Console.WriteLine("Done");
    }

    public async Task<int> AsyncMethodWithReturn()
    {
        await Task.Delay(50);
        return 42;
    }

    public async Task AsyncMethodWithMultipleAwaits()
    {
        await Task.Delay(10);
        await Task.Delay(20);
        await Task.Delay(30);
    }

    public async Task AsyncMethodWithConditionalAwait(bool condition)
    {
        if (condition)
        {
            await Task.Delay(100);
        }
        else
        {
            await Task.Delay(200);
        }
    }

    public async Task AsyncMethodWithLoopAwait()
    {
        for (int i = 0; i < 5; i++)
        {
            await Task.Delay(10 * i);
        }
    }

    public async Task AsyncMethodWithTryCatch()
    {
        try
        {
            await Task.Delay(100);
            throw new InvalidOperationException("Test exception");
        }
        catch (Exception ex)
        {
            await Task.Delay(50);
            Console.WriteLine($"Caught: {ex.Message}");
        }
    }

    public async Task<T> GenericAsyncMethod<T>(T input) where T : class
    {
        await Task.Delay(10);
        return input;
    }
}