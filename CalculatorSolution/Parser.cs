using System;
using System.Collections.Generic;

namespace CalculatorSolution
{
    public class Parser
    {
        public List<Token> ToRPN(List<Token> tokens)
        {
            if (tokens == null || tokens.Count == 0)
                throw new ArgumentException(ErrorMessages.InvalidExpression);

            var output = new List<Token>();
            var opStack = new Stack<Token>();

            for (int i = 0; i < tokens.Count; i++)
            {
                Token token = tokens[i];
                switch (token.Type)
                {
                    case "number":
                    case "variable":
                        output.Add(token);
                        break;
                    case "function":
                        opStack.Push(token);
                        break;
                    case "keyword":
                        // For now, treat keywords as variables in RPN - they will be handled in evaluation
                        output.Add(token);
                        break;
                    case "operator":
                        HandleOperator(token, tokens, i, output, opStack);
                        break;
                    case "left":
                        opStack.Push(token);
                        break;
                    case "right":
                        HandleRightParenthesis(token, output, opStack);
                        break;
                    case "left_brace":
                        opStack.Push(token);
                        break;
                    case "right_brace":
                        HandleRightBrace(token, output, opStack);
                        break;
                }
            }

            while (opStack.Count > 0)
            {
                Token t = opStack.Pop();
                if (t.Type == "left" || t.Type == "right" || t.Type == "left_brace" || t.Type == "right_brace")
                    throw new ArgumentException(string.Format(ErrorMessages.MismatchedParentheses, t.Position + 1));
                output.Add(t);
            }

            return output;
        }

        private void HandleOperator(Token token, List<Token> tokens, int i, List<Token> output, Stack<Token> opStack)
        {
            bool isUnary = i == 0 || tokens[i - 1].Type == "operator" || tokens[i - 1].Type == "left" || 
                          tokens[i - 1].Type == "function" || tokens[i - 1].Type == "keyword" || 
                          tokens[i - 1].Type == "left_brace";

            if (isUnary && token.Value == "-")
            {
                if (i > 0 && tokens[i - 1].Type == "operator" && tokens[i - 1].Value == "-")
                    throw new ArgumentException(string.Format(ErrorMessages.InvalidOperatorSequence, token.Position + 1));
                token.Value = "u-";
            }
            else if (isUnary && token.Value == "+")
            {
                return;
            }
            else if (!isUnary && i > 0 && tokens[i - 1].Type != "number" && 
                     tokens[i - 1].Type != "right" && tokens[i - 1].Type != "variable" && 
                     tokens[i - 1].Type != "right_brace" && tokens[i - 1].Type != "keyword")
            {
                throw new ArgumentException(string.Format(ErrorMessages.UnexpectedOperator, token.Position + 1));
            }

            while (opStack.Count > 0 && opStack.Peek().Type == "operator" &&
                   GetPrecedence(token.Value) <= GetPrecedence(opStack.Peek().Value))
            {
                output.Add(opStack.Pop());
            }
            opStack.Push(token);
        }

        private void HandleRightParenthesis(Token token, List<Token> output, Stack<Token> opStack)
        {
            while (opStack.Count > 0 && opStack.Peek().Type != "left")
            {
                output.Add(opStack.Pop());
            }
            if (opStack.Count == 0)
                throw new ArgumentException(string.Format(ErrorMessages.MismatchedParentheses, token.Position + 1));
            opStack.Pop();
            if (opStack.Count > 0 && opStack.Peek().Type == "function")
                output.Add(opStack.Pop());
        }

        private void HandleRightBrace(Token token, List<Token> output, Stack<Token> opStack)
        {
            while (opStack.Count > 0 && opStack.Peek().Type != "left_brace")
            {
                output.Add(opStack.Pop());
            }
            if (opStack.Count == 0)
                throw new ArgumentException(string.Format(ErrorMessages.MismatchedBraces, token.Position + 1));
            opStack.Pop();
        }

        private int GetPrecedence(string op)
        {
            return op switch
            {
                "+" or "-" => 1,
                "*" or "/" => 2,
                "u-" => 3,
                "<" or ">" or "<=" or ">=" or "==" or "!=" => 0,
                _ => 0
            };
        }
    }
}