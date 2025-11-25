using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using myapp.Commands;
using myapp.Models;
using myapp.Services;
using myapp.Services.Interfaces;

namespace myapp.Tests.Commands
{
    public class RoomTypesCommandHandlerTests
    {
        private readonly Mock<IHotelRepository> _hotelRepoMock;
        private readonly Mock<IAllocationService> _allocationServiceMock;
        private readonly RoomTypesCommandHandler _handler;

        public RoomTypesCommandHandlerTests()
        {
            _hotelRepoMock = new Mock<IHotelRepository>();
            _allocationServiceMock = new Mock<IAllocationService>();
            _handler = new RoomTypesCommandHandler(_hotelRepoMock.Object, _allocationServiceMock.Object);
        }

        #region Helper Methods

        private Hotel CreateHotel(string id)
        {
            return new Hotel(id, "Test Hotel", Array.Empty<RoomType>(), Array.Empty<Room>());
        }

        private DateRange CreateDateRange(int startDay, int endDay)
        {
            var start = new DateTime(2024, 1, startDay);
            var endExclusive = new DateTime(2024, 1, endDay + 1);
            return new DateRange(start, endExclusive);
        }

        private AllocatedRoom CreateAllocatedRoom(string roomTypeCode, bool isPartial = false)
        {
            return new AllocatedRoom(roomTypeCode, isPartial);
        }

        private AllocationResult CreateSuccessfulAllocationResult(params AllocatedRoom[] rooms)
        {
            return new AllocationResult(true, null, new List<AllocatedRoom>(rooms));
        }

        private AllocationResult CreateFailedAllocationResult(string errorMessage)
        {
            return new AllocationResult(false, errorMessage, null);
        }

        private void MockAllocationSuccess(Hotel hotel, DateRange range, int numPeople, params AllocatedRoom[] rooms)
        {
            _allocationServiceMock.Setup(a => a.Allocate(hotel, range, numPeople))
                .Returns(CreateSuccessfulAllocationResult(rooms));
        }

        private void MockAllocationFailure(Hotel hotel, DateRange range, int numPeople, string errorMessage)
        {
            _allocationServiceMock.Setup(a => a.Allocate(hotel, range, numPeople))
                .Returns(CreateFailedAllocationResult(errorMessage));
        }

        #endregion

        #region Success Path Tests

