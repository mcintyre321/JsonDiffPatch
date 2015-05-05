//Copyright (c) 2012, Jonathan Dickinson
//All rights reserved.
//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//o Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//o Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//o Neither the name of the DiffLib nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.IO;

namespace JsonDiffPatch.DiffLib
{
    /// <summary>
    /// Represents a way to format difference instructions.
    /// </summary>
    public interface IDifferenceFormatter<T>
    {
        /// <summary>
        /// Formats the specified difference instructions and
        /// writes them to the specified writer.
        /// </summary>
        /// <param name="left">The original left-hand content.</param>
        /// <param name="right">The original right-hand content.</param>
        /// <param name="instructions">The instructions to format.</param>
        /// <param name="target">The target writer.</param>
        void Format(IList<T> left, IList<T> right, IEnumerable<DifferenceInstruction> instructions, TextWriter target);
    }

    /// <summary>
    /// Represents mixins for <see cref="IDifferenceFormatter&lt;T&gt;"/>.
    /// </summary>
    public static class IDifferenceFormatterMixins
    {
        /// <summary>
        /// Formats the specified difference instructions and
        /// writes them to the specified writer.
        /// </summary>
        /// <param name="left">The original left-hand content.</param>
        /// <param name="right">The original right-hand content.</param>
        /// <param name="instructions">The instructions to format.</param>
        /// <param name="target">The target writer.</param>
        public static string Format<T>(this IDifferenceFormatter<T> formatter, IList<T> left, IList<T> right, IEnumerable<DifferenceInstruction> instructions)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            using (var stringwriter = new StringWriter())
            {
                formatter.Format(left, right, instructions, stringwriter);
                return stringwriter.ToString();
            }
        }
    }
}
