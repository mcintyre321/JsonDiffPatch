﻿using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavis;

namespace JsonDiffPatch
{
    public abstract class Operation
    {
        public JsonPointer Path { get; protected set; }

        public Operation()
        {

        }

        public Operation(JsonPointer path)
        {
            Path = path;
        }

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

        protected static string[] SplitPath(string path) => path.Split('/').Skip(1).ToArray();

        public abstract void Read(JObject jOperation);

        public static Operation Parse(string json)
        {
            return Build(JObject.Parse(json));
        }

        public static Operation Build(JObject jOperation)
        {
            var op = PatchDocument.CreateOperation((string)jOperation["op"]);
            op.Read(jOperation);
            return op;
        }
    }
}
