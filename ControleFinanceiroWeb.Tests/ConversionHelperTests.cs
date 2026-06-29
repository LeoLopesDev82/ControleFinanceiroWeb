using ControleFinanceiroWeb.Helpers;

namespace ControleFinanceiroWeb.Tests
{
    public class ConversionHelperTests
    {
        [Theory]
        [InlineData("123", 123)]
        [InlineData(" 456 ", 456)]
        public void ToNullableInt_ShouldReturnInteger_WhenStringIsValid(string input, int expected)
        {
            int? result = ConversionHelper.ToNullableInt(input);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("abc")]
        public void ToNullableInt_ShouldReturnNull_WhenStringIsInvalidOrEmpty(string? input)
        {
            int? result = ConversionHelper.ToNullableInt(input);

            Assert.Null(result);
        }

        [Theory]
        [InlineData("123,45", 123.45)]
        [InlineData("R$ 1.500,50", 1500.50)]
        [InlineData(" 25.5 ", 25.50)]
        [InlineData("0", 0.0)]
        public void ToNullableDecimal_ShouldReturnDecimal_WhenStringIsValid(string input, decimal expected)
        {
            decimal? result = ConversionHelper.ToNullableDecimal(input);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("abc")]
        public void ToNullableDecimal_ShouldReturnNull_WhenStringIsInvalidOrEmpty(string? input)
        {
            decimal? result = ConversionHelper.ToNullableDecimal(input);

            Assert.Null(result);
        }

        [Fact]
        public void ToNullableDateTime_ShouldReturnDateTime_WhenStringIsValid()
        {
            string dateStr = "2026-06-29";

            DateTime? result = ConversionHelper.ToNullableDateTime(dateStr);

            Assert.NotNull(result);
            Assert.Equal(new DateTime(2026, 6, 29), result.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("invalid-date")]
        public void ToNullableDateTime_ShouldReturnNull_WhenStringIsInvalid(string? input)
        {
            DateTime? result = ConversionHelper.ToNullableDateTime(input);

            Assert.Null(result);
        }

        [Fact]
        public void ToNullableString_ShouldReturnTrimmedString_WhenStringHasContent()
        {
            string input = "  Texto de teste   ";
            string? result = ConversionHelper.ToNullableString(input);

            Assert.Equal("Texto de teste", result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ToNullableString_ShouldReturnNull_WhenStringIsInvalidOrWhitespace(string? input)
        {
            string? result = ConversionHelper.ToNullableString(input);

            Assert.Null(result);
        }
    }
}