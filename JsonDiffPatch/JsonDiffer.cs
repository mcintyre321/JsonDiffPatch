using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavis;

namespace JsonDiffPatch
{
    /// <summary>
    /// Parts adapted from https://github.com/benjamine/jsondiffpatch/blob/42ce1b6ca30c4d7a19688a020fce021a756b43cc/src/filters/arrays.js
    /// </summary>
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
                return
                    Operation.Parse("{ 'op' : '" + op + "' , path: '" + path + "', value: " +
                                    (value == null ? "null" : value.ToString(Formatting.None)) + "}");
            else
                return
                    Operation.Parse("{ op : '" + op + "' , path : '" + Extend(path, key) + "' , value : " +
                                    (value == null ? "null" : value.ToString(Formatting.None)) + "}");
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

        internal static IEnumerable<Operation> CalculatePatch(JToken left, JToken right, CompareOptions compareOptions, string path = "")
        {
            if (left.Type != right.Type)
            {
                yield return JsonDiffer.Replace(path, "", right);
                yield break;
            }

            if (left.Type == JTokenType.Array)
            {
                Operation prev = null;
                foreach (var operation in ProcessArray(left, right, path, compareOptions))
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
                var lprops = ((IDictionary<string, JToken>) left).OrderBy(p => p.Key);
                var rprops = ((IDictionary<string, JToken>) right).OrderBy(p => p.Key);

                foreach (var removed in lprops.Except(rprops, MatchesKey.Instance))
                {
                    yield return JsonDiffer.Remove(path, removed.Key);
                }

                foreach (var added in rprops.Except(lprops, MatchesKey.Instance))
                {
                    yield return JsonDiffer.Add(path, added.Key, added.Value);
                }

                var matchedKeys = lprops.Select(x => x.Key).Intersect(rprops.Select(y => y.Key));
                var zipped = matchedKeys.Select(k => new {key = k, left = left[k], right = right[k]});

                foreach (var match in zipped)
                {
                    string newPath = path + "/" + match.key;
                    foreach (var patch in CalculatePatch(match.left, match.right, compareOptions, newPath))
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

        private static IEnumerable<Operation> ProcessArray(JToken left, JToken right, string path, CompareOptions compareOptions)
        {
            var comparer = new CustomCheckEqualityComparer(compareOptions, new JTokenEqualityComparer());

            int commonHead = 0;
            int commonTail = 0;
            var array1 = left.ToArray();
            var len1 = array1.Length;
            var array2 = right.ToArray();
            var len2 = array2.Length;
        //    if (len1 == 0 && len2 ==0 ) yield break;
            while (commonHead < len1 && commonHead < len2)
            {
                if (comparer.Equals(array1[commonHead], array2[commonHead]) == false) break;

                //diff and yield objects here
                foreach (var operation in CalculatePatch(array1[commonHead], array2[commonHead], compareOptions, path + "/" + commonHead))
                {
                    yield return operation;
                }
                commonHead++;

            }

            // separate common tail
            while (commonTail + commonHead < len1 && commonTail + commonHead < len2 )
            {
                if (comparer.Equals(array1[len1 - 1 - commonTail], array2[len2 - 1 - commonTail]) == false) break;

                var index1 = len1 - 1 - commonTail;
                var index2 = len2 - 1 - commonTail;
                foreach (var operation in CalculatePatch(array1[index1], array2[index2], compareOptions, path + "/" + index1))
                {
                    yield return operation;
                }
                commonTail++;
            }

            if (commonHead == 0 && commonTail == 0 && len2 > 0 && len1 > 0)
            {
                yield return new ReplaceOperation()
                {
                    Path = new JsonPointer(path),
                    Value = new JArray(array2)
                };
                yield break;
            }


            var leftMiddle = array1.Skip(commonHead).Take(array1.Length - commonTail - commonHead).ToArray();
            var rightMiddle = array2.Skip(commonHead ).Take(array2.Length - commonTail - commonHead).ToArray();

            // Just a replace of values!
            if (leftMiddle.Length == rightMiddle.Length)
            {
                for (int i = 0; i < leftMiddle.Length; i++)
                {
                    foreach (var operation in CalculatePatch(leftMiddle[i], rightMiddle[i], compareOptions, path + "/" + commonHead))
                    {
                        yield return operation;
                    }
                }

                yield break;
            }

            

            foreach (var jToken in leftMiddle)
            {
                yield return new RemoveOperation()
                {
                    Path = new JsonPointer(path + "/" + (commonHead ))
                };
            }
            for (int i = 0; i < rightMiddle.Length; i++)
            {
                yield return new AddOperation()
                {
                    Value = rightMiddle[i],
                    Path = new JsonPointer(path + "/" + (i + commonHead))
                };
            }

            //if (commonHead + commonTail == len1)
            //{
            //    if (len1 == len2)
            //    {
            //        // arrays are identical
            //        yield break;
            //    }
            //    // trivial case, a block (1 or more consecutive items) was added
           
            //    for (index = commonHead; index < len2 - commonTail; index++)
            //    {
            //        yield return new AddOperation()
            //        {
            //            Value = array2[index],
            //            Path = new JsonPointer(path + "/" + index)
            //        };
            //    }
            //}
            //if (commonHead + commonTail == len2)
            //{
            //    // trivial case, a block (1 or more consecutive items) was removed
            //    for (index = commonHead; index < len1 - commonTail; index++)
            //    {
            //        yield return new RemoveOperation()
            //        {
            //            Path = new JsonPointer(path + "/" + index)
            //        };
            //    }
            //}

            //var context = new Dictionary<string, object>();

            //var lcs = new ArrayLcs((a, b) => comparer.Equals((JToken)a, (JToken)b));
            //var trimmed1 = array1.Skip(commonHead).Take(len1 - commonTail).ToArray();
            //var trimmed2 = array2.Skip(commonHead).Take(len2 - commonTail).ToArray();
            //var seq = lcs.Get(trimmed1, trimmed2, context);
            //for (index = commonHead; index < len1 - commonTail; index++)
            //{
            //    if ((seq.indices1).IndexOf(index - commonHead) < 0)
            //    {
            //        // removed
            //        yield return new RemoveOperation()
            //        {
            //            Path = new JsonPointer(path + "/" + commonHead)
            //        };
            //        //removedItems.push(index);
            //    }
            //}

            //var detectMove = true;
            //if (context.options && context.options.arrays && context.options.arrays.detectMove === false)
            //{
            //    detectMove = false;
            //}
            //var includeValueOnMove = false;
            //if (context.options && context.options.arrays && context.options.arrays.includeValueOnMove)
            //{
            //    includeValueOnMove = true;
            //}

            //var removedItemsLength = removedItems.length;
            //for (index = commonHead; index < len2 - commonTail; index++)
            //{
            //    var indexOnArray2 = arrayIndexOf(seq.indices2, index - commonHead);
            //    if (indexOnArray2 < 0)
            //    {
            //        // added, try to match with a removed item and register as position move
            //        var isMove = false;
            //        if (detectMove && removedItemsLength > 0)
            //        {
            //            for (var removeItemIndex1 = 0; removeItemIndex1 < removedItemsLength; removeItemIndex1++)
            //            {
            //                index1 = removedItems[removeItemIndex1];
            //                if (matchItems(trimmed1, trimmed2, index1 - commonHead,
            //                  index - commonHead, matchContext))
            //                {
            //                    // store position move as: [originalValue, newPosition, ARRAY_MOVE]
            //                    result['_' + index1].splice(1, 2, index, ARRAY_MOVE);
            //                    if (!includeValueOnMove)
            //                    {
            //                        // don't include moved value on diff, to save bytes
            //                        result['_' + index1][0] = '';
            //                    }

            //                    index2 = index;
            //                    child = new DiffContext(context.left[index1], context.right[index2]);
            //                    context.push(child, index2);
            //                    removedItems.splice(removeItemIndex1, 1);
            //                    isMove = true;
            //                    break;
            //                }
            //            }
            //        }
            //        if (!isMove)
            //        {
            //            // added
            //            result[index] = [array2[index]];
            //        }
            //    }
            //    else
            //    {
            //        // match, do inner diff
            //        index1 = seq.indices1[indexOnArray2] + commonHead;
            //        index2 = seq.indices2[indexOnArray2] + commonHead;
            //        child = new DiffContext(context.left[index1], context.right[index2]);
            //        context.push(child, index2);
            //    }
            //}
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public PatchDocument Diff(JToken @from, JToken to)
        {
            return new PatchDocument(CalculatePatch(@from, to, CompareOptions.Default).ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="compareOptions">Use id comparison on array members to determine equality</param>
        /// <returns></returns>
        public PatchDocument Diff(JToken @from, JToken to, CompareOptions compareOptions)
        {
            if (compareOptions == null)
                throw new ArgumentNullException(nameof(compareOptions));

            return new PatchDocument(CalculatePatch(@from, to, compareOptions).ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="useIdPropertyToDetermineEquality">Use id property on array members to determine equality</param>
        /// <returns></returns>
        [Obsolete("Use Diff(@from, to, compareOptions) instead. Kept for backwards compatibility only.")]
        public PatchDocument Diff(JToken @from, JToken to, bool useIdPropertyToDetermineEquality)
        {
            CompareOptions compareOptions = new CompareOptions(useIdPropertyToDetermineEquality);

            return new PatchDocument(CalculatePatch(@from, to, compareOptions).ToArray());
        }
    }

    internal class CustomCheckEqualityComparer : IEqualityComparer<JToken>
    {
        private readonly CompareOptions _compareOptions;
        private readonly IEqualityComparer<JToken> _inner;

        public CustomCheckEqualityComparer(CompareOptions compareOptions, IEqualityComparer<JToken> inner)
        {
            _compareOptions = compareOptions;
            _inner = inner;
        }

        public bool Equals(JToken x, JToken y)
        {
            if (_compareOptions.EnableIdentifierCheck && x.Type == JTokenType.Object && y.Type == JTokenType.Object)
            {
                var xIdToken = x[_compareOptions.IdentifierProperty];
                var yIdToken = y[_compareOptions.IdentifierProperty];

                if (xIdToken != null && yIdToken != null)
                {
                    if (xIdToken.HasValues && y.HasValues)
                        return _inner.Equals(xIdToken, yIdToken);

                    if (xIdToken.HasValues != yIdToken.HasValues)
                        return false;
                }

                var xId = xIdToken?.Value<string>();
                var yId = yIdToken?.Value<string>();

                return (xId != null && xId == yId);
            }
            return _inner.Equals(x, y);
        }

        public int GetHashCode(JToken obj)
        {
            if (_compareOptions.EnableIdentifierCheck && obj.Type == JTokenType.Object)
            {
                var xIdToken = obj[_compareOptions.IdentifierProperty];
                var xId = xIdToken != null && xIdToken.HasValues ? xIdToken.Value<string>() : null;
                if (xId != null) return xId.GetHashCode() + _inner.GetHashCode(obj);
            }
            return _inner.GetHashCode(obj);
        }
    }
}