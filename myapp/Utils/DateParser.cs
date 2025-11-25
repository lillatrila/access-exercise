

using myapp.Models;

namespace myapp.Utils
{
public static class DateParser
    {
        // Parse format yyyyMMdd strictly
        public static DateTime ParseStrictDate(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new FormatException("Empty date string.");

            if (s.Length != 8 || !s.All(char.IsDigit))
                throw new FormatException($"Invalid date format '{s}'. Expected yyyyMMdd.");

            int year = int.Parse(s.Substring(0, 4));
            int month = int.Parse(s.Substring(4, 2));
            int day = int.Parse(s.Substring(6, 2));
            return new DateTime(year, month, day);
        }

        public static DateRange ParseSingleOrRange(string token)
        {
            token = token.Trim();
            if (token.Contains('-'))
            {
                var parts = token.Split('-', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                if (parts.Length != 2)
                    throw new FormatException("Range must be two dates joined by '-'.");

                var start = ParseStrictDate(parts[0]);
                var endInclusive = ParseStrictDate(parts[1]);
                // Interpret user input as inclusive range of nights: arrival = start, departure = endInclusive + 1 day
                if (endInclusive < start)
                    throw new FormatException("Range end is before start.");

                return new DateRange(start, endInclusive.AddDays(1));
            }
            else
            {
                var single = ParseStrictDate(token);
                return new DateRange(single, single.AddDays(1));
            }
        }
    }
}