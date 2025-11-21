using System;
using System.Collections.Generic;

namespace CalculatorSolution
{
    public class Evaluator
    {
        public double EvaluateRPN(List<Token> rpn, Dictionary<string, double> vars)
        {
            if (rpn == null || rpn.Count == 0)
                throw new ArgumentException(ErrorMessages.InvalidExpression);

            var stack = new Stack<double>();

            foreach (Token token in rpn)
            {
                switch (token.Type)
                {
                    case "number":
                        stack.Push(double.Parse(token.Value, System.Globalization.CultureInfo.InvariantCulture));
                        break;
                    case "variable":
                        if (!vars.TryGetValue(token.Value, out double val))
                            throw new ArgumentException("Undefined variable");
                        stack.Push(val);
                        break;
                    case "operator":
                        HandleOperator(token, stack);
                        break;
                    case "function":
                        HandleFunction(token, stack);
                        break;
                    case "keyword":
                        HandleKeyword(token, stack, vars);
                        break;
                }
            }

            if (stack.Count != 1)
                throw new ArgumentException(ErrorMessages.InvalidExpression);

            return stack.Pop();
        }

        private void HandleOperator(Token token, Stack<double> stack)
        {
            if (token.Value == "u-")
            {
                if (stack.Count < 1)
                    throw new ArgumentException(ErrorMessages.InvalidExpression);
                stack.Push(-stack.Pop());
                return;
            }

            if (stack.Count < 2)
                throw new ArgumentException(ErrorMessages.InvalidExpression);

            double b = stack.Pop();
            double a = stack.Pop();

            switch (token.Value)
            {
                case "+": stack.Push(a + b); break;
                case "-": stack.Push(a - b); break;
                case "*": stack.Push(a * b); break;
                case "/":
                    if (b == 0) throw new ArgumentException(ErrorMessages.DivisionByZero);
                    stack.Push(a / b);
                    break;
                case "<": stack.Push(a < b ? 1.0 : 0.0); break;
                case ">": stack.Push(a > b ? 1.0 : 0.0); break;
                case "<=": stack.Push(a <= b ? 1.0 : 0.0); break;
                case ">=": stack.Push(a >= b ? 1.0 : 0.0); break;
                case "==": stack.Push(Math.Abs(a - b) < 1e-10 ? 1.0 : 0.0); break;
                case "!=": stack.Push(Math.Abs(a - b) > 1e-10 ? 1.0 : 0.0); break;
            }
        }

        private void HandleFunction(Token token, Stack<double> stack)
        {
            if (stack.Count < 1)
                throw new ArgumentException(ErrorMessages.InvalidExpression);

            double arg = stack.Pop();
            switch (token.Value)
            {
                case "sin": stack.Push(Math.Sin(arg)); break;
                case "cos": stack.Push(Math.Cos(arg)); break;
                case "exp": stack.Push(Math.Exp(arg)); break;
            }
        }

        private void HandleKeyword(Token token, Stack<double> stack, Dictionary<string, double> vars)
        {
            if (token.Value == "if" || token.Value == "else")
            {
            }
        }
    }
}