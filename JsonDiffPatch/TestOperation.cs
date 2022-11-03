using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavis;

namespace JsonDiffPatch
{
    public class TestOperation : Operation
    {
        public JToken Value { get; set; }

        public override void Write(IJsonObjectWriter writer)
        {
            writer.WriteOp("test").
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
