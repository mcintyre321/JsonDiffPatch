using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Tavis
{
    public class PatchDocument
    {
        private readonly List<Operation> _Operations = new List<Operation>();

        public void ApplyTo(JToken jToken)
        {
            ApplyTo(new JsonNetTargetAdapter(jToken));
        }

        public void ApplyTo(IPatchTarget target)
        {
            foreach (var operation in _Operations)
            {
                target.ApplyOperation(operation);
            }
        }

        public void AddOperation(Operation operation)
        {
            _Operations.Add(operation);
        }
    }
}