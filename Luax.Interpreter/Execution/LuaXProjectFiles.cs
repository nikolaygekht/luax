using System.Collections;
using System.Collections.Generic;

namespace Luax.Interpreter.Execution
{
    /// <summary>
    /// The list of the files in the project
    /// </summary>
    public class LuaXProjectFiles : IReadOnlyList<string>
    {
        private readonly List<string> mFiles = new List<string>();

        public string this[int index] => mFiles[index];

        public int Count => mFiles.Count;

        public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)mFiles).GetEnumerator();

        internal void Add(string file) => mFiles.Add(file);

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)mFiles).GetEnumerator();
    }
}
