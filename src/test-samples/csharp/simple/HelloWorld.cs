using System;

namespace TestSamples.Simple
{
    /// <summary>
    /// A simple Hello World class for testing basic analysis
    /// </summary>
    public class HelloWorld
    {
        /// <summary>
        /// The main entry point
        /// </summary>
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello, Knowledge Network!");
            
            if (args.Length > 0)
            {
                Console.WriteLine($"Arguments: {string.Join(", ", args)}");
            }
        }

        /// <summary>
        /// A simple greeting method
        /// </summary>
        public void SayHello(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("Hello, Anonymous!");
            }
            else
            {
                Console.WriteLine($"Hello, {name}!");
            }
        }
    }
}