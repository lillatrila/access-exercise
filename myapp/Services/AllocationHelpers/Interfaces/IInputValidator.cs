using myapp.Models;
using myapp.Services.Interfaces;

namespace myapp.Services.AllocationHelpers.Interfaces
{
    public interface IInputValidator
    {
        (bool ok, AllocationResult? errorResult) Validate(Hotel? hotel, int numPeople);
    }
}
