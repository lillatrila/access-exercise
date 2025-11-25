using myapp.Commands;

public class AppEngine
    {
        private readonly AvailabilityCommandHandler _availabilityHandler;
        private readonly RoomTypesCommandHandler _roomTypesHandler;

        public AppEngine(
            AvailabilityCommandHandler availabilityHandler,
            RoomTypesCommandHandler roomTypesHandler)
        {
            _availabilityHandler = availabilityHandler;
            _roomTypesHandler = roomTypesHandler;
        }

        public void RunInteractiveLoop()
        {
            Console.WriteLine("Type commands. Blank line to exit. Examples:");
            Console.WriteLine("  Availability(H1, 20240901, SGL)");
            Console.WriteLine("  Availability(H1, 20240901-20240903, DBL)");
            Console.WriteLine("  RoomTypes(H1, 20240904, 3)");
            Console.WriteLine();

            while (true)
            {
                Console.Write("> ");
                string? line = Console.ReadLine();
                if (line == null) break;
                if (string.IsNullOrWhiteSpace(line)) break;

                if (CommandParser.TryParseAvailability(line, out var a))
                {
                    try
                    {
                        var outStr = _availabilityHandler.Execute(a.hotelId, a.range, a.roomType);
                        Console.WriteLine(outStr);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                    continue;
                }

                if (CommandParser.TryParseRoomTypes(line, out var r))
                {
                    try
                    {
                        var outStr = _roomTypesHandler.Execute(r.hotelId, r.range, r.numPeople);
                        Console.WriteLine(outStr);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                    continue;
                }

                Console.WriteLine("Error: unrecognized command or bad format.");
            }

            Console.WriteLine("Exiting.");
        }
    }