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
    /// Represents a way to simplify a diff while maintaining its effect.
    /// </summary>
    /// <param name="instructions">The instructions to simplify.</param>
    /// <returns>The list of simplified instructions.</returns>
    public delegate IEnumerable<DifferenceInstruction> DifferenceSimplifier(IEnumerable<DifferenceInstruction> instructions);

    /// <summary>
    /// Represents a repository of <see cref="DifferenceSimplifier"/> implementations.
    /// </summary>
    public static class DifferenceSimplifiers
    {
        private static readonly DifferenceSimplifier _mergeInstructions = MergeInstructionsImpl;

        /// <summary>
        /// Gets the merge instructions.
        /// </summary>
        /// <value>
        /// The merge instructions.
        /// </value>
        public static DifferenceSimplifier MergeInstructions
        {
            get
            {
                return _mergeInstructions;
            }
        }

        /// <summary>
        /// Combines a list of <see cref="DifferenceSimplifier"/> delegates into a single one.
        /// </summary>
        /// <param name="simplifiers">The simplifiers.</param>
        /// <returns>The new single <see cref="DifferenceSimplifier"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="simplifiers"/> is <c>null</c>.</exception>
        public static DifferenceSimplifier Combine(params DifferenceSimplifier[] simplifiers)
        {
            return Combine((IEnumerable<DifferenceSimplifier>)simplifiers);
        }

        /// <summary>
        /// Combines a list of <see cref="DifferenceSimplifier"/> delegates into a single one.
        /// </summary>
        /// <param name="simplifiers">The simplifiers.</param>
        /// <returns>The new single <see cref="DifferenceSimplifier"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="simplifiers"/> is <c>null</c>.</exception>
        public static DifferenceSimplifier Combine(IEnumerable<DifferenceSimplifier> simplifiers)
        {
            if (simplifiers == null) throw new ArgumentNullException("simplifiers");

            DifferenceSimplifier current = null;
            foreach (var item in simplifiers)
            {
                if (current == null)
                    current = item;
                else
                {
                    var previous = current;
                    current = x => current(previous(x));
                }
            }

            if (current == null)
                current = x => x;

            return current;
        }

        private static IEnumerable<DifferenceInstruction> MergeInstructionsImpl(IEnumerable<DifferenceInstruction> instructions)
        {
            DifferenceInstruction? previous = null;
            foreach (var instruction in instructions)
            {
                if (previous.HasValue)
                {
                    if ((previous.Value.Operation == DifferenceOperation.Removed &&
                        instruction.Operation == DifferenceOperation.Inserted) ||
                        (previous.Value.Operation == DifferenceOperation.Inserted &&
                        instruction.Operation == DifferenceOperation.Removed))
                    {
                        if (instruction.Operation == DifferenceOperation.Inserted &&
                            previous.Value.SubSequence.LeftLength == instruction.SubSequence.RightLength)
                        {
                            yield return new DifferenceInstruction(DifferenceOperation.Replaced,
                                new SubSequence(
                                    previous.Value.SubSequence.LeftIndex, instruction.SubSequence.RightIndex,
                                    previous.Value.SubSequence.LeftLength, instruction.SubSequence.RightLength));
                            previous = null;
                        }
                        else if (instruction.Operation == DifferenceOperation.Removed &&
                            instruction.SubSequence.LeftLength == previous.Value.SubSequence.RightLength)
                        {
                            yield return new DifferenceInstruction(DifferenceOperation.Replaced,
                                new SubSequence(
                                    instruction.SubSequence.LeftIndex, previous.Value.SubSequence.RightIndex,
                                    instruction.SubSequence.LeftLength, previous.Value.SubSequence.RightLength));
                            previous = null;
                        }
                        else
                        {
                            yield return previous.Value;
                            previous = instruction;
                        }
                    }
                    else
                    {
                        yield return previous.Value;
                        previous = instruction;
                    }
                }
                else previous = instruction;
            }

            if (previous.HasValue)
                yield return previous.Value;
        }
    }
}
