﻿using System;
using System.IO;
using Luax.Interpreter.Execution.Coverage;
using Luax.Interpreter.Infrastructure;
using Luax.Parser;

namespace Luax.Interpreter.Execution
{
    /// <summary>
    /// The base class for all executors
    /// </summary>
    public abstract class LuaXProjectExecutorBase
    {
        private LuaXApplication mApplication;
        private LuaXTypesLibrary mTypeLibrary;

        protected LuaXApplication Application => mApplication;
        public LuaXTypesLibrary TypesLibrary => mTypeLibrary;
        protected LuaXProject Project { get; }
        public bool EnableCoverage { get; set; }

        private CoverageAnalyzer mAnalyzer;

        public CoverageReport CoverageReport => mAnalyzer?.Report;

        public static Func<string, string> ReadFileCallback { get; set;  } = File.ReadAllText;

        protected LuaXProjectExecutorBase(LuaXProject project)
        {
            Project = project;
        }

        protected static string[] RemoveProjectNameFromCommandLine(string[] args)
        {
            if (args.Length < 2)
                return Array.Empty<string>();
            else
            {
                var r = new string[args.Length - 1];
                for (int i = 0; i < r.Length; i++)
                    r[i] = args[i + 1];
                return r;
            }
        }

        protected void Initialize(LuaXProject project)
        {
            mApplication = new LuaXApplication();
            foreach (var file in project.Files)
                mApplication.AddSource(file, ReadFileCallback(file));
            mApplication.Pass2();
            mTypeLibrary = new LuaXTypesLibrary(mApplication);
        }

        public virtual void PreRun()
        {
            if (EnableCoverage)
                mAnalyzer = new CoverageAnalyzer(mTypeLibrary);
        }

        public virtual void PostRun()
        {
        }

        public int Run(string[] args)
        {
            PreRun();
            int rc = RunBody(args);
            PostRun();
            return rc;
        }

        protected abstract int RunBody(string[] args);
    }
}
