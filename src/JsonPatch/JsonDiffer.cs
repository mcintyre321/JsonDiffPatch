using System;
using System.Collections.Generic;
using System.Linq;
using JsonDiffPatch.DiffLib;
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
                Operation prev = null;
                foreach (var operation in ProcessArray(left, right, path))
                {
                        var prevRemove = prev as RemoveOperation;
                        var add = operation as AddOperation;
                        if (prevRemove != null && add != null && add.Path.ToString() == prevRemove.Path.ToString())
                        {
                            yield return Replace(add.Path.ToString(), "", add.Value);
                            prev = null;
                        }
                        else
                        {
                            if (prev != null) yield return prev;
                            prev = operation;
                        }
                }
                if (prev != null)
                {
                    yield return prev;
                }
            }
            else if (left.Type == JTokenType.Object)
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

        private static IEnumerable<Operation> ProcessArray(JToken left, JToken right, string path)
        {
            var differ = new PatienceSequenceMatcher<JToken>(new JTokenEqualityComparer()).CreateDifferencer();

            var leftList = left.ToList();
            var differences = differ.FindDifferences(leftList, right.ToList());
            if (differences.Any())
            {
                foreach (var di in differences)
                {
                    var sequence = di.SubSequence;
                    switch (di.Operation)
                    {
                        case DifferenceOperation.Equal:
                            break;
                            ;
                        case DifferenceOperation.Inserted:
                            for (var i = 0; i < sequence.RightLength; i++)
                            {
                                yield return JsonDiffer.Add(path, sequence.RightIndex.ToString(), right[i + sequence.RightIndex]);
                            }
                            break;
                        case DifferenceOperation.Removed:
                            if (sequence.LeftLength == leftList.Count)
                            {
                                yield return JsonDiffer.Replace(path, "", right);
                                yield break;
                            }
                            else
                            {
                                for (var i = 0; i < sequence.LeftLength; i++)
                                {
                                    yield return Remove(path, sequence.LeftIndex.ToString());
                                }
                            }
                            break;
                        case DifferenceOperation.Replaced:
                            for (var i = 0; i < sequence.LeftLength; i++)
                            {
                                yield return Replace(path, sequence.LeftIndex.ToString(), right[sequence.RightIndex]);
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            //// No array insert or delete operators in jpatch (yet?)
            //yield return JsonDiffer.Replace(path, "", right);
            yield break;
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