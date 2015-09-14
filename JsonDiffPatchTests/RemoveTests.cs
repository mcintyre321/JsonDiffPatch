using System;
using JsonDiffPatch;
using Xunit;

namespace Tavis.JsonPatch.Tests
{
    public class RemoveTests
    {
        [Fact]
        public void Remove_a_property()
        {

            var sample = PatchTests.GetSample2();

            var patchDocument = new PatchDocument();
            var pointer = new JsonPointer("/books/0/author");

            patchDocument.AddOperation(new RemoveOperation() { Path = pointer });

            new JsonPatcher().Patch(ref sample, patchDocument);

            Assert.Throws(typeof(ArgumentException), () => { pointer.Find(sample); });
        }

        [Fact]
        public void Remove_an_array_element()
        {

            var sample = PatchTests.GetSample2();

            var patchDocument = new PatchDocument();
            var pointer = new JsonPointer("/books/0");

            patchDocument.AddOperation(new RemoveOperation() { Path = pointer });

            var patcher = new JsonPatcher();
            patcher.Patch(ref sample, patchDocument);

            Assert.Throws(typeof(ArgumentException), () =>
            {
                var x = pointer.Find("/books/1");
            });

        }
    }
}
