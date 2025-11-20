using System;
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
           [InlineData("", "Invalid expression")]
           [InlineData("1 2", "Invalid expression")]
           [InlineData("1 2 +1", "Invalid expression")]
           public void GivenInvalidExpression_WhenEvaluateCalled_ThenThrowsException(string expression, string expectedMessage)
           {
               var exception = Assert.Throws<ArgumentException>(() => _calculator.Evaluate(expression));
               Assert.Contains(expectedMessage, exception.Message);
           }
           
           [Fact(DisplayName = "Evaluate should handle very large numbers correctly")]
           public void GivenVeryLargeNumber_WhenEvaluateCalled_ThenReturnsInfinity()
           {
               double result = _calculator.Evaluate("exp(1000)");
               Assert.True(double.IsInfinity(result));
           }
           
           [Theory(DisplayName = "EvaluateMultiple should handle errors in individual expressions")]
           [InlineData("2+3; 4/0; 3-4", 2, "Division by zero")]
           [InlineData("2--3; 4*2", 1, "Invalid sequence '--'")]
           [InlineData("1+1; sin(; 3-1", 2, "Mismatched parentheses")]
           [InlineData("x=x", 1, "Undefined variable 'x'")]
           [InlineData("x+y", 1, "Undefined variable")]
           [InlineData("sin=1", 1, "Cannot assign to function name")]
           [InlineData("1=2", 1, "Invalid variable name")]
           public void GivenInvalidExpressionInMultiple_WhenEvaluateMultipleCalled_ThenThrowsExceptionForThatExpression(string expressions, int errorIndex, string expectedError)
           {
               var exception = Assert.Throws<Exception>(() => _calculator.EvaluateMultiple(expressions));
               Assert.Contains(expectedError, exception.Message);
               Assert.Contains($"Error in expression {errorIndex}", exception.Message);
           }
           
           public void Dispose()
           {
           }
       }
   }