        [Fact]
        public void Execute_WithValidAllocation_ReturnsSingleRoomType()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);
            var allocatedRoom = CreateAllocatedRoom("DOUBLE");

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 2, allocatedRoom);

            var result = _handler.Execute("H1", range, 2);

            Assert.Equal("H1: DOUBLE", result);
        }

        [Fact]
        public void Execute_WithValidAllocationMultipleRooms_ReturnsAllRoomTypes()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);
            var rooms = new[]
            {
                CreateAllocatedRoom("DOUBLE"),
                CreateAllocatedRoom("SINGLE"),
                CreateAllocatedRoom("DOUBLE")
            };

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 5, rooms);

            var result = _handler.Execute("H1", range, 5);

            Assert.Equal("H1: DOUBLE, SINGLE, DOUBLE", result);
        }

        [Fact]
        public void Execute_WithPartialRoom_MarkWithExclamation()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);
            var rooms = new[]
            {
                CreateAllocatedRoom("DOUBLE"),
                CreateAllocatedRoom("SINGLE", isPartial: true)
            };

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 3, rooms);

            var result = _handler.Execute("H1", range, 3);

            Assert.Equal("H1: DOUBLE, SINGLE!", result);
        }

        [Fact]
        public void Execute_WithMultiplePartialRooms_MarksAllPartials()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);
            var rooms = new[]
            {
                CreateAllocatedRoom("DOUBLE", isPartial: true),
                CreateAllocatedRoom("SINGLE", isPartial: true)
            };

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 2, rooms);

            var result = _handler.Execute("H1", range, 2);

            Assert.Equal("H1: DOUBLE!, SINGLE!", result);
        }

        [Fact]
        public void Execute_WithMixedPartialAndFullRooms_MarkOnlyPartials()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);
            var rooms = new[]
            {
                CreateAllocatedRoom("DOUBLE"),
                CreateAllocatedRoom("SINGLE", isPartial: true),
                CreateAllocatedRoom("DOUBLE"),
                CreateAllocatedRoom("SUITE", isPartial: true)
            };

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 7, rooms);

            var result = _handler.Execute("H1", range, 7);

            Assert.Equal("H1: DOUBLE, SINGLE!, DOUBLE, SUITE!", result);
        }

        [Fact]
        public void Execute_WithSingleRoom_ReturnsHotelIdAndRoomType()
        {
            var hotel = CreateHotel("HOTEL123");
            var range = CreateDateRange(1, 1);
            var rooms = new[] { CreateAllocatedRoom("DELUXE") };

            _hotelRepoMock.Setup(r => r.GetById("HOTEL123")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 2, rooms);

            var result = _handler.Execute("HOTEL123", range, 2);

            Assert.Equal("HOTEL123: DELUXE", result);
        }

        [Fact]
        public void Execute_WithLargeNumberOfRooms_ReturnsAllRoomTypes()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);
            var rooms = new[]
            {
                CreateAllocatedRoom("SINGLE"),
                CreateAllocatedRoom("SINGLE"),
                CreateAllocatedRoom("DOUBLE"),
                CreateAllocatedRoom("DOUBLE"),
                CreateAllocatedRoom("SUITE")
            };

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 9, rooms);

            var result = _handler.Execute("H1", range, 9);

            Assert.Equal("H1: SINGLE, SINGLE, DOUBLE, DOUBLE, SUITE", result);
        }

        [Fact]
        public void Execute_CallsRepositoryWithCorrectHotelId()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 2, CreateAllocatedRoom("DOUBLE"));

            _handler.Execute("H1", range, 2);

            _hotelRepoMock.Verify(r => r.GetById("H1"), Times.Once);
        }

        [Fact]
        public void Execute_CallsAllocationServiceWithCorrectParameters()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 5);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 4, CreateAllocatedRoom("DOUBLE"));

            _handler.Execute("H1", range, 4);

            _allocationServiceMock.Verify(a => a.Allocate(hotel, range, 4), Times.Once);
        }

        #endregion

        #region Error Cases - Unknown Hotel

        [Fact]
        public void Execute_WithUnknownHotel_ReturnsErrorMessage()
        {
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("INVALID")).Returns((Hotel)null!);

            var result = _handler.Execute("INVALID", range, 2);

            Assert.Equal("Error: unknown hotel 'INVALID'.", result);
        }

        [Fact]
        public void Execute_WithUnknownHotel_DoesNotCallAllocationService()
        {
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("INVALID")).Returns((Hotel)null!);

            _handler.Execute("INVALID", range, 2);

            _allocationServiceMock.Verify(a => a.Allocate(It.IsAny<Hotel>(), It.IsAny<DateRange>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Execute_WithUnknownHotelEmptyString_ReturnsErrorMessage()
        {
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("")).Returns((Hotel)null!);

            var result = _handler.Execute("", range, 2);

            Assert.Equal("Error: unknown hotel ''.", result);
        }

        [Fact]
        public void Execute_WithUnknownHotelSpecialCharacters_ReturnsErrorMessage()
        {
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H@#$")).Returns((Hotel)null!);

            var result = _handler.Execute("H@#$", range, 2);

            Assert.Equal("Error: unknown hotel 'H@#$'.", result);
        }

        [Fact]
        public void Execute_WithUnknownHotelLargeId_ReturnsErrorMessage()
        {
            var range = CreateDateRange(1, 1);
            var largeId = new string('H', 1000);

            _hotelRepoMock.Setup(r => r.GetById(largeId)).Returns((Hotel)null!);

            var result = _handler.Execute(largeId, range, 2);

            Assert.Equal($"Error: unknown hotel '{largeId}'.", result);
        }

        #endregion

        #region Error Cases - Allocation Failure

        [Fact]
        public void Execute_WithAllocationFailure_ReturnsErrorWithMessage()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationFailure(hotel, range, 100, "Not enough capacity");

            var result = _handler.Execute("H1", range, 100);

            Assert.Equal("Error: Not enough capacity", result);
        }

        [Fact]
        public void Execute_WithAllocationFailureInsufficientCapacity_ReturnsErrorMessage()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);
            const string errorMsg = "Not enough capacity available to allocate the requested number of people.";

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationFailure(hotel, range, 500, errorMsg);

            var result = _handler.Execute("H1", range, 500);

            Assert.Equal($"Error: {errorMsg}", result);
        }

        [Fact]
        public void Execute_WithAllocationFailureInvalidNumPeople_ReturnsErrorMessage()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationFailure(hotel, range, 0, "numPeople must be > 0");

            var result = _handler.Execute("H1", range, 0);

            Assert.Equal("Error: numPeople must be > 0", result);
        }

        [Fact]
        public void Execute_WithAllocationFailureUnknownHotel_ReturnsErrorMessage()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationFailure(hotel, range, 2, "Unknown hotel");

            var result = _handler.Execute("H1", range, 2);

            Assert.Equal("Error: Unknown hotel", result);
        }

        [Fact]
        public void Execute_WithAllocationFailure_StillCallsRepository()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationFailure(hotel, range, 2, "Some error");

            _handler.Execute("H1", range, 2);

            _hotelRepoMock.Verify(r => r.GetById("H1"), Times.Once);
        }

        [Fact]
        public void Execute_WithAllocationFailure_StillCallsAllocationService()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationFailure(hotel, range, 2, "Some error");

            _handler.Execute("H1", range, 2);

            _allocationServiceMock.Verify(a => a.Allocate(hotel, range, 2), Times.Once);
        }

        #endregion

        #region Output Format Tests

        [Fact]
        public void Execute_OutputFormatStartsWithHotelId()
        {
            var hotel = CreateHotel("MYHOTEL");
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("MYHOTEL")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 2, CreateAllocatedRoom("DOUBLE"));

            var result = _handler.Execute("MYHOTEL", range, 2);

            Assert.StartsWith("MYHOTEL:", result);
        }

        [Fact]
        public void Execute_OutputFormatUsesCommaSpaceSeparator()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);
            var rooms = new[]
            {
                CreateAllocatedRoom("DOUBLE"),
                CreateAllocatedRoom("SINGLE")
            };

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 3, rooms);

            var result = _handler.Execute("H1", range, 3);

            Assert.Contains(", ", result); // Verify separator is comma + space
            Assert.Equal("H1: DOUBLE, SINGLE", result);
        }

        [Fact]
        public void Execute_OutputPreservesHotelIdCasing()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 2, CreateAllocatedRoom("DOUBLE"));

            var result = _handler.Execute("H1", range, 2);

            Assert.StartsWith("H1:", result);
        }

        [Fact]
        public void Execute_PartialMarkerIsExclamation()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);
            var rooms = new[] { CreateAllocatedRoom("DOUBLE", isPartial: true) };

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 2, rooms);

            var result = _handler.Execute("H1", range, 2);

            Assert.Contains("!", result);
            Assert.Equal("H1: DOUBLE!", result);
        }

        [Fact]
        public void Execute_FullRoomHasNoExclamation()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);
            var rooms = new[] { CreateAllocatedRoom("DOUBLE", isPartial: false) };

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 2, rooms);

            var result = _handler.Execute("H1", range, 2);

            Assert.DoesNotContain("DOUBLE!", result);
            Assert.Equal("H1: DOUBLE", result);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Execute_WithSingleDayRange_ReturnsCorrectResult()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(5, 5);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 2, CreateAllocatedRoom("DOUBLE"));

            var result = _handler.Execute("H1", range, 2);

            Assert.Equal("H1: DOUBLE", result);
        }

        [Fact]
        public void Execute_WithLongDateRange_ReturnsCorrectResult()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 30);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 5, CreateAllocatedRoom("SUITE"));

            var result = _handler.Execute("H1", range, 5);

            Assert.Equal("H1: SUITE", result);
        }

        [Fact]
        public void Execute_WithOnePersonAllocation_ReturnsCorrectResult()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 1, CreateAllocatedRoom("SINGLE"));

            var result = _handler.Execute("H1", range, 1);

            Assert.Equal("H1: SINGLE", result);
        }

        [Fact]
        public void Execute_WithLargePersonCount_ReturnsCorrectResult()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);
            var rooms = Enumerable.Range(0, 50)
                .Select(i => CreateAllocatedRoom("DOUBLE"))
                .ToArray();

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 100, rooms);

            var result = _handler.Execute("H1", range, 100);

            Assert.Equal("H1: " + string.Join(", ", Enumerable.Repeat("DOUBLE", 50)), result);
        }

        [Fact]
        public void Execute_WithNumericRoomTypeCode_ReturnsCorrectly()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);
            var rooms = new[]
            {
                CreateAllocatedRoom("101"),
                CreateAllocatedRoom("102")
            };

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 4, rooms);

            var result = _handler.Execute("H1", range, 4);

            Assert.Equal("H1: 101, 102", result);
        }

        [Fact]
        public void Execute_WithSpecialCharactersInRoomTypeCode_ReturnsCorrectly()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);
            var rooms = new[]
            {
                CreateAllocatedRoom("ROOM-101"),
                CreateAllocatedRoom("ROOM_202")
            };

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 4, rooms);

            var result = _handler.Execute("H1", range, 4);

            Assert.Equal("H1: ROOM-101, ROOM_202", result);
        }

        [Fact]
        public void Execute_WithAllRoomsPartial_MarksAllAsPartial()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);
            var rooms = new[]
            {
                CreateAllocatedRoom("SINGLE", isPartial: true),
                CreateAllocatedRoom("SINGLE", isPartial: true),
                CreateAllocatedRoom("SINGLE", isPartial: true)
            };

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 3, rooms);

            var result = _handler.Execute("H1", range, 3);

            Assert.Equal("H1: SINGLE!, SINGLE!, SINGLE!", result);
        }

        [Fact]
        public void Execute_WithEmptyAllocationList_ReturnsHotelIdWithoutRooms()
        {
            var hotel = CreateHotel("H1");
            var range = CreateDateRange(1, 1);

            _hotelRepoMock.Setup(r => r.GetById("H1")).Returns(hotel);
            MockAllocationSuccess(hotel, range, 0); // Empty rooms list

            var result = _handler.Execute("H1", range, 0);

            Assert.Equal("H1: ", result);
        }

        #endregion
    }
}
