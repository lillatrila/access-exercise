using System.Collections.Generic;
using myapp.Models;

namespace myapp.Services.AllocationHelpers
{
    public interface IKnapsackSolver
    {
        (bool success, int bestCap, (int prevCap, int itemIndex)[] parent, string? errorMessage) Solve(List<(string TypeCode, int Count, int Capacity)> items, Hotel hotel, int numPeople);
    }
}
