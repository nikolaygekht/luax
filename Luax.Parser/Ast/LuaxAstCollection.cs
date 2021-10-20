using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luax.Parser.Ast
{
    public class LuaXAstCollection<T> : IReadOnlyList<T>
    {
        private readonly List<T> mList = new List<T>();

        public T this[int index] => mList[index];

        public int Count => mList.Count;

        public IEnumerator<T> GetEnumerator() => mList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal void Add(T item) => mList.Add(item);

        internal void AddRange(IEnumerable<T> items) => mList.AddRange(items);
    }
}
