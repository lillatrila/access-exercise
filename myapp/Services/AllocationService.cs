using myapp.Models;
using myapp.Services.Interfaces;
using myapp.Services.AllocationHelpers;
using myapp.Services.AllocationHelpers.Interfaces;

namespace myapp.Services
{
    public class AllocationService : IAllocationService
    {
        private readonly IAvailabilityService _availService;
        private readonly IInputValidator _inputValidator;
        private readonly IAvailabilityCollector _availabilityCollector;
        private readonly IItemBuilder _itemBuilder;
        private readonly IKnapsackSolver _knapsackSolver;
        private readonly IAllocationReconstructor _reconstructor;

        public AllocationService(
            IAvailabilityService availService,
            IInputValidator inputValidator,
            IAvailabilityCollector availabilityCollector,
            IItemBuilder itemBuilder,
            IKnapsackSolver knapsackSolver,
            IAllocationReconstructor reconstructor)
        {
            _availService = availService;
            _inputValidator = inputValidator;
            _availabilityCollector = availabilityCollector;
            _itemBuilder = itemBuilder;
            _knapsackSolver = knapsackSolver;
            _reconstructor = reconstructor;
        }

        public AllocationResult Allocate(Hotel hotel, DateRange range, int numPeople)
        {
            var validation = _inputValidator.Validate(hotel, numPeople);
            if (!validation.ok) return validation.errorResult!;

            var typeAvail = _availabilityCollector.Collect(hotel, range, _availService);

            long maxCapacity = typeAvail.Sum(t => (long)t.Rt.Size * t.Available);
            if (maxCapacity < numPeople)
                return new AllocationResult(false, "Not enough capacity available to allocate the requested number of people.", null);

            var items = _itemBuilder.Build(typeAvail);

            var dpResult = _knapsackSolver.Solve(items, hotel, numPeople);
            if (!dpResult.success) return new AllocationResult(false, dpResult.errorMessage ?? "Unable to find an allocation (unexpected).", null);

            var chosenCounts = _reconstructor.Reconstruct(dpResult.parent, items, dpResult.bestCap);

            var allocated = _reconstructor.BuildAllocatedRooms(hotel, chosenCounts, numPeople);
            if (allocated == null) return new AllocationResult(false, "Allocation failed to place all people (unexpected).", null);

            return new AllocationResult(true, null, allocated);
        }
    }
}