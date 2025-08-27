namespace KnowledgeNetwork.Domains.Code.Tests.TestData.SampleCode.EdgeCases;

public class ExceptionHandling
{
    public void SimpleTryCatch()
    {
        try
        {
            int divisor = 0;
            int result = 10 / divisor; // Avoid compile-time constant division by zero
        }
        catch (DivideByZeroException)
        {
            Console.WriteLine("Division by zero");
        }
    }

    public void TryCatchFinally()
    {
        try
        {
            throw new InvalidOperationException("Test");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Caught: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Finally block");
        }
    }

    public void MultipleCatchBlocks()
    {
        try
        {
            // Potentially problematic operation
            int.Parse("invalid");
        }
        catch (ArgumentNullException)
        {
            Console.WriteLine("Null argument");
        }
        catch (FormatException)
        {
            Console.WriteLine("Format error");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General error: {ex.Message}");
        }
    }

    public void NestedTryCatch()
    {
        try
        {
            try
            {
                int.Parse("invalid");
            }
            catch (FormatException inner)
            {
                throw new InvalidOperationException("Nested exception", inner);
            }
        }
        catch (InvalidOperationException outer)
        {
            Console.WriteLine($"Outer catch: {outer.Message}");
        }
    }

    public int TryWithReturn()
    {
        try
        {
            return 42;
        }
        catch (Exception)
        {
            return -1;
        }
        finally
        {
            Console.WriteLine("Finally executes after return");
        }
    }

    public void TryCatchWithRethrow()
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Logging: {ex.Message}");
            throw; // Rethrow without losing stack trace
        }
    }

    public void UsingStatement()
    {
        using (var stream = new MemoryStream())
        {
            stream.WriteByte(42);
        } // Implicit finally block for disposal
    }

    public async Task UsingStatementAsync()
    {
        using (var httpClient = new HttpClient())
        {
            await httpClient.GetStringAsync("https://api.example.com");
        }
    }
}