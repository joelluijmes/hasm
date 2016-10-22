using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using hasm.Properties;
using NLog;
using OfficeOpenXml;
using ParserLib;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm
{
    internal class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            var instruction = ParseInstructions().First();
            var register = HasmGrammer.GeneralRegister();   // same reference
            var defines = new Dictionary<string, Rule>
            {
                ["DST"] = register,
                ["SRC"] = register
            };

            _logger.Info($"{defines.Count} definitions");
            foreach (var pair in defines)
                _logger.Debug($"{pair.Key}: {pair.Value}");

            var parsed = HasmGrammer.Parse(instruction.Grammar, defines);
            _logger.Info($"{Environment.NewLine}{parsed.PrettyFormat()}");
        }
        
        private static List<Instruction> ParseInstructions()
        {
            var instructions = new List<Instruction>();

            using (var stream = new MemoryStream(Resources.Instructionset))
            using (var package = new ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets.First();
                
                var start = sheet.Dimension.Start;
                var end = sheet.Dimension.End;
                
                for (var row = start.Row + 1; row <= end.Row; ++row)
                {
                    var range = sheet.Cells[row, 1, row, end.Column];
                    var instruction = Instruction.Parse(range);

                    instructions.Add(instruction);
                    _logger.Debug($"Added: {instruction}");
                }
            }

            _logger.Info($"Parsed {instructions.Count} instructions from sheet");
            return instructions;
        }
    }
}
