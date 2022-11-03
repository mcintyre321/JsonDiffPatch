using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavis;

namespace JsonDiffPatch
{
    public class CopyOperation : Operation
    {
        public JsonPointer FromPath { get; set; }

        public override void Write(IJsonObjectWriter writer)
        {
            writer.WriteOp("copy").
                WritePath(Path).
                WriteFromPath(FromPath);
        }

        public override void Read(JObject jOperation)
        {
            Path = new JsonPointer(jOperation.GetValue("path"));
            FromPath = new JsonPointer(jOperation.GetValue("from"));
        }
    }
}
