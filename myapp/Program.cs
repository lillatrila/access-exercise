using myapp.Commands;
using Microsoft.Extensions.DependencyInjection;
using myapp.Services;
using myapp.Services.Interfaces;
using myapp.Services.AllocationHelpers.Interfaces;
using myapp.Services.AllocationHelpers;

public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage: dotnet run -- --hotels data/hotels.json --bookings data/bookings.json");
                    return;
                }
                var parsed = ParseArgs(args);
                if (!File.Exists(parsed.HotelsFile))
                {
                    Console.WriteLine($"Hotels file not found: {parsed.HotelsFile}");
                    return;
                }
                if (!File.Exists(parsed.BookingsFile))
                {
                    Console.WriteLine($"Bookings file not found: {parsed.BookingsFile}");
                    return;
                }

                 // Set up the DI container
                var services = new ServiceCollection();
                ConfigureServices(services, parsed.HotelsFile, parsed.BookingsFile);
                var serviceProvider = services.BuildServiceProvider();

                // Get AppEngine from the container
                var app = serviceProvider.GetRequiredService<AppEngine>();
                app.RunInteractiveLoop();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error: " + ex.Message);
            }
        }

        private static void ConfigureServices(IServiceCollection services, string hotelsFile, string bookingsFile)
        {
             // Register repositories with interfaces
            services.AddSingleton<IHotelRepository>(new HotelRepository(hotelsFile));
            services.AddSingleton<IBookingRepository>(new BookingRepository(bookingsFile));

            // Register services with interfaces
            services.AddSingleton(sp => new BookingsIndex(sp.GetRequiredService<IBookingRepository>().GetAll()));
            services.AddSingleton<IAvailabilityService, AvailabilityService>();
            services.AddSingleton<IAllocationService, AllocationService>();

            // Allocation helper services (transient, stateless)
            services.AddTransient<IInputValidator, InputValidator>();
            services.AddTransient<IAvailabilityCollector, AvailabilityCollector>();
            services.AddTransient<IItemBuilder, ItemBuilder>();
            services.AddTransient<IKnapsackSolver, KnapsackSolver>();
            services.AddTransient<IAllocationReconstructor, AllocationReconstructor>();

            // Register command handlers
            services.AddSingleton<AvailabilityCommandHandler>();
            services.AddSingleton<RoomTypesCommandHandler>();

            // Register AppEngine
            services.AddSingleton<AppEngine>();
        }

        private static (string HotelsFile, string BookingsFile) ParseArgs(string[] args)
        {
            string? hotels = null;
            string? bookings = null;
            for (int i = 0; i < args.Length; i++)
            {
                var a = args[i];
                if (a == "--hotels" && i + 1 < args.Length) hotels = args[++i];
                else if (a == "--bookings" && i + 1 < args.Length) bookings = args[++i];
            }
            if (hotels == null || bookings == null)
                throw new ArgumentException("Missing required args. Usage: --hotels hotels.json --bookings bookings.json");
            return (hotels, bookings);
        }
    }