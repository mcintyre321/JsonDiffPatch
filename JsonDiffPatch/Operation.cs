using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavis;

namespace JsonDiffPatch
{
    public abstract class Operation
    {
        public JsonPointer Path { get; set; }

        public abstract void Write(IJsonObjectWriter writer);

        public abstract void Read(JObject jOperation);

        public static Operation Parse(string json)
        {
            return Build(JObject.Parse(json));
        }

        public static Operation Build(JObject jOperation)
        {
            var op = OperationCreator.Create((string)jOperation["op"]);
            op.Read(jOperation);
            return op;
        }
    }
}
