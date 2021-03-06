﻿using hasm.Parsing.Models;

namespace hasm.Parsing.Providers.SheetParser
{
    internal sealed class EncodingSheetProvider : BaseSheetProvider<InstructionEncoding>
    {
        protected override string SheetName => "Encoding";

        protected override InstructionEncoding Parse(string[] row, InstructionEncoding previous)
            => InstructionEncoding.Parse(row);
    }
}