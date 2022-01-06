using System;
using System.Collections.Generic;
using Luax.Interpreter.Expression;
using Luax.Parser.Ast;

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibHttpPdasCommunicator
    {
        private static LuaXTypesLibrary mTypeLibrary;
        private static LuaXClassInstance mHttpPdasCommunicatorClass;

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
            typeLibrary.SearchClass("httpPdasCommunicator", out mHttpPdasCommunicatorClass);
            mHttpPdasCommunicatorClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__httpCommunicator", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
        }

        //public static extern create(httpCommunicator : httpCommunicator) : httpPdasCommunicator;
        [LuaXExternMethod("httpPdasCommunicator", "create")]
        public static object Create(LuaXObjectInstance httpCommunicator)
        {
            if (httpCommunicator.Properties["__httpClient"].Value is not List<string> l)
                throw new ArgumentException("The httpCommunicator isn't properly initialized", nameof(httpCommunicator));
            var @this = mHttpPdasCommunicatorClass.New(mTypeLibrary);
            @this.Properties["__httpCommunicator"].Value = httpCommunicator;

            httpCommunicator.Class.SearchMethod("setRequestHeader", null, out LuaXMethod setRequestHeader);
            LuaXMethodExecutor.Execute(setRequestHeader, mTypeLibrary, httpCommunicator, new object[] { "sdas_response-format", "1" }, out var _);
            LuaXMethodExecutor.Execute(setRequestHeader, mTypeLibrary, httpCommunicator, new object[] { "sdas_serialization", "x" }, out var _);

            return @this;
        }

        //public static extern sendMessage(url: string, fxmsg: string, callback: httpResponseCallback)
        [LuaXExternMethod("httpPdasCommunicator", "sendMessage")]
        public static object SendMessage(LuaXObjectInstance @this, string url, string fxmsg, LuaXObjectInstance callback)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties["__httpCommunicator"].Value is not LuaXObjectInstance httpCommunicator)
                throw new ArgumentException("The httpPdasCommunicator isn't properly initialized", nameof(@this));

            httpCommunicator.Class.SearchMethod("setRequestHeader", null, out LuaXMethod setRequestHeader);
            LuaXMethodExecutor.Execute(setRequestHeader, mTypeLibrary, httpCommunicator, new object[] { "PDAS_HEADER_DEFLATE", $"deflated('{fxmsg}')" }, out var _);

            httpCommunicator.Class.SearchMethod("get", null, out LuaXMethod @get);
            LuaXMethodExecutor.Execute(@get, mTypeLibrary, httpCommunicator, new object[] { url, callback }, out var _);
            return null;
        }
    }
}
