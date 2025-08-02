using System;
using System.Collections.Generic;

namespace TestApp
{
    /// <summary>
    /// A simple calculator class for testing
    /// </summary>
    public class Calculator : ICalculator
    {
        private readonly List<int> _history = new List<int>();
        
        public int Add(int a, int b)
        {
            var result = a + b;
            _history.Add(result);
            return result;
        }
        
        public int Multiply(int x, int y)
        {
            return x * y;
        }
        
        public IEnumerable<int> GetHistory()
        {
            return _history.AsReadOnly();
        }
    }
    
    public interface ICalculator
    {
        int Add(int a, int b);
        int Multiply(int x, int y);
        IEnumerable<int> GetHistory();
    }
    
    public abstract class BaseCalculator
    {
        public abstract int Calculate(int a, int b);
    }
}