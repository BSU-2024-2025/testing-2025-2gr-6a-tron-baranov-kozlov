using System;
using System.Collections.Generic;
using System.Linq;

namespace CalculatorSolution
{
    public class Calculator
    {
        private readonly Tokenizer _tokenizer;
        private readonly Parser _parser;
        private readonly Evaluator _evaluator;

        public Calculator()
        {
            _tokenizer = new Tokenizer();
            _parser = new Parser();
            _evaluator = new Evaluator();
        }

        public double Evaluate(string expression, Dictionary<string, double> vars = null)
        {
            vars ??= new Dictionary<string, double>();
            
            // If expression contains semicolons, use EvaluateMultiple
            if (expression.Contains(';'))
            {
                var results = EvaluateMultiple(expression);
                return results.Last(); // Return last result for single Evaluate call
            }
            
            // Handle complex conditional expressions with blocks
            if (expression.Trim().StartsWith("if") && expression.Contains('{'))
            {
                return EvaluateBlockConditional(expression, vars);
            }
            
            // Handle simple conditional expressions
            if (expression.Trim().StartsWith("if"))
            {
                return EvaluateSimpleConditional(expression, vars);
            }
            
            // Handle assignment expressions like "x=5"
            if (expression.Contains('=') && !expression.Contains("if") && !expression.Contains("=="))
            {
                return HandleAssignment(expression, vars);
            }
            
            var tokens = _tokenizer.Tokenize(expression);
            var rpn = _parser.ToRPN(tokens);
            return _evaluator.EvaluateRPN(rpn, vars);
        }

        private double HandleAssignment(string expression, Dictionary<string, double> vars)
        {
            var parts = expression.Split('=', 2);
            if (parts.Length != 2)
                throw new ArgumentException(ErrorMessages.InvalidAssignment);

            var varName = parts[0].Trim().ToLower();
            ValidateVariableName(varName);
            var value = Evaluate(parts[1].Trim(), vars);
            vars[varName] = value;
            return value;
        }

        private double EvaluateSimpleConditional(string expression, Dictionary<string, double> vars)
        {
            string expr = expression.Trim();
            
            if (expr.StartsWith("if ("))
            {
                int conditionStart = 4;
                int conditionEnd = expr.IndexOf(')', conditionStart);
                if (conditionEnd == -1) throw new ArgumentException(ErrorMessages.InvalidConditional);
                
                string condition = expr.Substring(conditionStart, conditionEnd - conditionStart).Trim();
                string rest = expr.Substring(conditionEnd + 1).Trim();
                
                // Evaluate condition
                double conditionResult = Evaluate(condition, vars);
                
                // Handle if-else
                if (rest.Contains("else"))
                {
                    int elseIndex = rest.IndexOf("else");
                    string truePart = rest.Substring(0, elseIndex).Trim();
                    string falsePart = rest.Substring(elseIndex + 4).Trim();
                    
                    return conditionResult != 0 ? Evaluate(truePart, vars) : Evaluate(falsePart, vars);
                }
                else
                {
                    return conditionResult != 0 ? Evaluate(rest, vars) : 0;
                }
            }
            
            throw new ArgumentException(ErrorMessages.InvalidConditional);
        }

