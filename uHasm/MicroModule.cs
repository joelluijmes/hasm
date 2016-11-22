using Ninject.Modules;

namespace hasm
{
    internal sealed class MicroModule : NinjectModule
    {
        public override void Load()
        {
            Bind<MicroAssembler>().To<MicroAssembler>();
        }
    }
}