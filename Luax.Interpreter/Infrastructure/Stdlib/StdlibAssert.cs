using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Luax.Interpreter.Expression;
using Luax.Parser.Ast;
using Luax.Parser.Ast.LuaExpression;

#pragma warning disable S125                // Sections of code should not be commented out
#pragma warning disable IDE1006             // Naming rule violation.

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibAssert
    {
        private static LuaXTypesLibrary mTypeLibrary;

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
        }

        //public static extern isTrue(condition : boolean, message : string) : void;
        [LuaXExternMethod("assert", "isTrue")]
        public static object isTrue(bool condition, string message)
        {
            if (!condition)
            {
                if (!string.IsNullOrEmpty(message))
                    message = " because " + message;
                throw new LuaXAssertionException("Expected the condition to be true but it is false" + (message ?? ""));
            }
            return null;
        }

        //public static extern isTrue(condition : boolean, message : string) : void;
        [LuaXExternMethod("assert", "isFalse")]
        public static object isFalse(bool condition, string message)
        {
            if (condition)
            {
                if (!string.IsNullOrEmpty(message))
                    message = " because " + message;
                throw new LuaXAssertionException("Expected the condition to be false but it is true" + (message ?? ""));
            }
            return null;
        }

        private static bool throws(LuaXObjectInstance action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (!action.Class.SearchMethod("invoke", null, out var method) || method.Arguments.Count != 0 || method.Static)
                throw new ArgumentException("The object passed does not have invoke method", nameof(action));

            bool thrown = false;
            try
            {
                LuaXMethodExecutor.Execute(method, mTypeLibrary, action, Array.Empty<object>(), out var _);
            }
            catch (LuaXExecutionException)
            {
                thrown = true;
            }
            return thrown;
        }

        //public static extern throws(action : assertAction, message : string) : void
        [LuaXExternMethod("assert", "throws")]
        public static object throws(LuaXObjectInstance action, string message)
        {
            if (!throws(action))
            {
                if (!string.IsNullOrEmpty(message))
                    message = " because " + message;
                throw new LuaXAssertionException("Expected the action to throw the exception but it does not" + (message ?? ""));
            }

            return null;
        }

        //public static extern throws(action : assertAction, message : string) : void
        [LuaXExternMethod("assert", "doesNotThrow")]
        public static object doesNotThrow(LuaXObjectInstance action, string message)
        {
            if (throws(action))
            {
                if (!string.IsNullOrEmpty(message))
                    message = " because " + message;
                throw new LuaXAssertionException("Expected the action to not throw the exception but it does" + (message ?? ""));
            }

            return null;
        }

        //public static extern approximatelyEquals(value : real, expected : real, delta : real, message : string) : void;
        [LuaXExternMethod("assert", "approximatelyEquals")]
        public static object approximatelyEquals(double value, double expected, double delta, string message)
        {
            if (delta == 0)
                delta = 1e-14;
            if (Math.Abs(value - expected) >= delta)
            {
                if (!string.IsNullOrEmpty(message))
                    message = " because " + message;
                throw new LuaXAssertionException($"Expected the value to be {expected}+-{delta} but it is {value}" + (message ?? ""));
            }
            return null;
        }

        private static bool ToConstant(object v, out LuaXConstant constant)
        {
            constant = null;
            var location = new LuaXElementLocation("internal", 0, 0);
            switch (v)
            {
                case int i:
                    constant = new LuaXConstant(i, location);
                    break;
                case double r:
                    constant = new LuaXConstant(r, location);
                    break;
                case string s:
                    constant = new LuaXConstant(s, location);
                    break;
                case DateTime dt:
                    constant = new LuaXConstant(dt, location);
                    break;
                case bool b:
                    constant = new LuaXConstant(b, location);
                    break;
            }
            return constant != null;
        }

        private static bool equalsLuaObjects(LuaXObjectInstance p1, LuaXObjectInstance p2)
        {
            if (p1 == null && p2 == null)
                return true;
            if (p1 == null || p2 == null)
                return false;

            if (p1.Class.LuaType.Name != p2.Class.LuaType.Name)
                return false;

            for (int i = 0; i < p1.Class.LuaType.Properties.Count; i++)
            {
                var p = p1.Class.LuaType.Properties[i];
                if (p.Static)
                    continue;
                if (p.Attributes.Any(attr => attr.Name == "IgnoreInEquals"))
                    continue;

                var o1 = p1.Properties[p.Name].Value;
                var o2 = p2.Properties[p.Name].Value;
                if (!equalsLuaValue(o1, o2))
                    return false;
            }

            return true;
        }

        private static bool equalsLuaValue(object p1, object p2)
        {
            if (p1 == null && p2 == null)
                return true;
            if (p1 == null || p2 == null)
                return false;

            if (p1 is LuaXObjectInstance o1 && p2 is LuaXObjectInstance o2)
                return equalsLuaObjects(o1, o2);
            else
            {
                if (!ToConstant(p1, out LuaXConstant c1))
                    return false;
                if (!ToConstant(p2, out LuaXConstant c2))
                    return false;

                var expression = new LuaXBinaryOperatorExpression(LuaXBinaryOperator.Equal, new LuaXConstantExpression(c1), new LuaXConstantExpression(c2), LuaXTypeDefinition.Boolean, null);
                bool v = false;
                try
                {
                    var rc = LuaXExpressionEvaluator.Evaluate(expression, mTypeLibrary, null, null);
                    if (rc is bool b)
                        v = b;
                }
                catch (LuaXExecutionException )
                {
                    //just consider that data is not equal if we cannot compare
                }
                return v;
            }
        }

        //public static extern equals(v1 : variant, v2 : variant, message : string) : void;
        [LuaXExternMethod("assert", "equals")]
        public static object equals(LuaXObjectInstance v1, LuaXObjectInstance v2, string message)
        {
            object p1, p2;
            p1 = (v1?.Properties["__data"].Value);
            p2 = (v2?.Properties["__data"].Value);

            var b = equalsLuaValue(p1, p2);
            if (!b)
            {
                if (!string.IsNullOrEmpty(message))
                    message = " because " + message;
                throw new LuaXAssertionException("Expected values to be equal but they aren't" + (message ?? ""));
            }
            return null;
        }
    }
}
