using System;
using System.Collections.Generic;
using Luax.Interpreter.Expression;
using Luax.Parser.Ast;

#pragma warning disable S125  // remove commented code: false positive, it is not the code, it is luax prototype

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibHttpCommunicator
    {
        private static LuaXTypesLibrary mTypeLibrary;

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
            typeLibrary.SearchClass("httpCommunicator", out var mHttpCommunicatorClass);
            mHttpCommunicatorClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__httpClient", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
        }

        //public extern httpCommunicator() : void;
        [LuaXExternMethod("httpCommunicator", "httpCommunicator")]
        public static object HttpCommunicator(LuaXObjectInstance @this)
        {
            @this.Properties["__httpClient"].Value = new List<string>() { "test content" };
            return null;
        }

        //public extern setRequestHeader(name: string, value: string) : void;
        [LuaXExternMethod("httpCommunicator", "setRequestHeader")]
        public static object SetRequestHeader(LuaXObjectInstance @this, string name, string value)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties["__httpClient"].Value is not List<string> l)
                throw new ArgumentException("The httpCommunicator isn't properly initialized", nameof(@this));

            l.Add($"{name} = {value}");
            return null;
        }

        //public extern get(url: string, callback: httpResponseCallback) : void;
        [LuaXExternMethod("httpCommunicator", "get")]
        public static object Get(LuaXObjectInstance @this, string url, LuaXObjectInstance callback)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties["__httpClient"].Value is not List<string> l)
                throw new ArgumentException("The httpCommunicator isn't properly initialized", nameof(@this));

            callback.Class.SearchMethod("onComplete", null, out LuaXMethod method);
            if(method == null)
                throw new ArgumentException("'onComplete' method not found in instance of 'httpResponseCallback' class", nameof(callback));

            l.Add(url);
            string content = string.Join(';', l.ToArray());
            LuaXMethodExecutor.Execute(method, mTypeLibrary, callback, new object[] { 200, content }, out var _);

            return null;
        }

        //public extern delete(url: string, callback: httpResponseCallback) : void;
        [LuaXExternMethod("httpCommunicator", "delete")]
        public static object Delete(LuaXObjectInstance @this, string url, LuaXObjectInstance callback)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties["__httpClient"].Value is not List<string> l)
                throw new ArgumentException("The httpCommunicator isn't properly initialized", nameof(@this));

            callback.Class.SearchMethod("onComplete", null, out LuaXMethod method);
            if (method == null)
                throw new ArgumentException("'onComplete' method not found in instance of 'httpResponseCallback' class", nameof(callback));

            l.Add(url);
            string content = string.Join(';', l.ToArray());
            LuaXMethodExecutor.Execute(method, mTypeLibrary, callback, new object[] { 200, content }, out var _);

            return null;
        }

        //public extern post(url: string, body: string, callback: httpResponseCallback) : void;
        [LuaXExternMethod("httpCommunicator", "post")]
        public static object Post(LuaXObjectInstance @this, string url, string body, LuaXObjectInstance callback)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties["__httpClient"].Value is not List<string> l)
                throw new ArgumentException("The httpCommunicator isn't properly initialized", nameof(@this));

            callback.Class.SearchMethod("onError", null, out LuaXMethod method);
            if (method == null)
                throw new ArgumentException("'onError' method not found in instance of 'httpResponseCallback' class", nameof(@this));

            l.Add(url);
            l.Add(body);
            string content = string.Join(';', l.ToArray());
            LuaXMethodExecutor.Execute(method, mTypeLibrary, callback, new object[] { content }, out var _);

            return null;
        }
    }
}
