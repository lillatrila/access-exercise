using myapp.Models;
using myapp.Utils;

public class HotelRepository : IHotelRepository
    {
        private readonly List<Hotel> _hotels = new();

        public HotelRepository(string hotelsFilePath)
        {
            if (!File.Exists(hotelsFilePath))
                throw new FileNotFoundException($"Hotels file not found: {hotelsFilePath}");

            var loaded = JsonFileLoader.Load<List<Hotel>>(hotelsFilePath) ?? new List<Hotel>();
            // Basic normalization & validation
            foreach (var h in loaded)
            {
                if (string.IsNullOrWhiteSpace(h.Id))
                    throw new Exception("Hotel missing id.");
                if (h.RoomTypes == null) throw new Exception($"Hotel {h.Id} missing roomTypes.");
                if (h.Rooms == null) throw new Exception($"Hotel {h.Id} missing rooms.");
                // Normalize room type codes to uppercase
                foreach (var rt in h.RoomTypes)
                {
                    if (string.IsNullOrWhiteSpace(rt.Code))
                        throw new Exception($"Hotel {h.Id} has a room type missing code.");
                }

                _hotels.Add(h);
            }
        }

        public IReadOnlyList<Hotel> GetAll() => _hotels;
        public Hotel GetById(string id) =>
            _hotels.FirstOrDefault(h => string.Equals(h.Id, id, StringComparison.OrdinalIgnoreCase));
    }