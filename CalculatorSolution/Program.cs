using System;

namespace CalculatorSolution
{
    class Program
    {
        static void Main(string[] args)
        {
            Calculator calc = new Calculator();
            Console.WriteLine("Calculator with multiple expressions support");
            Console.WriteLine("Enter expressions separated by ';' to calculate multiple at once");
            Console.WriteLine("Type 'exit' to quit\n");
          
            while (true)
            {
                Console.WriteLine("Enter an expression (or 'exit' to quit):");
                string input = Console.ReadLine();
              
                if (input.ToLower() == "exit") break;
              
                try
                {
                    var results = calc.Evaluate(input);
                    Console.WriteLine("Results:");
                    for (int i = 0; i < results.Count; i++)
                    {
                        Console.WriteLine($" Expression {i + 1}: {results[i]}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
              
                Console.WriteLine();
            }
        }
    }
}