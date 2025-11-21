using System;
using System.Linq;
using Xunit;

namespace CalculatorSolution.Tests
{
    public class CalculatorNegativeTests : IDisposable
    {
        private readonly Calculator _calculator;
        
        public CalculatorNegativeTests()
        {
            _calculator = new Calculator();
        }
        
        [Theory(DisplayName = "Evaluate should throw exception for invalid expressions")]
        [InlineData("9/0", "Division by zero")]
        [InlineData("2--3", "Invalid sequence '--'")]
        [InlineData("2-(4-(3+2)", "Mismatched parentheses")]
        [InlineData("2.1e5.3", "Invalid number")]
        [InlineData("sin()", "Invalid expression")]
        [InlineData("sin(1,2)", "Invalid character ','")]
        [InlineData("sin", "Cannot use function name 'sin' as variable")]
        [InlineData("sin(1", "Mismatched parentheses")]
        [InlineData("sin(1))", "Mismatched parentheses")]
        [InlineData("sin(1)2", "Invalid expression")]
        [InlineData("1 2", "Invalid expression")]
        [InlineData("1 2 +1", "Invalid expression")]
        public void GivenInvalidExpression_WhenEvaluateCalled_ThenThrowsException(string expression, string expectedMessage)
        {
            var exception = Assert.Throws<Exception>(() => _calculator.Evaluate(expression));
            Assert.Contains(expectedMessage, exception.Message);
        }
        
        [Fact(DisplayName = "Evaluate should handle very large numbers correctly")]
        public void GivenVeryLargeNumber_WhenEvaluateCalled_ThenReturnsInfinity()
        {
            var results = _calculator.Evaluate("exp(1000)");
            Assert.Single(results);
            Assert.True(double.IsInfinity(results[0]));
        }
        
        [Theory(DisplayName = "Evaluate should handle errors in individual expressions")]
        [InlineData("2+3; 4/0; 3-4", 2, "Division by zero")]
        [InlineData("2--3; 4*2", 1, "Invalid sequence '--'")]
        [InlineData("1+1; sin(; 3-1", 2, "Mismatched parentheses")]
        [InlineData("x=x", 1, "Undefined variable")] // Изменено
        [InlineData("x+y", 1, "Undefined variable")]
        [InlineData("sin=1", 1, "Cannot assign to function name")]
        [InlineData("1=2", 1, "Assignment not allowed in expression")]
        public void GivenInvalidExpressionInMultiple_WhenEvaluateCalled_ThenThrowsExceptionForThatExpression(string expressions, int errorIndex, string expectedError)
        {
            var exception = Assert.Throws<Exception>(() => _calculator.Evaluate(expressions));
            Assert.Contains(expectedError, exception.Message);
            Assert.Contains($"Error in expression {errorIndex}", exception.Message);
        }

        [Theory(DisplayName = "Evaluate should throw exception for invalid if expressions")]
        [InlineData("if (", "Mismatched parentheses")] // Изменено
        [InlineData("if )", "Invalid conditional expression at position 1")]
        [InlineData("if (x>5", "Mismatched parentheses")] // Изменено
        [InlineData("if x>5) 10", "Invalid conditional expression at position 1")]
        [InlineData("if () 10", "Invalid expression")]
        [InlineData("if (x>) 10", "Undefined variable")]
        [InlineData("if (5) 10 else", "Invalid conditional expression at position 1")]
        [InlineData("if (1) {", "Mismatched braces at position 1")]
        [InlineData("if (1) { x=5", "Mismatched braces at position 1")]
        [InlineData("if (1) x=5 else { y=10", "Mismatched braces at position 1")]
        [InlineData("if (1) { x=5 } else", "Invalid conditional expression at position 1")]
        [InlineData("if (1) { x=5 } else {", "Mismatched braces at position 1")]
        [InlineData("if 10", "Invalid conditional expression at position 1")]
        public void GivenInvalidIfExpression_WhenEvaluateCalled_ThenThrowsException(string expression, string expectedMessage)
        {
            var exception = Assert.Throws<Exception>(() => _calculator.Evaluate(expression));
            Assert.Contains(expectedMessage, exception.Message);
        }

