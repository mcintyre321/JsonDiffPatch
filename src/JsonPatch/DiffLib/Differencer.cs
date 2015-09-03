//Copyright (c) 2012, Jonathan Dickinson
//All rights reserved.
//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//o Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//o Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//o Neither the name of the DiffLib nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;

namespace JsonDiffPatch.DiffLib
{
    /// <summary>
    /// Represents a way to find the difference between two
    /// sequences using a specific longest common subsequence
    /// algorithm.
    /// </summary>
    public sealed class Differencer<T>
    {
        private readonly ISequenceMatcher<T> _sequenceMatcher;
        private DifferenceSimplifier _simplifier = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Differencer{T}" /> class.
        /// </summary>
        /// <param name="sequenceMatcher">The sequence matcher to use to find longest common subsequences.</param>
        public Differencer(ISequenceMatcher<T> sequenceMatcher)
        {
            if (sequenceMatcher == null) throw new ArgumentNullException("sequenceMatcher");
            _sequenceMatcher = sequenceMatcher;
        }

        /// <summary>
        /// Adds a simplifier to the differences.
        /// </summary>
        /// <param name="simplifier">The simplifier to add.</param>
        /// <returns><c>this</c> instance.</returns>
        public Differencer<T> AddSimplifier(DifferenceSimplifier simplifier)
        {
            if (simplifier == null) throw new ArgumentNullException("simplifier");
            if (_simplifier == null) _simplifier = simplifier;
            else _simplifier = DifferenceSimplifiers.Combine(_simplifier, simplifier);
            return this;
        }

        /// <summary>
        /// Calculates the difference between two sequences.
        /// </summary>
        /// <param name="left">The left-hand sequence.</param>
        /// <param name="right">The right-hand sequence.</param>
        /// <returns>
        /// The operations that should be applied to <see cref="left" /> in order to
        /// acquire <see cref="right" />.
        /// </returns>
        public IEnumerable<DifferenceInstruction> FindDifferences(IList<T> left, IList<T> right)
        {
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");

            var matches = _sequenceMatcher.FindMatchingBlocks(left, right);

            var lastLeftIndex = -1;
            var lastRightIndex = -1;

            foreach (var subSequence in matches)
            {
                if (lastLeftIndex != -1)
                {
                    var diff = subSequence.LeftIndex - lastLeftIndex;
                    if (diff > 0)
                        yield return new DifferenceInstruction(DifferenceOperation.Removed,
                            new SubSequence(lastLeftIndex, 0, diff, 0));
                }

                if (lastRightIndex != -1)
                {
                    var diff = subSequence.RightIndex - lastRightIndex;
                    if (diff > 0)
                        yield return new DifferenceInstruction(DifferenceOperation.Inserted,
                            new SubSequence(0, lastRightIndex, 0, diff));
                }

                if (subSequence.LeftLength > 0 && subSequence.LeftLength == subSequence.RightLength)
                {
                    yield return new DifferenceInstruction(DifferenceOperation.Equal, subSequence);
                }

                lastLeftIndex = subSequence.LeftEndIndex + 1;
                lastRightIndex = subSequence.RightEndIndex + 1;
            }

            if (lastLeftIndex < left.Count)
            {
                if (lastLeftIndex < 0) lastLeftIndex = 0;
                yield return new DifferenceInstruction(DifferenceOperation.Removed,
                            new SubSequence(lastLeftIndex, 0, left.Count - lastLeftIndex, 0));
            }

            if (lastRightIndex < right.Count)
            {
                if (lastRightIndex < 0) lastRightIndex = 0;
                yield return new DifferenceInstruction(DifferenceOperation.Inserted,
                            new SubSequence(0, lastRightIndex, 0, right.Count - lastRightIndex));
            }
        }

    }
}
