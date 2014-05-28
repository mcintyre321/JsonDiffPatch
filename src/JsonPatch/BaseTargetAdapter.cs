using Newtonsoft.Json.Linq;

namespace Tavis
{
    public abstract class BaseTargetAdapter<TDoc> where TDoc : class 
    {
        internal TDoc Doc { get; private set; }

        public BaseTargetAdapter(TDoc doc)
        {
            Doc = doc;
        }

        /// <summary>
        /// return value indicates that a new 
        /// </summary>
        /// <param name="operation"></param>
        /// <returns>a new root document, or null if one is not needed</returns>
        public void ApplyOperation(Operation operation)
        {
            if (operation is AddOperation) Add((AddOperation)operation);
            else if (operation is CopyOperation) Copy((CopyOperation)operation);
            else if (operation is MoveOperation) Move((MoveOperation)operation);
            else if (operation is RemoveOperation) Remove((RemoveOperation)operation);
            else if (operation is ReplaceOperation) Doc = Replace((ReplaceOperation)operation) ?? Doc;
            else if (operation is TestOperation) Test((TestOperation)operation);
        }

        protected abstract void Add(AddOperation operation);
        protected abstract void Copy(CopyOperation operation);
        protected abstract void Move(MoveOperation operation);
        protected abstract void Remove(RemoveOperation operation);
        protected abstract TDoc Replace(ReplaceOperation operation);
        protected abstract void Test(TestOperation operation);
        
    }
}