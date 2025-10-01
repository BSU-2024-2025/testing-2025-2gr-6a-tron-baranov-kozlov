namespace CalculatorSolution
{
    public static class Constants
    {
        public static readonly HashSet<string> KnownFunctions = new HashSet<string> { "sin", "cos", "exp" };
        public const char ExpressionSeparator = ';';
        public const char AssignmentOperator = '=';
        public const int AssignmentPartsCount = 2;
        public static readonly char[] OperatorChars = { '+', '-', '*', '/' };
        public static readonly char[] Parenthesis = { '(', ')' };
    }
}