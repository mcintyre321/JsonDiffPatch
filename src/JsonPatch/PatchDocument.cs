using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tavis
{
    public class PatchDocument
    {
        private readonly List<Operation> _Operations = new List<Operation>();


        public PatchDocument(params Operation[] operations)
        {
            foreach (var operation in operations)
            {
                AddOperation(operation);
            }
        }

        public List<Operation> Operations
        {
            get { return _Operations; }
        }

        public void ApplyTo(JToken jToken)
        {
            ApplyTo(new JsonNetTargetAdapter(jToken));
        }

        public void ApplyTo(IPatchTarget target)
        {
            foreach (var operation in Operations)
            {
                target.ApplyOperation(operation);
            }
        }

        public void AddOperation(Operation operation)
        {
            Operations.Add(operation);
        }

        public static PatchDocument Load(Stream document)
        {
            var reader = new StreamReader(document);
       
            return Parse(reader.ReadToEnd());
        }

        public static PatchDocument Parse(string jsondocument)
        {
            var root = JToken.Parse(jsondocument) as JArray;
            var document = new PatchDocument();


            if (root != null)
            {
                foreach (var jOperation in root.Children().Cast<JObject>())
                {
                    var op = CreateOperation((string)jOperation["op"]);
                    op.Read(jOperation);
                    document.AddOperation(op);
                }
            }
            return document;
        }

        private static Operation CreateOperation(string op)
        {
            switch (op)
            {
                case "add": return new AddOperation();
                case "copy": return new CopyOperation();
                case "move": return new MoveOperation();
                case "remove": return new RemoveOperation();
                case "replace": return new ReplaceOperation();
                case "test" : return new TestOperation();
            }
            return null;
        }

        /// <summary>
        /// Create memory stream with serialized version of PatchDocument 
        /// </summary>
        /// <returns></returns>
        public MemoryStream ToStream()
        {
            var stream = new MemoryStream();
            CopyToStream(stream);
            stream.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Copy serialized version of Patch document to provided stream
        /// </summary>
        /// <param name="stream"></param>
        public void CopyToStream(Stream stream)
        {
            var sw = new JsonTextWriter(new StreamWriter(stream));
            sw.Formatting = Formatting.Indented;

            sw.WriteStartArray();

            foreach (var operation in Operations)
            {
                operation.Write(sw);
            }

            
            sw.WriteEndArray();

            sw.Flush();
        }


     
    }
}