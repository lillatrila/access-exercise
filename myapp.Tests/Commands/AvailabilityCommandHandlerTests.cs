using System;
using Xunit;
using Moq;
using myapp.Commands;
using myapp.Models;
using myapp.Services.Interfaces;

namespace myapp.Tests.Commands
{
    public class AvailabilityCommandHandlerTests
    {
        private readonly Mock<IHotelRepository> _hotelRepoMock;
        private readonly Mock<IAvailabilityService> _availabilityServiceMock;
        private readonly AvailabilityCommandHandler _handler;

        public AvailabilityCommandHandlerTests()
        {
            _hotelRepoMock = new Mock<IHotelRepository>();
            _availabilityServiceMock = new Mock<IAvailabilityService>();
            _handler = new AvailabilityCommandHandler(_hotelRepoMock.Object, _availabilityServiceMock.Object);
        }

        #region Helper Methods

        private Hotel CreateHotel(string id, params RoomType[] roomTypes)
        {
            var rooms = new Room[0];
            return new Hotel(id, "Test Hotel", roomTypes, rooms);
        }

        private RoomType CreateRoomType(string code, int size = 2)
        {
            return new RoomType(code, size, $"Room type {code}", new[] { "WiFi" }, new[] { "AC" });
        }

        private DateRange CreateDateRange(int startDay, int endDay)
        {
            var start = new DateTime(2024, 1, startDay);
            var endExclusive = new DateTime(2024, 1, endDay + 1);
            return new DateRange(start, endExclusive);
        }

        #endregion

        #region Success Path Tests

        [Fact]
        public void Execute_WithValidHotelAndRoomType_ReturnsAvailabilityAsString()
        {
            var roomType = CreateRoomType("DOUBLE");
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            _availabilityServiceMock.Setup(a => a.GetAvailability(hotel, range, "DOUBLE")).Returns(5);

            var result = _handler.Execute("H1", range, "DOUBLE");

            Assert.Equal("5", result);
        }

        [Fact]
        public void Execute_WithZeroAvailability_ReturnsZero()
        {
            var roomType = CreateRoomType("SINGLE");
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            _availabilityServiceMock.Setup(a => a.GetAvailability(hotel, range, "SINGLE")).Returns(0);

            var result = _handler.Execute("H1", range, "SINGLE");

            Assert.Equal("0", result);
        }

        [Fact]
        public void Execute_WithHighAvailability_ReturnsLargeNumber()
        {
            var roomType = CreateRoomType("DELUXE");
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            _availabilityServiceMock.Setup(a => a.GetAvailability(hotel, range, "DELUXE")).Returns(100);

            var result = _handler.Execute("H1", range, "DELUXE");

            Assert.Equal("100", result);
        }

        [Fact]
        public void Execute_WithMultipleDaysRange_ReturnsCorrectAvailability()
        {
            var roomType = CreateRoomType("DOUBLE");
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 5);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            _availabilityServiceMock.Setup(a => a.GetAvailability(hotel, range, "DOUBLE")).Returns(15);

            var result = _handler.Execute("H1", range, "DOUBLE");

            Assert.Equal("15", result);
        }

        [Fact]
        public void Execute_WithCaseInsensitiveHotelId_ReturnsAvailability()
        {
            var roomType = CreateRoomType("DOUBLE");
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("h1")).Returns(hotel);
            _availabilityServiceMock.Setup(a => a.GetAvailability(hotel, range, "DOUBLE")).Returns(5);

            var result = _handler.Execute("h1", range, "DOUBLE");

