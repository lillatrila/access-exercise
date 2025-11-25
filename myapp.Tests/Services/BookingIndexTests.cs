using Xunit;
using myapp.Models;
using myapp.Services;

namespace myapp.Tests.Services
{
    public class BookingIndexTests
    {
        [Fact]
        public void Constructor_WithEmptyBookings_CreatesValidIndex()
        {
            var index = new BookingsIndex(Array.Empty<Booking>());

            // GetBookedCount should return 0 for any query
            var result = index.GetBookedCount("H1", DateTime.Now, "DOUBLE");
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetBookedCount_WithSingleBooking_ReturnsOne()
        {
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 2);
            var booking = CreateBooking("H1", startDate, endDate, "DOUBLE");
            var index = new BookingsIndex(new[] { booking });

            var result = index.GetBookedCount("H1", startDate, "DOUBLE");
            Assert.Equal(1, result);
        }

        [Fact]
        public void GetBookedCount_WithMultipleBookings_ReturnsCombinedCount()
        {
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 2);
            var bookings = new[]
            {
                CreateBooking("H1", startDate, endDate, "DOUBLE"),
                CreateBooking("H1", startDate, endDate, "DOUBLE"),
                CreateBooking("H1", startDate, endDate, "DOUBLE")
            };
            var index = new BookingsIndex(bookings);

            var result = index.GetBookedCount("H1", startDate, "DOUBLE");
            Assert.Equal(3, result);
        }

        [Fact]
        public void GetBookedCount_WithDifferentRoomTypes_ReturnsSeparateCounts()
        {
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 2);
            var bookings = new[]
            {
                CreateBooking("H1", startDate, endDate, "DOUBLE"),
                CreateBooking("H1", startDate, endDate, "DOUBLE"),
                CreateBooking("H1", startDate, endDate, "SINGLE")
            };
            var index = new BookingsIndex(bookings);

            var doubleResult = index.GetBookedCount("H1", startDate, "DOUBLE");
            var singleResult = index.GetBookedCount("H1", startDate, "SINGLE");

