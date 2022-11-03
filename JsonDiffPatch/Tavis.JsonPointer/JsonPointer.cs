//see https://github.com/tavis-software/Tavis.JsonPointer

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using JsonDiffPatch;

using Newtonsoft.Json.Linq;

namespace Tavis
{
    public class JsonPointer
    {
        private readonly JsonPointerTokensSource _Tokens;

        public JsonPointer(string pointer)
        {
            _Tokens = new JsonPointerTokensSource(Uri.UnescapeDataString(pointer));
        }

        public JsonPointer(JToken jToken)
        {
            _Tokens = new JsonPointerTokensSource(jToken.ToString());
        }

        private JsonPointer(JsonPointerTokensSource tokens)
        {
            _Tokens = tokens;
        }

        public bool IsNewPointer()
        {
            return _Tokens.Last() == "-";
        }

        public JsonPointer ParentPointer
        {
            get
            {
                if (_Tokens.IsEmpty) return null;
                return new JsonPointer(_Tokens.ForParent());
            }
        }

        public JToken Find(JToken sample)
        {
            if (_Tokens.IsEmpty)
            {
                return sample;
            }
            try
            {
                var pointer = sample;
                foreach (var token in _Tokens.Select(t => t.Replace("~1", "/").Replace("~0", "~")))
                {
                    if (pointer is JArray)
                    {
                        pointer = pointer[Convert.ToInt32(token)];
                    }
                    else
                    {
                        pointer = pointer[token];
                        if (pointer == null)
                        {
                            throw new ArgumentException("Cannot find " + token);
                        }

                    }
                }
                return pointer;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Failed to dereference pointer", ex);
            }
        }

        public override string ToString()
        {
            return "/" + String.Join("/", _Tokens);
        }
    }
}
