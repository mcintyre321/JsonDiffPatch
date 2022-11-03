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

        public override void Write(IJsonObjectWriter writer)
        {
            writer.WriteOp("remove").WritePath(Path);
        }

        public override void Read(JObject jOperation)
        {
            Path = new JsonPointer(jOperation.GetValue("path"));
        }
    }
}
