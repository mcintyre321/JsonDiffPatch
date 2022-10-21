using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavis;

namespace JsonDiffPatch
{
    public class ReplaceOperation : Operation
    {
        public JToken Value { get; private set; }

        public ReplaceOperation()
        {

        }

        public ReplaceOperation(JsonPointer path, JToken value) : base(path)
        {
            Value = value;
        }

        public override void Write(JsonWriter writer)
        {
            writer.WriteStartObject();

            WriteOp(writer, "replace");
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
