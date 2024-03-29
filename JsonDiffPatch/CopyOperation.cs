using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavis;

namespace JsonDiffPatch
{
    public class CopyOperation : Operation
    {
        public JsonPointer FromPath { get; private set; }

        public CopyOperation()
        {

        }

        public CopyOperation(JsonPointer path, JsonPointer fromPath) : base(path)
        {
            FromPath = fromPath;
        }

        public override void Write(JsonWriter writer)
        {
            writer.WriteStartObject();

            WriteOp(writer, "copy");
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
