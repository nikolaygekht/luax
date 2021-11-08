using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable S125 // Sections of code should not be commented out

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal class StdlibMix
    {
        //public static extern isTrue(condition : boolean, message : string) : void;
        [LuaXExternMethod("stdlib", "print")]
        public static object print(string text)
        {
            Console.WriteLine("{0}", text);
            return null;
        }
    }
}
