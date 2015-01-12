# JsonDiffPatch



This library is an implementation of a Json Patch [RFC 6902](http://tools.ietf.org/html/rfc6902).  
 * forked from https://github.com/tavis-software/Tavis.JsonPatch
 * plus a modified diff generator by Ian Mercer (http://blog.abodit.com/2014/05/json-patch-c-implementation/)  

The default example from the specification looks like this,

	[
	     { "op": "test", "path": "/a/b/c", "value": "foo" },
	     { "op": "remove", "path": "/a/b/c" },
	     { "op": "add", "path": "/a/b/c", "value": ["foo", "bar"] },
	     { "op": "replace", "path": "/a/b/c", "value": 42 },
	     { "op": "move", "from": "/a/b/c", "path": "/a/b/d" },
	     { "op": "copy", "from": "/a/b/d", "path": "/a/b/e" }
	]

This library allows you to create this document by doing, 


       var patchDoc = new PatchDocument( new Operation[]
            {
             new TestOperation() {Path = new JsonPointer("/a/b/c"), Value = new JValue("foo")}, 
             new RemoveOperation() {Path = new JsonPointer("/a/b/c") }, 
             new AddOperation() {Path = new JsonPointer("/a/b/c"), Value = new JArray(new JValue("foo"), new JValue("bar"))}, 
             new ReplaceOperation() {Path = new JsonPointer("/a/b/c"), Value = new JValue(42)}, 
             new MoveOperation() {FromPath = new JsonPointer("/a/b/c"), Path = new JsonPointer("/a/b/d") }, 
             new CopyOperation() {FromPath = new JsonPointer("/a/b/d"), Path = new JsonPointer("/a/b/e") }, 
            });

This document can be serialized to the wire format like this,

         var outputstream = patchDoc.ToStream();

You can also read patch documents from the wire representation and apply them to a JSON document.
	
	var targetDoc = JToken.Parse("{ 'foo': 'bar'}");
        var patchDoc = PatchDocument.Parse(@"[ { 'op': 'add', 'path': '/baz', 'value': 'qux' } ]");

        patchDoc.ApplyTo(targetDoc);

You can also generate a patchdocument by diffing two json objects

		var left = JToken.Parse(leftString);
            	var right = JToken.Parse(rightString);
            	var patchDoc = new JsonDiffer().Diff(left, right);

You can apply a patch document to a json object too.

		var left = JToken.Parse(leftString);
            	var patcher = new JsonPatcher();
            	patcher.Patch(ref left, patchDoc); //left is now updated by the patchDoc
            	


The unit tests provide examples of other usages.

This library is a PCL based library and so will work on Windows 8, Windows Phone 7.5, .Net 4.

A nuget package is available [here](http://www.nuget.org/packages/Tavis.JsonPatch).
