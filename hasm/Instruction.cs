using System;
using System.Collections.Generic;
using NLog;
using OfficeOpenXml;

namespace hasm
{
    internal class Instruction
    {
        public Instruction(string grammar, string description, string semantic, string[] encoding)
        {
            Grammar = grammar;
            Description = description;
            Semantic = semantic;
            Encoding = encoding;
        }

        public string Grammar { get; }
        public string Description { get; }
        public string Semantic { get; }
        public int Count => Encoding.Length*4;
        public string[] Encoding { get; }

        public override string ToString() => $"{Grammar.Split(' ')[0]} ({Description})";

        public static Instruction Parse(ExcelRange row)
        {
            var rowIndex = row.Start.Row;
            var grammar = row[rowIndex, 1].GetValue<string>();
            var description = row[rowIndex, 2].GetValue<string>();
            var semantic = row[rowIndex, 3].GetValue<string>();
            
            var strencoding = ParseToString(row[rowIndex, 4, rowIndex, 9].Value as object[,]);
            var encoding = ParseEncoding(strencoding);
            
            return new Instruction(grammar, description, semantic, encoding);
        }

        private static string[] ParseToString(object[,] multi)
        {
            var strarr = new string[multi.GetLength(1)];
            for (var i = 0; i < multi.GetLength(1); ++i)
                strarr[i] = multi[0, i]?.ToString();

            return strarr;
        }

        private static string[] ParseEncoding(IList<string> input)
        {
            if (input.Count != 6)
                throw new ArgumentException("Count of input must be 6", nameof(input));

            if (input[0] != null)
            {
                return input[4] == null
                    ? new[] {input[0], input[1], input[2], input[3]}
                    : new[] {input[0], input[1], input[2], input[3], input[4], input[5]};
            }

            if (input[4] == null)
                throw new ArgumentException("Invalid encoding input", nameof(input));

            return new[] {input[4], input[5]};
        }
    }
}