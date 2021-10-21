namespace Luax.Parser.Ast
{
    /// <summary>
    /// The factory for LuaXProperty
    /// </summary>
    internal class LuaXPropertyFactory : LuaXVariableFactory<LuaXProperty>
    {
        private readonly bool mStatic;
        private readonly bool mPublic;

        public LuaXPropertyFactory(bool @static, bool @public)
        {
            mStatic = @static;
            mPublic = @public;
        }

        public override LuaXProperty Create(string name, LuaXTypeDefinition type)
        {
            return new LuaXProperty()
            {
                Static = mStatic,
                Public = mPublic,
                Name = name,
                LuaType = type,
            };
        }
    }
}
