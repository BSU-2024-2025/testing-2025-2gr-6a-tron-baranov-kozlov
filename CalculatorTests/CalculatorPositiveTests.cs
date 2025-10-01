using System;
using Xunit;

namespace CalculatorSolution.Tests
{
    public class CalculatorPositiveTests : IDisposable
    {
        private readonly Calculator _calculator;

        public CalculatorPositiveTests()
        {
            _calculator = new Calculator();
        }

        [Theory(DisplayName = "Evaluate should return correct result for valid expressions")]
        [InlineData("1+3", 4.0)]
        [InlineData("-1", -1.0)]
        [InlineData("-1-1", -2.0)]
        [InlineData("-(1+1)", -2.0)]
        [InlineData("((2))", 2.0)]
        [InlineData("3*9", 27.0)]
        [InlineData("3*(-5)", -15.0)]
        [InlineData("3+4*8", 35.0)]
        [InlineData("3-(9-8)", 2.0)]
        [InlineData("4-(3+5)*2", -12.0)]
        [InlineData("2-(4-(3+5)*2)*(-2)", -22.0)]
        [InlineData("sin(0)", 0.0)]
        [InlineData("cos(0)", 1.0)]
        [InlineData("exp(0)", 1.0)]
        [InlineData("sin(3.141592653589793)", 0.0, 1e-10)]
        [InlineData("cos(3.141592653589793)", -1.0, 1e-10)]
        [InlineData("exp(1)", 2.718281828459045, 1e-10)]
        [InlineData("2*sin(1.5707963267948966)", 2.0, 1e-10)]
        [InlineData("sin(1)+cos(1)", 1.3817732906760363, 1e-10)]
        [InlineData("exp(2)*2", 14.7781121978613, 1e-10)]
        [InlineData("sin(cos(0))", 0.8414709848078965, 1e-10)]
        [InlineData("12.3e4", 12.3e4)]
        [InlineData("12.3e-4", 12.3e-4)]
        [InlineData("-12.3e-4", -12.3e-4)]
        public void GivenValidExpression_WhenEvaluateCalled_ThenReturnsExpectedResult(string expression, double expected, double precision = 1e-10)
        {
            double result = _calculator.Evaluate(expression);
            Assert.Equal(expected, result, precision);
        }

        [Theory(DisplayName = "EvaluateMultiple should handle multiple expressions separated by semicolons")]
        [InlineData("2+3; 4*2; 3-4", new[] { 5.0, 8.0, -1.0 })]
        [InlineData("1+1; 2*2; 3/2", new[] { 2.0, 4.0, 1.5 })]
        [InlineData("sin(0); cos(0)", new[] { 0.0, 1.0 })]
        [InlineData("5", new[] { 5.0 })]
        [InlineData("x=1+2; y=x-3; x+y; 7*8", new[] { 3.0, 0.0, 3.0, 56.0 })]
        public void GivenMultipleExpressions_WhenEvaluateMultipleCalled_ThenReturnsAllResults(string expressions, double[] expected)
        {
            var results = _calculator.EvaluateMultiple(expressions);
            Assert.Equal(expected, results);
        }

        public void Dispose()
        {
        }
    }
}