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
        private Assembly assembly;

        public ResourcesProjectReaderContent(Assembly assembly)
        {
            this.assembly = assembly;
        }

        public TextReader GetContentReader(string projectFilePath)
        {
            return new StringReader(ResourceReader.Read(assembly, projectFilePath));
        }

        public string GetFileFullPath(string projectFullDirPath, string filePath)
        {
            return Path.Combine(projectFullDirPath, filePath);
        }

        public string GetProjectFullDirPath(string projectName)
        {
            return "";
        }
    }
}



