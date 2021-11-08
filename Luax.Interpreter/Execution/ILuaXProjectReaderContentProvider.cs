
using System.IO;

namespace Luax.Interpreter.Execution
{
    /// <summary>
    /// The LuaX project reader content provider
    /// </summary>
    public interface ILuaXProjectReaderContentProvider
    {
        /// <summary>
        /// Get absolute path for inner project files
        /// </summary>
        /// <param name="projectFullDirPath">Full path for directory when contains project file</param>
        /// <param name="filePath">Relative or absolute inner project file path</param>
        /// <returns>Full absolute file path for inner project file</returns>
        string GetFileFullPath(string projectFullDirPath, string filePath);

        /// <summary>
        /// Get content reader for specific file
        /// </summary>
        /// <param name="filePath">Specific file</param>
        /// <returns>TextReader</returns>
        TextReader GetContentReader(string filePath);

        /// <summary>
        /// Get absolute directory path for specific project
        /// </summary>
        /// <param name="projectPath">Path to specific project</param>
        /// <returns>Absolute directory path</returns>
        string GetProjectFullDirPath(string projectPath);
    }
}
