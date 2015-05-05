//Copyright (c) 2012, Jonathan Dickinson
//All rights reserved.
//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//o Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//o Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//o Neither the name of the DiffLib nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonDiffPatch.DiffLib
{
    /// <summary>
    /// Represents a way to find longest common subsequences, using the patience LCS
    /// algorithm.
    /// </summary>
    /// <typeparam name="T">The type of item within the sequences.</typeparam>
    [SequenceMatcher("Patience")]
    public sealed class PatienceSequenceMatcher<T> : ISequenceMatcher<T>
    {
        #region Patience LCS
        private sealed class Card
        {
            public int BackReference;
            public readonly int Value;

            public Card(int value)
            {
                Value = value;
            }
        }

        private sealed class Pile : List<Card>, IComparable<Pile>
        {
            public Pile(Card card)
                : base(1)
            {
                Add(card);
            }

            public Card Peek()
            {
                return this[Count - 1];
            }

            public int CompareTo(Pile other)
            {
                if (Count == 0 && other.Count == 0) return 0;
                if (Count == 0) return -1;
                if (other.Count == 0) return 1;
                return Peek().Value.CompareTo(other.Peek().Value);
            }
        }

        // http://alfedenzo.livejournal.com/170301.html
        private static IEnumerable<int> PatienceBackreferenceLcs(IEnumerable<int> value)
        {
            var piles = new List<Pile>();
            foreach (var item in value)
            {
                var card = new Card(item);
                var pile = new Pile(card);

                var i = piles.BinarySearch(pile);
                if (i < 0) i = ~i;

                // The first pile doesn't contain back-references
                // but any other will be the lowest card on the deck.
                if (i != 0) card.BackReference = piles[i - 1].Count - 1;

                if (i != piles.Count) piles[i].Add(card);
                else piles.Add(pile);
            }

            if (piles.Count == 0) yield break;

            // Move from the last deck to the first (starting at the top-most card),
            // moving backwards following back-references.

            // The initial "back-reference" points to the top-most card on the deck.
            var backref = piles[piles.Count - 1].Count - 1;
            for (var i = piles.Count - 1; i >= 0; i--)
            {
                var card = piles[i].Skip(backref).First();
                backref = card.BackReference;
                yield return card.Value;
            }
        }
        #endregion

        private readonly IEqualityComparer<T> _equality;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatienceSequenceMatcher{T}"/> class.
        /// </summary>
        public PatienceSequenceMatcher()
            : this(null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PatienceSequenceMatcher{T}"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public PatienceSequenceMatcher(IEqualityComparer<T> comparer)
        {
            _equality = comparer ?? EqualityComparer<T>.Default;
        }

        /// <summary>
        /// Finds unique and common lines between <paramref name="a"/> and
        /// <paramref name="b"/>. They are then sorted according to their appearance
        /// in <paramref name="a"/>, while keeping track of the index in <paramref name="b"/>. Finally
        /// that list is sorted according to a patience sort and the longest common subsequence is
        /// extracted.
        /// </summary>
        /// <param name="a">The first list of values.</param>
        /// <param name="b">The second list of values.</param>
        /// <returns>
        /// A list of tuples that represent the longest common subsequence:
        /// (Index of item in <paramref name="a"/>, Index of item in <paramref name="b"/>).
        /// </returns>
        private IEnumerable<Tuple<int, int>> UniqueLcs(IList<T> a, IList<T> b)
        {
            // set index[line in a] = position of line in a unless
            // a is a duplicate, in which case it's set to None

            var index = new Dictionary<T, int>(_equality);
            for (var i = 0; i < a.Count; i++)
            {
                var line = a[i];
                if (index.ContainsKey(line))
                    index[line] = -1;
                else
                    index[line] = i;
            }

            // make btoa[i] = position of line i in a, unless
            // that line doesn't occur exactly once in both,
            // in which case it's set to None
            // conversely create atob (don't worry about the None)

            var btoa = new int[b.Count];
            var atob = new int[a.Count];
            var index2 = new Dictionary<T, int>(_equality);
            for (var i = 0; i < b.Count; i++)
            {
                btoa[i] = -1;
                var line = b[i];

                int next;
                if (index.TryGetValue(line, out next) && next != -1)
                {
                    int btoai;
                    if (index2.TryGetValue(line, out btoai) && btoai != -1)
                    {
                        btoa[btoai] = -1;
                        index[line] = -1;
                    }
                    else
                    {
                        index2[line] = i;
                        btoa[i] = next;
                        atob[next] = i;
                    }
                }
            }

            return PatienceBackreferenceLcs(btoa.Where(x => x != -1)).Select(x => Tuple.Create(x, atob[x])).Reverse();
        }

        /// <summary>
        /// Recursively applies <see cref="PatienceSequenceMatcher"/> to two lists.
        /// </summary>
        /// <param name="a">The first list.</param>
        /// <param name="b">The second list.</param>
        /// <param name="aLow">The starting index in <paramref name="a"/> (inclusive).</param>
        /// <param name="bLow">The starting index in <paramref name="b"/> (inclusive).</param>
        /// <param name="aHigh">The ending index in <paramref name="a"/> (exclusive).</param>
        /// <param name="bHigh">The ending index in <paramref name="b"/> (exclusive).</param>
        /// <param name="maxRecursion">The maximum number of recursive steps to take.</param>
        /// <returns>The resulting subsequences.</returns>
        private IEnumerable<Tuple<int, int>> RecurseMatches(IList<T> a, IList<T> b, int aLow, int bLow, int aHigh, int bHigh, int maxRecursion)
        {
            if (maxRecursion < 0 || aLow >= aHigh || bLow >= bHigh) yield break;

            var ct = 0;
            var lastAPos = aLow - 1;
            var lastBPos = bLow - 1;

            foreach (var pair in UniqueLcs(a.Sub(aLow, aHigh), b.Sub(bLow, bHigh)))
            {
                // recurse between lines which are unique in each file and match
                var apos = pair.Item1 + aLow;
                var bpos = pair.Item2 + bLow;

                // Most of the time, you will have a sequence of similar entries
                if (lastAPos + 1 != apos || lastBPos + 1 != bpos)
                {
                    foreach (var item in RecurseMatches(a, b, lastAPos + 1, lastBPos + 1, apos, bpos, maxRecursion - 1))
                    {
                        ct++;
                        yield return item;
                    }
                }
                lastAPos = apos;
                lastBPos = bpos;
                ct++;
                yield return Tuple.Create(apos, bpos);
            }

            if (ct > 0)
            {
                // find matches between the last match and the end
                foreach (var item in RecurseMatches(a, b, lastAPos + 1, lastBPos + 1, aHigh, bHigh, maxRecursion - 1))
                {
                    ct++;
                    yield return item;
                }
            }
            else if (_equality.Equals(a[aLow], b[bLow]))
            {
                // find matching lines at the very beginning
                while (aLow < aHigh && bLow < bHigh && _equality.Equals(a[aLow], b[bLow]))
                {
                    ct++;
                    yield return Tuple.Create(aLow, bLow);
                    aLow++;
                    bLow++;
                }
                foreach (var item in RecurseMatches(a, b, aLow, bLow, aHigh, bHigh, maxRecursion - 1))
                {
                    ct++;
                    yield return item;
                }
            }
            else if (_equality.Equals(a[aHigh - 1], b[bHigh - 1]))
            {
                // find matching lines at the very end
                var nahi = aHigh - 1;
                var nbhi = bHigh - 1;
                while (nahi > aLow && nbhi > bLow && _equality.Equals(a[nahi - 1], b[nbhi - 1]))
                {
                    nahi -= 1;
                    nbhi -= 1;
                }
                foreach (var item in RecurseMatches(a, b, lastAPos + 1, lastBPos + 1, nahi, nbhi, maxRecursion - 1))
                {
                    ct++;
                    yield return item;
                }
                for (var i = 0; i < aHigh - nahi; i++)
                {
                    ct++;
                    yield return Tuple.Create(nahi + i, nbhi + i);
                }
            }
        }

        /// <summary>
        /// Finds regions in lists of <see cref="Tuple&lt;int, int&gt;"/> where they both
        /// increment at the same time.
        /// </summary>
        /// <param name="list">The list to find sequences within.</param>
        /// <returns>The matching sequences.</returns>
        private IEnumerable<SubSequence> CollapseSequences(IEnumerable<Tuple<int, int>> list)
        {
            var starta = -1;
            var startb = -1;
            var length = 0;
            foreach (var pair in list)
            {
                var a = pair.Item1;
                var b = pair.Item2;

                if (starta != -1 && a == starta + length && b == startb + length)
                    length += 1;
                else
                {
                    if (starta != -1)
                        yield return new SubSequence(starta, startb, length);
                    starta = a;
                    startb = b;
                    length = 1;
                }
            }

            if (length != 0)
                yield return new SubSequence(starta, startb, length);
        }

        /// <summary>
        /// Finds blocks within two sequences that match.
        /// </summary>
        /// <param name="left">The left-hand sequence.</param>
        /// <param name="right">The right-hand sequence.</param>
        /// <returns>
        /// A list of sequences that represents the blocks that are equal in both
        /// the left-hand and right-hand sequences, ordered by their appearance in both.
        /// </returns>
        public IEnumerable<SubSequence> FindMatchingBlocks(IList<T> left, IList<T> right)
        {
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");

            var matches = RecurseMatches(left, right, 0, 0, left.Count, right.Count, 10);
            return CollapseSequences(matches);
        }

    }
}
