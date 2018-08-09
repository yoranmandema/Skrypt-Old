using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Skrypt;

namespace Skrypt.Tests {
    public class ParserTests {
        [Fact]
        public void ShouldParseNull() {
            var program = new Engine.SkryptEngine(@"null").Parse();

            Assert.NotNull(program.Nodes.First());
            Assert.Single(program.Nodes);
            Assert.Equal(Tokenization.TokenTypes.NullLiteral, program.Nodes.First().Type);
            Assert.Equal("null", program.Nodes.First().Body);
        }

        [Fact]
        public void ShouldParseNumeric () {
            var program = new Engine.SkryptEngine(@"42").Parse();

            Assert.NotNull(program.Nodes.First());
            Assert.Single(program.Nodes);
            Assert.Equal(Tokenization.TokenTypes.NumericLiteral, program.Nodes.First().Type);
            Assert.Equal("42",program.Nodes.First().Body);
        }

        [Theory]
        [InlineData(0, "0")]
        [InlineData(42, "42")]
        [InlineData(0.14, "0.14")]
        [InlineData(3.14159, "3.14159")]
        [InlineData(6.02214179e+23, "6.02214179e+23")]
        [InlineData(1.492417830e-10, "1.492417830e-10")]
        [InlineData(0, "0x0")]
        [InlineData(0, "0x0;")]
        [InlineData(0xabc, "0xabc")]
        [InlineData(0xdef, "0xdef")]
        [InlineData(0X1A, "0x1A")]
        [InlineData(0x10, "0x10")]
        [InlineData(0x100, "0x100")]
        [InlineData(0X04, "0x04")]
        [InlineData(1.189008226412092e+38, "0x5973772948c653ac1971f1576e03c4d4")]
        [InlineData(18446744073709552000d, "0xffffffffffffffff")]
        public void ShouldParseNumericLiterals(object expected, string source) {
            var program = new Engine.SkryptEngine(source).Parse();

            Assert.NotNull(program.Nodes.First());
            Assert.Single(program.Nodes);
            Assert.Equal(Convert.ToDouble(expected), Convert.ToDouble(program.Nodes.First().Body));
        }

        [Theory]
        [InlineData("Hello", @"""Hello""")]
        [InlineData("\n\r\t\v\b\f\\\'\"\0", @"""\n\r\t\v\b\f\\\'\""\0""")]
        // Todo: Add support for escaped unicode characters
        //[InlineData("\u0061", @"'\u0061'")]
        //[InlineData("\x61", @"'\x61'")]
        [InlineData("Hello\nworld", @"""Hello\nworld""")]
        [InlineData("Hello\\\nworld", @"""Hello\\\nworld""")]
        public void ShouldParseStringLiterals(string expected, string source) {
            var program = new Engine.SkryptEngine(source).Parse();

            Assert.NotNull(program.Nodes.First());
            Assert.Single(program.Nodes);
            Assert.Equal(expected, program.Nodes.First().Body);
        }
    }
}
