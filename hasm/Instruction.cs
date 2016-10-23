using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using OfficeOpenXml;

namespace hasm
{
    internal class Instruction
    {
        public Instruction(string grammar, string description, string semantic, string encoding)
        {
            Grammar = grammar;
            Description = description;
            Semantic = semantic;
            Encoding = encoding;
        }

        public string Grammar { get; }
        public string Description { get; }
        public string Semantic { get; }
        public int Count => Encoding.Length/8;
        public string Encoding { get; }

        public override string ToString() => $"{Grammar.Split(' ')[0]} ({Description})";

        public static Instruction Parse(ExcelRange row)
        {
            var rowIndex = row.Start.Row;
            var grammar = row[rowIndex, 1].GetValue<string>();
            var description = row[rowIndex, 2].GetValue<string>();
            var semantic = row[rowIndex, 3].GetValue<string>();
            
            var encoding = ParseToString(row[rowIndex, 4, rowIndex, 9].Value as object[,]);
            
            return new Instruction(grammar, description, semantic, encoding);
        }

        private static string ParseToString(object[,] multi)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < multi.GetLength(1); ++i)
                builder.Append(multi[0, i]);
            
            return builder.ToString();
        }
    }
}