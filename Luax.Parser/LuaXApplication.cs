using Luax.Parser.Ast;
using Luax.Parser.Ast.Builder;

namespace Luax.Parser
{
    /// <summary>
    /// The application
    /// </summary>
    public class LuaXApplication
    {
        /// <summary>
        /// All the classes in the code base
        /// </summary>
        public LuaXClassCollection Classes { get; } = new LuaXClassCollection();

        /// <summary>
        /// All the packages in the code base
        /// </summary>
        public LuaXAstNamedCollection<LuaXPackage> Packages { get; } = new LuaXAstNamedCollection<LuaXPackage>();

        /// <summary>
        /// Constructor
        /// </summary>
        public LuaXApplication()
        {
            var body = TypesLibHeader.ReadTypesLib();
            Classes.AddRange(body.Classes);
            Packages.AddRange(body.Packages);
            body = StdlibHeader.ReadStdlib();
            Classes.AddRange(body.Classes);
            Packages.AddRange(body.Packages);
        }

        /// <summary>
        /// Adds Lua source to the application
        ///
        /// The method DO NOT compiles the method bodies.
        /// Use <see cref="Pass2"/> method to compile all bodies when all the sources are added.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="source"></param>
        public void AddSource(string name, string source)
        {
            var parser = new LuaXAstGenerator();
            var body = parser.Compile(name, source);
            Classes.AddRange(body.Classes);
            Packages.AddRange(body.Packages);
        }

        /// <summary>
        /// Compiles method's bodies
        /// </summary>
        public void Pass2()
        {
            //force indexing
            Classes.Search("", out _);
            Packages.Search("", out _);

            for (int i = 0; i < Classes.Count; i++)
            {
                var parser = new LuaXAstTreeCreator(Classes[i].Location.Source, Classes);
                Classes[i].Pass2(this, parser);
            }
        }
    }
}
