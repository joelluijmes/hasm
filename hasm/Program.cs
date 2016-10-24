using System;
using System.Collections.Generic;
using System.Diagnostics;
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
#if DEBUG
	        if (Debugger.IsAttached)
	        {
		        var consoleRule = LogManager.Configuration.LoggingRules.First(r => r.Targets.Any(t => t.Name == "console"));
		        consoleRule.EnableLoggingForLevel(LogLevel.Debug);
	        }
#endif

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

			var hasm = new HasmGrammer(defines);
	        var parsed = hasm.ParseInstruction(ParseInstructions().First());
            _logger.Info(parsed.ParseTree("mov r1,r2").PrettyFormat());
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
