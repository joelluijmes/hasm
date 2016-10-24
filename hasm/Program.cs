using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using hasm.Properties;
using NLog;
using OfficeOpenXml;
using ParserLib;
using ParserLib.Evaluation;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm
{
    internal class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
#if DEBUG
	        if (Debugger.IsAttached)
	        {
		        var consoleRule = LogManager.Configuration.LoggingRules.First(r => r.Targets.Any(t => t.Name == "console"));
		        consoleRule.EnableLoggingForLevel(LogLevel.Debug);
	        }
#endif

            var defines = new Dictionary<string, HasmGrammer.OperandTypes>
            {
                ["DST"] = HasmGrammer.OperandTypes.DestinationRegister,
                ["SRC"] = HasmGrammer.OperandTypes.SourceRegister
            };

            _logger.Info($"{defines.Count} definitions");
            foreach (var pair in defines)
                _logger.Debug($"{pair.Key}: {pair.Value}");

			var hasm = new HasmGrammer(defines);
	        var instruction = ParseInstructions().First();
	        var parsed = hasm.ParseInstruction(instruction);

	        var input = "mov r1,r2";
	        var encoded = parsed.FirstValue<int>(input);

	        var binary = Convert.ToString(encoded, 2).PadLeft(instruction.Encoding.Length, '0');
	        _logger.Info($"Parsed {input} to encoding {binary}");
        }
        
        private static IEnumerable<Instruction> ParseInstructions()
        {
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
					
                    _logger.Debug($"Added: {instruction}");
					yield return instruction;
                }
            }
        }
    }
}
