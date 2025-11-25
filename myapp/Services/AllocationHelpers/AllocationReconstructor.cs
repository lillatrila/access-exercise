using System;
using System.Collections.Generic;
using System.Linq;
using myapp.Models;
using myapp.Services.AllocationHelpers.Interfaces;
using myapp.Services.Interfaces;

namespace myapp.Services.AllocationHelpers
{
    internal class AllocationReconstructor : IAllocationReconstructor
    {
        public Dictionary<string, int> Reconstruct((int prevCap, int itemIndex)[] parent, List<(string TypeCode, int Count, int Capacity)> items, int bestCap)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (bestCap < 0 || bestCap >= parent.Length) throw new ArgumentOutOfRangeException(nameof(bestCap));

            var chosenCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int cur = bestCap;
            while (cur > 0)
            {
                var p = parent[cur];
                // sentinel check - no more parents
                if (p.prevCap == -1 && p.itemIndex == -1) break;
                if (p.itemIndex < 0 || p.itemIndex >= items.Count) throw new InvalidOperationException("Invalid itemIndex in parent array.");

                var item = items[p.itemIndex];
                if (!chosenCounts.ContainsKey(item.TypeCode)) chosenCounts[item.TypeCode] = 0;
                chosenCounts[item.TypeCode] += item.Count;
                cur = p.prevCap;
            }
            return chosenCounts;
        }

        public List<AllocatedRoom>? BuildAllocatedRooms(Hotel hotel, Dictionary<string, int> chosenCounts, int numPeople)
        {
            if (hotel == null) throw new ArgumentNullException(nameof(hotel));
            if (chosenCounts == null) throw new ArgumentNullException(nameof(chosenCounts));
            if (numPeople < 0) throw new ArgumentOutOfRangeException(nameof(numPeople));

            if (numPeople == 0) return new List<AllocatedRoom>();

            var roomInstances = new List<(string TypeCode, int Capacity)>();
            foreach (var kv in chosenCounts)
            {
                var rt = hotel.GetRoomType(kv.Key);
                if (rt == null)
                {
                    // chosenCounts references a missing room type — fail explicitly
                    return null;
                }
                int count = kv.Value;
                for (int i = 0; i < count; i++)
                    roomInstances.Add((kv.Key, rt.Size));
            }

            // allocate largest rooms first
            roomInstances.Sort((a, b) => b.Capacity.CompareTo(a.Capacity));

            int remainingPeople = numPeople;
            var allocated = new List<AllocatedRoom>();
            foreach (var room in roomInstances)
            {
                if (remainingPeople <= 0)
                {
                    // stop adding further rooms once requirement is satisfied
                    break;
                }

                if (remainingPeople >= room.Capacity)
                {
                    // full room used
                    allocated.Add(new AllocatedRoom(room.TypeCode, false));
                    remainingPeople -= room.Capacity;
                }
                else
                {
                    // partially used last room
                    allocated.Add(new AllocatedRoom(room.TypeCode, true));
                    remainingPeople = 0;
                }
            }

            if (remainingPeople > 0) return null;
            return allocated;
        }
    }
}
