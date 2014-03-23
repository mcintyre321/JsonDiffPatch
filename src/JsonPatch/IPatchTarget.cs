using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tavis
{
    public interface IPatchTarget
    {
        void ApplyOperation(Operation operation);
    }
}
