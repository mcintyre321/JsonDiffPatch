using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace JsonDiffPatch
{
    internal class OperationCollection : IReadOnlyCollection<Operation>
    {
        private readonly List<Operation> _operations = new List<Operation>();

        public int Count => _operations.Count;

        public void Add(Operation operation)
        {
            _operations.Add(operation);
        }

        internal void AddRange(IEnumerable<Operation> operations)
        {
            _operations.AddRange(operations);
        }

        public IEnumerator<Operation> GetEnumerator()
        {
            return _operations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _operations.GetEnumerator();
        }
    }
}
