using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavis;

namespace JsonDiffPatch
{
    public class RemoveOperation : Operation
    {
        public RemoveOperation()
        {

        }

        public RemoveOperation(JsonPointer path) : base(path)
        {
        }

        public override void Write(JsonWriter writer)
        {
            writer.WriteStartObject();

            WriteOp(writer, "remove");
            WritePath(writer, Path);

            writer.WriteEndObject();
        }

        public override void Read(JObject jOperation)
        {
            Path = new JsonPointer(SplitPath((string)jOperation.GetValue("path")));
        }
    }
}
