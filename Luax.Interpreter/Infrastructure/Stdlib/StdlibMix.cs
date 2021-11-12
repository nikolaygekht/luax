using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hime.SDK.Grammars.LR;

#pragma warning disable S125 // Sections of code should not be commented out

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibMix
    {
        //public static extern isTrue(condition : boolean, message : string) : void;
        [LuaXExternMethod("stdlib", "print")]
        public static object Print(string text)
        {
            Console.WriteLine("{0}", text);
            return null;
        }
    }

}
