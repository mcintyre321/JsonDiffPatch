using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tavis
{
    public abstract class Operation
    {
        public JsonPointer Path { get; set; }

        public abstract void Write(JsonWriter writer);

        protected static void WriteOp(JsonWriter writer, string op)
        {
            writer.WritePropertyName("op");
            writer.WriteValue(op);
        }

        protected static void WritePath(JsonWriter writer, JsonPointer pointer)
        {
            writer.WritePropertyName("path");
            writer.WriteValue(pointer.ToString());
        }

        protected static void WriteFromPath(JsonWriter writer, JsonPointer pointer)
        {
            writer.WritePropertyName("from");
            writer.WriteValue(pointer.ToString());
        }
        protected static void WriteValue(JsonWriter writer, JToken value)
        {
            writer.WritePropertyName("value");
            value.WriteTo(writer);
        }

        public abstract void Read(JObject jOperation);
    }

    public class AddOperation : Operation
    {
        public JToken Value { get; set; }

        public override void Write(JsonWriter writer)
        {
            writer.WriteStartObject();

            WriteOp(writer, "add");
            WritePath(writer,Path);
            WriteValue(writer,Value);

            writer.WriteEndObject();
        }

        public override void Read(JObject jOperation)
        {
            Path = new JsonPointer((string)jOperation.GetValue("path"));
            Value = jOperation.GetValue("value");
        }
    }
    public class CopyOperation : Operation
    {
        public JsonPointer FromPath { get; set; }

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
            Path = new JsonPointer((string)jOperation.GetValue("path"));
            FromPath = new JsonPointer((string)jOperation.GetValue("from"));
        }
    }
    public class MoveOperation : Operation
    {
        public JsonPointer FromPath { get; set; }

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
            Path = new JsonPointer((string)jOperation.GetValue("path"));
            FromPath = new JsonPointer((string)jOperation.GetValue("from"));
        }
    }
    public class RemoveOperation : Operation
    {
        
        public override void Write(JsonWriter writer)
        {
            writer.WriteStartObject();

            WriteOp(writer, "remove");
            WritePath(writer, Path);

            writer.WriteEndObject();
        }

        public override void Read(JObject jOperation)
        {
            Path = new JsonPointer((string)jOperation.GetValue("path"));   
        }
    }
    public class ReplaceOperation : Operation
    {
        public JToken Value { get; set; }

        public override void Write(JsonWriter writer)
        {
            writer.WriteStartObject();

            WriteOp(writer, "replace");
            WritePath(writer, Path);
            WriteValue(writer, Value);

            writer.WriteEndObject();
        }

        public override void Read(JObject jOperation)
        {
            Path = new JsonPointer((string)jOperation.GetValue("path"));
            Value = jOperation.GetValue("value");
        }
    }
    public class TestOperation : Operation
    {
        public JToken Value { get; set; }

        public override void Write(JsonWriter writer)
        {
            writer.WriteStartObject();

            WriteOp(writer, "test");
            WritePath(writer, Path);
            WriteValue(writer, Value);

            writer.WriteEndObject();
        }

        public override void Read(JObject jOperation)
        {
            Path = new JsonPointer((string)jOperation.GetValue("path"));
            Value = jOperation.GetValue("value");
        }
    }
    

}