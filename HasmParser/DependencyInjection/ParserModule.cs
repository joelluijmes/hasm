using hasm.Parsing.Encoding;
using hasm.Parsing.Grammars;
using hasm.Parsing.Models;
using hasm.Parsing.Providers;
using hasm.Parsing.Providers.SheetParser;
using Ninject.Modules;

namespace hasm.Parsing.DependencyInjection
{
    public sealed class ParserModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IProvider<OperandEncoding>>().To<OperandSheetProvider>().InSingletonScope();
            Bind<IProvider<InstructionEncoding>>().To<EncodingSheetProvider>().InSingletonScope();
            Bind<IProvider<MicroFunction>>().To<MicroFunctionSheetProvider>().InSingletonScope();
            Bind<IProvider<OperandParser>>().To<HasmGrammar>().InSingletonScope();
            Bind<HasmGrammar>().To<HasmGrammar>().InSingletonScope();
            Bind<HasmEncoder>().To<HasmEncoder>().InSingletonScope();
        }
    }
}