using myapp.Models;

public interface IBookingRepository
    {
        IReadOnlyList<Booking> GetAll();
    }