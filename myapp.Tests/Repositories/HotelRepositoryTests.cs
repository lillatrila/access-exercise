using Xunit;
using myapp.Models;
using System.Text.Json;
using System.IO;

public class HotelRepositoryTests : IDisposable
{
    private readonly string _testDataDirectory;

    public HotelRepositoryTests()
    {
        _testDataDirectory = Path.Combine(Path.GetTempPath(), $"HotelRepoTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataDirectory);
    }

    [Fact]
    public void Constructor_WithValidHotelsFile_LoadsSuccessfully()
    {
        var hotelsFile = CreateHotelsJsonFile(new[]
        {
            CreateHotelData("H1", "Test Hotel", new[]
            {
                CreateRoomTypeData("DOUBLE", 2, "Double Room"),
                CreateRoomTypeData("SINGLE", 1, "Single Room")
            }, new[]
            {
                CreateRoomData("R101", "DOUBLE"),
                CreateRoomData("R102", "SINGLE")
            })
        });

        var repository = new HotelRepository(hotelsFile);

        Assert.NotNull(repository);
        Assert.Single(repository.GetAll());
    }

    [Fact]
    public void Constructor_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        var nonExistentPath = Path.Combine(_testDataDirectory, "nonexistent.json");

        Assert.Throws<FileNotFoundException>(() => new HotelRepository(nonExistentPath));
    }

    [Fact]
    public void Constructor_WithEmptyHotelsArray_CreatesEmptyRepository()
    {
        var hotelsFile = CreateHotelsJsonFile(Array.Empty<object>());

        var repository = new HotelRepository(hotelsFile);

        Assert.Empty(repository.GetAll());
    }

    [Fact]
    public void Constructor_WithMissingHotelId_ThrowsException()
    {
        var hotelWithoutId = new { name = "Test Hotel", roomTypes = new object[0], rooms = new object[0] };
        var hotelsFile = CreateHotelsJsonFile(new[] { hotelWithoutId });

        Assert.Throws<Exception>(() => new HotelRepository(hotelsFile));
    }

    [Fact]
    public void Constructor_WithEmptyHotelId_ThrowsException()
    {
        var hotelWithEmptyId = new { id = "  ", name = "Test Hotel", roomTypes = new object[0], rooms = new object[0] };
        var hotelsFile = CreateHotelsJsonFile(new[] { hotelWithEmptyId });

        Assert.Throws<Exception>(() => new HotelRepository(hotelsFile));
    }

    [Fact]
    public void Constructor_WithMissingRoomTypes_ThrowsException()
    {
        var hotelWithoutRoomTypes = new { id = "H1", name = "Test Hotel", rooms = new object[0] };
        var hotelsFile = CreateHotelsJsonFile(new[] { hotelWithoutRoomTypes });

        Assert.Throws<Exception>(() => new HotelRepository(hotelsFile));
    }

    [Fact]
    public void Constructor_WithMissingRooms_ThrowsException()
    {
        var hotelWithoutRooms = new { id = "H1", name = "Test Hotel", roomTypes = new object[0] };
        var hotelsFile = CreateHotelsJsonFile(new[] { hotelWithoutRooms });

        Assert.Throws<Exception>(() => new HotelRepository(hotelsFile));
    }

    [Fact]
    public void Constructor_WithRoomTypeMissingCode_ThrowsException()
    {
        var roomTypeWithoutCode = new { size = 2, description = "Double Room" };
        var hotelsFile = CreateHotelsJsonFile(new[]
        {
            CreateHotelData("H1", "Test Hotel", new[] { roomTypeWithoutCode }, new object[0])
        });

        Assert.Throws<Exception>(() => new HotelRepository(hotelsFile));
    }

