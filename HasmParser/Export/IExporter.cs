using System.Collections.Generic;
using System.Threading.Tasks;

namespace hasm.Parsing.Export
{
    public interface IExporter
    {
        Task Export(IEnumerable<IAssembled> listing);
    }
}
