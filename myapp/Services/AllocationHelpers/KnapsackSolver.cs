using System;
using System.Collections.Generic;
using System.Linq;
using myapp.Models;

namespace myapp.Services.AllocationHelpers
{
    internal class KnapsackSolver : IKnapsackSolver
    {
        public (bool success, int bestCap, (int prevCap, int itemIndex)[] parent, string? errorMessage) Solve(List<(string TypeCode, int Count, int Capacity)> items, Hotel hotel, int numPeople)
        {
            int maxRoomSize = hotel.RoomTypes.Max(rt => rt.Size);
            int capMax = numPeople + maxRoomSize;
            var INF = int.MaxValue / 4;
            var dp = Enumerable.Repeat(INF, capMax + 1).ToArray();
            var parent = new (int prevCap, int itemIndex)[capMax + 1];
            dp[0] = 0;
            parent[0] = (-1, -1);

            for (int idx = 0; idx < items.Count; idx++)
            {
                var it = items[idx];
                int w = it.Capacity;
                int cost = it.Count;
                for (int cap = capMax; cap >= 0; cap--)
                {
                    if (dp[cap] == INF) continue;
                    int newCap = Math.Min(capMax, cap + w);
                    int newRooms = dp[cap] + cost;
                    if (newRooms < dp[newCap])
                    {
                        dp[newCap] = newRooms;
                        parent[newCap] = (cap, idx);
                    }
                }
            }

            int bestCap = -1;
            int bestRooms = INF;
            for (int cap = numPeople; cap <= capMax; cap++)
            {
                if (dp[cap] < bestRooms)
                {
                    bestRooms = dp[cap];
                    bestCap = cap;
                }
                else if (dp[cap] == bestRooms && bestRooms != INF && bestCap != -1)
                {
                    if (cap < bestCap) bestCap = cap;
                }
            }

            if (bestCap == -1 || bestRooms == INF)
                return (false, -1, parent, "Unable to find an allocation (unexpected).");

            return (true, bestCap, parent, null);
        }
    }
}
