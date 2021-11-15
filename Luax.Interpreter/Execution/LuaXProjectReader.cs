using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Luax.Parser.Ast;

namespace Luax.Interpreter.Execution
{
    /// <summary>
    /// The reader of the project.ini file
    /// </summary>
    public static class LuaXProjectReader
    {
        public static ILuaXProjectReaderContentProvider ProjectContentProvider { get; internal set; } = new DefaultLuaXProjectReaderContentProvider();

        public static LuaXProject Read(string projectFile)
        {
            if (projectFile.EndsWith(".luax"))
            {
                var project = new LuaXProject(projectFile)
                {
                    ProjectType = LuaXProjectType.ConsoleApp
                };
                project.Options.Add("type", "console");
                project.Files.Add(projectFile);
                return project;
            }
            else
            {
                using var reader = ProjectContentProvider.GetContentReader(projectFile);
                return Read(projectFile, reader);
            }
        }

        public static LuaXProject Read(string projectName, TextReader projectContent)
        {
            string projectFullDirectoryPath = ProjectContentProvider.GetProjectFullDirPath(projectName);

            var project = new LuaXProject(projectName)
            {
                ProjectType = LuaXProjectType.Unknown
            };

            var dfa = new ProjectReaderDfa(projectName, project, projectFullDirectoryPath, ProjectContentProvider);

            while (true)
            {
                var line = projectContent.ReadLine();
                if (!dfa.Feed(line))
                    break;
            }

            if (project.ProjectType == LuaXProjectType.Unknown)
            {
                project.ProjectType = LuaXProjectType.ConsoleApp;
                project.Options.Add("type", "console");
            }
            if (project.Files.Count == 0)
                throw new LuaXAstGeneratorException(new LuaXElementLocation(projectName, dfa.LineNo, 1), "The project does not have any files");
            return project;
        }
    }
}
