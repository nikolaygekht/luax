using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Luax.Interpreter.Execution
{
    /// <summary>
    /// The list of the files in the project
    /// </summary>
    public class LuaXProjectOptions : IReadOnlyList<KeyValuePair<string, string>>
    {
        private readonly List<KeyValuePair<string, string>> mFiles = new List<KeyValuePair<string, string>>();

        public KeyValuePair<string, string> this[int index] => mFiles[index];

        public KeyValuePair<string, string> this[string optionName] => mFiles.Find(p => p.Key == optionName);

        public int Count => mFiles.Count;

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, string>>)mFiles).GetEnumerator();

        internal void Add(string key, string value) => mFiles.Add(new KeyValuePair<string, string>(key, value));

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)mFiles).GetEnumerator();
    }
}
