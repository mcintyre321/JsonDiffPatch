using System.Linq;
using JsonDiffPatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tavis.JsonPatch.Tests
{
    [TestFixture]
    public class PatchTests2
    {
        [TestCase("{a:1, b:2, c:3}",
            "[{\"op\":\"remove\",\"path\":\"/c\"}]",
            ExpectedResult = "{\"a\":1,\"b\":2}",
            TestName = "Patch can remove works for a simple value")]
        [TestCase("{a:1, b:2, c:3}",
            "[{\"op\":\"replace\",\"path\":\"\",value:{\"x\":0}}]",
            ExpectedResult = "{\"x\":0}",
            TestName = "Can patch the root object")]
        [TestCase("{\"\":1 }",
            "[{\"op\":\"replace\",\"path\":\"/\",value:{\"x\":0}}]",
            ExpectedResult = "{\"\":{\"x\":0}}",
            TestName = "Can patch a space named property")]
         
        public string JsonPatchesWorks(string leftString, string patchString)
        {
            var left = JToken.Parse(leftString);
            var patchDoc = PatchDocument.Parse(patchString);
            var patcher = new JsonPatcher();
            patcher.Patch(ref left, patchDoc);


            var patchJson = left.ToString(Formatting.None);
            return patchJson;
        }
    }
}