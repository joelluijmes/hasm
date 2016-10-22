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

            Rule rule = null;
            Action<Rule, Rule> appendRule = (target, appendend) =>
            {
                if (target == null)
                    rule = appendend;
                else if (Equals(target.GetChildren().Last(), Whitespace))
                    rule = target + appendend;
                else
                    rule = target + MatchChar(',') + appendend;
            };

            _logger.Info($"Parsing {grammar}..");
            foreach (var operand in parts)
            {
                Rule tmp;
                if (defines.TryGetValue(operand, out tmp))
                {
                    _logger.Debug($"Found definition for {operand}: {tmp}");
                    appendRule(rule, tmp);
                }
                else
                {
                    _logger.Debug($"No definition found for {operand}");
                    if (rule != null)
                        _logger.Warn($"Assuming that operand '{operand}' is MatchString");

                    appendRule(rule, MatchString(operand, true) + Whitespace);
                }
            }
            _logger.Info($"Parsed {grammar}: {rule}");

            return rule;
        }
    }
}