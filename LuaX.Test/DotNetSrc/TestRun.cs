using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Luax.Interpreter.Execution;
using Xunit;

namespace Luax.Test.DotNetSrc
{
    public class TestRun
    {
        private static TextReader GetReader(string filePath)
        {
            var fileResourceName = "Luax.Test." + (filePath.Replace('/', '.')
                    .Replace('\\', '.'));
            var assembly = typeof(TestRun).Assembly;
            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                if (resourceName == fileResourceName)
                {
                    var stream = assembly.GetManifestResourceStream(resourceName);
                    return new StreamReader(stream, Encoding.UTF8, true);
                }
            }
            throw new IOException($"Resource for {filePath} is not found");
        }

        private static string GetContent(string filePath)
        {
            using var reader = GetReader(filePath);
            return reader.ReadToEnd();
        }

        private sealed class TestContentProvider : ILuaXProjectReaderContentProvider
        {
            public TextReader GetContentReader(string filePath)
                => GetReader(filePath);

            public string GetFileFullPath(string projectFullDirPath, string filePath)
            {
                return filePath;
            }

            public string GetProjectFullDirPath(string projectPath)
            {
                return projectPath;
            }
        }

        [Fact]
        public void RunTestProject()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            LuaXProjectReader.ProjectContentProvider = new TestContentProvider();
            var project = LuaXProjectReader.Read("project.ini");
            project.Should().NotBeNull();

            LuaXProjectExecutorBase.ReadFileCallback = (name) => GetContent(name);
            var executor = new LuaXTestExecutor(project);
            var failed = new List<string>();
            executor.OnTest += (_, args) =>
            {
                if (args.Status != LuaXTestStatus.OK)
                    failed.Add($"{args.Class}.{args.Method}({args.Data})");
            };
            var rc = executor.Run(Array.Empty<string>());
            failed.Should().BeEmpty();
            executor.TotalTests.Should().Be(executor.SuccessfullTests);
            rc.Should().Be(0);
        }
    }
}