        private double EvaluateBlockConditional(string expression, Dictionary<string, double> vars)
        {
            string expr = expression.Trim();
            
            if (expr.StartsWith("if ("))
            {
                int conditionStart = 4;
                int conditionEnd = expr.IndexOf(')', conditionStart);
                if (conditionEnd == -1) throw new ArgumentException(ErrorMessages.InvalidConditional);
                
                string condition = expr.Substring(conditionStart, conditionEnd - conditionStart).Trim();
                string rest = expr.Substring(conditionEnd + 1).Trim();
                
                // Evaluate condition
                double conditionResult = Evaluate(condition, vars);
                
                // Extract true block (inside {})
                int trueBlockStart = rest.IndexOf('{');
                int trueBlockEnd = FindMatchingBrace(rest, trueBlockStart);
                if (trueBlockStart == -1 || trueBlockEnd == -1) 
                    throw new ArgumentException(ErrorMessages.MismatchedBraces);
                
                string trueBlock = rest.Substring(trueBlockStart + 1, trueBlockEnd - trueBlockStart - 1).Trim();
                string afterTrueBlock = rest.Substring(trueBlockEnd + 1).Trim();
                
                // Handle else block
                if (afterTrueBlock.StartsWith("else"))
                {
                    string elsePart = afterTrueBlock.Substring(4).Trim();
                    
                    if (elsePart.StartsWith("{"))
                    {
                        int elseBlockStart = 0;
                        int elseBlockEnd = FindMatchingBrace(elsePart, elseBlockStart);
                        if (elseBlockEnd == -1) throw new ArgumentException(ErrorMessages.MismatchedBraces);
                        
                        string elseBlock = elsePart.Substring(1, elseBlockEnd - 1).Trim();
                        return conditionResult != 0 ? EvaluateBlock(trueBlock, vars) : EvaluateBlock(elseBlock, vars);
                    }
                    else
                    {
                        // Simple else without braces
                        return conditionResult != 0 ? EvaluateBlock(trueBlock, vars) : Evaluate(elsePart, vars);
                    }
                }
                else
                {
                    // No else block
                    return conditionResult != 0 ? EvaluateBlock(trueBlock, vars) : 0;
                }
            }
            
            throw new ArgumentException(ErrorMessages.InvalidConditional);
        }

        private double EvaluateBlock(string block, Dictionary<string, double> vars)
        {
            // A block may contain multiple statements separated by semicolons
            if (block.Contains(';'))
            {
                var results = EvaluateMultiple(block);
                return results.Last();
            }
            else
            {
                return Evaluate(block, vars);
            }
        }

        private int FindMatchingBrace(string text, int startIndex)
        {
            int braceCount = 0;
            for (int i = startIndex; i < text.Length; i++)
            {
                if (text[i] == '{') braceCount++;
                else if (text[i] == '}') braceCount--;
                
                if (braceCount == 0) return i;
            }
            return -1;
        }

        public List<double> EvaluateMultiple(string expressions)
        {
            if (string.IsNullOrWhiteSpace(expressions))
                throw new ArgumentException(ErrorMessages.InvalidExpression);

            var results = new List<double>();
            var expressionList = expressions.Split(Constants.ExpressionSeparator, StringSplitOptions.RemoveEmptyEntries);
            var vars = new Dictionary<string, double>();

            for (int i = 0; i < expressionList.Length; i++)
            {
                var expression = expressionList[i].Trim();
                if (string.IsNullOrEmpty(expression))
                    continue;

                try
                {
                    double result = HandleExpression(expression, vars);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format(ErrorMessages.ExpressionError, i + 1, expression, ex.Message));
                }
            }

            return results;
        }

        private double HandleExpression(string expression, Dictionary<string, double> vars)
        {
            // Handle complex conditional with blocks
            if (expression.Trim().StartsWith("if") && expression.Contains('{'))
            {
                return EvaluateBlockConditional(expression, vars);
            }
            
            // Handle simple conditional
            if (expression.Trim().StartsWith("if"))
            {
                return EvaluateSimpleConditional(expression, vars);
            }
            
            // Handle assignment
            if (expression.Contains('=') && !expression.Contains("if") && !expression.Contains("=="))
            {
                return HandleAssignment(expression, vars);
            }
            
            // Handle regular expression
            return Evaluate(expression, vars);
        }

        private void ValidateVariableName(string varName)
        {
            if (string.IsNullOrWhiteSpace(varName))
                throw new ArgumentException(ErrorMessages.InvalidVariableName);

            if (!varName.All(char.IsLetter))
                throw new ArgumentException(ErrorMessages.InvalidVariableName);

            if (Constants.KnownFunctions.Contains(varName) || Constants.KnownKeywords.Contains(varName))
                throw new ArgumentException(ErrorMessages.CannotAssignToFunction);
        }
    }
}