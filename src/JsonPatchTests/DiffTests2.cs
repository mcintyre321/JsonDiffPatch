using System.IO;
using JsonDiffPatch;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tavis.JsonPatch.Tests
{
    public class DiffTests2 { 

        [Test]
        public void ComplexExampleWithDeepArrayChange()
        {
            
            var leftPath = @".\samples\scene{0}a.json";
            var rightPath = @".\samples\scene{0}b.json";
            var i = 1;
            while(File.Exists(string.Format(leftPath, i)))
            {
                var scene1Text = File.ReadAllText(string.Format(leftPath, i));
                var scene1 = JToken.Parse(scene1Text);
                var scene2Text = File.ReadAllText(string.Format(rightPath, i));
                var scene2 = JToken.Parse(scene2Text);
                var patchDoc = new JsonDiffer().Diff(scene1, scene2, true);
                //Assert.AreEqual("[{\"op\":\"remove\",\"path\":\"/items/0/entities/1\"}]",
                var patcher = new JsonPatcher();
                patcher.Patch(ref scene1, patchDoc);
                Assert.True(JToken.DeepEquals(scene1, scene2));
                i++;
            }
        }


    }
}