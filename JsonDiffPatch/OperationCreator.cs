using System;
using System.Collections.Generic;
using System.Text;

namespace JsonDiffPatch
{
    internal static class OperationCreator
    {
        public static Operation Create(string op)
        {
            switch (op)
            {
                case "add": return new AddOperation();
                case "copy": return new CopyOperation();
                case "move": return new MoveOperation();
                case "remove": return new RemoveOperation();
                case "replace": return new ReplaceOperation();
                case "test": return new TestOperation();
            }
            return null;
        }
    }
}
