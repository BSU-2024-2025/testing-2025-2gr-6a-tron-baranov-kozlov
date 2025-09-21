using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CalculatorSolution
{
   public class Calculator
{
    public double Evaluate(string expression)
    {
        var tokens = Tokenize(expression);
        var rpn = ToRPN(tokens);
        return EvaluateRPN(rpn);
    }

    private List<Token> Tokenize(string expr)
    {
        List<Token> tokens = new List<Token>();
        int i = 0;
        while (i < expr.Length)
        {
            char c = expr[i];
            if (char.IsWhiteSpace(c)) { i++; continue; }
            if (char.IsDigit(c) || c == '.')
            {
                int start = i;
                StringBuilder sb = new StringBuilder();
                while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.' || expr[i] == 'e' || expr[i] == 'E' || ((expr[i] == '+' || expr[i] == '-') && i > 0 && (expr[i - 1] == 'e' || expr[i - 1] == 'E'))))
                {
                    sb.Append(expr[i]);
                    i++;
                }
                if (!double.TryParse(sb.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double num))
                {
                    throw new Exception($"Invalid number at position {start + 1}");
                }
                tokens.Add(new Token { Type = "number", Value = num.ToString(CultureInfo.InvariantCulture), Position = start });
                if (i < expr.Length)
                {
                    char next = expr[i];
                    if (char.IsLetter(next) || next == '(')
                    {
                        tokens.Add(new Token { Type = "operator", Value = "*", Position = i });
                    }
                }
                continue;
            }
            if (c == '+' || c == '-' || c == '*' || c == '/')
            {
                tokens.Add(new Token { Type = "operator", Value = c.ToString(), Position = i });
                i++;
                continue;
            }
            if (c == '(')
            {
                tokens.Add(new Token { Type = "left", Value = "(", Position = i });
                i++;
                continue;
            }
            if (c == ')')
            {
                tokens.Add(new Token { Type = "right", Value = ")", Position = i });
                i++;
                if (i < expr.Length)
                {
                    char next = expr[i];
                    if (char.IsDigit(next) || next == '.' || char.IsLetter(next) || next == '(')
                    {
                        tokens.Add(new Token { Type = "operator", Value = "*", Position = i });
                    }
                }
                continue;
            }
            if (char.IsLetter(c))
            {
                int start = i;
                StringBuilder sb = new StringBuilder();
                while (i < expr.Length && char.IsLetter(expr[i]))
                {
                    sb.Append(expr[i]);
                    i++;
                }
                string func = sb.ToString().ToLower();
                if (func != "sin" && func != "cos" && func != "exp")
                {
                    throw new Exception($"Unknown function {func} at position {start + 1}");
                }
                tokens.Add(new Token { Type = "function", Value = func, Position = start });
                continue;
            }
            throw new Exception($"Invalid character {c} at position {i + 1}");
        }
        return tokens;
    }

    private List<Token> ToRPN(List<Token> tokens)
    {
        List<Token> output = new List<Token>();
        Stack<Token> opStack = new Stack<Token>();
        for (int i = 0; i < tokens.Count; i++)
        {
            Token token = tokens[i];
            if (token.Type == "number")
            {
                output.Add(token);
            }
            else if (token.Type == "function")
            {
                opStack.Push(token);
            }
            else if (token.Type == "operator")
            {
                bool isUnary = i == 0 || tokens[i - 1].Type == "operator" || tokens[i - 1].Type == "left" || tokens[i - 1].Type == "function";
                if (isUnary && token.Value == "-")
                {
                    if (i > 0 && tokens[i - 1].Type == "operator" && tokens[i - 1].Value == "-")
                    {
                        throw new Exception($"Invalid sequence '--' at position {token.Position + 1}");
                    }
                    token.Value = "u-";
                }
                else if (isUnary && token.Value == "+")
                {
                    continue;
                }
                else if (!isUnary && i > 0 && tokens[i - 1].Type != "number" && tokens[i - 1].Type != "right")
                {
                    throw new Exception($"Unexpected operator at position {token.Position + 1}");
                }
                while (opStack.Count > 0 && opStack.Peek().Type == "operator" &&
                       GetPrecedence(token.Value) <= GetPrecedence(opStack.Peek().Value))
                {
                    output.Add(opStack.Pop());
                }
                opStack.Push(token);
            }
            else if (token.Type == "left")
            {
                opStack.Push(token);
            }
            else if (token.Type == "right")
            {
                while (opStack.Count > 0 && opStack.Peek().Type != "left")
                {
                    output.Add(opStack.Pop());
                }
                if (opStack.Count == 0)
                {
                    throw new Exception($"Mismatched parentheses at position {token.Position + 1}");
                }
                opStack.Pop();
                if (opStack.Count > 0 && opStack.Peek().Type == "function")
                {
                    output.Add(opStack.Pop());
                }
            }
        }
        while (opStack.Count > 0)
        {
            Token t = opStack.Pop();
            if (t.Type == "left" || t.Type == "right")
            {
                throw new Exception($"Mismatched parentheses at position {t.Position + 1}");
            }
            output.Add(t);
        }
        return output;
    }

    private int GetPrecedence(string op)
    {
        switch (op)
        {
            case "+":
            case "-":
                return 1;
            case "*":
            case "/":
                return 2;
            case "u-":
                return 3;
            default:
                return 0;
        }
    }

    private double EvaluateRPN(List<Token> rpn)
    {
        Stack<double> stack = new Stack<double>();
        foreach (Token token in rpn)
        {
            if (token.Type == "number")
            {
                stack.Push(double.Parse(token.Value, CultureInfo.InvariantCulture));
            }
            else if (token.Type == "operator")
            {
                if (token.Value == "u-")
                {
                    if (stack.Count < 1) throw new Exception("Invalid expression");
                    double a = stack.Pop();
                    stack.Push(-a);
                }
                else
                {
                    if (stack.Count < 2) throw new Exception("Invalid expression");
                    double b = stack.Pop();
                    double a = stack.Pop();
                    switch (token.Value)
                    {
                        case "+": stack.Push(a + b); break;
                        case "-": stack.Push(a - b); break;
                        case "*": stack.Push(a * b); break;
                        case "/":
                            if (b == 0) throw new Exception("Division by zero");
                            stack.Push(a / b); break;
                    }
                }
            }
            else if (token.Type == "function")
            {
                if (stack.Count < 1) throw new Exception("Invalid expression");
                double arg = stack.Pop();
                switch (token.Value)
                {
                    case "sin": stack.Push(Math.Sin(arg)); break;
                    case "cos": stack.Push(Math.Cos(arg)); break;
                    case "exp": stack.Push(Math.Exp(arg)); break;
                }
            }
        }
        if (stack.Count != 1) throw new Exception("Invalid expression");
        return stack.Pop();
    }
}
}
