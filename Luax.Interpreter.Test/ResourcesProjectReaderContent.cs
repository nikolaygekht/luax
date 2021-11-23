using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Luax.Interpreter.Execution;
using Luax.Parser;
using Luax.Parser.Ast;
using Xunit;

namespace Luax.Interpreter.Test
{
    public class ResourcesProjectReaderContent : ILuaXProjectReaderContentProvider
    {
        private readonly Assembly assembly;

        public ResourcesProjectReaderContent(Assembly assembly)
        {
            this.assembly = assembly;
        }

        public TextReader GetContentReader(string filePath)
        {
            return new StringReader(ResourceReader.Read(assembly, filePath));
        }

        public string GetFileFullPath(string projectFullDirPath, string filePath)
        {
            return Path.Combine(projectFullDirPath, filePath);
        }

        public string GetProjectFullDirPath(string projectPath)
        {
            return "";
        }
    }
}



