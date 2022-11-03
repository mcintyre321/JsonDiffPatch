using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonDiffPatch
{
    public class PatchDocument
    {
        private readonly static PatchDocumentSerializer documentSerializer = new PatchDocumentSerializer();

        private readonly OperationCollection _Operations = new OperationCollection();

        public PatchDocument()
        {

        }

        public PatchDocument(params Operation[] operations)
        {
            foreach (var operation in operations)
            {
                AddOperation(operation);
            }
        }

        public PatchDocument(IEnumerable<Operation> operations)
        {
            _Operations.AddRange(operations);
        }

        public IReadOnlyCollection<Operation> Operations
        {
            get { return _Operations; }
        }

        public void AddOperation(Operation operation)
        {
            _Operations.Add(operation);
        }

        public static PatchDocument Load(Stream document)
        {
            var reader = new StreamReader(document);
       
            return Parse(reader.ReadToEnd());
        }

        public static PatchDocument Load(JArray document)
        {
            var root = new PatchDocument();

            if (document != null)
            {
                foreach (var jOperation in document.Children().Cast<JObject>())
                {
                    var op = Operation.Build(jOperation);
                    root.AddOperation(op);
                }
            }
            
            return root;
        }
        
        public static PatchDocument Parse(string jsondocument)
        {
            var root = JToken.Parse(jsondocument) as JArray;
            
            return Load(root);
        }

        public MemoryStream ToStream()
        {
            return documentSerializer.ToStream(this);
        }

        public override string ToString()
        {
            return documentSerializer.Serialize(this, Formatting.Indented); //ToString(Formatting.Indented);
        }

        public string ToString(Formatting formatting)
        {
            return documentSerializer.Serialize(this, formatting); //ToString(Formatting.Indented);
        }
    }
}