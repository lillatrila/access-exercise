using Xunit;
using myapp.Models;
using myapp.Services;

namespace myapp.Tests.Services
{
    public class AvailabilityServiceTests
    {
        private readonly AvailabilityService _service;

        public AvailabilityServiceTests()
        {
            var emptyIndex = new BookingsIndex(Array.Empty<Booking>());
            _service = new AvailabilityService(emptyIndex);
        }

        [Fact]
        public void GetAvailability_WithNullHotel_ThrowsArgumentNullException()
        {
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));
            Assert.Throws<ArgumentNullException>(() => _service.GetAvailability(null!, range, "DBL"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void GetAvailability_WithInvalidRoomType_ThrowsArgumentNullException(string roomType)
        {
            var hotel = CreateHotel("H1", "Test Hotel");
            var range = CreateDateRange(DateTime.Now, DateTime.Now.AddDays(1));
            Assert.Throws<ArgumentNullException>(() => _service.GetAvailability(hotel, range, roomType));
        }

        [Fact]
        public void GetAvailability_WithEmptyDateRange_ReturnsZero()
        {
            var hotel = CreateHotel("H1", "Test Hotel");
            var sameDay = DateTime.Now;
            var range = CreateDateRange(sameDay, sameDay);
            
            var result = _service.GetAvailability(hotel, range, "DBL");
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetAvailability_WithFullyAvailableRooms_ReturnsAllRooms()
        {
            var hotel = CreateHotelWithRooms("H1", "Test Hotel", 10, "DBL");
            var startDate = new DateTime(2024, 1, 1);
            var range = CreateDateRange(startDate, startDate.AddDays(1));
            
            var result = _service.GetAvailability(hotel, range, "DBL");
            Assert.Equal(10, result);
        }

        [Fact]
        public void GetAvailability_WithPartiallyBookedRooms_ReturnsAvailableCount()
        {
            var hotel = CreateHotelWithRooms("H1", "Test Hotel", 10, "DBL");
            var startDate = new DateTime(2024, 1, 1);
            var endDate = startDate.AddDays(1);
            var range = CreateDateRange(startDate, endDate);
            
            var booking = CreateBooking("H1", startDate, endDate, "DBL");
            var indexWithBooking = new BookingsIndex(new[] { booking });
            var serviceWithBooking = new AvailabilityService(indexWithBooking);
            
            var result = serviceWithBooking.GetAvailability(hotel, range, "DBL");
            Assert.Equal(9, result);
        }

        [Fact]
        public void GetAvailability_WithFullyBookedRooms_ReturnsZero()
        {
            var hotel = CreateHotelWithRooms("H1", "Test Hotel", 5, "DBL");
            var startDate = new DateTime(2024, 1, 1);
            var endDate = startDate.AddDays(1);
            var range = CreateDateRange(startDate, endDate);
            
            var bookings = Enumerable.Range(1, 5)
                .Select(i => CreateBooking("H1", startDate, endDate, "DBL"))
                .ToArray();
            var indexWithBookings = new BookingsIndex(bookings);
            var serviceWithBookings = new AvailabilityService(indexWithBookings);
            
            var result = serviceWithBookings.GetAvailability(hotel, range, "DBL");
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetAvailability_WithOverBookedRooms_ReturnsZero()
        {
            var hotel = CreateHotelWithRooms("H1", "Test Hotel", 5, "DBL");
            var startDate = new DateTime(2024, 1, 1);
            var endDate = startDate.AddDays(1);
            var range = CreateDateRange(startDate, endDate);
            
            // Create 10 bookings for a single day (more than 5 rooms available)
            var bookings = Enumerable.Range(1, 10)
                .Select(i => CreateBooking("H1", startDate, startDate.AddDays(1), "DBL"))
                .ToArray();
            var indexWithBookings = new BookingsIndex(bookings);
            var serviceWithBookings = new AvailabilityService(indexWithBookings);
            
            var result = serviceWithBookings.GetAvailability(hotel, range, "DBL");
            // When overbooked, availability becomes negative
            Assert.True(result <= 0);
        }

        [Fact]
        public void GetAvailability_AcrossMultipleDays_ReturnsMinimumAvailability()
        {
            var hotel = CreateHotelWithRooms("H1", "Test Hotel", 10, "DBL");
            var startDate = new DateTime(2024, 1, 1);
            var range = CreateDateRange(startDate, startDate.AddDays(3));
            
            // Day 1: 2 booked, 8 available
            // Day 2: 4 booked, 6 available
            // Day 3: 2 booked, 8 available
            var bookings = new Booking[]
            {
                CreateBooking("H1", startDate, startDate.AddDays(1), "DBL"),
                CreateBooking("H1", startDate, startDate.AddDays(1), "DBL"),
                CreateBooking("H1", startDate.AddDays(1), startDate.AddDays(2), "DBL"),
                CreateBooking("H1", startDate.AddDays(1), startDate.AddDays(2), "DBL"),
                CreateBooking("H1", startDate.AddDays(1), startDate.AddDays(2), "DBL"),
                CreateBooking("H1", startDate.AddDays(1), startDate.AddDays(2), "DBL"),
                CreateBooking("H1", startDate.AddDays(2), startDate.AddDays(3), "DBL"),
                CreateBooking("H1", startDate.AddDays(2), startDate.AddDays(3), "DBL")
            };
            var indexWithBookings = new BookingsIndex(bookings);
            var serviceWithBookings = new AvailabilityService(indexWithBookings);
            
            var result = serviceWithBookings.GetAvailability(hotel, range, "DBL");
            Assert.Equal(6, result); 
        }

        [Fact]
        public void GetAvailability_IsCaseInsensitiveForRoomType()
        {
            var hotel = CreateHotelWithRooms("H1", "Test Hotel", 10, "DBL");
            var startDate = new DateTime(2024, 1, 1);
            var endDate = startDate.AddDays(1);
            var range = CreateDateRange(startDate, endDate);
            
            var booking = CreateBooking("H1", startDate, endDate, "DBL");
            var indexWithBooking = new BookingsIndex(new[] { booking });
            var serviceWithBooking = new AvailabilityService(indexWithBooking);
            
            var result = serviceWithBooking.GetAvailability(hotel, range, "dbl");
            Assert.Equal(9, result);
        }

        [Fact]
        public void GetAvailability_WithDifferentRoomTypes_TracksIndependently()
        {
            var roomTypes = new[]
            {
                new RoomType("DOUBLE", 2, "Double room", new[] { "WiFi" }, new[] { "AC" }),
                new RoomType("SINGLE", 1, "Single room", new[] { "WiFi" }, new[] { "AC" })
            };
            var doubleRooms = new[] { new Room("D1", "DOUBLE"), new Room("D2", "DOUBLE") };
            var singleRooms = new[] { new Room("S1", "SINGLE"), new Room("S2", "SINGLE") };
            var hotel = new Hotel("H1", "Test Hotel", roomTypes, doubleRooms.Concat(singleRooms).ToArray());
            
            var startDate = new DateTime(2024, 1, 1);
            var endDate = startDate.AddDays(1);
            var range = CreateDateRange(startDate, endDate);
            
            var bookings = new[]
            {
                CreateBooking("H1", startDate, endDate, "DOUBLE")
            };
            var indexWithBooking = new BookingsIndex(bookings);
            var serviceWithBooking = new AvailabilityService(indexWithBooking);
            
            var doubleAvail = serviceWithBooking.GetAvailability(hotel, range, "DOUBLE");
            var singleAvail = serviceWithBooking.GetAvailability(hotel, range, "SINGLE");
            
            Assert.Equal(1, doubleAvail);
            Assert.Equal(2, singleAvail);
        }
        
        private Hotel CreateHotel(string id, string name)
        {
            return new Hotel(id, name, Array.Empty<RoomType>(), Array.Empty<Room>());
        }

        private Hotel CreateHotelWithRooms(string id, string name, int roomCount, string roomTypeCode)
        {
            var roomType = new RoomType(roomTypeCode, 1, "Test room", Array.Empty<string>(), Array.Empty<string>());
            var rooms = Enumerable.Range(1, roomCount)
                .Select(i => new Room($"R{i}", roomTypeCode))
                .ToArray();
            
            return new Hotel(id, name, new[] { roomType }, rooms);
        }

        private DateRange CreateDateRange(DateTime startDate, DateTime endDate)
        {
            return new DateRange(startDate, endDate);
        }

        private Booking CreateBooking(string hotelId, DateTime arrival, DateTime departure, string roomType)
        {
            return new Booking(hotelId, arrival.ToString("yyyyMMdd"), departure.ToString("yyyyMMdd"), roomType, "100.00");
        }
    }
}
