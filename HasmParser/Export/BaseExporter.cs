using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace hasm.Parsing.Export
{
    public abstract class BaseExporter : IExporter, IDisposable
    {
        protected BaseExporter(Stream stream)
        {
            Writer = new StreamWriter(stream, System.Text.Encoding.ASCII, 1024, true);
        }

        public StreamWriter Writer { get; }

        public void Dispose()
        {
            Writer.Dispose();
        }

        public async Task Export(IEnumerable<IAssembled> listing)
        {
            foreach (var assembled in listing)
                await Export(assembled);

            await Export((IAssembled) null);
        }

        protected abstract Task Export(IAssembled assembled);
    }
}
