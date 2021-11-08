using System.IO;

namespace Luax.Interpreter.Execution
{
    internal class DefaultLuaXProjectReaderContentProvider : ILuaXProjectReaderContentProvider
    {
        public TextReader GetContentReader(string projectFilePath)
        {
            return File.OpenText(projectFilePath);
        }

        public string GetFileFullPath(string projectFullDirPath, string filePath)
        {
            if (Path.IsPathFullyQualified(filePath))
                return filePath;
            else
                return Path.Combine(projectFullDirPath, filePath);
        }

        public string GetProjectFullDirPath(string projectName)
        {
            return new FileInfo(projectName).Directory.FullName;
        }
    }
}