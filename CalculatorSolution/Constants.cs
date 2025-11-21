using System.Collections.Generic;

namespace CalculatorSolution
{
    public static class Constants
    {
        public static readonly HashSet<string> KnownFunctions = new HashSet<string> { "sin", "cos", "exp" };
        public static readonly HashSet<string> KnownKeywords = new HashSet<string> { "if", "else", "while" };
        public const char ExpressionSeparator = ';';
        public const char AssignmentOperator = '=';
        public const int AssignmentPartsCount = 2;
        public static readonly char[] OperatorChars = { '+', '-', '*', '/', '<', '>' };
        public static readonly char[] Parenthesis = { '(', ')' };
        public static readonly char[] Braces = { '{', '}' };
        public static readonly string[] ComparisonOperators = { "<", ">", "<=", ">=", "==", "!=" };
    }
}