using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace Core
{
    public static class Extensions
    {
        private static dynamic ToExpando(DataTable table, DataRow row)
        {
            var expandoObject = new ExpandoObject() as IDictionary<string, object>;

            foreach (DataColumn column in table.Columns)
                expandoObject.Add(column.ColumnName.ToTitleCase(), row[column]);

            return expandoObject;
        }

        public static IEnumerable<dynamic> ReadAsDynamicEnumerable(this DataTable table)
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {
                yield return ToExpando(table, table.Rows[i]);
            }
        }

        public static string ToTitleCase(this string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return string.Empty;
            }

            string text = string.Empty;
            var values = Regex.Replace(word, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ").Split(' ');

            foreach (string t in values)
            {
                text += System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t.ToLower());
            }

            return text;
        }

        public static bool IsNullOrEmpty(this object value)
        {
            //TODO: refactor
            return value == null || value.ToString() == string.Empty;
        }

        public static DateTime LastWorkDay(this DateTime date)
        {
            do
            {
                date = date.AddDays(-1);
            }
            while (IsWeekend(date));

            return date;
        }

        private static bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday ||
                   date.DayOfWeek == DayOfWeek.Friday;
        }

        public static string DateTimeFormat(this DateTime date)
        {
            if (date == null)
            {
                return string.Empty;
            }
            return date.ToString("yyyy-MM-dd");
        }
    }
}
