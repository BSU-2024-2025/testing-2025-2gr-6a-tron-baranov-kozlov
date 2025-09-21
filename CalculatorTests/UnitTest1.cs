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
        [InlineData("3*9", 27.0)]
        [InlineData("3.2+(9-8)", 4.2)]
        [InlineData("4+2*(3+3)", 16.0)]
        [InlineData("3+4*8", 35.0)]
        [InlineData("sin(0)", 0.0)]
        [InlineData("4.2030e4+2.1e5", 252030.0)]
        public void GivenValidExpression_WhenEvaluateCalled_ThenReturnsExpectedResult(string expression, double expected)
        {
            double result = _calculator.Evaluate(expression);
            Assert.Equal(expected, result, 1e-10);
        }

        public void Dispose()
        {
        }
    }

    public class CalculatorNegativeTests : IDisposable
    {
        private readonly Calculator _calculator;

        public CalculatorNegativeTests()
        {
            _calculator = new Calculator();
        }

        [Theory(DisplayName = "Evaluate should throw exception for invalid expressions")]
        [InlineData("9/0", "Division by zero")]
        [InlineData("2--3", "Invalid sequence '--' at position 3")]
        [InlineData("2-(4-(3+2)", "Mismatched parentheses at position 6")]
        [InlineData("2.1e5.3", "Invalid number at position 1")]
        public void GivenInvalidExpression_WhenEvaluateCalled_ThenThrowsException(string expression, string expectedMessage)
        {
            var exception = Assert.Throws<Exception>(() => _calculator.Evaluate(expression));
            Assert.Contains(expectedMessage, exception.Message);
        }

        public void Dispose()
        {
        }
    }
    }
