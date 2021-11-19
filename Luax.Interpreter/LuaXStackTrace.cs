using Luax.Parser.Ast;

namespace Luax.Interpreter
{
    public class LuaXStackTrace : LuaXAstCollection<LuaXStackFrame>
    {
        public LuaXStackFrame GetLastFrame()
        {
            return this[^1];
        }

        public void Add(LuaXMethod callSite, LuaXElementLocation location)
        {
            var newFrame = new LuaXStackFrame(callSite, location);
            Add(newFrame);
        }
    }
}
