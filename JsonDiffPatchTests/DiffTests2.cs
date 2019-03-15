using System;
using System.IO;
using System.Reflection;
using JsonDiffPatch;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tavis.JsonPatch.Tests
{
    public class DiffTests2 { 

        [Test]
        public void ComplexExampleWithDeepArrayChange()
        {
            var currentDir = Path.GetDirectoryName(Assembly.GetCallingAssembly().CodeBase);
            var leftPath = Path.Combine(currentDir, @"Samples\scene{0}a.json").Replace("file://", "").Replace("file:\\", "");
            var rightPath = Path.Combine(currentDir, @"Samples\scene{0}b.json").Replace("file://", "").Replace("file:\\", "");
            var i = 1;
            var filesFound = false;
            while(File.Exists(string.Format(leftPath, i)))
            {
                filesFound = true;

                var scene1Text = File.ReadAllText(string.Format(leftPath, i));
                var scene1 = JToken.Parse(scene1Text);
                var scene2Text = File.ReadAllText(string.Format(rightPath, i));
                var scene2 = JToken.Parse(scene2Text);
                var patchDoc = new JsonDiffer().Diff(scene1, scene2, new CompareOptions(true));
                //Assert.AreEqual("[{\"op\":\"remove\",\"path\":\"/items/0/entities/1\"}]",
                var patcher = new JsonPatcher();
                patcher.Patch(ref scene1, patchDoc);
                Assert.True(JToken.DeepEquals(scene1, scene2));
                i++;
            }

            Assert.IsTrue(filesFound);
        }

        [Test]
        public void ComplexExampleWithDeepArrayChangeOtherIdProperty()
        {
            var currentDir = Path.GetDirectoryName(Assembly.GetCallingAssembly().CodeBase);
            var leftPath = Path.Combine(currentDir, @"Samples\scene{0}a_otherid.json").Replace("file://", "").Replace("file:\\", "");
            var rightPath = Path.Combine(currentDir, @"Samples\scene{0}b_otherid.json").Replace("file://", "").Replace("file:\\", "");
            var i = 1;
            var filesFound = false;
            while (File.Exists(Path.Combine(currentDir, string.Format(leftPath, i))))
            {
                filesFound = true;

                var scene1Text = File.ReadAllText(Path.Combine(currentDir, string.Format(leftPath, i)));
                var scene1 = JToken.Parse(scene1Text);
                var scene2Text = File.ReadAllText(Path.Combine(currentDir, string.Format(rightPath, i)));
                var scene2 = JToken.Parse(scene2Text);
                var patchDoc = new JsonDiffer().Diff(scene1, scene2, new CompareOptions(true, "otherId"));
                var patcher = new JsonPatcher();
                patcher.Patch(ref scene1, patchDoc);
                Assert.True(JToken.DeepEquals(scene1, scene2));
                i++;
            }

            Assert.IsTrue(filesFound);
        }

        [Test]
        public void ComplexExampleWithDeepArrayChangeComplexIdProperty()
        {
            var currentDir = Path.GetDirectoryName(Assembly.GetCallingAssembly().CodeBase);
            var leftPath = Path.Combine(currentDir, @"Samples\scene{0}a_complex_id.json").Replace("file://", "").Replace("file:\\", "");
            var rightPath = Path.Combine(currentDir, @"Samples\scene{0}b_complex_id.json").Replace("file://", "").Replace("file:\\", "");
            var i = 1;
            var filesFound = false;
            while (File.Exists(string.Format(leftPath, i)))
            {
                filesFound = true;

                var scene1Text = File.ReadAllText(string.Format(leftPath, i));
                var scene1 = JToken.Parse(scene1Text);
                var scene2Text = File.ReadAllText(string.Format(rightPath, i));
                var scene2 = JToken.Parse(scene2Text);
                var patchDoc = new JsonDiffer().Diff(scene1, scene2, new CompareOptions(true));
                var patcher = new JsonPatcher();
                patcher.Patch(ref scene1, patchDoc);
                Assert.True(JToken.DeepEquals(scene1, scene2));
                i++;
            }

            Assert.IsTrue(filesFound);
        }
    }
}