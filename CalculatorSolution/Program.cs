using System;

namespace CalculatorSolution
{
    class Program
    {
        static void Main(string[] args)
        {
            Calculator calc = new Calculator();
            while (true)
            {
                Console.WriteLine("Enter an expression (or 'exit' to quit):");
                string expression = Console.ReadLine();
                if (expression.ToLower() == "exit") break;
                try
                {
                    double result = calc.Evaluate(expression);
                    Console.WriteLine($"Result: {result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}