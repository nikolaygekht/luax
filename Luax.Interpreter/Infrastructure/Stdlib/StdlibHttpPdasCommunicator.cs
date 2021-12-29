using System;
using System.Collections.Generic;
using Luax.Interpreter.Expression;
using Luax.Parser.Ast;

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibHttpPdasCommunicator
    {
        private static LuaXTypesLibrary mTypeLibrary;

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
        }

        //public static extern sendFxmsg(url: string, fxmsg: string, callback: httpResponseCallback)
        [LuaXExternMethod("httpPdasCommunicator", "sendFxmsg")]
        public static object SendFxmsg(string url, string fxmsg, LuaXObjectInstance callback)
        {

            callback.Class.SearchMethod("onComplete", null, out LuaXMethod method);
            if (method == null)
                throw new ArgumentException("'onComplete' method not found in instance of 'httpResponseCallback' class", nameof(callback));

            string content = $"{fxmsg};{url}";
            LuaXMethodExecutor.Execute(method, mTypeLibrary, callback, new object[] { 200, content }, out var _);
            return null;
        }
    }
}
