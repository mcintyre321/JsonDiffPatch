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

        

        public void AddOperation(Operation operation)
        {
            Operations.Add(operation);
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

        public static Operation CreateOperation(string op)
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
            CopyToStream(stream, Formatting.Indented);
            stream.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Copy serialized version of Patch document to provided stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="formatting"></param>
        public void CopyToStream(Stream stream, Formatting formatting = Formatting.Indented)
        {
            var sw = new JsonTextWriter(new StreamWriter(stream));
            sw.Formatting = formatting;

            sw.WriteStartArray();

            foreach (var operation in Operations)
            {
                operation.Write(sw);
            }

            
            sw.WriteEndArray();

            sw.Flush();
        }

        public override string ToString()
        {
            return ToString(Formatting.Indented);
        }

        public string ToString(Formatting formatting)
        {
            using (var ms = new MemoryStream())
            {
                CopyToStream(ms, formatting);
                ms.Position = 0;
                using (StreamReader reader = new StreamReader(ms, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
