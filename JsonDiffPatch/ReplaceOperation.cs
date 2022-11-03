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

        public override void Write(IJsonObjectWriter writer)
        {
            writer.WriteOp("replace").
                WritePath(Path).
                WriteValue(Value);
        }

        public override void Read(JObject jOperation)
        {
            Path = new JsonPointer(jOperation.GetValue("path"));
            Value = jOperation.GetValue("value");
        }
    }
}
