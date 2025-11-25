using myapp.Models;
namespace myapp.Services.Interfaces
{
public interface IAvailabilityService
    {
        int GetAvailability(Hotel hotel, DateRange range, string roomTypeCode);
    }
}