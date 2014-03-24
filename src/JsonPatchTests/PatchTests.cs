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
        public void Replace_a_property_value_with_a_new_value()
        {

            var sample = GetSample2();

            var patchDocument = new PatchDocument();
            var pointer = new JsonPointer("/books/0/author");

            patchDocument.AddOperation(new ReplaceOperation(){Path = pointer, Value = new JValue("Bob Brown")});

            patchDocument.ApplyTo(new JsonNetTargetAdapter(sample));

            Assert.Equal("Bob Brown", (string)pointer.Find(sample));
        }

        [Fact]
        public void Replace_a_property_value_with_an_object()
        {

            var sample = GetSample2();

            var patchDocument = new PatchDocument();
            var pointer = new JsonPointer("/books/0/author");

            patchDocument.AddOperation(new ReplaceOperation() { Path = pointer, Value = new JObject(new []{new JProperty("hello","world") }) });

            patchDocument.ApplyTo(new JsonNetTargetAdapter(sample));

            var newPointer = new JsonPointer("/books/0/author/hello");
            Assert.Equal("world", (string)newPointer.Find(sample));
        }

        [Fact]
        public void Remove_a_property()
        {

            var sample = GetSample2();

            var patchDocument = new PatchDocument();
            var pointer = new JsonPointer("/books/0/author");

            patchDocument.AddOperation(new RemoveOperation() { Path = pointer});

            patchDocument.ApplyTo(sample);

            Assert.Throws(typeof(ArgumentException),() => { pointer.Find(sample); });
        }

        [Fact]
        public void Remove_an_array_element()
        {

            var sample = GetSample2();

            var patchDocument = new PatchDocument();
            var pointer = new JsonPointer("/books/0");

            patchDocument.AddOperation(new RemoveOperation() { Path = pointer });

            patchDocument.ApplyTo(sample);

            Assert.Throws(typeof (ArgumentException), () =>
            {
                var x = pointer.Find("/books/1");
            });
            
        }

        [Fact]
        public void Test_a_value()
        {

            var sample = GetSample2();

            var patchDocument = new PatchDocument();
            var pointer = new JsonPointer("/books/0/author");

            patchDocument.AddOperation(new TestOperation() { Path = pointer, Value = new JValue("Billy Burton") });

            Assert.Throws(typeof(InvalidOperationException), () =>
            {
                patchDocument.ApplyTo(sample);
            });

        }

        [Fact]
        public void Add_an_array_element()
        {

            var sample = GetSample2();

            var patchDocument = new PatchDocument();
            var pointer = new JsonPointer("/books");

            patchDocument.AddOperation(new AddOperation() { Path = pointer, Value = new JObject(new []{new JProperty("author","James Brown")}) });

            patchDocument.ApplyTo(sample);

            var list = sample["books"] as JArray;

            Assert.Equal(3,list.Count);

        }

        [Fact]
        public void Add_an_existing_member_property()  // Why isn't this replace?
        {

            var sample = GetSample2();

            var patchDocument = new PatchDocument();
            var pointer = new JsonPointer("/books/0/title");

            patchDocument.AddOperation(new AddOperation() { Path = pointer, Value = new JValue("Little Red Riding Hood") });

            patchDocument.ApplyTo(sample);


            var result = (string)pointer.Find(sample);
            Assert.Equal("Little Red Riding Hood", result);

        }

        [Fact]
        public void Add_an_non_existing_member_property()  // Why isn't this replace?
        {

            var sample = GetSample2();

            var patchDocument = new PatchDocument();
            var pointer = new JsonPointer("/books/0/ISBN");

            patchDocument.AddOperation(new AddOperation() { Path = pointer, Value = new JValue("213324234343") });

            patchDocument.ApplyTo(sample);


            var result = (string)pointer.Find(sample);
            Assert.Equal("213324234343", result);

        }

        [Fact]
        public void Move_property()
        {
            var sample = GetSample2();

            var patchDocument = new PatchDocument();
            var frompointer = new JsonPointer("/books/0/author");
            var topointer = new JsonPointer("/books/1/author");

            patchDocument.AddOperation(new MoveOperation() { FromPath = frompointer, Path = topointer});

            patchDocument.ApplyTo(sample);


            var result = (string)topointer.Find(sample);
            Assert.Equal("F. Scott Fitzgerald", result);
        }

        [Fact]
        public void Move_array_element()
        {
            var sample = GetSample2();

            var patchDocument = new PatchDocument();
            var frompointer = new JsonPointer("/books/1");
            var topointer = new JsonPointer("/books/0/child");

            patchDocument.AddOperation(new MoveOperation() { FromPath = frompointer, Path = topointer });

            patchDocument.ApplyTo(sample);


            var result = topointer.Find(sample);
            Assert.IsType(typeof(JObject), result);
        }

        [Fact]
        public void Copy_array_element()
        {
            var sample = GetSample2();

            var patchDocument = new PatchDocument();
            var frompointer = new JsonPointer("/books/0");
            var topointer = new JsonPointer("/books/-");

            patchDocument.AddOperation(new CopyOperation() { FromPath = frompointer, Path = topointer });

            patchDocument.ApplyTo(sample);

            var result = new JsonPointer("/books/2").Find(sample);
            Assert.IsType(typeof(JObject), result);
        
        }

        [Fact]
        public void Copy_property()
        {
            var sample = GetSample2();

            var patchDocument = new PatchDocument();
            var frompointer = new JsonPointer("/books/0/ISBN");
            var topointer = new JsonPointer("/books/1/ISBN");

            patchDocument.AddOperation(new AddOperation() { Path = frompointer, Value = new JValue("21123123")});
            patchDocument.AddOperation(new CopyOperation() { FromPath = frompointer, Path = topointer });

            patchDocument.ApplyTo(sample);

            var result = new JsonPointer("/books/1/ISBN").Find(sample);
            Assert.Equal("21123123", result);
        }

        public JToken GetSample2()
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
