using System;
using System.IO;
using FluentAssertions;
using Luax.Interpreter.Execution;
using Luax.Parser;
using Luax.Parser.Ast;
using Xunit;

namespace Luax.Interpreter.Test
{
    public class TestConsoleExecutor
    {
        public TestConsoleExecutor()
        {
            LuaXProjectReader.ProjectContentProvider = new ResourcesProjectReaderContent(typeof(TestProjectReader).Assembly);
            LuaXProjectExecutorBase.ReadFileCallback = name => ResourceReader.Read(typeof(TestProjectReader).Assembly, name);
        }

        [Fact]
        public void ExecuteConsole_Ok_Args_ReturnInt()
        {
            var project = LuaXProjectReader.Read("ConsoleAppOK1.luax");
            var executor = new LuaXConsoleExecutor(project);
            var i = executor.Run(new string[] { "a", "b", "c" });
            i.Should().Be(3);
        }

        [Fact]
        public void ExecuteConsole_Ok_NoArgs_ReturnVoid()
        {
            var project = LuaXProjectReader.Read("ConsoleAppOK2.luax");
            var executor = new LuaXConsoleExecutor(project);
            var i = executor.Run(new string[] { "a", "b", "c" });
            i.Should().Be(0);
        }

        [Fact]
        public void ExecuteConsole_Error_NoClass()
        {
            var project = LuaXProjectReader.Read("ConsoleAppError1.luax");
            var executor = new LuaXConsoleExecutor(project);
            ((Action)(() => executor.Run(Array.Empty<string>()))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void ExecuteConsole_Error_NoMain()
        {
            var project = LuaXProjectReader.Read("ConsoleAppError2.luax");
            var executor = new LuaXConsoleExecutor(project);
            ((Action)(() => executor.Run(Array.Empty<string>()))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void ExecuteConsole_Error_MainReturnType()
        {
            var project = LuaXProjectReader.Read("ConsoleAppError3.luax");
            var executor = new LuaXConsoleExecutor(project);
            ((Action)(() => executor.Run(Array.Empty<string>()))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void ExecuteConsole_Error_MainArgs()
        {
            var project = LuaXProjectReader.Read("ConsoleAppError4.luax");
            var executor = new LuaXConsoleExecutor(project);
            ((Action)(() => executor.Run(Array.Empty<string>()))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void ExecuteConsole_Error_MainNoStatic()
        {
            var project = LuaXProjectReader.Read("ConsoleAppError5.luax");
            var executor = new LuaXConsoleExecutor(project);
            ((Action)(() => executor.Run(Array.Empty<string>()))).Should().Throw<LuaXAstGeneratorException>();
        }
    }
}



