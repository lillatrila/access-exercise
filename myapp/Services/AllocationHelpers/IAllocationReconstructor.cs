using System.Collections.Generic;
using myapp.Models;
using myapp.Services.Interfaces;

namespace myapp.Services.AllocationHelpers
{
    public interface IAllocationReconstructor
    {
        Dictionary<string, int> Reconstruct((int prevCap, int itemIndex)[] parent, List<(string TypeCode, int Count, int Capacity)> items, int bestCap);
        List<AllocatedRoom>? BuildAllocatedRooms(Hotel hotel, Dictionary<string, int> chosenCounts, int numPeople);
    }
}
