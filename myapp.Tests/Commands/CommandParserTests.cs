using System;
using Xunit;
using myapp.Commands;
using myapp.Models;

namespace myapp.Tests.Commands
{
    public class CommandParserTests
    {
        #region TryParseAvailability Tests

        [Fact]
        public void TryParseAvailability_WithValidSingleDate_ReturnsTrue()
        {
            const string input = "Availability(H1, 20240101, DOUBLE)";

            var success = CommandParser.TryParseAvailability(input, out var result);

            Assert.True(success);
            Assert.Equal("H1", result.hotelId);
            Assert.Equal("DOUBLE", result.roomType);
            Assert.Equal(new DateTime(2024, 1, 1), result.range.Start);
            Assert.Equal(new DateTime(2024, 1, 2), result.range.EndExclusive);
        }

        [Fact]
        public void TryParseAvailability_WithValidDateRange_ReturnsTrue()
        {
            const string input = "Availability(H1, 20240101-20240105, DOUBLE)";

            var success = CommandParser.TryParseAvailability(input, out var result);

            Assert.True(success);
            Assert.Equal("H1", result.hotelId);
            Assert.Equal("DOUBLE", result.roomType);
            Assert.Equal(new DateTime(2024, 1, 1), result.range.Start);
            Assert.Equal(new DateTime(2024, 1, 6), result.range.EndExclusive);
        }

        [Fact]
        public void TryParseAvailability_WithSingleRoomType_ReturnsTrue()
        {
            const string input = "Availability(HOTEL123, 20240115, SINGLE)";

            var success = CommandParser.TryParseAvailability(input, out var result);

            Assert.True(success);
            Assert.Equal("HOTEL123", result.hotelId);
            Assert.Equal("SINGLE", result.roomType);
        }

        [Fact]
        public void TryParseAvailability_WithDifferentHotelId_ReturnsTrue()
        {
            const string input = "Availability(MyHotel, 20240201, SUITE)";

            var success = CommandParser.TryParseAvailability(input, out var result);

            Assert.True(success);
            Assert.Equal("MyHotel", result.hotelId);
            Assert.Equal("SUITE", result.roomType);
        }

        [Fact]
        public void TryParseAvailability_WithExtraWhitespace_ReturnsTrue()
        {
            const string input = "  Availability  (  H1  ,  20240101  ,  DOUBLE  )  ";

            var success = CommandParser.TryParseAvailability(input, out var result);

            Assert.True(success);
            Assert.Equal("H1", result.hotelId);
            Assert.Equal("DOUBLE", result.roomType);
        }

        [Fact]
        public void TryParseAvailability_WithLowercaseCommand_ReturnsTrue()
        {
            const string input = "availability(H1, 20240101, DOUBLE)";

            var success = CommandParser.TryParseAvailability(input, out var result);

            Assert.True(success);
            Assert.Equal("H1", result.hotelId);
        }

        [Fact]
        public void TryParseAvailability_WithMixedCaseCommand_ReturnsTrue()
        {
            const string input = "AVAILABILITY(H1, 20240101, DOUBLE)";

            var success = CommandParser.TryParseAvailability(input, out var result);

            Assert.True(success);
            Assert.Equal("H1", result.hotelId);
        }

