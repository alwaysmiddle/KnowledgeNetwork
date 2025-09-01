using System;

namespace TestData.Simple
{
    /// <summary>
    /// Simple calculator class for testing method analysis
    /// </summary>
    public class BasicCalculator
    {
        /// <summary>
        /// Simple linear method - no loops or conditionals
        /// Expected: 3 nodes, 2 edges, complexity = 1
        /// </summary>
        public int Add(int a, int b)
        {
            var result = a + b;
            return result;
        }

        /// <summary>
        /// Method with conditional logic
        /// Expected: 4 nodes, 4 edges, complexity = 2
        /// </summary>
        public int SafeDivide(int a, int b)
        {
            if (b == 0)
                return 0;
            
            return a / b;
        }

        /// <summary>
        /// Method with loop
        /// Expected: 5 nodes, 6 edges, complexity = 3
        /// </summary>
        public int Factorial(int n)
        {
            int result = 1;
            for (int i = 1; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }
    }
}