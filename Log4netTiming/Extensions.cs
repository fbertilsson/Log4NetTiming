using System;
using System.Data;
using System.Text;

namespace Log4netTiming
{
    public static class Extensions
    {
        private const string Separator = ";";

        /// <summary>
        /// Returns a CSV representation of <paramref name="table"/>. For use by spreadsheets etc.
        /// </summary>
        /// <param name="table"></param>
        /// <returns>A string on CSV format</returns>
        public static string ToCsv(this DataTable table)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < table.Columns.Count; i++)
            {
                builder.Append(table.Columns[i].ColumnName);
                builder.Append(i == table.Columns.Count - 1 ? Environment.NewLine : Separator);
            }

            foreach (DataRow row in table.Rows)
            {
                for (var i = 0; i < table.Columns.Count; i++)
                {
                    builder.Append(row[i]);
                    builder.Append(i == table.Columns.Count - 1 ? Environment.NewLine : Separator);
                }
            }

            return builder.ToString();
        }
    }
}
