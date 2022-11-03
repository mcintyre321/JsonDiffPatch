using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json.Linq;

namespace JsonDiffPatch
{
    public interface IJsonObjectWriter
    {
        void WriteProperty(string name, string value);

        void WriteProperty(string name, JToken value);
    }
}
