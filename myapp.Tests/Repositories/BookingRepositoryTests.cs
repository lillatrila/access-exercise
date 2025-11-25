using Xunit;
using myapp.Models;
using System.Text.Json;
using System.IO;

public class BookingRepositoryTests : IDisposable
{
    private readonly string _testDataDirectory;

    public BookingRepositoryTests()
    {
        _testDataDirectory = Path.Combine(Path.GetTempPath(), $"BookingRepoTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataDirectory);
    }

    [Fact]
    public void Constructor_WithValidBookingsFile_LoadsSuccessfully()
    {
        var bookingsFile = CreateBookingsJsonFile(new[]
        {
            CreateBookingData("H1", "20240101", "20240102", "DOUBLE", "100.00")
        });

        var repository = new BookingRepository(bookingsFile);

        Assert.NotNull(repository);
        Assert.Single(repository.GetAll());
    }

    [Fact]
    public void Constructor_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        var nonExistentPath = Path.Combine(_testDataDirectory, "nonexistent.json");

        Assert.Throws<FileNotFoundException>(() => new BookingRepository(nonExistentPath));
    }

    [Fact]
    public void Constructor_WithEmptyBookingsArray_CreatesEmptyRepository()
    {
        var bookingsFile = CreateBookingsJsonFile(Array.Empty<object>());

        var repository = new BookingRepository(bookingsFile);

        Assert.Empty(repository.GetAll());
    }

    [Fact]
    public void Constructor_WithMissingHotelId_ThrowsException()
    {
        var bookingWithoutHotelId = new { arrival = "20240101", departure = "20240102", roomType = "DOUBLE", roomRate = "100" };
        var bookingsFile = CreateBookingsJsonFile(new[] { bookingWithoutHotelId });

        Assert.Throws<Exception>(() => new BookingRepository(bookingsFile));
    }

    [Fact]
    public void Constructor_WithEmptyHotelId_ThrowsException()
    {
        var bookingWithEmptyHotelId = CreateBookingData("  ", "20240101", "20240102", "DOUBLE", "100.00");
        var bookingsFile = CreateBookingsJsonFile(new[] { bookingWithEmptyHotelId });

        Assert.Throws<Exception>(() => new BookingRepository(bookingsFile));
    }

    [Fact]
    public void Constructor_WithMissingArrivalDate_ThrowsException()
    {
        var bookingWithoutArrival = new { hotelId = "H1", departure = "20240102", roomType = "DOUBLE", roomRate = "100" };
        var bookingsFile = CreateBookingsJsonFile(new[] { bookingWithoutArrival });

        Assert.Throws<Exception>(() => new BookingRepository(bookingsFile));
    }

    [Fact]
    public void Constructor_WithEmptyArrivalDate_ThrowsException()
    {
        var bookingWithEmptyArrival = CreateBookingData("H1", "  ", "20240102", "DOUBLE", "100.00");
        var bookingsFile = CreateBookingsJsonFile(new[] { bookingWithEmptyArrival });

        Assert.Throws<Exception>(() => new BookingRepository(bookingsFile));
    }

    [Fact]
    public void Constructor_WithMissingDepartureDate_ThrowsException()
    {
        var bookingWithoutDeparture = new { hotelId = "H1", arrival = "20240101", roomType = "DOUBLE", roomRate = "100" };
        var bookingsFile = CreateBookingsJsonFile(new[] { bookingWithoutDeparture });

        Assert.Throws<Exception>(() => new BookingRepository(bookingsFile));
    }

    [Fact]
    public void Constructor_WithEmptyDepartureDate_ThrowsException()
    {
        var bookingWithEmptyDeparture = CreateBookingData("H1", "20240101", "  ", "DOUBLE", "100.00");
        var bookingsFile = CreateBookingsJsonFile(new[] { bookingWithEmptyDeparture });

        Assert.Throws<Exception>(() => new BookingRepository(bookingsFile));
    }