    [Fact]
    public void Constructor_WithEmptyRoomTypeCode_ThrowsException()
    {
        var roomTypeWithEmptyCode = CreateRoomTypeData("  ", 2, "Double Room");
        var hotelsFile = CreateHotelsJsonFile(new[]
        {
            CreateHotelData("H1", "Test Hotel", new[] { roomTypeWithEmptyCode }, new object[0])
        });

        Assert.Throws<Exception>(() => new HotelRepository(hotelsFile));
    }

    [Fact]
    public void GetAll_ReturnsAllLoadedHotels()
    {
        var hotelsFile = CreateHotelsJsonFile(new[]
        {
            CreateHotelData("H1", "Hotel One", new[] { CreateRoomTypeData("DOUBLE", 2) }, new object[0]),
            CreateHotelData("H2", "Hotel Two", new[] { CreateRoomTypeData("SINGLE", 1) }, new object[0]),
            CreateHotelData("H3", "Hotel Three", new[] { CreateRoomTypeData("DOUBLE", 2) }, new object[0])
        });

        var repository = new HotelRepository(hotelsFile);
        var hotels = repository.GetAll();

        Assert.Equal(3, hotels.Count);
        Assert.Contains(hotels, h => h.Id == "H1");
        Assert.Contains(hotels, h => h.Id == "H2");
        Assert.Contains(hotels, h => h.Id == "H3");
    }

    [Fact]
    public void GetAll_ReturnsReadOnlyCollection()
    {
        var hotelsFile = CreateHotelsJsonFile(new[]
        {
            CreateHotelData("H1", "Test Hotel", new[] { CreateRoomTypeData("DOUBLE", 2) }, new object[0])
        });

        var repository = new HotelRepository(hotelsFile);
        var hotels = repository.GetAll();

        Assert.IsAssignableFrom<IReadOnlyList<Hotel>>(hotels);
    }

    [Fact]
    public void GetById_WithExactMatch_ReturnsHotel()
    {
        var hotelsFile = CreateHotelsJsonFile(new[]
        {
            CreateHotelData("H1", "Test Hotel", new[] { CreateRoomTypeData("DOUBLE", 2) }, new object[0])
        });

        var repository = new HotelRepository(hotelsFile);
        var hotel = repository.GetById("H1");

        Assert.NotNull(hotel);
        Assert.Equal("H1", hotel.Id);
        Assert.Equal("Test Hotel", hotel.Name);
    }

    [Fact]
    public void GetById_WithDifferentCase_ReturnsHotel()
    {
        var hotelsFile = CreateHotelsJsonFile(new[]
        {
            CreateHotelData("H1", "Test Hotel", new[] { CreateRoomTypeData("DOUBLE", 2) }, new object[0])
        });

        var repository = new HotelRepository(hotelsFile);

        var hotelLower = repository.GetById("h1");
        var hotelUpper = repository.GetById("H1");
        var hotelMixed = repository.GetById("H1");

        Assert.NotNull(hotelLower);
        Assert.NotNull(hotelUpper);
        Assert.NotNull(hotelMixed);
        Assert.Equal(hotelLower.Id, hotelUpper.Id);
    }

    [Fact]
    public void GetById_WithNonExistentId_ReturnsNull()
    {
        var hotelsFile = CreateHotelsJsonFile(new[]
        {
            CreateHotelData("H1", "Test Hotel", new[] { CreateRoomTypeData("DOUBLE", 2) }, new object[0])
        });

        var repository = new HotelRepository(hotelsFile);
        var hotel = repository.GetById("H999");

        Assert.Null(hotel);
    }

    [Fact]
    public void GetById_WithMultipleHotels_ReturnsCorrectOne()
    {
        var hotelsFile = CreateHotelsJsonFile(new[]
        {
            CreateHotelData("H1", "Hotel One", new[] { CreateRoomTypeData("DOUBLE", 2) }, new object[0]),
            CreateHotelData("H2", "Hotel Two", new[] { CreateRoomTypeData("SINGLE", 1) }, new object[0]),
            CreateHotelData("H3", "Hotel Three", new[] { CreateRoomTypeData("QUAD", 4) }, new object[0])
        });

        var repository = new HotelRepository(hotelsFile);

        var hotel1 = repository.GetById("H1");
        var hotel2 = repository.GetById("H2");
        var hotel3 = repository.GetById("H3");

        Assert.Equal("Hotel One", hotel1.Name);
        Assert.Equal("Hotel Two", hotel2.Name);
        Assert.Equal("Hotel Three", hotel3.Name);
    }

