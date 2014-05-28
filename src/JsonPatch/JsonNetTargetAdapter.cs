using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Tavis
{
    public class JsonNetTargetAdapter : BaseTargetAdapter<JToken>
    {
        public JsonNetTargetAdapter(JToken target) : base(target)
        {
        }


        protected override JToken Replace(ReplaceOperation operation)
        {
            var token = operation.Path.Find(base.Doc);
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

        protected override void Add(AddOperation operation)
        {
            JToken token = null;
            JObject parenttoken = null;
            try
            {
                if (operation.Path.IsNewPointer())
                {
                    var parentPointer = operation.Path.ParentPointer;
                    token = parentPointer.Find(base.Doc) as JArray;
                }
                else
                {
                    token = operation.Path.Find(base.Doc);
                }
            }
            catch (ArgumentException)
            {
                var parentPointer = operation.Path.ParentPointer;
                parenttoken = parentPointer.Find(base.Doc) as JObject;
            }

            if (token == null && parenttoken != null)
            {
                parenttoken.Add(operation.Path.ToString().Split('/').Last(), operation.Value);
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


        protected override void Remove(RemoveOperation operation)
        {
            var token = operation.Path.Find(base.Doc);
            if (token.Parent is JProperty)
            {
                token.Parent.Remove();
            }
            else
            {
                token.Remove();
            }
        }

        protected override void Move(MoveOperation operation)
        {
            if (operation.Path.ToString().StartsWith(operation.FromPath.ToString())) throw new ArgumentException("To path cannot be below from path");

            var token = operation.FromPath.Find(base.Doc);
            Remove(new RemoveOperation(){Path = operation.FromPath});
            Add(new AddOperation() {Path = operation.Path, Value = token});
        }

        protected override void Test(TestOperation operation)
        {
            var existingValue = operation.Path.Find(base.Doc);
            if (!existingValue.Equals(base.Doc))
            {
                throw new InvalidOperationException("Value at " + operation.Path.ToString() + " does not match.");
            }
        }

        protected override void Copy(CopyOperation operation)
        {
            var token = operation.FromPath.Find(base.Doc);  // Do I need to clone this?
            Add(new AddOperation() {Path = operation.Path, Value = token});
        }
    }
}