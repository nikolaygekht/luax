using Luax.Parser.Ast;

namespace Luax.Interpreter
{
    public class LuaXStackFrame
    {
        public LuaXElementLocation Location { get; }
        public LuaXMethod CallSite { get; }

        public LuaXStackFrame(LuaXMethod callSite, LuaXElementLocation callLocation)
        {
            Location = callLocation;
            CallSite = callSite;
        }

        public bool IsTheSame(LuaXStackFrame otherFrame)
        {
            if (otherFrame == null || CallSite == null && CallSite != otherFrame.CallSite ||
                Location == null && Location != otherFrame.Location)
                return false;

            return Location?.IsTheSame(otherFrame.Location) == true && CallSite != null &&
                   CallSite.Class.Name == otherFrame.CallSite.Class.Name && CallSite.Name == otherFrame.CallSite.Name;
        }
    }
}