namespace myapp.Models
{
 public record Hotel(string Id, string Name, RoomType[] RoomTypes, Room[] Rooms)
    {
        public int GetRoomCountForType(string roomTypeCode) =>
            Rooms.Count(r => string.Equals(r.RoomTypeCode, roomTypeCode, StringComparison.OrdinalIgnoreCase));

        public RoomType GetRoomType(string code) =>
            RoomTypes.FirstOrDefault(rt => string.Equals(rt.Code, code, StringComparison.OrdinalIgnoreCase));
    }
}
