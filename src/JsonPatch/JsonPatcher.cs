using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace JsonDiffPatch
{
    public class JsonPatcher : AbstractPatcher<JToken>
    {

        protected override JToken Replace(ReplaceOperation operation, JToken target)
        {
            var token = operation.Path.Find(target);
            if (token.Parent == null)
            {
                return operation.Value;
            }
            else
            {
                token.Replace(operation.Value);
                return null;
            }
        }

        protected override void Add(AddOperation operation, JToken target)
        {
            JToken token = null;
            JObject parenttoken = null;
            var propertyName = operation.Path.ToString().Split('/').LastOrDefault();
            try
            {
                var parentArray = operation.Path.ParentPointer.Find(target) as JArray;

                if (parentArray == null || propertyName == "-")
                {
                    if (operation.Path.IsNewPointer())
                    {
                        var parentPointer = operation.Path.ParentPointer;
                        token = parentPointer.Find(target) as JArray;
                    }
                    else
                    {
                        token = operation.Path.Find(target);
                    }
                }
                else
                {
                    parentArray.Insert(int.Parse(propertyName), operation.Value);
                    return;
                }
            }
            catch (ArgumentException)
            {
                var parentPointer = operation.Path.ParentPointer;
                parenttoken = parentPointer.Find(target) as JObject;
            }

            if (token == null && parenttoken != null)
            {
                parenttoken.Add(propertyName, operation.Value);
            }
            else if (token is JArray)
            {
                var array = token as JArray;
                
                array.Add(operation.Value);
            }
            else if (token.Parent is JProperty)
            {
                var prop = token.Parent as JProperty;
                prop.Value = operation.Value;
            }
        }


        protected override void Remove(RemoveOperation operation, JToken target)
        {
            var token = operation.Path.Find(target);
            if (token.Parent is JProperty)
            {
                token.Parent.Remove();
            }
            else
            {
                token.Remove();
            }
        }

        protected override void Move(MoveOperation operation, JToken target)
        {
            if (operation.Path.ToString().StartsWith(operation.FromPath.ToString())) throw new ArgumentException("To path cannot be below from path");

            var token = operation.FromPath.Find(target);
            Remove(new RemoveOperation(){Path = operation.FromPath}, target);
            Add(new AddOperation() {Path = operation.Path, Value = token}, target);
        }

        protected override void Test(TestOperation operation, JToken target)
        {
            var existingValue = operation.Path.Find(target);
            if (!existingValue.Equals(target))
            {
                throw new InvalidOperationException("Value at " + operation.Path.ToString() + " does not match.");
            }
        }

        protected override void Copy(CopyOperation operation, JToken target)
        {
            var token = operation.FromPath.Find(target);  // Do I need to clone this?
            Add(new AddOperation() {Path = operation.Path, Value = token}, target);
        }
    }
}