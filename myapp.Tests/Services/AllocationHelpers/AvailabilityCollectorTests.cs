using System;
using System.Collections.Generic;
using Xunit;
using myapp.Models;
using myapp.Services.AllocationHelpers;
using myapp.Services.Interfaces;

namespace myapp.Tests.Services.AllocationHelpers
{
    internal class FakeAvailabilityService : IAvailabilityService
    {
        private readonly Dictionary<string, int> _map;
        public readonly List<(Hotel hotel, DateRange range, string code)> Calls = new();

        public FakeAvailabilityService(Dictionary<string,int> map)
        {
            _map = map ?? new Dictionary<string,int>();
        }

        public int GetAvailability(Hotel hotel, DateRange range, string roomTypeCode)
        {
            Calls.Add((hotel, range, roomTypeCode));
            return _map.TryGetValue(roomTypeCode, out var v) ? v : 0;
        }
    }

    public class AvailabilityCollectorTests
    {
        private AvailabilityCollector Create() => new AvailabilityCollector();

        private Hotel CreateHotel(params (string code, int size, string desc)[] types)
        {
            var rts = new List<RoomType>();
            var rooms = new List<Room>();
            int id = 1;
            foreach (var t in types)
            {
                rts.Add(new RoomType(t.code, t.size, t.desc, Array.Empty<string>(), Array.Empty<string>()));
                rooms.Add(new Room($"R{id}", t.code));
                id++;
            }
            return new Hotel("H1", "TestHotel", rts.ToArray(), rooms.ToArray());
        }

        [Fact]
        public void Collect_ReturnsUppercasedCodes_AndAvailability()
        {
            var hotel = CreateHotel(("double", 2, "D"), ("single", 1, "S"));
            var map = new Dictionary<string,int> { { "DOUBLE", 3 }, { "SINGLE", 2 } };
            var fake = new FakeAvailabilityService(map);
            var range = new DateRange(DateTime.Today, DateTime.Today.AddDays(1));

            var collector = Create();
            var list = collector.Collect(hotel, range, fake);

            Assert.Equal(2, list.Count);
            Assert.Contains(list, t => t.Rt.Code == "DOUBLE" && t.Available == 3);
            Assert.Contains(list, t => t.Rt.Code == "SINGLE" && t.Available == 2);
        }

        [Fact]
        public void Collect_ClampsNegativeAvailability_ToZero()
        {
            var hotel = CreateHotel(("x", 2, "X"));
            var map = new Dictionary<string,int> { { "X", -5 } };
            var fake = new FakeAvailabilityService(map);
            var range = new DateRange(DateTime.Today, DateTime.Today.AddDays(1));

            var collector = Create();
            var list = collector.Collect(hotel, range, fake);

            Assert.Single(list);
            Assert.Equal("X", list[0].Rt.Code);
            Assert.Equal(0, list[0].Available);
        }

        [Fact]
        public void Collect_CallsAvailabilityService_WithUppercaseCode_AndGivenRange()
        {
            var hotel = CreateHotel(("lower", 1, "L"));
            var map = new Dictionary<string,int> { { "LOWER", 1 } };
            var fake = new FakeAvailabilityService(map);
            var range = new DateRange(new DateTime(2025,1,1), new DateTime(2025,1,3));

            var collector = Create();
            var list = collector.Collect(hotel, range, fake);

            Assert.Single(fake.Calls);
            var call = fake.Calls[0];
            Assert.Equal(hotel, call.hotel);
            Assert.Equal(range.Start, call.range.Start);
            Assert.Equal(range.EndExclusive, call.range.EndExclusive);
            Assert.Equal("LOWER", call.code);
        }

        [Fact]
        public void Collect_PreservesRoomTypeFields_OtherThanCode()
        {
            var hotel = CreateHotel(("t", 4, "RoomDesc"));
            var map = new Dictionary<string,int> { { "T", 2 } };
            var fake = new FakeAvailabilityService(map);
            var range = new DateRange(DateTime.Today, DateTime.Today.AddDays(1));

            var collector = Create();
            var list = collector.Collect(hotel, range, fake);

            Assert.Single(list);
            var rt = list[0].Rt;
            Assert.Equal("T", rt.Code);
            Assert.Equal(4, rt.Size);
            Assert.Equal("RoomDesc", rt.Description);
        }

        [Fact]
        public void Collect_WithNoRoomTypes_ReturnsEmpty()
        {
            var hotel = new Hotel("H", "Empty", Array.Empty<RoomType>(), Array.Empty<Room>());
            var fake = new FakeAvailabilityService(new Dictionary<string,int>());
            var range = new DateRange(DateTime.Today, DateTime.Today.AddDays(1));

            var collector = Create();
            var list = collector.Collect(hotel, range, fake);

            Assert.Empty(list);
        }
    }
}
