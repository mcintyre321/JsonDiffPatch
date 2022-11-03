using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonDiffPatch
{
    internal class PatchDocumentSerializer
    {
        public string Serialize(PatchDocument patchDocument, Formatting formatting)
        {
            using (var ms = new MemoryStream())
            {
                using (var obj = CopyToStream(patchDocument, ms, formatting))
                {
                    ms.Position = 0;
                    using (StreamReader reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        /// <summary>
        /// Create memory stream with serialized version of PatchDocument 
        /// </summary>
        /// <returns></returns>
        public MemoryStream ToStream(PatchDocument patchDocument)
        {
            var stream = new MemoryStream();
            CopyToStream(patchDocument, stream, Formatting.Indented);
            stream.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Copy serialized version of Patch document to provided stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="formatting"></param>
        private IDisposable CopyToStream(PatchDocument patchDocument, Stream stream, Formatting formatting = Formatting.Indented)
        {
            var sw = new JsonArrayWriter(stream, formatting);
            sw.WriteStartArray();
            foreach (var operation in patchDocument.Operations)
            {
                sw.WriteStartObject();
                operation.Write(sw);
                sw.WriteEndObject();
            }
            sw.WriteEndArray();
            return sw;
        }

        private class JsonArrayWriter : IJsonObjectWriter, IDisposable
        {
            private readonly JsonTextWriter jsonTextWriter;

            public JsonArrayWriter(Stream stream, Formatting formatting)
            {
                jsonTextWriter = new JsonTextWriter(new StreamWriter(stream));
                jsonTextWriter.Formatting = formatting;
            }

            public void Dispose()
            {
                ((IDisposable)jsonTextWriter).Dispose();
            }

            public void WriteProperty(string name, string value)
            {
                jsonTextWriter.WritePropertyName(name);
                jsonTextWriter.WriteValue(value);
            }

            public void WriteProperty(string name, JToken value)
            {
                jsonTextWriter.WritePropertyName(name);
                value.WriteTo(jsonTextWriter);
            }

            internal void WriteStartArray()
            {
                jsonTextWriter.WriteStartArray();
            }

            internal void WriteEndArray()
            {
                jsonTextWriter.WriteEndArray();
                jsonTextWriter.Flush();
            }

            internal void WriteStartObject()
            {
                jsonTextWriter.WriteStartObject();
            }

            internal void WriteEndObject()
            {
                jsonTextWriter.WriteEndObject();
            }
        }
    }
}
