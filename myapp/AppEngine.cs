using myapp.Commands;
using myapp.Utils;

public class AppEngine
    {
        private readonly AvailabilityCommandHandler _availabilityHandler;
        private readonly RoomTypesCommandHandler _roomTypesHandler;
        private readonly IConsoleHelpers _console;

        public AppEngine(
            AvailabilityCommandHandler availabilityHandler,
            RoomTypesCommandHandler roomTypesHandler,
            IConsoleHelpers console)
        {
            _availabilityHandler = availabilityHandler;
            _roomTypesHandler = roomTypesHandler;
            _console = console;
        }

        public void RunInteractiveLoop()
        {
            _console.WriteLine("Type commands. Blank line to exit. Examples:");
            _console.WriteLine("  Availability(H1, 20240901, SGL)");
            _console.WriteLine("  Availability(H1, 20240901-20240903, DBL)");
            _console.WriteLine("  RoomTypes(H1, 20240904, 3)");
            _console.WriteLine();

            while (true)
            {
                _console.Write("> ");
                string? line = _console.ReadLine();
                if (line == null) break;
                if (string.IsNullOrWhiteSpace(line)) break;

                if (CommandParser.TryParseAvailability(line, out var a))
                {
                    try
                    {
                        var outStr = _availabilityHandler.Execute(a.hotelId, a.range, a.roomType);
                        _console.WriteLine(outStr);
                    }
                    catch (Exception ex)
                    {
                        _console.WriteLine("Error: " + ex.Message);
                    }
                    continue;
                }

                if (CommandParser.TryParseRoomTypes(line, out var r))
                {
                    try
                    {
                        var outStr = _roomTypesHandler.Execute(r.hotelId, r.range, r.numPeople);
                        _console.WriteLine(outStr);
                    }
                    catch (Exception ex)
                    {
                        _console.WriteLine("Error: " + ex.Message);
                    }
                    continue;
                }

                _console.WriteLine("Error: unrecognized command or bad format.");
            }

            _console.WriteLine("Exiting.");
        }
    }