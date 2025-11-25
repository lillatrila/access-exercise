using myapp.Models;
using myapp.Services.Interfaces;

namespace myapp.Services.AllocationHelpers
{
    internal class InputValidator : IInputValidator
    {
        public (bool ok, AllocationResult? errorResult) Validate(Hotel? hotel, int numPeople)
        {
            if (hotel == null) return (false, new AllocationResult(false, "Unknown hotel", null));
            if (numPeople <= 0) return (false, new AllocationResult(false, "numPeople must be > 0", null));
            return (true, null);
        }
    }
}
