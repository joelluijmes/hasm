using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MicParser;
using MicParser.OpCode;

namespace MicAssembler
{
    internal sealed class Disassembler
    {
        public IList<MicroInstruction> Parse(IEnumerable<string> listing)
        {
            var address = 0;
            return listing
                .Select(l => Convert.ToInt64(Regex.Replace(l, "\\s+", ""), 16))
                .Select(v => new MicroOpCode {Value = v})
                .Select(m => new MicroInstruction("", m, "") {Address = address++})
                .ToList();
        }
    }
}