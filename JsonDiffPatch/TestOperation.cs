using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavis;

namespace JsonDiffPatch
{
    public class TestOperation : Operation
    {
        public JToken Value { get; private set; }

        public TestOperation()
        {
        }

        public TestOperation(JsonPointer path, JToken value) : base(path)
        {
            Value = value;
        }

        public override void Write(JsonWriter writer)
        {
            writer.WriteStartObject();

            WriteOp(writer, "test");
            WritePath(writer, Path);
            WriteValue(writer, Value);

            writer.WriteEndObject();
        }

        public override void Read(JObject jOperation)
        {
            Path = new JsonPointer(SplitPath((string)jOperation.GetValue("path")));
            Value = jOperation.GetValue("value");
        }
    }
}
