using myapp.Models;
using myapp.Utils;

public class BookingRepository : IBookingRepository
    {
        private readonly List<Booking> _bookings = new();

        public BookingRepository(string bookingsFilePath)
        {
            if (!File.Exists(bookingsFilePath))
                throw new FileNotFoundException($"Bookings file not found: {bookingsFilePath}");

            var loaded = JsonFileLoader.Load<List<Booking>>(bookingsFilePath) ?? new List<Booking>();

            foreach (var b in loaded)
            {
                if (string.IsNullOrWhiteSpace(b.HotelId))
                    throw new Exception("Booking missing hotelId.");
                if (string.IsNullOrWhiteSpace(b.Arrival) || string.IsNullOrWhiteSpace(b.Departure))
                    throw new Exception($"Booking for hotel {b.HotelId} missing dates.");
                    
                var a = b.ParseArrival();
                var d = b.ParseDeparture();
                if (d <= a) throw new Exception($"Booking for hotel {b.HotelId} has departure <= arrival.");
                _bookings.Add(b);
            }
        }

        public IReadOnlyList<Booking> GetAll() => _bookings;
    }
