using System.Collections.Generic;
using System.IO;
using System.Linq;
using hasm.Parsing.Properties;
using NLog;
using OfficeOpenXml;

namespace hasm.Parsing.Providers.SheetParser
{
    public abstract class BaseSheetProvider
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected abstract string SheetName { get; }

        public static byte[] Instructionset { get; set; } = Resources.Instructionset;

        protected static IEnumerable<string[]> EnumerateRows(string sheetName)
        {
            using (var stream = new MemoryStream(Instructionset))
            {
                using (var package = new ExcelPackage(stream))
                {
                    var sheet = package.Workbook.Worksheets.First(w => w.Name == sheetName);
                    var start = sheet.Dimension.Start;
                    var end = sheet.Dimension.End;

                    for (var row = start.Row + 1; row <= end.Row; ++row)
                    {
                        var multi = sheet.Cells[row, 1, row, end.Column].Value as object[,];
                        var arrAsString = ConvertToStringArray(multi);
                        if (arrAsString.All(string.IsNullOrEmpty))
                            yield break;

                        yield return arrAsString;
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

    public abstract class BaseSheetProvider<T> : BaseSheetProvider, IProvider<T> where T : class
    {
        private IList<T> _items;

        public virtual IList<T> Items => _items ?? (_items = ParseSheet());

        protected abstract T Parse(string[] row, T previous);

        protected IList<T> ParseSheet()
        {
            var list = new List<T>();
            T previous = null;

            foreach (var row in EnumerateRows(SheetName))
            {
                var current = Parse(row, previous);
                if (EqualityComparer<T>.Default.Equals(current, previous))
                    continue;

                previous = current;

                list.Add(current);
                Logger.Debug($"Added: {current}");
            }

            return list;
        }
    }
}
