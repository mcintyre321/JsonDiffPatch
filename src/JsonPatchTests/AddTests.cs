using Newtonsoft.Json.Linq;
using Xunit;

namespace Tavis.JsonPatch.Tests
{
    public class AddTests
    {

        [Fact]
        public void Add_an_array_element()
        {

            var sample = PatchTests.GetSample2();

            var patchDocument = new PatchDocument();
            var pointer = new JsonPointer("/books");

            patchDocument.AddOperation(new AddOperation() { Path = pointer, Value = new JObject(new[] { new JProperty("author", "James Brown") }) });

            var patcher = new JsonPatcher();
            patcher.Patch(ref sample, patchDocument);

            var list = sample["books"] as JArray;

            Assert.Equal(3, list.Count);

        }

        [Fact]
        public void Add_an_existing_member_property()  // Why isn't this replace?
        {

            var sample = PatchTests.GetSample2();

            var patchDocument = new PatchDocument();
            var pointer = new JsonPointer("/books/0/title");

            patchDocument.AddOperation(new AddOperation() { Path = pointer, Value = new JValue("Little Red Riding Hood") });

            var patcher = new JsonPatcher();
            patcher.Patch(ref sample, patchDocument);


            var result = (string)pointer.Find(sample);
            Assert.Equal("Little Red Riding Hood", result);

        }

        [Fact]
        public void Add_an_non_existing_member_property()  // Why isn't this replace?
        {

            var sample = PatchTests.GetSample2();

            var patchDocument = new PatchDocument();
            var pointer = new JsonPointer("/books/0/ISBN");

            patchDocument.AddOperation(new AddOperation() { Path = pointer, Value = new JValue("213324234343") });

            var patcher = new JsonPatcher();
            patcher.Patch(ref sample, patchDocument);


            var result = (string)pointer.Find(sample);
            Assert.Equal("213324234343", result);

        }

    }
}