        [Theory(DisplayName = "Evaluate should throw exception for invalid while expressions")]
        [InlineData("while (", "Mismatched parentheses")] // Изменено
        [InlineData("while )", "Invalid while loop")]
        [InlineData("while (x<5", "Mismatched parentheses")] // Изменено
        [InlineData("while x<5) x=x+1", "Invalid while loop")]
        [InlineData("while () x=x+1", "Invalid expression")]
        [InlineData("while (x>) x=x+1", "Undefined variable")]
        [InlineData("while (1) {", "Mismatched braces at position 1")] // Изменено
        [InlineData("while (1) { x=5", "Mismatched braces at position 1")] // Изменено
        [InlineData("while 10", "Invalid while loop")]
        [InlineData("while (1) x=", "Invalid expression")]
        [InlineData("while (1) x=;", "Invalid expression")]
        public void GivenInvalidWhileExpression_WhenEvaluateCalled_ThenThrowsException(string expression, string expectedMessage)
        {
            var exception = Assert.Throws<Exception>(() => _calculator.Evaluate(expression));
            Assert.Contains(expectedMessage, exception.Message);
        }

        [Theory(DisplayName = "Evaluate should handle infinite loop protection")]
        [InlineData("x=1; while (1) x=x+1", "While loop exceeded maximum iterations")]
        [InlineData("while (1) 1", "While loop exceeded maximum iterations")]
        [InlineData("x=10; while (x > 0) x=x+1", "While loop exceeded maximum iterations")]
        public void GivenInfiniteLoop_WhenEvaluateCalled_ThenThrowsException(string expressions, string expectedMessage)
        {
            var exception = Assert.Throws<Exception>(() => _calculator.Evaluate(expressions));
            Assert.Contains(expectedMessage, exception.Message);
        }

        [Theory(DisplayName = "Evaluate should handle undefined variables in conditions")]
        [InlineData("if (x > y) 10", "Undefined variable")]
        [InlineData("while (x < y) z=1", "Undefined variable")]
        public void GivenUndefinedVariablesInConditions_WhenEvaluateCalled_ThenThrowsException(string expression, string expectedMessage)
        {
            var exception = Assert.Throws<Exception>(() => _calculator.Evaluate(expression));
            Assert.Contains(expectedMessage, exception.Message);
        }

        [Theory(DisplayName = "Evaluate should handle syntax errors in conditional blocks")]
        [InlineData("if (1) { x= }", "Invalid expression")]
        [InlineData("if (1) { x=5; y= }", "Invalid expression")]
        [InlineData("while (1) { x= }", "Invalid expression")]
        [InlineData("if (1) { 2+ }", "Invalid expression")]
        [InlineData("if (1) { sin( }", "Mismatched parentheses")] // Изменено
        [InlineData("while (1) { 1 2 }", "Invalid expression")]
        public void GivenSyntaxErrorsInBlocks_WhenEvaluateCalled_ThenThrowsException(string expression, string expectedMessage)
        {
            var exception = Assert.Throws<Exception>(() => _calculator.Evaluate(expression));
            Assert.Contains(expectedMessage, exception.Message);
        }

        [Theory(DisplayName = "Evaluate should handle complex nested errors")]
        [InlineData("if (if) 10", "Invalid conditional expression at position 1")]
        [InlineData("while (while) x=1", "Invalid while loop")]
        [InlineData("if (1) while (2) { { }", "Mismatched braces at position 1")]
        [InlineData("while (1) if (2) { { }", "Mismatched braces at position 1")]
        public void GivenComplexNestedErrors_WhenEvaluateCalled_ThenThrowsException(string expression, string expectedMessage)
        {
            var exception = Assert.Throws<Exception>(() => _calculator.Evaluate(expression));
            Assert.Contains(expectedMessage, exception.Message);
        }

        [Theory(DisplayName = "Evaluate should handle assignment errors in conditions")]
        [InlineData("if (x=5) 10", "Assignment not allowed in condition")]
        [InlineData("while (x=1) y=2", "Assignment not allowed in condition")]
        [InlineData("if (sin=1) 10", "Assignment not allowed in condition")]
        [InlineData("while (1=2) x=1", "Assignment not allowed in condition")]
        public void GivenAssignmentErrorsInConditions_WhenEvaluateCalled_ThenThrowsException(string expression, string expectedMessage)
        {
            var exception = Assert.Throws<Exception>(() => _calculator.Evaluate(expression));
            Assert.Contains(expectedMessage, exception.Message);
        }
        
        public void Dispose()
        {
        }
    }
}