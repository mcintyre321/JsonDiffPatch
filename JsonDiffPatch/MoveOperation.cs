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

        public override void Write(IJsonObjectWriter writer)
        {
            writer.WriteOp("move").
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
