using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json.Linq;
using Tavis;

namespace JsonDiffPatch
{
    internal static class JsonObjectWriterExtensions
    {
        public static IJsonObjectWriter WriteOp(this IJsonObjectWriter writer, string op)
        {
            writer.WriteProperty("op", op);
            return writer;
        }

        public static IJsonObjectWriter WritePath(this IJsonObjectWriter writer, JsonPointer pointer)
        {
            writer.WriteProperty("path", pointer.ToString());
            return writer;
        }

        public static IJsonObjectWriter WriteFromPath(this IJsonObjectWriter writer, JsonPointer pointer)
        {
            writer.WriteProperty("from", pointer.ToString());
            return writer;
        }
        public static IJsonObjectWriter WriteValue(this IJsonObjectWriter writer, JToken value)
        {
            writer.WriteProperty("value", value);
            return writer;
        }
    }
}
