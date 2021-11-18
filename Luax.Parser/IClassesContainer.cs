using Luax.Parser.Ast;

namespace Luax.Parser
{
    public interface IClassesContainer
    {
        /// <summary>
        /// All the classes in the container
        /// </summary>
        LuaXClassCollection Classes { get; }
        /// <summary>
        /// The reference to the owner container
        /// </summary>
        IClassesContainer OwnerContainer { get; set; }
    }
}
