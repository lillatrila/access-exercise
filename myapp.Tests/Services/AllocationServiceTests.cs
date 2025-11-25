using Xunit;
using Moq;
using myapp.Models;
using myapp.Services;
using myapp.Services.Interfaces;
using myapp.Services.AllocationHelpers.Interfaces;

namespace myapp.Tests.Services
{
    public class AllocationServiceTests
    {
        private readonly Mock<IAvailabilityService> _mockAvailService;
        private readonly Mock<IInputValidator> _mockInputValidator;
        private readonly Mock<IAvailabilityCollector> _mockAvailabilityCollector;
        private readonly Mock<IItemBuilder> _mockItemBuilder;
        private readonly Mock<IKnapsackSolver> _mockKnapsackSolver;
        private readonly Mock<IAllocationReconstructor> _mockReconstructor;
        private readonly AllocationService _service;

        public AllocationServiceTests()
        {
            _mockAvailService = new Mock<IAvailabilityService>();
            _mockInputValidator = new Mock<IInputValidator>();
            _mockAvailabilityCollector = new Mock<IAvailabilityCollector>();
            _mockItemBuilder = new Mock<IItemBuilder>();
            _mockKnapsackSolver = new Mock<IKnapsackSolver>();
            _mockReconstructor = new Mock<IAllocationReconstructor>();

            _service = new AllocationService(
                _mockAvailService.Object,
                _mockInputValidator.Object,
                _mockAvailabilityCollector.Object,
                _mockItemBuilder.Object,
                _mockKnapsackSolver.Object,
                _mockReconstructor.Object);
        }

        [Fact]
        public void Allocate_WithNullHotel_ReturnsFalse()
        {
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));
            _mockInputValidator
                .Setup(x => x.Validate(null, 5))
                .Returns((false, new AllocationResult(false, "Unknown hotel", null)));

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

            _mockInputValidator
                .Setup(x => x.Validate(hotel, numPeople))
                .Returns((false, new AllocationResult(false, "numPeople must be > 0", null)));

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

            _mockInputValidator
                .Setup(x => x.Validate(hotel, 10))
                .Returns((true, null));
            
            _mockAvailabilityCollector
                .Setup(x => x.Collect(hotel, range, _mockAvailService.Object))
                .Returns(new List<(RoomType Rt, int Available)>
                {
                    (new RoomType("DOUBLE", 2, "Double room", Array.Empty<string>(), Array.Empty<string>()), 2)
                });

            var result = _service.Allocate(hotel, range, 10); // Need 10 people but max capacity is 4

