namespace JsonDiffPatch
{
    public abstract class AbstractPatcher<TDoc> where TDoc : class 
    {

        /// <summary>
        /// Apply the patch document to the target
        /// </summary>
        /// <param name="target">has to be a ref, as the object may be replaced with something quite different for remove operations that apply to the root</param>
        public virtual void Patch(ref TDoc target, PatchDocument document)
        {
            foreach (var operation in document.Operations)
            {
                target = ApplyOperation(operation, target);
            }
        }

        /// <summary>
        /// return value indicates that a new 
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="target"></param>
        /// <returns>a new root document, or null if one is not needed</returns>
        public virtual TDoc ApplyOperation(Operation operation, TDoc target)
        {
            if (operation is AddOperation) Add((AddOperation)operation, target);
            else if (operation is CopyOperation) Copy((CopyOperation)operation, target);
            else if (operation is MoveOperation) Move((MoveOperation)operation, target);
            else if (operation is RemoveOperation) Remove((RemoveOperation)operation, target);
            else if (operation is ReplaceOperation) target = Replace((ReplaceOperation)operation, target) ?? target;
            else if (operation is TestOperation) Test((TestOperation)operation, target);
            return target;
        }

        protected abstract void Add(AddOperation operation, TDoc target);
        protected abstract void Copy(CopyOperation operation, TDoc target);
        protected abstract void Move(MoveOperation operation, TDoc target);
        protected abstract void Remove(RemoveOperation operation, TDoc target);
        protected abstract TDoc Replace(ReplaceOperation operation, TDoc target);
        protected abstract void Test(TestOperation operation, TDoc target);
        
    }
}