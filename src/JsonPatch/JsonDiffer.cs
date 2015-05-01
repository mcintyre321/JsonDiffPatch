using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonDiffPatch
{
    public class JsonDiffer
    {
        internal static string Extend(string path, string extension)
        {
            // TODO: JSON property name needs escaping for path ??
            return path + "/" + extension;
        }

        private static Operation Build(string op, string path, string key, JToken value)
        {
            if (string.IsNullOrEmpty(key))
                return Operation.Parse("{ 'op' : '" + op + "' , path: '" + path + "', value: " + (value == null ? "null" : value.ToString(Formatting.None)) + "}");
            else
                return Operation.Parse("{ op : '" + op + "' , path : '" + Extend(path, key) + "' , value : " + (value == null ? "null" : value.ToString(Formatting.None)) + "}");
        }

        internal static Operation Add(string path, string key, JToken value)
        {
            return Build("add", path, key, value);
        }

        internal static Operation Remove(string path, string key)
        {
            return Build("remove", path, key, null);
        }

        internal static Operation Replace(string path, string key, JToken value)
        {
            return Build("replace", path, key, value);
        }

        internal static IEnumerable<Operation> CalculatePatch(JToken left, JToken right, string path = "")
        {
            if (left.Type != right.Type)
            {
                yield return JsonDiffer.Replace(path, "", right);
                yield break;
            }

            if (left.Type == JTokenType.Array)
            {
                if (left.Children().SequenceEqual(right.Children(), new JTokenEqualityComparer()))
                    yield break;

                // No array insert or delete operators in jpatch (yet?)
                yield return JsonDiffer.Replace(path, "", right);
                yield break;
            }

            if (left.Type == JTokenType.Object)
            {
                var lprops = ((IDictionary<string, JToken>)left).OrderBy(p => p.Key);
                var rprops = ((IDictionary<string, JToken>)right).OrderBy(p => p.Key);

                foreach (var removed in lprops.Except(rprops, MatchesKey.Instance))
                {
                    yield return JsonDiffer.Remove(path, removed.Key);
                }

                foreach (var added in rprops.Except(lprops, MatchesKey.Instance))
                {
                    yield return JsonDiffer.Add(path, added.Key, added.Value);
                }

                var matchedKeys = lprops.Select(x => x.Key).Intersect(rprops.Select(y => y.Key));
                var zipped = matchedKeys.Select(k => new { key = k, left = left[k], right = right[k] });

                foreach (var match in zipped)
                {
                    string newPath = path + "/" + match.key;
                    foreach (var patch in CalculatePatch(match.left, match.right, newPath))
                        yield return patch;
                }
                yield break;
            }
            else
            {
                // Two values, same type, not JObject so no properties

                if (left.ToString() == right.ToString())
                    yield break;
                else
                    yield return JsonDiffer.Replace(path, "", right);
            }
        }

        private class MatchesKey : IEqualityComparer<KeyValuePair<string, JToken>>
        {
            public static MatchesKey Instance = new MatchesKey();
            public bool Equals(KeyValuePair<string, JToken> x, KeyValuePair<string, JToken> y)
            {
                return x.Key.Equals(y.Key);
            }

            public int GetHashCode(KeyValuePair<string, JToken> obj)
            {
                return obj.Key.GetHashCode();
            }
        }

        public PatchDocument Diff(JToken @from, JToken to)
        {
            return new PatchDocument(CalculatePatch(@from, to).ToArray());
        }
    }
}