using System;
using System.Collections.Generic;

namespace TestSamples.Simple
{
    public class BasicClass
    {
        private string _name;
        private int _value;
        
        public string Name 
        { 
            get => _name; 
            set => _name = value ?? throw new ArgumentNullException(nameof(value)); 
        }
        
        public int Value { get; set; }
        
        public BasicClass(string name, int value)
        {
            Name = name;
            Value = value;
        }
        
        public void DisplayInfo()
        {
            Console.WriteLine($"Name: {Name}, Value: {Value}");
        }
        
        public int Calculate(int input)
        {
            return Value + input;
        }
        
        public override string ToString()
        {
            return $"BasicClass({Name}, {Value})";
        }
    }
}