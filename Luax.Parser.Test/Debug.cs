using Luax.Parser.Test.Utils;
using Xunit;

namespace Luax.Parser.Test
{
    public class Debug
    {
        //[Fact]
        public void Test()
        {
            var app = new LuaXApplication();
            app.CompileResource("Debug");
            app.Pass2();
        }
    }
}
