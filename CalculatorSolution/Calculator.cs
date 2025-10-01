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
            var tokens = _tokenizer.Tokenize(expression);
            var rpn = _parser.ToRPN(tokens);
            return _evaluator.EvaluateRPN(rpn, vars);
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
            if (expression.Contains(Constants.AssignmentOperator))
            {
                var parts = expression.Split(Constants.AssignmentOperator, Constants.AssignmentPartsCount);
                if (parts.Length != Constants.AssignmentPartsCount)
                    throw new ArgumentException(ErrorMessages.InvalidAssignment);

                var varName = parts[0].Trim().ToLower();
                ValidateVariableName(varName);

                var value = Evaluate(parts[1].Trim(), vars);
                vars[varName] = value;
                return value;
            }

            return Evaluate(expression, vars);
        }

        private void ValidateVariableName(string varName)
        {
            if (string.IsNullOrWhiteSpace(varName))
                throw new ArgumentException(ErrorMessages.InvalidVariableName);

            if (!varName.All(char.IsLetter))
                throw new ArgumentException(ErrorMessages.InvalidVariableName);

            if (Constants.KnownFunctions.Contains(varName))
                throw new ArgumentException(ErrorMessages.CannotAssignToFunction);
        }
    }
}