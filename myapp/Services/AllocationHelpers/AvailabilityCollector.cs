using System.Collections.Generic;
using myapp.Models;
using myapp.Services.AllocationHelpers.Interfaces;
using myapp.Services.Interfaces;

namespace myapp.Services.AllocationHelpers
{
    internal class AvailabilityCollector : IAvailabilityCollector
    {
        public List<(RoomType Rt, int Available)> Collect(Hotel hotel, DateRange range, IAvailabilityService availService)
        {
            var list = new List<(RoomType Rt, int Available)>();
            foreach (var rt in hotel.RoomTypes)
            {
                var code = rt.Code.ToUpperInvariant();
                var avail = availService.GetAvailability(hotel, range, code);
                if (avail < 0) avail = 0;
                list.Add((rt with { Code = rt.Code.ToUpperInvariant() }, avail));
            }
            return list;
        }
    }
}
