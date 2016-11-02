using System.Collections.Generic;
using System.IO;
using System.Linq;
using hasm.Parsing.Properties;
using NLog;
using OfficeOpenXml;

namespace hasm.Parsing.Parsers.Sheet
{
    public abstract class BaseSheetParser<T> where T : class
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IList<T> _items;

        protected abstract string SheetName { get; }

        public virtual IList<T> Items => _items ?? (_items = ParseSheet());

        protected abstract T Parse(string[] row, T previous);

        protected IList<T> ParseSheet()
        {
            var list = new List<T>();
            T previous = null;

            foreach (var row in EnumerateRows(SheetName))
            {
                var current = Parse(row, previous);
                previous = current;

                list.Add(current);
                _logger.Debug($"Added: {current}");
            }

            return list;
        }

        private static IEnumerable<string[]> EnumerateRows(string sheetName)
        {
            using (var stream = new MemoryStream(Resources.Instructionset))
            {
                using (var package = new ExcelPackage(stream))
                {
                    var sheet = package.Workbook.Worksheets.First(w => w.Name == sheetName);
                    var start = sheet.Dimension.Start;
                    var end = sheet.Dimension.End;

                    for (var row = start.Row + 1; row <= end.Row; ++row)
                    {
                        var multi = sheet.Cells[row, 1, row, end.Column].Value as object[,];
                        yield return ConvertToStringArray(multi);
                    }
                }
            }
        }

        private static string[] ConvertToStringArray(object[,] multi)
        {
            var y = multi.GetLength(1);
            var str = new string[y];
            for (var i = 0; i < y; ++i)
                str[i] = multi[0, i]?.ToString() ?? "";

            return str;
        }
    }
}