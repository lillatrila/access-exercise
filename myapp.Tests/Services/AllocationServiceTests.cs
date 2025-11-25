using Xunit;
using Moq;
using myapp.Models;
using myapp.Services;
using myapp.Services.Interfaces;

namespace myapp.Tests.Services
{
    public class AllocationServiceTests
    {
        private readonly Mock<IAvailabilityService> _mockAvailService;
        private readonly AllocationService _service;

        public AllocationServiceTests()
        {
            _mockAvailService = new Mock<IAvailabilityService>();
            _service = new AllocationService(_mockAvailService.Object);
        }

        [Fact]
        public void Allocate_WithNullHotel_ReturnsFalse()
        {
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));
            var result = _service.Allocate(null!, range, 5);

            Assert.False(result.Success);
            Assert.Equal("Unknown hotel", result.ErrorMessage);
            Assert.Null(result.Rooms);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Allocate_WithNonPositivePeople_ReturnsFalse(int numPeople)
        {
            var hotel = CreateHotel("H1", "Test Hotel");
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));

            var result = _service.Allocate(hotel, range, numPeople);

            Assert.False(result.Success);
            Assert.Equal("numPeople must be > 0", result.ErrorMessage);
            Assert.Null(result.Rooms);
        }

        [Fact]
        public void Allocate_WithInsufficientCapacity_ReturnsFalse()
        {
            var hotel = CreateHotelWithRooms("H1", "Test Hotel", 2, "DOUBLE", 2); // 2 rooms of size 2 = capacity 4
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));

            MockAvailability(hotel, range, "DOUBLE", 2); // 2 rooms available

            var result = _service.Allocate(hotel, range, 10); // Need 10 people but max capacity is 4

            Assert.False(result.Success);
            Assert.Contains("Not enough capacity", result.ErrorMessage);
            Assert.Null(result.Rooms);
        }

        [Fact]
        public void Allocate_WithExactCapacity_SucceedsWithoutPartial()
        {
            var hotel = CreateHotelWithRooms("H1", "Test Hotel", 2, "DOUBLE", 2); // 2 rooms of size 2 = capacity 4
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));

            MockAvailability(hotel, range, "DOUBLE", 2); // 2 rooms available

            var result = _service.Allocate(hotel, range, 4); // Exactly 4 people

            Assert.True(result.Success);
            Assert.NotNull(result.Rooms);
            Assert.Equal(2, result.Rooms.Count);
            Assert.All(result.Rooms, room => Assert.False(room.IsPartial));
        }

        [Fact]
        public void Allocate_WithPartialRoom_MarksLastRoomAsPartial()
        {
            var hotel = CreateHotelWithRooms("H1", "Test Hotel", 2, "DOUBLE", 2); // 2 rooms of size 2
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));

            MockAvailability(hotel, range, "DOUBLE", 2);

            var result = _service.Allocate(hotel, range, 3); // 3 people: 1 full + 1 partial

            Assert.True(result.Success);
            Assert.NotNull(result.Rooms);
            Assert.Equal(2, result.Rooms.Count);
            Assert.False(result.Rooms[0].IsPartial); // First room fully occupied
            Assert.True(result.Rooms[1].IsPartial);  // Second room partially occupied
        }

        [Fact]
        public void Allocate_WithMultipleRoomTypes_AllocatesOptimally()
        {
            var roomTypes = new[]
            {
                new RoomType("SINGLE", 1, "Single room", Array.Empty<string>(), Array.Empty<string>()),
                new RoomType("DOUBLE", 2, "Double room", Array.Empty<string>(), Array.Empty<string>())
            };
            var hotel = new Hotel("H1", "Test Hotel", roomTypes, CreateRoomInstances(2, "SINGLE", 2, "DOUBLE"));
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));

            MockAvailability(hotel, range, "SINGLE", 2);
            MockAvailability(hotel, range, "DOUBLE", 2);

            var result = _service.Allocate(hotel, range, 5); // 5 people

            Assert.True(result.Success);
            Assert.NotNull(result.Rooms);
            // Optimal allocation: use 2 DOUBLEs (capacity 4) + 1 SINGLE (capacity 1) = 5 total with 3 rooms
            Assert.Equal(3, result.Rooms.Count);
        }

        [Fact]
        public void Allocate_WithNegativeAvailability_TreatsAsZero()
        {
            var hotel = CreateHotelWithRooms("H1", "Test Hotel", 2, "DOUBLE", 2);
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));

            MockAvailability(hotel, range, "DOUBLE", -5); // Negative availability (overbooked)

            var result = _service.Allocate(hotel, range, 2);

            Assert.False(result.Success);
            Assert.Contains("Not enough capacity", result.ErrorMessage);
        }

        [Fact]
        public void Allocate_WithSingleSmallRoom_SucceedsWithAllocationToOne()
        {
            var hotel = CreateHotelWithRooms("H1", "Test Hotel", 1, "SINGLE", 1); // 1 room of size 1
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));

            MockAvailability(hotel, range, "SINGLE", 1);

            var result = _service.Allocate(hotel, range, 1);

            Assert.True(result.Success);
            Assert.NotNull(result.Rooms);
            Assert.Single(result.Rooms);
            Assert.Equal("SINGLE", result.Rooms[0].RoomTypeCode);
            Assert.False(result.Rooms[0].IsPartial);
        }

        [Fact]
        public void Allocate_WithMultipleSmallRooms_SucceedsWithCorrectAllocation()
        {
            var hotel = CreateHotelWithRooms("H1", "Test Hotel", 4, "SINGLE", 1); // 4 rooms of size 1
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));

            MockAvailability(hotel, range, "SINGLE", 4);

            var result = _service.Allocate(hotel, range, 3);

            Assert.True(result.Success);
            Assert.NotNull(result.Rooms);
            Assert.Equal(3, result.Rooms.Count);
            Assert.All(result.Rooms, room => Assert.False(room.IsPartial));
        }

        [Fact]
        public void Allocate_WithLargeRoomAndSmallRequest_MinimizesRooms()
        {
            var roomTypes = new[]
            {
                new RoomType("QUAD", 4, "Quad room", Array.Empty<string>(), Array.Empty<string>())
            };
            var hotel = new Hotel("H1", "Test Hotel", roomTypes, CreateRoomInstances(1, "QUAD"));
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));

            MockAvailability(hotel, range, "QUAD", 1);

            var result = _service.Allocate(hotel, range, 1);

            Assert.True(result.Success);
            Assert.NotNull(result.Rooms);
            Assert.Single(result.Rooms); // Uses 1 room even though capacity is 4
            Assert.True(result.Rooms[0].IsPartial); // Marked as partial
        }

        [Fact]
        public void Allocate_WithNoAvailableRooms_ReturnsFalse()
        {
            var hotel = CreateHotelWithRooms("H1", "Test Hotel", 2, "DOUBLE", 2);
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));

            MockAvailability(hotel, range, "DOUBLE", 0); // No rooms available

            var result = _service.Allocate(hotel, range, 2);

            Assert.False(result.Success);
            Assert.Contains("Not enough capacity", result.ErrorMessage);
        }

        // Helper methods
        private Hotel CreateHotel(string id, string name)
        {
            return new Hotel(id, name, Array.Empty<RoomType>(), Array.Empty<Room>());
        }

        private Hotel CreateHotelWithRooms(string id, string name, int roomCount, string roomTypeCode, int roomSize)
        {
            var roomType = new RoomType(roomTypeCode, roomSize, "Test room", Array.Empty<string>(), Array.Empty<string>());
            var rooms = CreateRoomInstances(roomCount, roomTypeCode);

            return new Hotel(id, name, new[] { roomType }, rooms);
        }

        private Room[] CreateRoomInstances(int count, string roomTypeCode)
        {
            return Enumerable.Range(1, count)
                .Select(i => new Room($"R{i}", roomTypeCode))
                .ToArray();
        }

        private Room[] CreateRoomInstances(int count1, string type1, int count2, string type2)
        {
            var rooms1 = Enumerable.Range(1, count1)
                .Select(i => new Room($"{type1}_{i}", type1))
                .ToArray();
            var rooms2 = Enumerable.Range(1, count2)
                .Select(i => new Room($"{type2}_{i}", type2))
                .ToArray();

            return rooms1.Concat(rooms2).ToArray();
        }

        private DateRange CreateDateRange(DateTime startDate, DateTime endDate)
        {
            return new DateRange(startDate, endDate);
        }

        private void MockAvailability(Hotel hotel, DateRange range, string roomType, int availability)
        {
            _mockAvailService.Setup(x => x.GetAvailability(hotel, range, roomType))
                .Returns(availability);
        }
    }
}
