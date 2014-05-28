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
    public class CopyTests
    {
        [Fact]
        public void Copy_array_element()
        {
            var sample = PatchTests.GetSample2();

            var patchDocument = new PatchDocument();
            var frompointer = new JsonPointer("/books/0");
            var topointer = new JsonPointer("/books/-");

            patchDocument.AddOperation(new CopyOperation() { FromPath = frompointer, Path = topointer });

            patchDocument.ApplyTo(ref sample);

            var result = new JsonPointer("/books/2").Find(sample);
            Assert.IsType(typeof(JObject), result);

        }

        [Fact]
        public void Copy_property()
        {
            var sample = PatchTests.GetSample2();

            var patchDocument = new PatchDocument();
            var frompointer = new JsonPointer("/books/0/ISBN");
            var topointer = new JsonPointer("/books/1/ISBN");

            patchDocument.AddOperation(new AddOperation() { Path = frompointer, Value = new JValue("21123123") });
            patchDocument.AddOperation(new CopyOperation() { FromPath = frompointer, Path = topointer });

            patchDocument.ApplyTo(ref sample);

            var result = new JsonPointer("/books/1/ISBN").Find(sample);
            Assert.Equal("21123123", result);
        }
    }
}
