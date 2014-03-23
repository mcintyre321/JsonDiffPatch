namespace Tavis
{
    public abstract class BaseTargetAdapter : IPatchTarget
    {
        public void ApplyOperation(Operation operation)
        {
            if (operation is AddOperation) Add((AddOperation)operation);
            else if (operation is CopyOperation) Copy((CopyOperation)operation);
            else if (operation is MoveOperation) Move((MoveOperation)operation);
            else if (operation is RemoveOperation) Remove((RemoveOperation)operation);
            else if (operation is ReplaceOperation) Replace((ReplaceOperation)operation);
            else if (operation is TestOperation) Test((TestOperation)operation);
        }

        protected abstract void Add(AddOperation operation);
        protected abstract void Copy(CopyOperation operation);
        protected abstract void Move(MoveOperation operation);
        protected abstract void Remove(RemoveOperation operation);
        protected abstract void Replace(ReplaceOperation operation);
        protected abstract void Test(TestOperation operation);
        
    }
}