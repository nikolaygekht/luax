using System.Runtime.CompilerServices;
using Luax.Parser.Ast;

namespace Luax.Interpreter.Execution
{
    internal sealed class ProjectReaderDfa
    {
        private const int modeNothing = 0;
        private const int modeOptions = 1;
        private const int modeFiles = 2;
        private int mMode = modeNothing;
        private readonly LuaXProject mProject;
        private readonly string mProjectName;
        private readonly string mProjectFullDirectoryPath;
        private readonly ILuaXProjectReaderContentProvider mContentProvider;
        public int LineNo { get; private set; } = 0;

        public ProjectReaderDfa(string projectName, LuaXProject project, string projectFullDirectoryPath, ILuaXProjectReaderContentProvider contentProvider)
        {
            mProjectName = projectName;
            mProject = project;
            mProjectFullDirectoryPath = projectFullDirectoryPath;
            mContentProvider = contentProvider;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessSection(string line)
        {
            if (line == "[options]")
                mMode = modeOptions;
            else if (line == "[files]")
                mMode = modeFiles;
            else
                throw new LuaXAstGeneratorException(new LuaXElementLocation(mProjectName, LineNo, 1), "A section name can be either options or files");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ParseOption(string line, out string key, out string value)
        {
            var eqIndex = line.IndexOf('=');
            if (eqIndex < 1)
            {
                key = line;
                value = "";
            }
            else if (eqIndex == line.Length - 1)
            {
                key = line.Substring(0, eqIndex).Trim();
                value = "";
            }
            else
            {
                key = line.Substring(0, eqIndex).Trim();
                value = line[(eqIndex + 1)..].Trim();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessOption(string line)
        {
            ParseOption(line, out var key, out var value);

            if (key == "type")
            {
                if (value == "console")
                    mProject.ProjectType = LuaXProjectType.ConsoleApp;
                else if (value == "test")
                    mProject.ProjectType = LuaXProjectType.TestSuite;
                else
                    throw new LuaXAstGeneratorException(new LuaXElementLocation(mProjectName, LineNo, 1), "Unknown project type");
            }
            mProject.Options.Add(key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessFile(string line)
        {
            var filePath = mContentProvider.GetFileFullPath(mProjectFullDirectoryPath, line);
            mProject.Files.Add(filePath);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Feed(string line)
        {
            if (line == null)
                return false;

            LineNo++;

            line = line.Trim();
            if (line.StartsWith(';') || line.StartsWith("--"))
                return true;

            if (line.StartsWith('['))
            {
                ProcessSection(line);
                return true;
            }

            switch (mMode)
            {
                case modeNothing:
                    throw new LuaXAstGeneratorException(new LuaXElementLocation(mProjectName, LineNo, 1), "A comment or a section name is expected here");
                case modeFiles:
                    ProcessFile(line);
                    break;
                case modeOptions:
                    ProcessOption(line);
                    break;
            }

            return true;
        }
    }
}
