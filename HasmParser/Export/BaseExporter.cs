using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace hasm.Parsing.Export
{
    public abstract class BaseExporter : IExporter
    {
        private readonly Stream _stream;

        protected BaseExporter(Stream stream)
        {
            _stream = stream;
        }

        public async Task Export(IEnumerable<IAssembled> listing)
        {
            using (var writer = new StreamWriter(_stream))
            {
                foreach (var assembled in listing)
                    await Export(writer, assembled);

                await Export(writer, null);
            }
        }

        protected abstract Task Export(TextWriter writer, IAssembled assembled);
    }
}