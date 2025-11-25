using myapp.Models;
using myapp.Services;
using myapp.Services.Interfaces;

namespace myapp.Commands
{
public class RoomTypesCommandHandler
    {
        private readonly IHotelRepository _hotelRepo;
        private readonly IAllocationService _alloc;

        public RoomTypesCommandHandler(IHotelRepository hotelRepo, IAllocationService alloc)
        {
            _hotelRepo = hotelRepo;
            _alloc = alloc;
        }

        public string Execute(string hotelId, DateRange range, int numPeople)
        {
            var hotel = _hotelRepo.GetById(hotelId);
            if (hotel == null) return $"Error: unknown hotel '{hotelId}'.";

            var res = _alloc.Allocate(hotel, range, numPeople);
            if (!res.Success) return $"Error: {res.ErrorMessage}";

            var roomsList = res.Rooms ?? new List<AllocatedRoom>();
            var parts = roomsList.Select(r => r.RoomTypeCode + (r.IsPartial ? "!" : "")).ToArray();
            return $"{hotel.Id}: {string.Join(", ", parts)}";
        }
    }
}