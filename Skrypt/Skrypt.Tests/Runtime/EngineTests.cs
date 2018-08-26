using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt;
using Skrypt.Engine;
using Xunit;

namespace Skrypt.Tests.Runtime {
    public class EngineTests {
        private SkryptEngine _engine;

        public EngineTests() {
            _engine = new SkryptEngine()
                .SetValue("log", new Action<object>(Console.WriteLine))
                .SetValue("assert", new Action<Library.Native.System.Boolean>(x => Assert.True(x)))
                .SetValue("equal", new Action<object, object>(Assert.Equal))
                ;
        }

        void Run (string code) {
            _engine.Parse(code);
        }

        [Fact]
        public void ShouldConstructArray() {
            Run(@"
                o = []
                assert(o.Length == 0)
            ");
        }

        [Theory]
        [InlineData(@"
                o = fn () {}
                assert(o() == null)
            ")]
        [InlineData(@"
                o = () => {}
                assert(o() == null)
            ")]
        [InlineData(@"
                o = _ => {}
                assert(o() == null)
            ")]
        public void ShouldConstructFunctionLiteral(string source) {
            Run(source);
        }

        [Theory]
        [InlineData(@"
                class o {
                    o () {}
                }

                assert(o().Type == o)
            ")]
        [InlineData(@"
                class o {
                    public class p {
                        p () {}
                    }
                }
                assert(o.p().Type == o.p)
            ")]
        public void ShouldConstructClass(string source) {
            Run(source);
        }
    }
}
