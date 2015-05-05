//Copyright (c) 2012, Jonathan Dickinson
//All rights reserved.
//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//o Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//o Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//o Neither the name of the DiffLib nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;

namespace JsonDiffPatch.DiffLib
{
    /// <summary>
    /// Represents a difference instruction.
    /// </summary>
    public struct DifferenceInstruction : IEquatable<DifferenceInstruction>, IFormattable
    {
        /// <summary>
        /// Gets the operation that should be performed to turn the left data
        /// into the right data.
        /// </summary>
        public readonly DifferenceOperation Operation;

        /// <summary>
        /// Get the sub sequence that contains the data the instruction applies to.
        /// </summary>
        public readonly SubSequence SubSequence;

        /// <summary>
        /// Initializes a new instance of the <see cref="DifferenceInstruction" /> struct.
        /// </summary>
        /// <param name="operation">The operation that should be performed to turn the left data
        /// into the right data.</param>
        /// <param name="subsequence">The sub sequence that contains the data the instruction applies to.</param>
        public DifferenceInstruction(DifferenceOperation operation, SubSequence subsequence)
        {
            Operation = operation;
            SubSequence = subsequence;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return ((int)Operation) ^ SubSequence.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is DifferenceInstruction)) return false;
            return Equals((DifferenceInstruction)obj);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the other parameter; otherwise, false.
        /// </returns>
        public bool Equals(DifferenceInstruction other)
        {
            return Operation == other.Operation && SubSequence.Equals(other.SubSequence);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString("S", null);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            switch (Operation)
            {
                case DifferenceOperation.Equal:
                    return string.Format(formatProvider, "= {0}", SubSequence.ToString(format, formatProvider));
                case DifferenceOperation.Inserted:
                    return string.Format(formatProvider, "+ {0}", SubSequence.ToString(format, formatProvider));
                case DifferenceOperation.Replaced:
                    return string.Format(formatProvider, "> {0}", SubSequence.ToString(format, formatProvider));
                case DifferenceOperation.Removed:
                    return string.Format(formatProvider, "- {0}", SubSequence.ToString(format, formatProvider));
                default:
                    return string.Format(formatProvider, "{0} {1}", Operation, SubSequence.ToString(format, formatProvider));
            }
        }
    }
}
