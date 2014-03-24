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
    public class ReplaceTests
    {
        [Fact]
        public void Replace_a_property_value_with_a_new_value()
        {

            var sample = PatchTests.GetSample2();

            var patchDocument = new PatchDocument();
            var pointer = new JsonPointer("/books/0/author");

            patchDocument.AddOperation(new ReplaceOperation() { Path = pointer, Value = new JValue("Bob Brown") });

            patchDocument.ApplyTo(new JsonNetTargetAdapter(sample));

            Assert.Equal("Bob Brown", (string)pointer.Find(sample));
        }

        [Fact]
        public void Replace_a_property_value_with_an_object()
        {

            var sample = PatchTests.GetSample2();

            var patchDocument = new PatchDocument();
            var pointer = new JsonPointer("/books/0/author");

            patchDocument.AddOperation(new ReplaceOperation() { Path = pointer, Value = new JObject(new[] { new JProperty("hello", "world") }) });

            patchDocument.ApplyTo(new JsonNetTargetAdapter(sample));

            var newPointer = new JsonPointer("/books/0/author/hello");
            Assert.Equal("world", (string)newPointer.Find(sample));
        }

    }
}
