using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavis;

namespace JsonDiffPatch
{
    public class MoveOperation : Operation
    {
        public JsonPointer FromPath { get; private set; }

        public MoveOperation()
        {

        }

        public MoveOperation(JsonPointer path, JsonPointer fromPath) : base(path)
        {
            FromPath = fromPath;
        }

        public override void Write(JsonWriter writer)
        {
            writer.WriteStartObject();

            WriteOp(writer, "move");
            WritePath(writer, Path);
            WriteFromPath(writer, FromPath);

            writer.WriteEndObject();
        }

        public override void Read(JObject jOperation)
        {
            Path = new JsonPointer(SplitPath((string)jOperation.GetValue("path")));
            FromPath = new JsonPointer(SplitPath((string)jOperation.GetValue("from")));
        }
    }
}
