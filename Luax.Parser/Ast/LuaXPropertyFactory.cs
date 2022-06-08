namespace Luax.Parser.Ast
{
    /// <summary>
    /// The factory for LuaXProperty
    /// </summary>
    public class LuaXPropertyFactory : LuaXVariableFactory<LuaXProperty>
    {
        private readonly bool mStatic;
        private readonly LuaXVisibility mVisibility;

        public LuaXPropertyFactory(bool @static, LuaXVisibility visibility)
        {
            mStatic = @static;
            mVisibility = visibility;
        }

        public override LuaXProperty Create(string name, LuaXTypeDefinition type, LuaXElementLocation location)
        {
            return new LuaXProperty()
            {
                Static = mStatic,
                Visibility = mVisibility,
                Name = name,
                LuaType = type,
                Location = location
            };
        }
    }
}
