using Ninject.Modules;

namespace hasm
{
    internal sealed class HasmModule : NinjectModule
    {
        public override void Load()
        {
            Bind<HasmAssembler>().To<HasmAssembler>();
        }
    }
}
