using myapp.Models;

public interface IHotelRepository
    {
        IReadOnlyList<Hotel> GetAll();
        Hotel GetById(string id);
    }