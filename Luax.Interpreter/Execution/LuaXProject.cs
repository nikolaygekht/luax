using System.Dynamic;

namespace Luax.Interpreter.Execution
{
    public enum LuaXProjectType
    {
        Unknown,
        ConsoleApp,
        TestSuite
    }

    /// <summary>
    /// The LuaX project file
    /// </summary>
    public class LuaXProject
    {
        public string Name { get; }

        public LuaXProjectType ProjectType {get ; internal set; }

        public LuaXProject(string name)
        {
            Name = name;
            Options.Add("name", name);
        }

        public LuaXProjectFiles Files { get; } = new LuaXProjectFiles();
        public LuaXProjectOptions Options { get; } = new LuaXProjectOptions();
    }
}
