using myapp.Models;
using myapp.Services.Interfaces;

namespace myapp.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly BookingsIndex _index;

        public AvailabilityService(BookingsIndex index) { _index = index; }

        public int GetAvailability(Hotel hotel, DateRange range, string roomTypeCode)
        {
            if (hotel == null) throw new ArgumentNullException(nameof(hotel));
            if (string.IsNullOrWhiteSpace(roomTypeCode)) throw new ArgumentNullException(nameof(roomTypeCode));

            var normalized = roomTypeCode.ToUpperInvariant();
            var totalRooms = hotel.GetRoomCountForType(normalized);
            int minAvail = int.MaxValue;
            bool anyNight = false;

            foreach (var night in range.Nights())
            {
                anyNight = true;
                var booked = _index.GetBookedCount(hotel.Id, night, normalized);
                var avail = totalRooms - booked;
                if (avail < minAvail) minAvail = avail;
            }

            if (!anyNight) return 0;
            return minAvail == int.MaxValue ? 0 : minAvail;
        }
    }
}