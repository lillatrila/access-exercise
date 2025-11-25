using myapp.Models;
using myapp.Services.Interfaces;

namespace myapp.Commands
{ 
public class AvailabilityCommandHandler
    {
        private readonly IHotelRepository _hotelRepo;
        private readonly IAvailabilityService _availability;

        public AvailabilityCommandHandler(IHotelRepository hotelRepo, IAvailabilityService availability)
        {
            _hotelRepo = hotelRepo;
            _availability = availability;
        }

        public string Execute(string hotelId, DateRange range, string roomType)
        {
            var hotel = _hotelRepo.GetById(hotelId);
            if (hotel == null) return $"Error: unknown hotel '{hotelId}'.";

            var rt = hotel.GetRoomType(roomType);
            if (rt == null) return $"Error: unknown room type '{roomType}' for hotel '{hotelId}'.";

            int avail = _availability.GetAvailability(hotel, range, rt.Code);
            return avail.ToString();
        }
    }
}