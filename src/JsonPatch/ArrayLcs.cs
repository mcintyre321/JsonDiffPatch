using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JsonDiffPatch
{
    class ArrayLcs
    {
        private readonly Func<object, object, bool> _matchObject;
        private Func<object[], object[], int, int, IDictionary, bool> _match;

        public ArrayLcs(Func<object, object, bool> matchObject)
        {
            _matchObject = matchObject;
            _match = (array1, array2, index1, index2, context) => _matchObject(array1[index1], array2[index2]);
        }

        public class BackTrackResult
        {
            public List<object> sequence { get; set; } = new List<object>();
            public List<int> indices1 { get; set; } = new List<int>();
            public List<int> indices2 { get; set; } = new List<int>();
        }


        private int[][] LengthMatrix(object[] array1, object[] array2, IDictionary context)
        {
            var len1 = array1.Length;
            var len2 = array2.Length;

            // initialize empty matrix of len1+1 x len2+1
            var matrix = new int[len1 + 1][];
            for (var x = 0; x < matrix.Length; x++)
            {
                matrix[x] = Enumerable.Repeat(0, len2 + 1).ToArray();
            }
            // save sequence lengths for each coordinate
            for (var x = 1; x < len1 + 1; x++)
            {
                for (var y = 1; y < len2 + 1; y++)
                {
                    if (_match(array1, array2, x - 1, y - 1, context))
                    {
                        matrix[x][y] = 1 + (int)matrix[x - 1][y - 1];
                    }
                    else
                    {
                        matrix[x][y] = Math.Max(matrix[x - 1][y], matrix[x][y - 1]);
                    }
                }
            }
            return matrix;
        }

        private BackTrackResult backtrack(int[][] matrix, object[] array1, object[] array2, int index1, int index2, IDictionary context)
        {
            if (index1 == 0 || index2 == 0)
            {
                return new BackTrackResult();
            }

            if (_match(array1, array2, index1 - 1, index2 - 1, context))
            {
                var subsequence = backtrack(matrix, array1, array2, index1 - 1, index2 - 1, context);
                subsequence.sequence.Add(array1[index1 - 1]);
                subsequence.indices1.Add(index1 - 1);
                subsequence.indices2.Add(index2 - 1);
                return subsequence;
            }

            if (matrix[index1][index2 - 1] > matrix[index1 - 1][index2])
            {
                return backtrack(matrix, array1, array2, index1, index2 - 1, context);
            }
            else
            {
                return backtrack(matrix, array1, array2, index1 - 1, index2, context);
            }
        }



        public BackTrackResult Get(Object[] array1, object[] array2, IDictionary context)
        {
            context = context ?? new Dictionary<string, object>();
            var matrix = LengthMatrix(array1, array2, context);
            var result = backtrack(matrix, array1, array2, array1.Length, array2.Length, context);
            //if (typeof array1 == = 'string' && typeof array2 == = 'string')
            //{
            //    result.sequence = result.sequence.join('');
            //}
            return result;
        }
    }
}