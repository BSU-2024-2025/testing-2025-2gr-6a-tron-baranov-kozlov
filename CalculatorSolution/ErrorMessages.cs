namespace CalculatorSolution
{
    public static class ErrorMessages
    {
        public const string InvalidExpression = "Invalid expression";
        public const string InvalidAssignment = "Invalid assignment";
        public const string InvalidVariableName = "Invalid variable name";
        public const string CannotAssignToFunction = "Cannot assign to function name";
        public const string InvalidNumberFormat = "Invalid number at position {0}";
        public const string InvalidCharacter = "Invalid character '{0}' at position {1}";
        public const string UnknownFunction = "Unknown function {0} at position {1}";
        public const string FunctionAsVariable = "Cannot use function name '{0}' as variable at position {1}";
        public const string InvalidOperatorSequence = "Invalid sequence '--' at position {0}";
        public const string UnexpectedOperator = "Unexpected operator at position {0}";
        public const string MismatchedParentheses = "Mismatched parentheses at position {0}";
        public const string MismatchedBraces = "Mismatched braces at position {0}";
        public const string DivisionByZero = "Division by zero";
        public const string UndefinedVariable = "Undefined variable '{0}' at position {1}";
        public const string ExpressionError = "Error in expression {0}: '{1}' - {2}";
        public const string UnexpectedKeyword = "Unexpected keyword '{0}' at position {1}";
        public const string InvalidConditional = "Invalid conditional expression at position {0}";
    }
}