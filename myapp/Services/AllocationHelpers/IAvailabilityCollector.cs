using System.Collections.Generic;
using myapp.Models;
using myapp.Services.Interfaces;

namespace myapp.Services.AllocationHelpers
{
    public interface IAvailabilityCollector
    {
        List<(RoomType Rt, int Available)> Collect(Hotel hotel, DateRange range, IAvailabilityService availService);
    }
}
