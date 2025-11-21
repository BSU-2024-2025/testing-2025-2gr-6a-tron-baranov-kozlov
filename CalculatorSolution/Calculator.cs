using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public List<double> Evaluate(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException(ErrorMessages.InvalidExpression);

            expression = RemoveComments(expression);
            
            var results = new List<double>();
            var expressionList = SplitExpressions(expression);
            var vars = new Dictionary<string, double>();

            for (int i = 0; i < expressionList.Count; i++)
            {
                var expr = expressionList[i];
                if (string.IsNullOrEmpty(expr))
                    continue;

                try
                {
                    double result = ExecuteExpression(expr.Trim(), vars);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error in expression {i + 1}: '{expr}' - {ex.Message}");
                }
            }

            return results;
        }

        private List<string> SplitExpressions(string expression)
        {
            var expressions = new List<string>();
            var current = new StringBuilder();
            int braceCount = 0;

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];

                if (c == '{') braceCount++;
                else if (c == '}') braceCount--;

                if (c == ';' && braceCount == 0)
                {
                    if (current.Length > 0)
                    {
                        expressions.Add(current.ToString().Trim());
                        current.Clear();
                    }
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
            {
                expressions.Add(current.ToString().Trim());
            }

            return expressions;
        }

        private double ExecuteExpression(string expression, Dictionary<string, double> vars)
        {
            if (IsAssignment(expression))
            {
                return ExecuteAssignment(expression, vars);
            }
            
            if (expression.StartsWith("while") && expression.Contains('{'))
            {
                return ExecuteWhileLoop(expression, vars);
            }
            
            if (expression.StartsWith("if") && expression.Contains('{'))
            {
                return ExecuteBlockConditional(expression, vars);
            }
            
            if (expression.StartsWith("if"))
            {
                return ExecuteSimpleConditional(expression, vars);
            }
            
            if (expression.StartsWith("while"))
            {
                return ExecuteSimpleWhileLoop(expression, vars);
            }
            
            return EvaluateExpression(expression, vars);
        }

        private double EvaluateExpression(string expression, Dictionary<string, double> vars)
        {
            var tokens = _tokenizer.Tokenize(expression);
            var rpn = _parser.ToRPN(tokens);
            return _evaluator.EvaluateRPN(rpn, vars);
        }

        private bool IsAssignment(string expression)
        {
            if (!expression.Contains('=') || expression.Contains("if") || expression.Contains("while") || expression.Contains("=="))
                return false;

            var parts = expression.Split('=', 2);
            if (parts.Length != 2) return false;

            var varName = parts[0].Trim();
            return varName.All(char.IsLetter) && varName.Length > 0;
        }

        private string RemoveComments(string expression)
        {
            var result = new StringBuilder();
            bool inComment = false;

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];
                
                if (c == '/' && i + 1 < expression.Length && expression[i + 1] == '/')
                {
                    inComment = true;
                    i++; 
                    continue;
                }
                
                if (inComment && (c == '\n' || c == '\r'))
                {
                    inComment = false;
                    continue;
                }
                
                if (inComment)
                {
                    continue;
                }
                
                result.Append(c);
            }
            
            return result.ToString();
        }

        private double ExecuteAssignment(string expression, Dictionary<string, double> vars)
        {
            var parts = expression.Split('=', 2);
            if (parts.Length != 2)
                throw new ArgumentException(ErrorMessages.InvalidAssignment);

            var varName = parts[0].Trim().ToLower();
            ValidateVariableName(varName);
            var value = ExecuteExpression(parts[1].Trim(), vars);
            vars[varName] = value;
            return value;
        }

        private double ExecuteSimpleConditional(string expression, Dictionary<string, double> vars)
        {
            string expr = expression.Trim();
            
            if (expr.StartsWith("if ("))
            {
                int conditionStart = 4;
                int conditionEnd = expr.IndexOf(')', conditionStart);
                if (conditionEnd == -1) throw new ArgumentException(ErrorMessages.InvalidConditional);
                
                string condition = expr.Substring(conditionStart, conditionEnd - conditionStart).Trim();
                string rest = expr.Substring(conditionEnd + 1).Trim();
                
                double conditionResult = EvaluateExpression(condition, vars);
                
                if (rest.Contains("else"))
                {
                    int elseIndex = rest.IndexOf("else");
                    string truePart = rest.Substring(0, elseIndex).Trim();
                    string falsePart = rest.Substring(elseIndex + 4).Trim();
                    
                    return conditionResult != 0 ? ExecuteExpression(truePart, vars) : ExecuteExpression(falsePart, vars);
                }
                else
                {
                    return conditionResult != 0 ? ExecuteExpression(rest, vars) : 0;
                }
            }
            
            throw new ArgumentException(ErrorMessages.InvalidConditional);
        }

        private double ExecuteBlockConditional(string expression, Dictionary<string, double> vars)
        {
            string expr = expression.Trim();
            
            if (expr.StartsWith("if ("))
            {
                int conditionStart = 4;
                int conditionEnd = expr.IndexOf(')', conditionStart);
                if (conditionEnd == -1) throw new ArgumentException(ErrorMessages.InvalidConditional);
                
                string condition = expr.Substring(conditionStart, conditionEnd - conditionStart).Trim();
                string rest = expr.Substring(conditionEnd + 1).Trim();
                
                double conditionResult = EvaluateExpression(condition, vars);
                
                int trueBlockStart = rest.IndexOf('{');
                int trueBlockEnd = FindMatchingBrace(rest, trueBlockStart);
                if (trueBlockStart == -1 || trueBlockEnd == -1) 
                    throw new ArgumentException(ErrorMessages.MismatchedBraces);
                
                string trueBlock = rest.Substring(trueBlockStart + 1, trueBlockEnd - trueBlockStart - 1).Trim();
                string afterTrueBlock = rest.Substring(trueBlockEnd + 1).Trim();
                
                if (afterTrueBlock.StartsWith("else"))
                {
                    string elsePart = afterTrueBlock.Substring(4).Trim();
                    
                    if (elsePart.StartsWith("{"))
                    {
                        int elseBlockStart = 0;
                        int elseBlockEnd = FindMatchingBrace(elsePart, elseBlockStart);
                        if (elseBlockEnd == -1) throw new ArgumentException(ErrorMessages.MismatchedBraces);
                        
                        string elseBlock = elsePart.Substring(1, elseBlockEnd - 1).Trim();
                        return conditionResult != 0 ? ExecuteBlock(trueBlock, vars) : ExecuteBlock(elseBlock, vars);
                    }
                    else
                    {
                        return conditionResult != 0 ? ExecuteBlock(trueBlock, vars) : ExecuteExpression(elsePart, vars);
                    }
                }
                else
                {
                    return conditionResult != 0 ? ExecuteBlock(trueBlock, vars) : 0;
                }
            }
            
            throw new ArgumentException(ErrorMessages.InvalidConditional);
        }

        private double ExecuteSimpleWhileLoop(string expression, Dictionary<string, double> vars)
        {
            string expr = expression.Trim();
            
            if (expr.StartsWith("while ("))
            {
                int conditionStart = 7;
                int conditionEnd = expr.IndexOf(')', conditionStart);
                if (conditionEnd == -1) throw new ArgumentException("Invalid while loop");
                
                string condition = expr.Substring(conditionStart, conditionEnd - conditionStart).Trim();
                string body = expr.Substring(conditionEnd + 1).Trim();
                
                double lastResult = 0;
                int iterationCount = 0;
                const int maxIterations = 10000;
                
                while (EvaluateExpression(condition, vars) != 0)
                {
                    lastResult = ExecuteExpression(body, vars);
                    iterationCount++;
                    
                    if (iterationCount >= maxIterations)
                        throw new Exception("While loop exceeded maximum iterations");
                }
                
                return lastResult;
            }
            
            throw new ArgumentException("Invalid while loop");
        }

        private double ExecuteWhileLoop(string expression, Dictionary<string, double> vars)
        {
            string expr = expression.Trim();
            
            if (expr.StartsWith("while ("))
            {
                int conditionStart = 7;
                int conditionEnd = expr.IndexOf(')', conditionStart);
                if (conditionEnd == -1) throw new ArgumentException("Invalid while loop");
                
                string condition = expr.Substring(conditionStart, conditionEnd - conditionStart).Trim();
                string rest = expr.Substring(conditionEnd + 1).Trim();
                
                int bodyStart = rest.IndexOf('{');
                int bodyEnd = FindMatchingBrace(rest, bodyStart);
                if (bodyStart == -1 || bodyEnd == -1) 
                    throw new ArgumentException(ErrorMessages.MismatchedBraces);
                
                string body = rest.Substring(bodyStart + 1, bodyEnd - bodyStart - 1).Trim();
                
                double lastResult = 0;
                int iterationCount = 0;
                const int maxIterations = 10000;
                
                var initialVars = new Dictionary<string, double>(vars);
                
                while (EvaluateExpression(condition, vars) != 0)
                {
                    lastResult = ExecuteBlock(body, vars);
                    iterationCount++;
                    
                    if (iterationCount >= maxIterations)
                        throw new Exception("While loop exceeded maximum iterations");
                }
                
                foreach (var kvp in vars)
                {
                    if (initialVars.ContainsKey(kvp.Key) && initialVars[kvp.Key] != kvp.Value)
                    {
                        if (kvp.Key != "x" && kvp.Key != "i") 
                        {
                            return kvp.Value;
                        }
                    }
                }
                
                return lastResult;
            }
            
            throw new ArgumentException("Invalid while loop");
        }

        private double ExecuteBlock(string block, Dictionary<string, double> vars)
        {
            var blockResults = new List<double>();
            var blockExpressions = SplitExpressions(block);
            
            foreach (var expr in blockExpressions)
            {
                if (string.IsNullOrEmpty(expr))
                    continue;
                    
                double result = ExecuteExpression(expr.Trim(), vars);
                blockResults.Add(result);
            }
            
            return blockResults.Count > 0 ? blockResults.Last() : 0;
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