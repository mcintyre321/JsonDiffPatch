using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Tavis;
using Xunit;

namespace JsonPatchTests
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
                patchDocument.ApplyTo(ref sample);
            });

        }
    }
}
