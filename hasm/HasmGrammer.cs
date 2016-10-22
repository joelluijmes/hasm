using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NLog;
using ParserLib.Parsing;
using ParserLib.Parsing.Rules;

namespace hasm
{
    internal sealed class HasmGrammer : Grammar
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static Rule GeneralRegister()
        {
            var range = Enumerable.Range(0, 8)
                .Select(i => i.ToString()[0])
                .Select(i => ConvertToValue("r" + i, int.Parse, MatchChar(i)));

            return FirstValue<int>("GeneralRegister", MatchChar('R', true) + Or(range));
        }
        
        public static Rule Parse(string grammar, IDictionary<string, Rule> defines)
        {
            if (grammar == null)
                throw new ArgumentNullException(nameof(grammar));
            if (defines == null)
                throw new ArgumentNullException(nameof(defines));

            var parts = grammar.Split(new []{',', ' '}, StringSplitOptions.RemoveEmptyEntries);

            var rule = Whitespace.Optional;

            _logger.Info($"Parsing {grammar}..");
            foreach (var operand in parts)
            {
                Rule tmp;
                if (defines.TryGetValue(operand, out tmp))
                {
                    rule += Whitespace + tmp;
                    _logger.Debug($"Found definition for {operand}: {tmp}");
                }
                else
                {
                    _logger.Debug($"No definition found for {operand}");
                    if (!rule.Equals(Whitespace.Optional))
                        _logger.Warn($"Assuming that operand '{operand}' is MatchString");

                    rule += MatchString(operand, true);
                }
            }
            _logger.Info($"Parsed {grammar}: {rule}");

            return rule;
        }
    }
}