using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace TestSamples.Complex
{
    public class AsyncMethods
    {
        public async Task<string> ProcessDataAsync(List<int> data)
        {
            if (data == null || !data.Any())
            {
                throw new ArgumentException("Data cannot be null or empty");
            }

            var results = new List<string>();
            
            foreach (var item in data)
            {
                if (item > 0)
                {
                    var result = await ProcessPositiveAsync(item);
                    results.Add(result);
                }
                else if (item < 0)
                {
                    var result = await ProcessNegativeAsync(item);
                    results.Add(result);
                }
                else
                {
                    results.Add("Zero");
                }
            }

            return string.Join(", ", results);
        }

        private async Task<string> ProcessPositiveAsync(int value)
        {
            await Task.Delay(10); // Simulate async work
            
            return value switch
            {
                > 100 => "Large positive",
                > 10 => "Medium positive", 
                _ => "Small positive"
            };
        }

        private async Task<string> ProcessNegativeAsync(int value)
        {
            await Task.Delay(5); // Simulate async work
            
            if (value < -100)
            {
                return "Large negative";
            }
            else if (value < -10)
            {
                return "Medium negative";
            }
            else
            {
                return "Small negative";
            }
        }

        public async Task<T> RetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
        {
            var attempt = 0;
            
            while (attempt < maxRetries)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex) when (attempt < maxRetries - 1)
                {
                    await Task.Delay(1000 * (attempt + 1)); // Exponential backoff
                    attempt++;
                }
            }
            
            throw new InvalidOperationException($"Operation failed after {maxRetries} attempts");
        }
    }
}