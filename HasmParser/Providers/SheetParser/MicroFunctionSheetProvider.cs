using hasm.Parsing.Models;

namespace hasm.Parsing.Providers.SheetParser
{
    public sealed class MicroFunctionSheetProvider : BaseSheetProvider<MicroFunction>
    {
        protected override string SheetName => "MicroInstructions";

        protected override MicroFunction Parse(string[] row, MicroFunction previous)
        {
            var instruction = MicroInstruction.Parse(row);
            if (!string.IsNullOrEmpty(row[0]))
                return new MicroFunction(row[0], instruction);

            previous.MicroInstructions.Add(instruction);
            return previous;
        }
    }
}
