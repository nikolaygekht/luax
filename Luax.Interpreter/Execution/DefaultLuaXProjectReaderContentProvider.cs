using System.IO;

namespace Luax.Interpreter.Execution
{
    internal class DefaultLuaXProjectReaderContentProvider : ILuaXProjectReaderContentProvider
    {
        public TextReader GetContentReader(string filePath)
        {
            return File.OpenText(filePath);
        }

        public string GetFileFullPath(string projectFullDirPath, string filePath)
        {
            if (Path.IsPathFullyQualified(filePath))
                return filePath;
            else
                return Path.Combine(projectFullDirPath, filePath);
        }

        public string GetProjectFullDirPath(string projectPath)
        {
            return new FileInfo(projectPath).Directory.FullName;
        }
    }
}