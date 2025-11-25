using System.Collections.Generic;
using myapp.Models;
using myapp.Services.AllocationHelpers.Interfaces;

namespace myapp.Services.AllocationHelpers
{
    internal class ItemBuilder : IItemBuilder
    {
        public List<(string TypeCode, int Count, int Capacity)> Build(List<(RoomType Rt, int Available)> typeAvail)
        {
            var items = new List<(string TypeCode, int Count, int Capacity)>();
            foreach (var t in typeAvail)
            {
                int remain = t.Available;
                int k = 1;
                while (remain > 0)
                {
                    int take = System.Math.Min(k, remain);
                    items.Add((t.Rt.Code, take, take * t.Rt.Size));
                    remain -= take;
                    k <<= 1;
                }
            }
            return items;
        }
    }
}
