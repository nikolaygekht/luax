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
        public static Func<string, TextReader> OpenProjectFileCallback { get; set; } = projectFile => File.OpenText(projectFile);

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
                using var reader = OpenProjectFileCallback(projectFile);
                return Read(projectFile, reader);
            }
        }

        public static LuaXProject Read(string projectName, TextReader projectContent)
        {
            const int modeNothing = 0;
            const int modeOptions = 1;
            const int modeFiles = 2;
            int mode = modeNothing;
            var project = new LuaXProject(projectName)
            {
                ProjectType = LuaXProjectType.Unknown
            };
            int lineNo = 0;
            while (true)
            {
                lineNo++;
                var line = projectContent.ReadLine();
                if (line == null)
                    break;
                line = line.Trim();
                if (line.StartsWith(';') || line.StartsWith("--"))
                    continue;
                if (line.StartsWith('['))
                {
                    if (line == "[options]")
                        mode = modeOptions;
                    else if (line == "[files]")
                        mode = modeFiles;
                    else
                        throw new LuaXAstGeneratorException(new LuaXElementLocation(projectName, lineNo, 1), "A section name can be either options or files");
                    continue;
                }
                switch (mode)
                {
                    case modeNothing:
                        throw new LuaXAstGeneratorException(new LuaXElementLocation(projectName, lineNo, 1), "A comment or a section name is expected here");
                    case modeFiles:
                        project.Files.Add(line);
                        break;
                    case modeOptions:
                        {
                            var eqIndex = line.IndexOf('=');
                            if (eqIndex < 1)
                                project.Options.Add(line, "");
                            else if (eqIndex == line.Length - 1)
                                project.Options.Add(line.Substring(0, eqIndex).Trim(), "");
                            else
                            {
                                var key = line.Substring(0, eqIndex).Trim();
                                var value = line[(eqIndex + 1)..].Trim();
                                if (key == "type")
                                {
                                    if (value == "console")
                                        project.ProjectType = LuaXProjectType.ConsoleApp;
                                    else if (value == "test")
                                        project.ProjectType = LuaXProjectType.TestSuite;
                                    else
                                        throw new LuaXAstGeneratorException(new LuaXElementLocation(projectName, lineNo, 1), "Unknown project type");
                                }
                                project.Options.Add(key, value);
                            }
                        }
                        break;
                }
            }
            if (project.ProjectType == LuaXProjectType.Unknown)
            {
                project.ProjectType = LuaXProjectType.ConsoleApp;
                project.Options.Add("type", "console");
            }
            if (project.Files.Count == 0)
                throw new LuaXAstGeneratorException(new LuaXElementLocation(projectName, lineNo, 1), "The project does not have any files");
            return project;
        }
    }
}