            Assert.Equal("5", result);
        }

        [Fact]
        public void Execute_WithCaseInsensitiveRoomType_ReturnsAvailability()
        {
            var roomType = CreateRoomType("DOUBLE");
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            _availabilityServiceMock.Setup(a => a.GetAvailability(hotel, range, "DOUBLE")).Returns(5);

            var result = _handler.Execute("H1", range, "double");

            Assert.Equal("5", result);
        }

        [Fact]
        public void Execute_WithMultipleRoomTypes_ReturnsCorrectOneForDOUBLE()
        {
            var doubleRoom = CreateRoomType("DOUBLE");
            var singleRoom = CreateRoomType("SINGLE");
            var hotel = CreateHotel("H1", doubleRoom, singleRoom);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            _availabilityServiceMock.Setup(a => a.GetAvailability(hotel, range, "DOUBLE")).Returns(5);

            var result = _handler.Execute("H1", range, "DOUBLE");

            Assert.Equal("5", result);
        }

        [Fact]
        public void Execute_WithMultipleRoomTypes_ReturnsCorrectOneForSINGLE()
        {
            var doubleRoom = CreateRoomType("DOUBLE");
            var singleRoom = CreateRoomType("SINGLE");
            var hotel = CreateHotel("H1", doubleRoom, singleRoom);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            _availabilityServiceMock.Setup(a => a.GetAvailability(hotel, range, "SINGLE")).Returns(3);

            var result = _handler.Execute("H1", range, "SINGLE");

            Assert.Equal("3", result);
        }

        [Fact]
        public void Execute_CallsRepositoryWithCorrectHotelId()
        {
            var roomType = CreateRoomType("DOUBLE");
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            _availabilityServiceMock.Setup(a => a.GetAvailability(hotel, range, "DOUBLE")).Returns(5);

            _handler.Execute("H1", range, "DOUBLE");

            _hotelRepoMock.Verify(r => r.GetById("H1"), Times.Once);
        }

        [Fact]
        public void Execute_CallsAvailabilityServiceWithCorrectParameters()
        {
            var roomType = CreateRoomType("DOUBLE");
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            _availabilityServiceMock.Setup(a => a.GetAvailability(hotel, range, "DOUBLE")).Returns(5);

            _handler.Execute("H1", range, "DOUBLE");

            _availabilityServiceMock.Verify(a => a.GetAvailability(hotel, range, "DOUBLE"), Times.Once);
        }

        #endregion

        #region Error Cases - Unknown Hotel

        [Fact]
        public void Execute_WithUnknownHotel_ReturnsErrorMessage()
        {
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("INVALID")).Returns((Hotel)null!);

            var result = _handler.Execute("INVALID", range, "DOUBLE");

            Assert.Equal("Error: unknown hotel 'INVALID'.", result);
        }

        [Fact]
        public void Execute_WithUnknownHotel_DoesNotCallAvailabilityService()
        {
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("INVALID")).Returns((Hotel)null!);

            _handler.Execute("INVALID", range, "DOUBLE");

            _availabilityServiceMock.Verify(a => a.GetAvailability(It.IsAny<Hotel>(), It.IsAny<DateRange>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Execute_WithUnknownHotel_DoesNotCallRepositoryForGetRoomType()
        {
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("INVALID")).Returns((Hotel)null!);

            _handler.Execute("INVALID", range, "DOUBLE");

            // Verify that only GetById was called
            _hotelRepoMock.Verify(r => r.GetById("INVALID"), Times.Once);
        }

        [Fact]
        public void Execute_WithUnknownHotelEmptyString_ReturnsErrorMessage()
        {
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("")).Returns((Hotel)null!);

            var result = _handler.Execute("", range, "DOUBLE");

            Assert.Equal("Error: unknown hotel ''.", result);
        }

        [Fact]
        public void Execute_WithUnknownHotelSpecialCharacters_ReturnsErrorMessage()
        {
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H@#$")).Returns((Hotel)null!);

            var result = _handler.Execute("H@#$", range, "DOUBLE");

            Assert.Equal("Error: unknown hotel 'H@#$'.", result);
        }

        #endregion

        #region Error Cases - Unknown Room Type

        [Fact]
        public void Execute_WithUnknownRoomType_ReturnsErrorMessage()
        {
            var roomType = CreateRoomType("DOUBLE");
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);

            var result = _handler.Execute("H1", range, "INVALID");

            Assert.Equal("Error: unknown room type 'INVALID' for hotel 'H1'.", result);
        }

        [Fact]
        public void Execute_WithUnknownRoomType_DoesNotCallAvailabilityService()
        {
            var roomType = CreateRoomType("DOUBLE");
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);

            _handler.Execute("H1", range, "INVALID");

            _availabilityServiceMock.Verify(a => a.GetAvailability(It.IsAny<Hotel>(), It.IsAny<DateRange>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Execute_WithUnknownRoomType_StillCallsGetById()
        {
            var roomType = CreateRoomType("DOUBLE");
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);

            _handler.Execute("H1", range, "INVALID");

            _hotelRepoMock.Verify(r => r.GetById("H1"), Times.Once);
        }

        [Fact]
        public void Execute_WithUnknownRoomTypeEmptyString_ReturnsErrorMessage()
        {
            var roomType = CreateRoomType("DOUBLE");
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);

            var result = _handler.Execute("H1", range, "");

            Assert.Equal("Error: unknown room type '' for hotel 'H1'.", result);
        }

        [Fact]
        public void Execute_WithUnknownRoomTypeSpecialCharacters_ReturnsErrorMessage()
        {
            var roomType = CreateRoomType("DOUBLE");
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);

            var result = _handler.Execute("H1", range, "ROOM@TYPE");

            Assert.Equal("Error: unknown room type 'ROOM@TYPE' for hotel 'H1'.", result);
        }

        [Fact]
        public void Execute_WithUnknownRoomTypeNoRoomTypesInHotel_ReturnsErrorMessage()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);

            var result = _handler.Execute("H1", range, "DOUBLE");

            Assert.Equal("Error: unknown room type 'DOUBLE' for hotel 'H1'.", result);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Execute_WithNegativeAvailability_ReturnsNegativeNumber()
        {
            var roomType = CreateRoomType("DOUBLE");
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            _availabilityServiceMock.Setup(a => a.GetAvailability(hotel, range, "DOUBLE")).Returns(-5);

            var result = _handler.Execute("H1", range, "DOUBLE");

            Assert.Equal("-5", result);
        }

        [Fact]
        public void Execute_WithMaxIntAvailability_ReturnsMaxInt()
        {
            var roomType = CreateRoomType("DOUBLE");
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            _availabilityServiceMock.Setup(a => a.GetAvailability(hotel, range, "DOUBLE")).Returns(int.MaxValue);

            var result = _handler.Execute("H1", range, "DOUBLE");

            Assert.Equal(int.MaxValue.ToString(), result);
        }

        [Fact]
        public void Execute_WithMinIntAvailability_ReturnsMinInt()
        {
            var roomType = CreateRoomType("DOUBLE");
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            _availabilityServiceMock.Setup(a => a.GetAvailability(hotel, range, "DOUBLE")).Returns(int.MinValue);

            var result = _handler.Execute("H1", range, "DOUBLE");

            Assert.Equal(int.MinValue.ToString(), result);
        }

        [Fact]
        public void Execute_WithHotelIdDifferentCase_ReturnsCorrectAvailability()
        {
            var roomType = CreateRoomType("DOUBLE");
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            _availabilityServiceMock.Setup(a => a.GetAvailability(hotel, range, "DOUBLE")).Returns(5);

            var result = _handler.Execute("H1", range, "DOUBLE");

            Assert.Equal("5", result);
        }

        [Fact]
        public void Execute_WithRoomTypeCodePreserved_PassesOriginalCodeToService()
        {
            var roomType = CreateRoomType("DOUBLE", 2);
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            _availabilityServiceMock.Setup(a => a.GetAvailability(hotel, range, "DOUBLE")).Returns(5);

            _handler.Execute("H1", range, "DOUBLE");

            // Verify that the code from the RoomType is used
            _availabilityServiceMock.Verify(a => a.GetAvailability(hotel, range, "DOUBLE"), Times.Once);
        }

        [Fact]
        public void Execute_WithSingleDayRange_ReturnsCorrectAvailability()
        {
            var roomType = CreateRoomType("DOUBLE");
            var hotel = CreateHotel("H1", roomType);
            var start = new DateTime(2024, 1, 15);
            var range = new DateRange(start, start.AddDays(1));

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            _availabilityServiceMock.Setup(a => a.GetAvailability(hotel, range, "DOUBLE")).Returns(7);

            var result = _handler.Execute("H1", range, "DOUBLE");

            Assert.Equal("7", result);
        }

        [Fact]
        public void Execute_WithLongDateRange_ReturnsCorrectAvailability()
        {
            var roomType = CreateRoomType("DOUBLE");
            var hotel = CreateHotel("H1", roomType);
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(2024, 1, 31);
            var range = new DateRange(start, end);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            _availabilityServiceMock.Setup(a => a.GetAvailability(hotel, range, "DOUBLE")).Returns(60);

            var result = _handler.Execute("H1", range, "DOUBLE");

            Assert.Equal("60", result);
        }

        [Fact]
        public void Execute_WithMixedCaseRoomTypeAndHotel_ReturnsCorrectAvailability()
        {
            var roomType = CreateRoomType("DOUBLE");
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            _availabilityServiceMock.Setup(a => a.GetAvailability(hotel, range, "DOUBLE")).Returns(5);

            var result = _handler.Execute("H1", range, "DoUbLe");

            Assert.Equal("5", result);
        }

        [Fact]
        public void Execute_WithNumericRoomTypeCode_ReturnsCorrectAvailability()
        {
            var roomType = CreateRoomType("101");
            var hotel = CreateHotel("H1", roomType);
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            _availabilityServiceMock.Setup(a => a.GetAvailability(hotel, range, "101")).Returns(3);

            var result = _handler.Execute("H1", range, "101");

            Assert.Equal("3", result);
        }

        #endregion
    }
}
