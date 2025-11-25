using System.Text.RegularExpressions;
using myapp.Models;
using myapp.Utils;

namespace myapp.Commands
{

public static class CommandParser
    {
        private static readonly Regex AvailabilityRegex = new(@"^\s*Availability\s*\(\s*([^\s,()]+)\s*,\s*([^\s,()]+)\s*,\s*([^\s,()]+)\s*\)\s*$", RegexOptions.IgnoreCase);
        
        private static readonly Regex RoomTypesRegex = new(@"^\s*RoomTypes\s*\(\s*([^\s,()]+)\s*,\s*([^\s,()]+)\s*,\s*([0-9]+)\s*\)\s*$", RegexOptions.IgnoreCase);

        public static bool TryParseAvailability(string input, out (string hotelId, DateRange range, string roomType) result)
        {
            result = default;
            var m = AvailabilityRegex.Match(input);
            if (!m.Success) return false;
            try
            {
                var hotelId = m.Groups[1].Value;
                var dateToken = m.Groups[2].Value;
                var roomType = m.Groups[3].Value;
                var range = DateParser.ParseSingleOrRange(dateToken);
                result = (hotelId, range, roomType);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryParseRoomTypes(string input, out (string hotelId, DateRange range, int numPeople) result)
        {
            result = default;
            var m = RoomTypesRegex.Match(input);
            if (!m.Success) return false;
            try
            {
                var hotelId = m.Groups[1].Value;
                var dateToken = m.Groups[2].Value;
                var numPeople = int.Parse(m.Groups[3].Value);
                var range = DateParser.ParseSingleOrRange(dateToken);
                result = (hotelId, range, numPeople);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}