        [Fact]
        public void TryParseAvailability_WithMissingCommand_ReturnsFalse()
        {
            const string input = "(H1, 20240101, DOUBLE)";

            var success = CommandParser.TryParseAvailability(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseAvailability_WithWrongCommand_ReturnsFalse()
        {
            const string input = "RoomTypes(H1, 20240101, DOUBLE)";

            var success = CommandParser.TryParseAvailability(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseAvailability_WithMissingHotelId_ReturnsFalse()
        {
            const string input = "Availability(, 20240101, DOUBLE)";

            var success = CommandParser.TryParseAvailability(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseAvailability_WithMissingDate_ReturnsFalse()
        {
            const string input = "Availability(H1, , DOUBLE)";

            var success = CommandParser.TryParseAvailability(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseAvailability_WithMissingRoomType_ReturnsFalse()
        {
            const string input = "Availability(H1, 20240101, )";

            var success = CommandParser.TryParseAvailability(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseAvailability_WithInvalidDateFormat_ReturnsFalse()
        {
            const string input = "Availability(H1, 01-01-2024, DOUBLE)";

            var success = CommandParser.TryParseAvailability(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseAvailability_WithInvalidDate_ReturnsFalse()
        {
            const string input = "Availability(H1, 20241301, DOUBLE)";

            var success = CommandParser.TryParseAvailability(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseAvailability_WithBackwardDateRange_ReturnsFalse()
        {
            const string input = "Availability(H1, 20240105-20240101, DOUBLE)";

            var success = CommandParser.TryParseAvailability(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseAvailability_WithSameDateRange_ReturnsTrue()
        {
            const string input = "Availability(H1, 20240101-20240101, DOUBLE)";

            var success = CommandParser.TryParseAvailability(input, out var result);

            Assert.True(success);
            Assert.Equal(new DateTime(2024, 1, 1), result.range.Start);
            Assert.Equal(new DateTime(2024, 1, 2), result.range.EndExclusive);
        }

        [Fact]
        public void TryParseAvailability_WithExtraParenthesis_ReturnsFalse()
        {
            const string input = "Availability((H1, 20240101, DOUBLE))";

            var success = CommandParser.TryParseAvailability(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseAvailability_WithMissingParenthesis_ReturnsFalse()
        {
            const string input = "Availability(H1, 20240101, DOUBLE";

            var success = CommandParser.TryParseAvailability(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseAvailability_WithTooManyParameters_ReturnsFalse()
        {
            const string input = "Availability(H1, 20240101, DOUBLE, EXTRA)";

            var success = CommandParser.TryParseAvailability(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseAvailability_WithTooFewParameters_ReturnsFalse()
        {
            const string input = "Availability(H1, 20240101)";

            var success = CommandParser.TryParseAvailability(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseAvailability_WithEmptyString_ReturnsFalse()
        {
            const string input = "";

            var success = CommandParser.TryParseAvailability(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseAvailability_WithWhitespaceOnly_ReturnsFalse()
        {
            const string input = "   ";

            var success = CommandParser.TryParseAvailability(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseAvailability_WithSpecialCharactersInHotelId_ReturnsTrue()
        {
            const string input = "Availability(H-1, 20240101, DOUBLE)";

            var success = CommandParser.TryParseAvailability(input, out var result);

            Assert.True(success);
            Assert.Equal("H-1", result.hotelId);
        }

        [Fact]
        public void TryParseAvailability_WithUnderscoreInHotelId_ReturnsTrue()
        {
            const string input = "Availability(H_1, 20240101, DOUBLE)";

            var success = CommandParser.TryParseAvailability(input, out var result);

            Assert.True(success);
            Assert.Equal("H_1", result.hotelId);
        }

        [Fact]
        public void TryParseAvailability_WithNumbersInRoomType_ReturnsTrue()
        {
            const string input = "Availability(H1, 20240101, DOUBLE2)";

            var success = CommandParser.TryParseAvailability(input, out var result);

            Assert.True(success);
            Assert.Equal("DOUBLE2", result.roomType);
        }

        #endregion

        #region TryParseRoomTypes Tests

        [Fact]
        public void TryParseRoomTypes_WithValidSingleDate_ReturnsTrue()
        {
            const string input = "RoomTypes(H1, 20240101, 4)";

            var success = CommandParser.TryParseRoomTypes(input, out var result);

            Assert.True(success);
            Assert.Equal("H1", result.hotelId);
            Assert.Equal(4, result.numPeople);
            Assert.Equal(new DateTime(2024, 1, 1), result.range.Start);
            Assert.Equal(new DateTime(2024, 1, 2), result.range.EndExclusive);
        }

        [Fact]
        public void TryParseRoomTypes_WithValidDateRange_ReturnsTrue()
        {
            const string input = "RoomTypes(H1, 20240101-20240105, 4)";

            var success = CommandParser.TryParseRoomTypes(input, out var result);

            Assert.True(success);
            Assert.Equal("H1", result.hotelId);
            Assert.Equal(4, result.numPeople);
            Assert.Equal(new DateTime(2024, 1, 1), result.range.Start);
            Assert.Equal(new DateTime(2024, 1, 6), result.range.EndExclusive);
        }

        [Fact]
        public void TryParseRoomTypes_WithSinglePerson_ReturnsTrue()
        {
            const string input = "RoomTypes(H1, 20240101, 1)";

            var success = CommandParser.TryParseRoomTypes(input, out var result);

            Assert.True(success);
            Assert.Equal(1, result.numPeople);
        }

        [Fact]
        public void TryParseRoomTypes_WithLargePeopleCount_ReturnsTrue()
        {
            const string input = "RoomTypes(H1, 20240101, 100)";

            var success = CommandParser.TryParseRoomTypes(input, out var result);

            Assert.True(success);
            Assert.Equal(100, result.numPeople);
        }

        [Fact]
        public void TryParseRoomTypes_WithZeroPeople_ReturnsTrue()
        {
            const string input = "RoomTypes(H1, 20240101, 0)";

            var success = CommandParser.TryParseRoomTypes(input, out var result);

            Assert.True(success);
            Assert.Equal(0, result.numPeople);
        }

        [Fact]
        public void TryParseRoomTypes_WithExtraWhitespace_ReturnsTrue()
        {
            const string input = "  RoomTypes  (  H1  ,  20240101  ,  4  )  ";

            var success = CommandParser.TryParseRoomTypes(input, out var result);

            Assert.True(success);
            Assert.Equal("H1", result.hotelId);
            Assert.Equal(4, result.numPeople);
        }

        [Fact]
        public void TryParseRoomTypes_WithLowercaseCommand_ReturnsTrue()
        {
            const string input = "roomtypes(H1, 20240101, 4)";

            var success = CommandParser.TryParseRoomTypes(input, out var result);

            Assert.True(success);
            Assert.Equal("H1", result.hotelId);
        }

        [Fact]
        public void TryParseRoomTypes_WithMixedCaseCommand_ReturnsTrue()
        {
            const string input = "ROOMTYPES(H1, 20240101, 4)";

            var success = CommandParser.TryParseRoomTypes(input, out var result);

            Assert.True(success);
            Assert.Equal("H1", result.hotelId);
        }

        [Fact]
        public void TryParseRoomTypes_WithWrongCommand_ReturnsFalse()
        {
            const string input = "Availability(H1, 20240101, 4)";

            var success = CommandParser.TryParseRoomTypes(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseRoomTypes_WithMissingHotelId_ReturnsFalse()
        {
            const string input = "RoomTypes(, 20240101, 4)";

            var success = CommandParser.TryParseRoomTypes(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseRoomTypes_WithMissingDate_ReturnsFalse()
        {
            const string input = "RoomTypes(H1, , 4)";

            var success = CommandParser.TryParseRoomTypes(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseRoomTypes_WithMissingNumPeople_ReturnsFalse()
        {
            const string input = "RoomTypes(H1, 20240101, )";

            var success = CommandParser.TryParseRoomTypes(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseRoomTypes_WithInvalidDateFormat_ReturnsFalse()
        {
            const string input = "RoomTypes(H1, 01-01-2024, 4)";

            var success = CommandParser.TryParseRoomTypes(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseRoomTypes_WithInvalidDate_ReturnsFalse()
        {
            const string input = "RoomTypes(H1, 20241301, 4)";

            var success = CommandParser.TryParseRoomTypes(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseRoomTypes_WithBackwardDateRange_ReturnsFalse()
        {
            const string input = "RoomTypes(H1, 20240105-20240101, 4)";

            var success = CommandParser.TryParseRoomTypes(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseRoomTypes_WithSameDateRange_ReturnsTrue()
        {
            const string input = "RoomTypes(H1, 20240101-20240101, 4)";

            var success = CommandParser.TryParseRoomTypes(input, out var result);

            Assert.True(success);
            Assert.Equal(new DateTime(2024, 1, 1), result.range.Start);
            Assert.Equal(new DateTime(2024, 1, 2), result.range.EndExclusive);
        }

        [Fact]
        public void TryParseRoomTypes_WithNegativePeople_ReturnsFalse()
        {
            const string input = "RoomTypes(H1, 20240101, -5)";

            var success = CommandParser.TryParseRoomTypes(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseRoomTypes_WithNonNumericPeople_ReturnsFalse()
        {
            const string input = "RoomTypes(H1, 20240101, ABC)";

            var success = CommandParser.TryParseRoomTypes(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseRoomTypes_WithDecimalPeople_ReturnsFalse()
        {
            const string input = "RoomTypes(H1, 20240101, 4.5)";

            var success = CommandParser.TryParseRoomTypes(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseRoomTypes_WithExtraParenthesis_ReturnsFalse()
        {
            const string input = "RoomTypes((H1, 20240101, 4))";

            var success = CommandParser.TryParseRoomTypes(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseRoomTypes_WithMissingParenthesis_ReturnsFalse()
        {
            const string input = "RoomTypes(H1, 20240101, 4";

            var success = CommandParser.TryParseRoomTypes(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseRoomTypes_WithTooManyParameters_ReturnsFalse()
        {
            const string input = "RoomTypes(H1, 20240101, 4, EXTRA)";

            var success = CommandParser.TryParseRoomTypes(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseRoomTypes_WithTooFewParameters_ReturnsFalse()
        {
            const string input = "RoomTypes(H1, 20240101)";

            var success = CommandParser.TryParseRoomTypes(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseRoomTypes_WithEmptyString_ReturnsFalse()
        {
            const string input = "";

            var success = CommandParser.TryParseRoomTypes(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseRoomTypes_WithWhitespaceOnly_ReturnsFalse()
        {
            const string input = "   ";

            var success = CommandParser.TryParseRoomTypes(input, out _);

            Assert.False(success);
        }

        [Fact]
        public void TryParseRoomTypes_WithSpecialCharactersInHotelId_ReturnsTrue()
        {
            const string input = "RoomTypes(H-1, 20240101, 4)";

            var success = CommandParser.TryParseRoomTypes(input, out var result);

            Assert.True(success);
            Assert.Equal("H-1", result.hotelId);
        }

        [Fact]
        public void TryParseRoomTypes_WithUnderscoreInHotelId_ReturnsTrue()
        {
            const string input = "RoomTypes(H_1, 20240101, 4)";

            var success = CommandParser.TryParseRoomTypes(input, out var result);

            Assert.True(success);
            Assert.Equal("H_1", result.hotelId);
        }

        [Fact]
        public void TryParseRoomTypes_WithLeadingZeroInPeople_ReturnsTrue()
        {
            const string input = "RoomTypes(H1, 20240101, 04)";

            var success = CommandParser.TryParseRoomTypes(input, out var result);

            Assert.True(success);
            Assert.Equal(4, result.numPeople);
        }

        [Fact]
        public void TryParseRoomTypes_WithMaxIntPeople_ReturnsTrue()
        {
            string input = $"RoomTypes(H1, 20240101, {int.MaxValue})";

            var success = CommandParser.TryParseRoomTypes(input, out var result);

            Assert.True(success);
            Assert.Equal(int.MaxValue, result.numPeople);
        }

        #endregion
    }
}
