using System;
using System.IO;
using FluentAssertions;
using Luax.Interpreter.Execution;
using Luax.Parser;
using Luax.Parser.Ast;
using Xunit;

namespace Luax.Interpreter.Test
{
    public class TestProjectReader
    {
        public TestProjectReader()
        {
            LuaXProjectReader.ProjectContentProvider = new ResourcesProjectReaderContent(typeof(TestProjectReader).Assembly);
        }

        [Fact]
        public void ReadProject_Success_1()
        {
            var project = LuaXProjectReader.Read("Project1.ini");
            project.Name.Should().Be("Project1.ini");
            project.ProjectType.Should().Be(LuaXProjectType.ConsoleApp);
            project.Options.Should().HaveCount(6);
            project.Options[0].Key.Should().Be("name");
            project.Options[0].Value.Should().Be("Project1.ini");
            project.Options[1].Key.Should().Be("type");
            project.Options[1].Value.Should().Be("console");
            project.Options[2].Key.Should().Be("option1");
            project.Options[2].Value.Should().Be("");
            project.Options[3].Key.Should().Be("=option2");
            project.Options[3].Value.Should().Be("");
            project.Options[4].Key.Should().Be("option3");
            project.Options[4].Value.Should().Be("");
            project.Options[5].Key.Should().Be("option4");
            project.Options[5].Value.Should().Be("123");

            project.Files.Should().HaveCount(3);
            project.Files.Should().Contain("file1");
            project.Files.Should().Contain("file2");
            project.Files.Should().Contain("file3");
        }

        [Fact]
        public void ReadProject_Success_2()
        {
            var project = LuaXProjectReader.Read("AssignTest.luax");
            project.Name.Should().Be("AssignTest.luax");
            project.ProjectType.Should().Be(LuaXProjectType.ConsoleApp);
            project.Options.Should().HaveCount(2);
            project.Options[0].Key.Should().Be("name");
            project.Options[0].Value.Should().Be("AssignTest.luax");
            project.Options[1].Key.Should().Be("type");
            project.Options[1].Value.Should().Be("console");

            project.Files.Should().HaveCount(1);
            project.Files.Should().Contain("AssignTest.luax");
        }

        [Fact]
        public void ReadProject_Success_3()
        {
            var project = LuaXProjectReader.Read("Project2.ini");
            project.Name.Should().Be("Project2.ini");
            project.ProjectType.Should().Be(LuaXProjectType.TestSuite);
            project.Options.Should().HaveCount(2);
            project.Options[0].Key.Should().Be("name");
            project.Options[0].Value.Should().Be("Project2.ini");
            project.Options[1].Key.Should().Be("type");
            project.Options[1].Value.Should().Be("test");

            project.Files.Should().HaveCount(1);
            project.Files.Should().Contain("file1");
        }

        [Fact]
        public void ReadProject_Fail_UnknownSection()
        {
            ((Action)(() => LuaXProjectReader.Read("Project4.ini"))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void ReadProject_Fail_NoFiles()
        {
            ((Action)(() => LuaXProjectReader.Read("Project3.ini"))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void ReadProject_Fail_NoSection()
        {
            ((Action)(() => LuaXProjectReader.Read("Project5.ini"))).Should().Throw<LuaXAstGeneratorException>();
        }
    }
}



