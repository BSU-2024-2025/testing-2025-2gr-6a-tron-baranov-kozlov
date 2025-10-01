using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CalculatorSolution
{
    public class Tokenizer
    {
        public List<Token> Tokenize(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException(ErrorMessages.InvalidExpression);

            var tokens = new List<Token>();
            int i = 0;

            while (i < expression.Length)
            {
                char c = expression[i];
                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                if (char.IsDigit(c) || c == '.')
                {
                    tokens.Add(TokenizeNumber(expression, ref i));
                    HandleImplicitMultiplication(tokens, expression, i);
                    continue;
                }

                if (Constants.OperatorChars.Contains(c))
                {
                    tokens.Add(new Token { Type = "operator", Value = c.ToString(), Position = i });
                    i++;
                    continue;
                }

                if (c == Constants.Parenthesis[0])
                {
                    HandleLeftParenthesis(tokens, i);
                    i++;
                    continue;
                }

                if (c == Constants.Parenthesis[1])
                {
                    tokens.Add(new Token { Type = "right", Value = ")", Position = i });
                    i++;
                    HandleImplicitMultiplication(tokens, expression, i);
                    continue;
                }

                if (char.IsLetter(c))
                {
                    tokens.Add(TokenizeIdentifier(expression, ref i));
                    continue;
                }

                throw new ArgumentException(string.Format(ErrorMessages.InvalidCharacter, c, i + 1));
            }

            return tokens;
        }

        private Token TokenizeNumber(string expression, ref int i)
        {
            int start = i;
            var sb = new StringBuilder();

            while (i < expression.Length && 
                   (char.IsDigit(expression[i]) || expression[i] == '.' || expression[i] == 'e' || expression[i] == 'E' ||
                    ((expression[i] == '+' || expression[i] == '-') && i > 0 && (expression[i - 1] == 'e' || expression[i - 1] == 'E'))))
            {
                sb.Append(expression[i]);
                i++;
            }

            if (!double.TryParse(sb.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                throw new ArgumentException(string.Format(ErrorMessages.InvalidNumberFormat, start + 1));
            }

            return new Token { Type = "number", Value = sb.ToString(), Position = start };
        }

        private Token TokenizeIdentifier(string expression, ref int i)
        {
            int start = i;
            var sb = new StringBuilder();

            while (i < expression.Length && char.IsLetter(expression[i]))
            {
                sb.Append(expression[i]);
                i++;
            }

            string id = sb.ToString().ToLower();
            bool isFunction = i < expression.Length && expression[i] == Constants.Parenthesis[0];

            if (isFunction)
            {
                if (!Constants.KnownFunctions.Contains(id))
                    throw new ArgumentException(string.Format(ErrorMessages.UnknownFunction, id, start + 1));
                return new Token { Type = "function", Value = id, Position = start };
            }

            if (Constants.KnownFunctions.Contains(id))
                throw new ArgumentException(string.Format(ErrorMessages.FunctionAsVariable, id, start + 1));

            return new Token { Type = "variable", Value = id, Position = start };
        }

        private void HandleImplicitMultiplication(List<Token> tokens, string expression, int i)
        {
            if (i < expression.Length && (char.IsLetter(expression[i]) || expression[i] == Constants.Parenthesis[0]))
            {
                tokens.Add(new Token { Type = "operator", Value = "*", Position = i });
            }
        }

        private void HandleLeftParenthesis(List<Token> tokens, int i)
        {
            if (tokens.Count > 0 && (tokens[tokens.Count - 1].Type == "number" || tokens[tokens.Count - 1].Type == "right"))
            {
                tokens.Add(new Token { Type = "operator", Value = "*", Position = i });
            }
            tokens.Add(new Token { Type = "left", Value = "(", Position = i });
        }
    }
}