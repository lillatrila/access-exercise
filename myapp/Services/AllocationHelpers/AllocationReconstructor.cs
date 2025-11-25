using System.Collections.Generic;
using System.Linq;
using myapp.Models;
using myapp.Services.Interfaces;

namespace myapp.Services.AllocationHelpers
{
    internal class AllocationReconstructor : IAllocationReconstructor
    {
        public Dictionary<string, int> Reconstruct((int prevCap, int itemIndex)[] parent, List<(string TypeCode, int Count, int Capacity)> items, int bestCap)
        {
            var chosenCounts = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
            int cur = bestCap;
            while (cur > 0)
            {
                var p = parent[cur];
                if (p.prevCap == -1 && p.itemIndex == -1) break;
                var item = items[p.itemIndex];
                if (!chosenCounts.ContainsKey(item.TypeCode)) chosenCounts[item.TypeCode] = 0;
                chosenCounts[item.TypeCode] += item.Count;
                cur = p.prevCap;
            }
            return chosenCounts;
        }

        public List<AllocatedRoom>? BuildAllocatedRooms(Hotel hotel, Dictionary<string, int> chosenCounts, int numPeople)
        {
            var roomInstances = new List<(string TypeCode, int Capacity)>();
            foreach (var kv in chosenCounts)
            {
                var rt = hotel.GetRoomType(kv.Key);
                if (rt == null) continue;
                int count = kv.Value;
                for (int i = 0; i < count; i++)
                    roomInstances.Add((kv.Key, rt.Size));
            }

            roomInstances.Sort((a, b) => b.Capacity.CompareTo(a.Capacity));

            int remainingPeople = numPeople;
            var allocated = new List<AllocatedRoom>();
            foreach (var room in roomInstances)
            {
                if (remainingPeople <= 0)
                {
                    allocated.Add(new AllocatedRoom(room.TypeCode, true));
                }
                else if (remainingPeople >= room.Capacity)
                {
                    allocated.Add(new AllocatedRoom(room.TypeCode, false));
                    remainingPeople -= room.Capacity;
                }
                else
                {
                    allocated.Add(new AllocatedRoom(room.TypeCode, true));
                    remainingPeople = 0;
                }
            }

            if (remainingPeople > 0) return null;
            return allocated;
        }
    }
}
