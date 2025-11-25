using myapp.Models;

namespace myapp.Services.Interfaces
{ 
public interface IAllocationService
{
    AllocationResult Allocate(Hotel hotel, DateRange range, int numPeople);
}

public record AllocatedRoom(string RoomTypeCode, bool IsPartial);

public record AllocationResult(bool Success, string? ErrorMessage, List<AllocatedRoom>? Rooms);
}