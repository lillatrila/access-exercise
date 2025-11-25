using myapp.Models;
namespace myapp.Services{

public class BookingsIndex
    {
        private readonly Dictionary<(string hotelId, DateTime date, string roomType), int> _index =
            new();

        public BookingsIndex(IEnumerable<Booking> bookings)
        {
            foreach (var b in bookings)
            {
                var arrival = b.ParseArrival();
                var departure = b.ParseDeparture();
                for (var d = arrival.Date; d < departure.Date; d = d.AddDays(1))
                {
                    var key = (b.HotelId.ToUpperInvariant(), d, b.RoomType.ToUpperInvariant());
                    if (_index.ContainsKey(key)) _index[key]++; else _index[key] = 1;
                }
            }
        }

        public int GetBookedCount(string hotelId, DateTime night, string roomType)
        {
            var key = (hotelId.ToUpperInvariant(), night.Date, roomType.ToUpperInvariant());
            return _index.TryGetValue(key, out var c) ? c : 0;
        }
    }
}