using myapp.Utils;

namespace myapp.Models
{
  public record Booking(string HotelId, string Arrival, string Departure, string RoomType, string RoomRate)
    {
        public DateTime ParseArrival() => DateParser.ParseStrictDate(Arrival);
        public DateTime ParseDeparture() => DateParser.ParseStrictDate(Departure);
    }
}
