using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json.Linq;

namespace JsonDiffPatch
{
    internal class JsonPointerTokensSource : IEnumerable<string>
    {
        private readonly IReadOnlyList<string> _list;
        private readonly int startIndex;
        private readonly int lastIndex;

        public bool IsEmpty => lastIndex < startIndex;

        public JsonPointerTokensSource(string pointer)
        {
            _list = pointer.Split('/').ToList();
            startIndex = 1;
            lastIndex = _list.Count - 1;
        }

        private JsonPointerTokensSource(IReadOnlyList<string> list, int startIndex, int lastIndex)
        {
            _list = list;
            this.startIndex = startIndex;
            this.lastIndex = lastIndex;
        }

        public string Last()
        {
            return lastIndex >= startIndex ? _list[lastIndex] : string.Empty;
        }

        internal JsonPointerTokensSource ForParent()
        {
            return new JsonPointerTokensSource(_list, startIndex, lastIndex - 1);
        }

        public IEnumerator<string> GetEnumerator()
        {
            for (int i = startIndex; i <= lastIndex; i++)
            {
                yield return _list[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