    [Fact]
    public void Constructor_WithDepartureEqualToArrival_ThrowsException()
    {
        var bookingWithEqualDates = CreateBookingData("H1", "20240101", "20240101", "DOUBLE", "100.00");
        var bookingsFile = CreateBookingsJsonFile(new[] { bookingWithEqualDates });

        Assert.Throws<Exception>(() => new BookingRepository(bookingsFile));
    }

    [Fact]
    public void Constructor_WithDepartureBeforeArrival_ThrowsException()
    {
        var bookingWithBackwardDates = CreateBookingData("H1", "20240102", "20240101", "DOUBLE", "100.00");
        var bookingsFile = CreateBookingsJsonFile(new[] { bookingWithBackwardDates });

        Assert.Throws<Exception>(() => new BookingRepository(bookingsFile));
    }

    [Fact]
    public void GetAll_ReturnsAllLoadedBookings()
    {
        var bookingsFile = CreateBookingsJsonFile(new[]
        {
            CreateBookingData("H1", "20240101", "20240102", "DOUBLE", "100.00"),
            CreateBookingData("H1", "20240102", "20240104", "SINGLE", "50.00"),
            CreateBookingData("H2", "20240110", "20240112", "DOUBLE", "120.00")
        });

        var repository = new BookingRepository(bookingsFile);
        var bookings = repository.GetAll();

        Assert.Equal(3, bookings.Count);
    }

    [Fact]
    public void GetAll_ReturnsReadOnlyCollection()
    {
        var bookingsFile = CreateBookingsJsonFile(new[]
        {
            CreateBookingData("H1", "20240101", "20240102", "DOUBLE", "100.00")
        });

        var repository = new BookingRepository(bookingsFile);
        var bookings = repository.GetAll();

        Assert.IsAssignableFrom<IReadOnlyList<Booking>>(bookings);
    }

    [Fact]
    public void Constructor_PreservesBookingData()
    {
        var bookingsFile = CreateBookingsJsonFile(new[]
        {
            CreateBookingData("H1", "20240115", "20240120", "DBL", "99.99")
        });

        var repository = new BookingRepository(bookingsFile);
        var booking = repository.GetAll().First();

        Assert.Equal("H1", booking.HotelId);
        Assert.Equal("20240115", booking.Arrival);
        Assert.Equal("20240120", booking.Departure);
        Assert.Equal("DBL", booking.RoomType);
        Assert.Equal("99.99", booking.RoomRate);
    }

    [Fact]
    public void Constructor_WithMultipleBookingsSameHotel_LoadsAll()
    {
        var bookingsFile = CreateBookingsJsonFile(new[]
        {
            CreateBookingData("H1", "20240101", "20240103", "DOUBLE", "100.00"),
            CreateBookingData("H1", "20240103", "20240105", "DOUBLE", "100.00"),
            CreateBookingData("H1", "20240105", "20240107", "SINGLE", "50.00")
        });

        var repository = new BookingRepository(bookingsFile);
        var bookings = repository.GetAll();

        Assert.Equal(3, bookings.Count);
        Assert.All(bookings, b => Assert.Equal("H1", b.HotelId));
    }

    [Fact]
    public void Constructor_WithMultipleHotels_LoadsAll()
    {
        var bookingsFile = CreateBookingsJsonFile(new[]
        {
            CreateBookingData("H1", "20240101", "20240103", "DOUBLE", "100.00"),
            CreateBookingData("H2", "20240101", "20240103", "DOUBLE", "120.00"),
            CreateBookingData("H3", "20240101", "20240103", "DOUBLE", "110.00")
        });

        var repository = new BookingRepository(bookingsFile);
        var bookings = repository.GetAll();

        Assert.Equal(3, bookings.Count);
        Assert.Contains(bookings, b => b.HotelId == "H1");
        Assert.Contains(bookings, b => b.HotelId == "H2");
        Assert.Contains(bookings, b => b.HotelId == "H3");
    }

