using System;

namespace TestSamples.EdgeCases
{
    // This file contains intentionally malformed code to test error handling
    
    public class MalformedCode
    {
        // Missing closing brace for method
        public void IncompleteMethod()
        {
            Console.WriteLine("This method is missing a closing brace");
        
        // Syntax error - missing semicolon
        public string BrokenProperty { get set }
        
        // Invalid method signature
        public void (string name)
        {
            Console.WriteLine($"Hello {name}");
        }
    }
    
    // Missing closing brace for class
}