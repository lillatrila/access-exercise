using System;
using System.Collections.Generic;
using Xunit;
using myapp.Models;
using myapp.Services.AllocationHelpers;

namespace myapp.Tests.Services.AllocationHelpers
{
	public class AllocationReconstructorTests
	{
		private AllocationReconstructor Create() => new AllocationReconstructor();

		private Hotel CreateHotel(params (string code, int size)[] types)
		{
			var rts = new List<RoomType>();
			var rooms = new List<Room>();
			int id = 1;
			foreach (var t in types)
			{
				rts.Add(new RoomType(t.code, t.size, "", Array.Empty<string>(), Array.Empty<string>()));
				// create one room instance per type
				rooms.Add(new Room($"R{id}", t.code));
				id++;
			}
			return new Hotel("H1", "Test", rts.ToArray(), rooms.ToArray());
		}

		[Fact]
		public void Reconstruct_ReturnsChosenCounts_ForSimpleParent()
		{
			var recon = Create();
			var items = new List<(string TypeCode, int Count, int Capacity)>
			{
				("DOUBLE", 1, 2)
			};
			var parent = new (int prevCap, int itemIndex)[3];
			parent[0] = (-1, -1); // sentinel
			parent[1] = (-1, -1);
			parent[2] = (0, 0); // came from capacity 0 using item 0

			var result = recon.Reconstruct(parent, items, 2);

			Assert.Single(result);
			Assert.Equal(1, result["DOUBLE"]);
		}

		[Fact]
		public void Reconstruct_StopsAtSentinel()
		{
			var recon = Create();
			var items = new List<(string TypeCode, int Count, int Capacity)>
			{
				("A",1,1), ("B",1,2)
			};
			var parent = new (int prevCap, int itemIndex)[4];
			parent[0] = (-1, -1);
			parent[1] = (0, 0);
			parent[2] = (-1, -1); // sentinel at cap 2
			parent[3] = (2, 1);

			// bestCap points to 3; loop should hit sentinel at cap 2 and stop
			var result = recon.Reconstruct(parent, items, 3);

			Assert.Single(result);
			Assert.Equal(1, result["B"]);
		}

		[Fact]
		public void Reconstruct_Throws_OnInvalidBestCap()
		{
			var recon = Create();
			var items = new List<(string TypeCode, int Count, int Capacity)>();
			var parent = new (int prevCap, int itemIndex)[2];

			Assert.Throws<ArgumentOutOfRangeException>(() => recon.Reconstruct(parent, items, 5));
		}

		[Fact]
		public void BuildAllocatedRooms_ReturnsEmpty_ForZeroPeople()
		{
			var recon = Create();
			var hotel = CreateHotel(("DOUBLE", 2));
			var allocated = recon.BuildAllocatedRooms(hotel, new Dictionary<string,int>(), 0);
			Assert.NotNull(allocated);
			Assert.Empty(allocated!);
		}

		[Fact]
		public void BuildAllocatedRooms_ReturnsPartialRoom_WhenNeeded()
		{
			var recon = Create();
			var hotel = CreateHotel(("DOUBLE", 2));
			var chosen = new Dictionary<string,int> { { "DOUBLE", 1 } };

			var allocated = recon.BuildAllocatedRooms(hotel, chosen, 1);

			Assert.NotNull(allocated);
			Assert.Single(allocated!);
			Assert.True(allocated![0].IsPartial);
			Assert.Equal("DOUBLE", allocated[0].RoomTypeCode);
		}

		[Fact]
		public void BuildAllocatedRooms_ReturnsNull_WhenMissingRoomType()
		{
			var recon = Create();
			var hotel = CreateHotel(("SINGLE", 1));
			var chosen = new Dictionary<string,int> { { "MISSING", 1 } };

			var allocated = recon.BuildAllocatedRooms(hotel, chosen, 1);

			Assert.Null(allocated);
		}

		[Fact]
		public void BuildAllocatedRooms_ReturnsNull_WhenInsufficientCapacity()
		{
			var recon = Create();
			var hotel = CreateHotel(("SINGLE", 1));
			var chosen = new Dictionary<string,int> { { "SINGLE", 1 } };

			// need 2 people but only one single room allocated
			var allocated = recon.BuildAllocatedRooms(hotel, chosen, 2);

			Assert.Null(allocated);
		}
	}
}

