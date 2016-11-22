using System.Collections.Generic;

namespace hasm.Parsing.Parsers
{
    public interface IProvider<T> where T : class
    {
        IList<T> Items { get; }
    }
}