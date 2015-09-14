using System;
using JsonDiffPatch;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Tavis.JsonPatch.Tests
{
    public class TestTests
    {
        [Fact]
        public void Test_a_value()
        {

            var sample = PatchTests.GetSample2();

            var patchDocument = new PatchDocument();
            var pointer = new JsonPointer("/books/0/author");

            patchDocument.AddOperation(new TestOperation() { Path = pointer, Value = new JValue("Billy Burton") });

            Assert.Throws(typeof(InvalidOperationException), () =>
            {
                var patcher = new JsonPatcher();
                patcher.Patch(ref sample, patchDocument);
            });

        }
    }
}
