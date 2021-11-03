using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luax.Interpreter.Expression;
using Luax.Parser.Ast;

namespace Luax.Interpreter.Infrastructure
{
    /// <summary>
    /// The instance of the class type
    /// </summary>
    public class LuaXClassInstance
    {
        /// <summary>
        /// The class definition
        /// </summary>
        public LuaXClass LuaType { get; set; }

        /// <summary>
        /// The static properties of the class
        /// </summary>
        public LuaXVariableInstanceSet StaticProperties { get; }

        private LuaXMethod Constructor { get; }

        internal LuaXClassInstance(LuaXClass classDefinition)
        {
            LuaType = classDefinition;
            StaticProperties = InitializeStatic(classDefinition);

            if (SearchMethod(classDefinition.Name, null, out var constructor))
            {
                if (!constructor.ReturnType.IsVoid() || constructor.Arguments.Count > 0)
                    throw new LuaXAstGeneratorException(constructor.Location, "Constructors must have void return type and no parameters");
                Constructor = constructor;
            }
        }

        public LuaXObjectInstance New(LuaXTypesLibrary types)
        {
            var v = new LuaXObjectInstance(this);
            if (Constructor != null)
                LuaXMethodExecutor.Execute(Constructor, types, v, Array.Empty<object>(), out var _);
            return v;
        }

        internal static LuaXVariableInstanceSet InitializeStatic(LuaXClass definition)
            => InitializeProperties(definition, definition => definition.Properties.Where(p => p.Static));

        internal static LuaXVariableInstanceSet InitializeInstance(LuaXClass definition)
            => InitializeProperties(definition, definition => definition.Properties.Where(p => !p.Static));

        private static LuaXVariableInstanceSet InitializeProperties(LuaXClass definition, Func<LuaXClass, IEnumerable<LuaXProperty>> getter)
        {
            var propertySet = new LuaXVariableInstanceSet();
            while (definition != null)
            {
                var properties = getter(definition);
                foreach (var property in properties)
                    if (!propertySet.Contains(property.Name))
                        propertySet.Add(property.LuaType, property.Name);
                definition = definition.ParentClass;
            }
            return propertySet;
        }

        private readonly Dictionary<string, LuaXMethod> mMethodCache = new Dictionary<string, LuaXMethod>();

        /// <summary>
        /// Searches the method in the class
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="exactClass"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public bool SearchMethod(string methodName, string exactClass, out LuaXMethod method)
        {
            var key = $"{exactClass ?? LuaType.Name}.{methodName}";
            if (mMethodCache.TryGetValue(key, out method))
                return true;

            LuaXClass target = LuaType;
            if (!string.IsNullOrEmpty(exactClass))
            {
                while (target != null && target.Name != exactClass)
                    target = target.ParentClass;
            }

            if (target == null)
                return false;

            var rc = target.SearchMethod(methodName, out method);
            if (rc)
                mMethodCache.Add(key, method);
            return rc;
        }
    }
}
