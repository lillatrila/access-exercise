namespace myapp.Models;

public readonly struct DateRange
    {
        public DateTime Start { get; }
        public DateTime EndExclusive { get; }
        public DateRange(DateTime start, DateTime endExclusive)
        {
            Start = start;
            EndExclusive = endExclusive;
        }

        public IEnumerable<DateTime> Nights()
        {
            for (var dt = Start; dt < EndExclusive; dt = dt.AddDays(1))
                yield return dt.Date;
        }
    }