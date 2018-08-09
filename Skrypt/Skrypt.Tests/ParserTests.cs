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
        public void ShouldParseNumeric () {
            var program = new Engine.SkryptEngine(@"42").Parse();

            Assert.NotNull(program.Nodes.First());
            Assert.Single(program.Nodes);
            Assert.Equal(Tokenization.TokenTypes.NumericLiteral, program.Nodes.First().Type);
            Assert.Equal("42",program.Nodes.First().Body);
        }
    }
}
