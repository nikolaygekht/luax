using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Luax.Interpreter.Execution;
using Luax.Parser;
using Xunit;

namespace Luax.Interpreter.Test
{
    public class TestTestExecutor
    {
        private readonly LuaXTestExecutor mExecutor;
        private readonly List<LuaXTestStatusEventArgs> mExecutionResults = new List<LuaXTestStatusEventArgs>();

        public TestTestExecutor()
        {
            LuaXProjectReader.ProjectContentProvider = new ResourcesProjectReaderContent(typeof(TestProjectReader).Assembly);
            LuaXProjectExecutorBase.ReadFileCallback = name => ResourceReader.Read(typeof(TestProjectReader).Assembly, name);
            var project = new LuaXProject("TestSuite")
            {
                ProjectType = LuaXProjectType.TestSuite
            };
            project.Options.Add("type", "test");
            project.Files.Add("TestSuite.luax");
            mExecutor = new LuaXTestExecutor(project);
            mExecutor.OnTest += (sender, args) => mExecutionResults.Add(args);
        }

        [Fact]
        public void NotAttributeClassIgnored()
        {
            mExecutor.Run(System.Array.Empty<string>());
            mExecutionResults.Should().HaveNotElementsMatching(args => args.Class == "toignore");
        }

        [Fact]
        public void NotAttributeMethodIgnored()
        {
            mExecutor.Run(System.Array.Empty<string>());
            mExecutionResults.Should().HaveNotElementsMatching(args => args.Class == "suite1" && args.Method == "nofact");
        }

        [Fact]
        public void ExecuteFact()
        {
            mExecutor.Run(System.Array.Empty<string>());
            var args = mExecutionResults.Find(args => args.Class == "suite1" && args.Method == "fact1");
            args.Should().NotBeNull();
            args.Data.Should().BeEmpty();
            args.Status.Should().Be(LuaXTestStatus.OK);
            args.Message.Should().BeEmpty();
        }

        [Fact]
        public void FailReturnType()
        {
            mExecutor.Run(System.Array.Empty<string>());
            var args = mExecutionResults.Find(args => args.Class == "suite1" && args.Method == "fact2");
            args.Should().NotBeNull();
            args.Data.Should().BeEmpty();
            args.Status.Should().Be(LuaXTestStatus.Incorrect);
        }

        [Fact]
        public void FailArguments()
        {
            mExecutor.Run(System.Array.Empty<string>());
            var args = mExecutionResults.Find(args => args.Class == "suite1" && args.Method == "fact3");
            args.Should().NotBeNull();
            args.Data.Should().BeEmpty();
            args.Status.Should().Be(LuaXTestStatus.Incorrect);
        }

        [Fact]
        public void FailWhenStatic()
        {
            mExecutor.Run(System.Array.Empty<string>());
            var args = mExecutionResults.Find(args => args.Class == "suite1" && args.Method == "fact4");
            args.Should().NotBeNull();
            args.Data.Should().BeEmpty();
            args.Status.Should().Be(LuaXTestStatus.Incorrect);
        }

        [Fact]
        public void ExecuteAssert()
        {
            mExecutor.Run(System.Array.Empty<string>());
            var args = mExecutionResults.Find(args => args.Class == "suite1" && args.Method == "fact5");
            args.Should().NotBeNull();
            args.Data.Should().BeEmpty();
            args.Status.Should().Be(LuaXTestStatus.Assert);
            args.Message.Should().Contain("assert should work");
        }

        [Fact]
        public void ExecuteTheory_OK()
        {
            mExecutor.Run(System.Array.Empty<string>());
            var args = mExecutionResults.Find(args => args.Class == "suite1" && args.Method == "theory1" &&
                        args.Data == "1, 1.5, \"abcd\", true");
            args.Should().NotBeNull();
            args.Status.Should().Be(LuaXTestStatus.OK);
        }

        [Fact]
        public void ExecuteTheory_Assert()
        {
            mExecutor.Run(System.Array.Empty<string>());
            var args = mExecutionResults.Find(args => args.Class == "suite1" && args.Method == "theory1" &&
                        args.Data == "2, 1.5, \"abcd\", true");
            args.Should().NotBeNull();
            args.Status.Should().Be(LuaXTestStatus.Assert);
        }

        [Fact]
        public void ExecuteTheory_WrongArgumentType()
        {
            mExecutor.Run(System.Array.Empty<string>());

            mExecutionResults.Should().HaveElementMatching(args => args.Class == "suite1" &&
                args.Method == "theory1" &&
                args.Status == LuaXTestStatus.Incorrect &&
                args.Message.Contains("1th argument"));

            mExecutionResults.Should().HaveElementMatching(args => args.Class == "suite1" &&
                args.Method == "theory1" &&
                args.Status == LuaXTestStatus.Incorrect &&
                args.Message.Contains("2th argument"));

            mExecutionResults.Should().HaveElementMatching(args => args.Class == "suite1" &&
                args.Method == "theory1" &&
                args.Status == LuaXTestStatus.Incorrect &&
                args.Message.Contains("3th argument"));

            mExecutionResults.Should().HaveElementMatching(args => args.Class == "suite1" &&
                args.Method == "theory1" &&
                args.Status == LuaXTestStatus.Incorrect &&
                args.Message.Contains("4th argument"));
        }

        [Fact]
        public void ExecuteTheory_WrongArgumentCount()
        {
            mExecutor.Run(System.Array.Empty<string>());

            mExecutionResults.Should().HaveElementMatching(args => args.Class == "suite1" &&
                args.Method == "theory1" &&
                args.Status == LuaXTestStatus.Incorrect &&
                args.Message.Contains("arguments count"));
        }
    }
}



