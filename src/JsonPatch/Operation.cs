using Newtonsoft.Json.Linq;

namespace Tavis
{
    public abstract class Operation
    {
        public JsonPointer Path { get; set; }
    }

    public class AddOperation : Operation
    {
        public JToken Value { get; set; }
    }
    public class CopyOperation : Operation
    {
        public JsonPointer FromPath { get; set; }
    }
    public class MoveOperation : Operation
    {
        public JsonPointer FromPath { get; set; }
    }
    public class RemoveOperation : Operation
    {
    }
    public class ReplaceOperation : Operation
    {
        public JToken Value { get; set; }
    }
    public class TestOperation : Operation
    {
        public JToken Value { get; set; }
    }
    

}