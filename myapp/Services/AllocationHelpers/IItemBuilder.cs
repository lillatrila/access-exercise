using System.Collections.Generic;
using myapp.Models;

namespace myapp.Services.AllocationHelpers
{
    public interface IItemBuilder
    {
        List<(string TypeCode, int Count, int Capacity)> Build(List<(RoomType Rt, int Available)> typeAvail);
    }
}
