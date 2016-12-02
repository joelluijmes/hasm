using System.Collections.Generic;

namespace hasm.Parsing.Providers
{
    public interface IProvider<T> where T : class
    {
        IList<T> Items { get; }
    }
}