            Assert.False(result.Success);
            Assert.Contains("Not enough capacity", result.ErrorMessage);
            Assert.Null(result.Rooms);
        }

        [Fact]
        public void Allocate_WithExactCapacity_SucceedsWithoutPartial()
        {
            var hotel = CreateHotelWithRooms("H1", "Test Hotel", 2, "DOUBLE", 2);
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));
            var numPeople = 4;

            var typeAvail = new List<(RoomType Rt, int Available)>
            {
                (new RoomType("DOUBLE", 2, "Double", Array.Empty<string>(), Array.Empty<string>()), 2)
            };
            var items = new List<(string, int, int)> { ("DOUBLE", 2, 4) };
            var allocatedRooms = new List<AllocatedRoom>
            {
                new AllocatedRoom("DOUBLE", false),
                new AllocatedRoom("DOUBLE", false)
            };

            SetupSuccessfulAllocation(hotel, range, numPeople, typeAvail, items, allocatedRooms);

            var result = _service.Allocate(hotel, range, numPeople);

            Assert.True(result.Success);
            Assert.NotNull(result.Rooms);
            Assert.Equal(2, result.Rooms.Count);
            Assert.All(result.Rooms, room => Assert.False(room.IsPartial));
        }

        [Fact]
        public void Allocate_WithPartialRoom_MarksLastRoomAsPartial()
        {
            var hotel = CreateHotelWithRooms("H1", "Test Hotel", 2, "DOUBLE", 2);
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));
            var numPeople = 3;

            var typeAvail = new List<(RoomType Rt, int Available)>
            {
                (new RoomType("DOUBLE", 2, "Double", Array.Empty<string>(), Array.Empty<string>()), 2)
            };
            var items = new List<(string, int, int)> { ("DOUBLE", 2, 4) };
            var allocatedRooms = new List<AllocatedRoom>
            {
                new AllocatedRoom("DOUBLE", false),
                new AllocatedRoom("DOUBLE", true)
            };

            SetupSuccessfulAllocation(hotel, range, numPeople, typeAvail, items, allocatedRooms);

            var result = _service.Allocate(hotel, range, numPeople);

            Assert.True(result.Success);
            Assert.NotNull(result.Rooms);
            Assert.Equal(2, result.Rooms.Count);
            Assert.False(result.Rooms[0].IsPartial);
            Assert.True(result.Rooms[1].IsPartial);
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
            var numPeople = 5;

            var typeAvail = new List<(RoomType Rt, int Available)>
            {
                (roomTypes[0], 2),
                (roomTypes[1], 2)
            };
            var items = new List<(string, int, int)> { ("SINGLE", 2, 2), ("DOUBLE", 2, 4) };
            var allocatedRooms = new List<AllocatedRoom>
            {
                new AllocatedRoom("DOUBLE", false),
                new AllocatedRoom("DOUBLE", false),
                new AllocatedRoom("SINGLE", false)
            };

            SetupSuccessfulAllocation(hotel, range, numPeople, typeAvail, items, allocatedRooms);

            var result = _service.Allocate(hotel, range, numPeople);

            Assert.True(result.Success);
            Assert.NotNull(result.Rooms);
            Assert.Equal(3, result.Rooms.Count);
        }

        [Fact]
        public void Allocate_WithNegativeAvailability_TreatsAsZero()
        {
            var hotel = CreateHotelWithRooms("H1", "Test Hotel", 2, "DOUBLE", 2);
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));

            _mockInputValidator
                .Setup(x => x.Validate(hotel, 2))
                .Returns((true, null));
            
            _mockAvailabilityCollector
                .Setup(x => x.Collect(hotel, range, _mockAvailService.Object))
                .Returns(new List<(RoomType Rt, int Available)>
                {
                    (new RoomType("DOUBLE", 2, "Double", Array.Empty<string>(), Array.Empty<string>()), 0)
                });

            var result = _service.Allocate(hotel, range, 2);

            Assert.False(result.Success);
            Assert.Contains("Not enough capacity", result.ErrorMessage);
        }

        [Fact]
        public void Allocate_WithSingleSmallRoom_SucceedsWithAllocationToOne()
        {
            var hotel = CreateHotelWithRooms("H1", "Test Hotel", 1, "SINGLE", 1);
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));
            var numPeople = 1;

            var typeAvail = new List<(RoomType Rt, int Available)>
            {
                (new RoomType("SINGLE", 1, "Single", Array.Empty<string>(), Array.Empty<string>()), 1)
            };
            var items = new List<(string, int, int)> { ("SINGLE", 1, 1) };
            var allocatedRooms = new List<AllocatedRoom>
            {
                new AllocatedRoom("SINGLE", false)
            };

            SetupSuccessfulAllocation(hotel, range, numPeople, typeAvail, items, allocatedRooms);

            var result = _service.Allocate(hotel, range, numPeople);

            Assert.True(result.Success);
            Assert.NotNull(result.Rooms);
            Assert.Single(result.Rooms);
            Assert.Equal("SINGLE", result.Rooms[0].RoomTypeCode);
            Assert.False(result.Rooms[0].IsPartial);
        }

        [Fact]
        public void Allocate_WithMultipleSmallRooms_SucceedsWithCorrectAllocation()
        {
            var hotel = CreateHotelWithRooms("H1", "Test Hotel", 4, "SINGLE", 1);
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));
            var numPeople = 3;

            var typeAvail = new List<(RoomType Rt, int Available)>
            {
                (new RoomType("SINGLE", 1, "Single", Array.Empty<string>(), Array.Empty<string>()), 4)
            };
            var items = new List<(string, int, int)> { ("SINGLE", 4, 4) };
            var allocatedRooms = new List<AllocatedRoom>
            {
                new AllocatedRoom("SINGLE", false),
                new AllocatedRoom("SINGLE", false),
                new AllocatedRoom("SINGLE", false)
            };

            SetupSuccessfulAllocation(hotel, range, numPeople, typeAvail, items, allocatedRooms);

            var result = _service.Allocate(hotel, range, numPeople);

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
            var numPeople = 1;

            var typeAvail = new List<(RoomType Rt, int Available)>
            {
                (roomTypes[0], 1)
            };
            var items = new List<(string, int, int)> { ("QUAD", 1, 4) };
            var allocatedRooms = new List<AllocatedRoom>
            {
                new AllocatedRoom("QUAD", true)
            };

            SetupSuccessfulAllocation(hotel, range, numPeople, typeAvail, items, allocatedRooms);

            var result = _service.Allocate(hotel, range, numPeople);

            Assert.True(result.Success);
            Assert.NotNull(result.Rooms);
            Assert.Single(result.Rooms);
            Assert.True(result.Rooms[0].IsPartial);
        }

        [Fact]
        public void Allocate_WithNoAvailableRooms_ReturnsFalse()
        {
            var hotel = CreateHotelWithRooms("H1", "Test Hotel", 2, "DOUBLE", 2);
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));

            _mockInputValidator
                .Setup(x => x.Validate(hotel, 2))
                .Returns((true, null));
            
            _mockAvailabilityCollector
                .Setup(x => x.Collect(hotel, range, _mockAvailService.Object))
                .Returns(new List<(RoomType Rt, int Available)>
                {
                    (new RoomType("DOUBLE", 2, "Double", Array.Empty<string>(), Array.Empty<string>()), 0)
                });

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

        private void SetupSuccessfulAllocation(
            Hotel hotel,
            DateRange range,
            int numPeople,
            List<(RoomType Rt, int Available)> typeAvail,
            List<(string TypeCode, int Count, int Capacity)> items,
            List<AllocatedRoom> allocatedRooms)
        {
            _mockInputValidator
                .Setup(x => x.Validate(hotel, numPeople))
                .Returns((true, null));

            _mockAvailabilityCollector
                .Setup(x => x.Collect(hotel, range, _mockAvailService.Object))
                .Returns(typeAvail);

            _mockItemBuilder
                .Setup(x => x.Build(typeAvail))
                .Returns(items);

            var parent = new (int, int)[numPeople + 5];
            parent[0] = (-1, -1);
            _mockKnapsackSolver
                .Setup(x => x.Solve(items, hotel, numPeople))
                .Returns((true, numPeople, parent, null));

            _mockReconstructor
                .Setup(x => x.Reconstruct(parent, items, numPeople))
                .Returns(new Dictionary<string, int>());

            _mockReconstructor
                .Setup(x => x.BuildAllocatedRooms(hotel, It.IsAny<Dictionary<string, int>>(), numPeople))
                .Returns(allocatedRooms);
        }
    }
}
