using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Tavis;
using Xunit;

namespace JsonPatchTests
{
    public class PatchTests
    {
        [Fact]
        public void CreateEmptyPatch()
        {

            var sample = GetSample2();
            var sampletext = sample.ToString();

            var patchDocument = new PatchDocument();
            patchDocument.ApplyTo(new JsonNetTargetAdapter(sample));

            Assert.Equal(sampletext,sample.ToString());
        }


        [Fact]
        public void LoadPatch1()
        {
            var patchDoc =
                PatchDocument.Load(this.GetType()
                    .Assembly.GetManifestResourceStream(this.GetType(), "Samples.LoadTest1.json"));

            Assert.NotNull(patchDoc);
            Assert.Equal(6,patchDoc.Operations.Count);
        }


        [Fact]
        public void TestExample1()
        {
            var targetDoc = JToken.Parse("{ 'foo': 'bar'}");
            var patchDoc = PatchDocument.Parse(@"[
     { 'op': 'add', 'path': '/baz', 'value': 'qux' }
   ]");
            patchDoc.ApplyTo(targetDoc);


            Assert.Equal(JToken.Parse(@"{
     'foo': 'bar',
     'baz': 'qux'
   }"), targetDoc);
        }


        [Fact]
        public void SerializePatchDocument()
        {
            var patchDoc = new PatchDocument( new Operation[]
            {
             new TestOperation() {Path = new JsonPointer("/a/b/c"), Value = new JValue("foo")}, 
             new RemoveOperation() {Path = new JsonPointer("/a/b/c") }, 
             new AddOperation() {Path = new JsonPointer("/a/b/c"), Value = new JArray(new JValue("foo"), new JValue("bar"))}, 
             new ReplaceOperation() {Path = new JsonPointer("/a/b/c"), Value = new JValue(42)}, 
             new MoveOperation() {FromPath = new JsonPointer("/a/b/c"), Path = new JsonPointer("/a/b/d") }, 
             new CopyOperation() {FromPath = new JsonPointer("/a/b/d"), Path = new JsonPointer("/a/b/e") }, 
            });

            var outputstream = patchDoc.ToStream();
            var output = new StreamReader(outputstream).ReadToEnd();

            var jOutput = JToken.Parse(output);

            var jExpected = JToken.Parse(new StreamReader(this.GetType()
                .Assembly.GetManifestResourceStream(this.GetType(), "Samples.LoadTest1.json")).ReadToEnd());
            Assert.True(JToken.DeepEquals(jExpected,jOutput));
        }



        public static JToken GetSample2()
        {
            return JToken.Parse(@"{
    'books': [
        {
          'title' : 'The Great Gatsby',
          'author' : 'F. Scott Fitzgerald'
        },
        {
          'title' : 'The Grapes of Wrath',
          'author' : 'John Steinbeck'
        }
    ]
}");
        }
    }
}
