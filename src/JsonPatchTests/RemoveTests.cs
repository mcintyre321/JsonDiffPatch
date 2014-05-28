using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavis;
using Xunit;

namespace JsonPatchTests
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

            patchDocument.ApplyTo(ref sample);

            Assert.Throws(typeof(ArgumentException), () => { pointer.Find(sample); });
        }

        [Fact]
        public void Remove_an_array_element()
        {

            var sample = PatchTests.GetSample2();

            var patchDocument = new PatchDocument();
            var pointer = new JsonPointer("/books/0");

            patchDocument.AddOperation(new RemoveOperation() { Path = pointer });

            patchDocument.ApplyTo(ref sample);

            Assert.Throws(typeof(ArgumentException), () =>
            {
                var x = pointer.Find("/books/1");
            });

        }
    }
}
