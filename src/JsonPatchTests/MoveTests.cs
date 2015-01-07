using JsonDiffPatch;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Tavis.JsonPatch.Tests
{
    public class MoveTests
    {

        [Fact]
        public void Move_property()
        {
            var sample = PatchTests.GetSample2();

            var patchDocument = new PatchDocument();
            var frompointer = new JsonPointer("/books/0/author");
            var topointer = new JsonPointer("/books/1/author");

            patchDocument.AddOperation(new MoveOperation() { FromPath = frompointer, Path = topointer });

            var patcher = new JsonPatcher();
            patcher.Patch(ref sample, patchDocument);


            var result = (string)topointer.Find(sample);
            Assert.Equal("F. Scott Fitzgerald", result);
        }

        [Fact]
        public void Move_array_element()
        {
            var sample = PatchTests.GetSample2();

            var patchDocument = new PatchDocument();
            var frompointer = new JsonPointer("/books/1");
            var topointer = new JsonPointer("/books/0/child");

            patchDocument.AddOperation(new MoveOperation() { FromPath = frompointer, Path = topointer });

            var patcher = new JsonPatcher();
            patcher.Patch(ref sample, patchDocument);


            var result = topointer.Find(sample);
            Assert.IsType(typeof(JObject), result);
        }


    }
}
