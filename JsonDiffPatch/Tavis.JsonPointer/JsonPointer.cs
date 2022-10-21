//see https://github.com/tavis-software/Tavis.JsonPointer

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Tavis
{
    public class JsonPointer
    {
        private readonly IReadOnlyList<string> _Tokens;

        public JsonPointer(string pointer)
        {
            _Tokens = pointer.Split('/').Skip(1).Select(Decode).ToArray();
        }

        internal JsonPointer(IReadOnlyList<string> tokens)
        {
            _Tokens = tokens;
        }
        private string Decode(string token)
        {
            return Uri.UnescapeDataString(token).Replace("~1", "/").Replace("~0", "~");
        }

        public bool IsNewPointer()
        {
            return _Tokens[_Tokens.Count - 1] == "-";
        }

        public JsonPointer ParentPointer
        {
            get
            {
                if (_Tokens.Count == 0) return null;

                var tokens = new string[_Tokens.Count - 1];
                for (int i = 0; i < _Tokens.Count - 1; i++)
                {
                    tokens[i] = _Tokens[i];
                }

                return new JsonPointer(tokens);
            }
        }

        public JToken Find(JToken sample)
        {
            if (_Tokens.Count == 0)
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
                throw  new ArgumentException("Failed to dereference pointer",ex);
            }
        }

        public override string ToString()
        {
            return "/" + String.Join("/", _Tokens);
        }
    }
}