    [Fact]
    public void GetById_WithEmptyString_ReturnsNull()
    {
        var hotelsFile = CreateHotelsJsonFile(new[]
        {
            CreateHotelData("H1", "Test Hotel", new[] { CreateRoomTypeData("DOUBLE", 2) }, new object[0])
        });

        var repository = new HotelRepository(hotelsFile);
        var hotel = repository.GetById("");

        Assert.Null(hotel);
    }

    [Fact]
    public void GetById_WithWhitespaceId_ReturnsNull()
    {
        var hotelsFile = CreateHotelsJsonFile(new[]
        {
            CreateHotelData("H1", "Test Hotel", new[] { CreateRoomTypeData("DOUBLE", 2) }, new object[0])
        });

        var repository = new HotelRepository(hotelsFile);
        var hotel = repository.GetById("   ");

        Assert.Null(hotel);
    }

    [Fact]
    public void Constructor_LoadsHotelWithCompleteData()
    {
        var hotelsFile = CreateHotelsJsonFile(new[]
        {
            CreateHotelData("H1", "Luxury Hotel", new[]
            {
                CreateRoomTypeData("DOUBLE", 2, "Double Room"),
                CreateRoomTypeData("SINGLE", 1, "Single Room")
            }, new[]
            {
                CreateRoomData("R101", "DOUBLE"),
                CreateRoomData("R102", "SINGLE")
            })
        });

        var repository = new HotelRepository(hotelsFile);
        var hotel = repository.GetById("H1");

        Assert.NotNull(hotel);
        Assert.Equal("Luxury Hotel", hotel.Name);
        Assert.Equal(2, hotel.RoomTypes.Length);
        Assert.Equal(2, hotel.Rooms.Length);
    }

    [Fact]
    public void Constructor_WithLargeDataset_LoadsSuccessfully()
    {
        var hotels = Enumerable.Range(1, 50)
            .Select(i => CreateHotelData($"H{i}", $"Hotel {i}", new[]
            {
                CreateRoomTypeData("DOUBLE", 2),
                CreateRoomTypeData("SINGLE", 1)
            }, Enumerable.Range(1, 5)
                .Select(j => CreateRoomData($"R{i}{j:D2}", j % 2 == 0 ? "DOUBLE" : "SINGLE"))
                .ToArray()))
            .ToArray();

        var hotelsFile = CreateHotelsJsonFile(hotels);

        var repository = new HotelRepository(hotelsFile);

        Assert.Equal(50, repository.GetAll().Count);
        Assert.NotNull(repository.GetById("H1"));
        Assert.NotNull(repository.GetById("H50"));
    }

    // Helper methods
    private string CreateHotelsJsonFile(object[] hotels)
    {
        var filePath = Path.Combine(_testDataDirectory, $"hotels_{Guid.NewGuid()}.json");
        var json = JsonSerializer.Serialize(hotels, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
        return filePath;
    }

    private object CreateHotelData(string id, string name, object[] roomTypes, object[] rooms)
    {
        return new { id, name, roomTypes, rooms };
    }

    private object CreateRoomTypeData(string code, int size, string description = "Test Room")
    {
        return new
        {
            code,
            size,
            description,
            amenities = new[] { "WiFi" },
            features = new[] { "Non-smoking" }
        };
    }

    private object CreateRoomData(string roomId, string roomTypeCode)
    {
        return new { roomId, roomTypeCode };
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDataDirectory))
        {
            Directory.Delete(_testDataDirectory, recursive: true);
        }
    }
}