    [Fact]
    public void Constructor_WithDifferentRoomTypes_LoadsAll()
    {
        var bookingsFile = CreateBookingsJsonFile(new[]
        {
            CreateBookingData("H1", "20240101", "20240102", "SINGLE", "50.00"),
            CreateBookingData("H1", "20240102", "20240104", "DOUBLE", "100.00"),
            CreateBookingData("H1", "20240104", "20240106", "QUAD", "150.00")
        });

        var repository = new BookingRepository(bookingsFile);
        var bookings = repository.GetAll();

        Assert.Equal(3, bookings.Count);
        Assert.Contains(bookings, b => b.RoomType == "SINGLE");
        Assert.Contains(bookings, b => b.RoomType == "DOUBLE");
        Assert.Contains(bookings, b => b.RoomType == "QUAD");
    }

    [Fact]
    public void Constructor_WithValidDateRanges_LoadsSuccessfully()
    {
        var bookingsFile = CreateBookingsJsonFile(new[]
        {
            CreateBookingData("H1", "20240101", "20240102", "DOUBLE", "100.00"), // 1 night
            CreateBookingData("H1", "20240105", "20240115", "DOUBLE", "100.00"), // 10 nights
            CreateBookingData("H1", "20241201", "20250101", "DOUBLE", "100.00")  // multi-month
        });

        var repository = new BookingRepository(bookingsFile);
        var bookings = repository.GetAll();

        Assert.Equal(3, bookings.Count);
    }

    [Fact]
    public void Constructor_WithLargeDataset_LoadsSuccessfully()
    {
        var bookings = Enumerable.Range(0, 100)
            .Select(i => CreateBookingData(
                $"H{i % 10}",
                $"2024010{(i % 8) + 1:D1}",
                $"2024010{(i % 8) + 2:D1}",
                i % 2 == 0 ? "DOUBLE" : "SINGLE",
                (50 + i).ToString()))
            .ToArray();

        var bookingsFile = CreateBookingsJsonFile(bookings);

        var repository = new BookingRepository(bookingsFile);

        Assert.Equal(100, repository.GetAll().Count);
    }

    [Fact]
    public void Constructor_WithInvalidDateFormat_ThrowsException()
    {
        var bookingWithInvalidDate = CreateBookingData("H1", "01-01-2024", "20240102", "DOUBLE", "100.00");
        var bookingsFile = CreateBookingsJsonFile(new[] { bookingWithInvalidDate });

        Assert.Throws<FormatException>(() => new BookingRepository(bookingsFile));
    }

    [Fact]
    public void Constructor_WithInvalidArrivalDateFormat_ThrowsException()
    {
        var bookingWithInvalidArrival = CreateBookingData("H1", "invalid", "20240102", "DOUBLE", "100.00");
        var bookingsFile = CreateBookingsJsonFile(new[] { bookingWithInvalidArrival });

        Assert.Throws<FormatException>(() => new BookingRepository(bookingsFile));
    }

    [Fact]
    public void Constructor_WithInvalidDepartureDateFormat_ThrowsException()
    {
        var bookingWithInvalidDeparture = CreateBookingData("H1", "20240101", "invalid", "DOUBLE", "100.00");
        var bookingsFile = CreateBookingsJsonFile(new[] { bookingWithInvalidDeparture });

        Assert.Throws<FormatException>(() => new BookingRepository(bookingsFile));
    }

    // Helper methods
    private string CreateBookingsJsonFile(object[] bookings)
    {
        var filePath = Path.Combine(_testDataDirectory, $"bookings_{Guid.NewGuid()}.json");
        var json = JsonSerializer.Serialize(bookings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
        return filePath;
    }

    private object CreateBookingData(string hotelId, string arrival, string departure, string roomType, string roomRate)
    {
        return new { hotelId, arrival, departure, roomType, roomRate };
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDataDirectory))
        {
            Directory.Delete(_testDataDirectory, recursive: true);
        }
    }
}
