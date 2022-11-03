using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavis;

namespace JsonDiffPatch
{
    public class AddOperation : Operation
    {
        public JToken Value { get; set; }

        public override void Write(IJsonObjectWriter writer)
        {
            writer.WriteOp("add").
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
