using hasm.Parsing.Grammars;
using hasm.Parsing.Models;
using hasm.Parsing.Parsers;
using hasm.Parsing.Parsers.Sheet;
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
            Bind<HasmGrammar>().To<HasmGrammar>().InSingletonScope();
        }
    }
}