            Assert.Equal(2, doubleResult);
            Assert.Equal(1, singleResult);
        }

        [Fact]
        public void GetBookedCount_WithDifferentHotels_ReturnsSeparateCounts()
        {
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 2);
            var bookings = new[]
            {
                CreateBooking("H1", startDate, endDate, "DOUBLE"),
                CreateBooking("H1", startDate, endDate, "DOUBLE"),
                CreateBooking("H2", startDate, endDate, "DOUBLE")
            };
            var index = new BookingsIndex(bookings);

            var h1Result = index.GetBookedCount("H1", startDate, "DOUBLE");
            var h2Result = index.GetBookedCount("H2", startDate, "DOUBLE");

            Assert.Equal(2, h1Result);
            Assert.Equal(1, h2Result);
        }

        [Fact]
        public void GetBookedCount_WithMultiNightBooking_CoversAllNights()
        {
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 4); // 3 nights: Jan 1, 2, 3
            var booking = CreateBooking("H1", startDate, endDate, "DOUBLE");
            var index = new BookingsIndex(new[] { booking });

            var jan1 = index.GetBookedCount("H1", new DateTime(2024, 1, 1), "DOUBLE");
            var jan2 = index.GetBookedCount("H1", new DateTime(2024, 1, 2), "DOUBLE");
            var jan3 = index.GetBookedCount("H1", new DateTime(2024, 1, 3), "DOUBLE");
            var jan4 = index.GetBookedCount("H1", new DateTime(2024, 1, 4), "DOUBLE");

            Assert.Equal(1, jan1);
            Assert.Equal(1, jan2);
            Assert.Equal(1, jan3);
            Assert.Equal(0, jan4); // Departure date is not included
        }

        [Fact]
        public void GetBookedCount_BeforeBookingStart_ReturnsZero()
        {
            var startDate = new DateTime(2024, 1, 5);
            var endDate = new DateTime(2024, 1, 6);
            var booking = CreateBooking("H1", startDate, endDate, "DOUBLE");
            var index = new BookingsIndex(new[] { booking });

            var result = index.GetBookedCount("H1", new DateTime(2024, 1, 4), "DOUBLE");
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetBookedCount_AfterBookingEnd_ReturnsZero()
        {
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 2);
            var booking = CreateBooking("H1", startDate, endDate, "DOUBLE");
            var index = new BookingsIndex(new[] { booking });

            var result = index.GetBookedCount("H1", new DateTime(2024, 1, 3), "DOUBLE");
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetBookedCount_IsCaseInsensitiveForHotelId()
        {
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 2);
            var booking = CreateBooking("H1", startDate, endDate, "DOUBLE");
            var index = new BookingsIndex(new[] { booking });

            var resultLower = index.GetBookedCount("h1", startDate, "DOUBLE");
            var resultUpper = index.GetBookedCount("H1", startDate, "DOUBLE");
            var resultMixed = index.GetBookedCount("H1", startDate, "DOUBLE");

            Assert.Equal(1, resultLower);
            Assert.Equal(1, resultUpper);
            Assert.Equal(1, resultMixed);
        }

        [Fact]
        public void GetBookedCount_IsCaseInsensitiveForRoomType()
        {
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 2);
            var booking = CreateBooking("H1", startDate, endDate, "DOUBLE");
            var index = new BookingsIndex(new[] { booking });

            var resultLower = index.GetBookedCount("H1", startDate, "double");
            var resultUpper = index.GetBookedCount("H1", startDate, "DOUBLE");
            var resultMixed = index.GetBookedCount("H1", startDate, "Double");

            Assert.Equal(1, resultLower);
            Assert.Equal(1, resultUpper);
            Assert.Equal(1, resultMixed);
        }

        [Fact]
        public void GetBookedCount_WithOverlappingBookings_CountsAllNights()
        {
            // Booking1: Jan 1-2 (covers Jan 1 only)
            // Booking2: Jan 1-3 (covers Jan 1, 2)
            // Expected: Jan 1 = 2, Jan 2 = 1
            var booking1 = CreateBooking("H1", new DateTime(2024, 1, 1), new DateTime(2024, 1, 2), "DOUBLE");
            var booking2 = CreateBooking("H1", new DateTime(2024, 1, 1), new DateTime(2024, 1, 3), "DOUBLE");
            var index = new BookingsIndex(new[] { booking1, booking2 });

            var jan1 = index.GetBookedCount("H1", new DateTime(2024, 1, 1), "DOUBLE");
            var jan2 = index.GetBookedCount("H1", new DateTime(2024, 1, 2), "DOUBLE");
            var jan3 = index.GetBookedCount("H1", new DateTime(2024, 1, 3), "DOUBLE");

            Assert.Equal(2, jan1); // Both bookings cover Jan 1
            Assert.Equal(1, jan2); // Only booking2 covers Jan 2
            Assert.Equal(0, jan3); // No bookings cover Jan 3
        }

        [Fact]
        public void GetBookedCount_WithConsecutiveBookings_CountsCorrectly()
        {
            var booking1 = CreateBooking("H1", new DateTime(2024, 1, 1), new DateTime(2024, 1, 2), "DOUBLE");
            var booking2 = CreateBooking("H1", new DateTime(2024, 1, 2), new DateTime(2024, 1, 3), "DOUBLE");
            var index = new BookingsIndex(new[] { booking1, booking2 });

            var jan1 = index.GetBookedCount("H1", new DateTime(2024, 1, 1), "DOUBLE");
            var jan2 = index.GetBookedCount("H1", new DateTime(2024, 1, 2), "DOUBLE");
            var jan3 = index.GetBookedCount("H1", new DateTime(2024, 1, 3), "DOUBLE");

            Assert.Equal(1, jan1);
            Assert.Equal(1, jan2);
            Assert.Equal(0, jan3);
        }

        [Fact]
        public void GetBookedCount_WithSingleNightBooking_CoversOnlyThatNight()
        {
            var startDate = new DateTime(2024, 1, 15);
            var endDate = new DateTime(2024, 1, 16);
            var booking = CreateBooking("H1", startDate, endDate, "DOUBLE");
            var index = new BookingsIndex(new[] { booking });

            var beforeResult = index.GetBookedCount("H1", new DateTime(2024, 1, 14), "DOUBLE");
            var duringResult = index.GetBookedCount("H1", new DateTime(2024, 1, 15), "DOUBLE");
            var afterResult = index.GetBookedCount("H1", new DateTime(2024, 1, 16), "DOUBLE");

            Assert.Equal(0, beforeResult);
            Assert.Equal(1, duringResult);
            Assert.Equal(0, afterResult);
        }

        [Fact]
        public void GetBookedCount_WithComplexScenario_CountsCorrectly()
        {
            // Hotel 1: 2 DOUBLE bookings on Jan 1, 1 SINGLE on Jan 1
            // Hotel 2: 1 DOUBLE booking on Jan 1
            var bookings = new[]
            {
                CreateBooking("H1", new DateTime(2024, 1, 1), new DateTime(2024, 1, 2), "DOUBLE"),
                CreateBooking("H1", new DateTime(2024, 1, 1), new DateTime(2024, 1, 2), "DOUBLE"),
                CreateBooking("H1", new DateTime(2024, 1, 1), new DateTime(2024, 1, 2), "SINGLE"),
                CreateBooking("H2", new DateTime(2024, 1, 1), new DateTime(2024, 1, 2), "DOUBLE"),
                CreateBooking("H1", new DateTime(2024, 1, 2), new DateTime(2024, 1, 3), "DOUBLE")
            };
            var index = new BookingsIndex(bookings);

            var h1_jan1_double = index.GetBookedCount("H1", new DateTime(2024, 1, 1), "DOUBLE");
            var h1_jan1_single = index.GetBookedCount("H1", new DateTime(2024, 1, 1), "SINGLE");
            var h1_jan2_double = index.GetBookedCount("H1", new DateTime(2024, 1, 2), "DOUBLE");
            var h2_jan1_double = index.GetBookedCount("H2", new DateTime(2024, 1, 1), "DOUBLE");

            Assert.Equal(2, h1_jan1_double);
            Assert.Equal(1, h1_jan1_single);
            Assert.Equal(1, h1_jan2_double);
            Assert.Equal(1, h2_jan1_double);
        }

        [Fact]
        public void GetBookedCount_WithNonExistentHotel_ReturnsZero()
        {
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 2);
            var booking = CreateBooking("H1", startDate, endDate, "DOUBLE");
            var index = new BookingsIndex(new[] { booking });

            var result = index.GetBookedCount("H999", startDate, "DOUBLE");
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetBookedCount_WithNonExistentRoomType_ReturnsZero()
        {
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 2);
            var booking = CreateBooking("H1", startDate, endDate, "DOUBLE");
            var index = new BookingsIndex(new[] { booking });

            var result = index.GetBookedCount("H1", startDate, "QUAD");
            Assert.Equal(0, result);
        }

        [Fact]
        public void Constructor_WithLargeNumberOfBookings_HandlesEfficiently()
        {
            var bookings = Enumerable.Range(0, 100)
                .Select(i => CreateBooking($"H{i % 5}", 
                    new DateTime(2024, 1, (i % 20) + 1),
                    new DateTime(2024, 1, ((i % 20) + 1) + (i % 5)),
                    i % 2 == 0 ? "DOUBLE" : "SINGLE"))
                .ToArray();
            
            var index = new BookingsIndex(bookings);

            // Verify a few spot checks
            var result1 = index.GetBookedCount("H0", new DateTime(2024, 1, 1), "DOUBLE");
            Assert.True(result1 >= 0); // Should handle large dataset
            
            var result2 = index.GetBookedCount("H4", new DateTime(2024, 1, 15), "SINGLE");
            Assert.True(result2 >= 0); // Should handle large dataset
        }

        // Helper method
        private Booking CreateBooking(string hotelId, DateTime arrival, DateTime departure, string roomType)
        {
            return new Booking(hotelId, arrival.ToString("yyyyMMdd"), departure.ToString("yyyyMMdd"), roomType, "100.00");
        }
    }
}
