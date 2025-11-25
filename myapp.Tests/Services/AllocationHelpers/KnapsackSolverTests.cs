using System;
using System.Collections.Generic;
using Xunit;
using myapp.Models;
using myapp.Services.AllocationHelpers;

namespace myapp.Tests.Services.AllocationHelpers
{
	public class KnapsackSolverTests
	{
		private KnapsackSolver Create() => new KnapsackSolver();

		private Hotel CreateHotel(params (string code, int size)[] types)
		{
			var rts = new List<RoomType>();
			var rooms = new List<Room>();
			int id = 1;
			foreach (var t in types)
			{
				rts.Add(new RoomType(t.code, t.size, "", Array.Empty<string>(), Array.Empty<string>()));
				rooms.Add(new Room($"R{id}", t.code));
				id++;
			}
			return new Hotel("H1", "Test", rts.ToArray(), rooms.ToArray());
		}

		[Fact]
		public void Solve_SimpleCombination_FindsSolution()
		{
			var solver = Create();
			var hotel = CreateHotel(("DOUBLE", 2), ("SINGLE", 1));
			var items = new List<(string TypeCode, int Count, int Capacity)>
			{
				("DOUBLE", 1, 2),
				("SINGLE", 1, 1)
			};

			var (success, bestCap, parent, err) = solver.Solve(items, hotel, 3);

			Assert.True(success);
			Assert.Null(err);
			Assert.InRange(bestCap, 3, 4); // should cover exact 3 (2+1) or clamped 3/4
			Assert.True(parent.Length > 0);
			Assert.Equal((-1, -1), parent[0]);
		}

		[Fact]
		public void Solve_NoItems_ReturnsFailure()
		{
			var solver = Create();
			var hotel = CreateHotel(("SINGLE", 1));
			var items = new List<(string TypeCode, int Count, int Capacity)>();

			var (success, bestCap, parent, err) = solver.Solve(items, hotel, 1);

			Assert.False(success);
			Assert.NotNull(err);
		}

		[Fact]
		public void Solve_PrefersFewerRooms_WhenAvailable()
		{
			var solver = Create();
			var hotel = CreateHotel(("BIG", 3), ("S", 1));
			var items = new List<(string TypeCode, int Count, int Capacity)>
			{
				("BIG", 1, 3),
				("S1", 1, 1),
				("S2", 1, 1),
				("S3", 1, 1)
			};

			var (success, bestCap, parent, err) = solver.Solve(items, hotel, 3);

			Assert.True(success);
			// prefer the single BIG item (1 room) over 3 smalls (3 rooms)
			Assert.Equal(3, bestCap);
		}

		[Fact]
		public void Solve_UsesMultipleItems_ToReachTarget()
		{
			var solver = Create();
			var hotel = CreateHotel(("A", 2), ("B", 2));
			var items = new List<(string TypeCode, int Count, int Capacity)>
			{
				("A", 1, 2),
				("B", 1, 2)
			};

			var (success, bestCap, parent, err) = solver.Solve(items, hotel, 3);

			Assert.True(success);
			Assert.Equal(4, bestCap);
		}

		[Fact]
		public void Solve_ParentArray_HasExpectedLengthAndSentinel()
		{
			var solver = Create();
			var hotel = CreateHotel(("S", 1));
			var items = new List<(string TypeCode, int Count, int Capacity)>
			{
				("S", 1, 1)
			};

			int numPeople = 5;
			int maxRoomSize = 1; // from hotel
			int expectedLen = numPeople + maxRoomSize + 1;

			var (success, bestCap, parent, err) = solver.Solve(items, hotel, numPeople);

			Assert.False(success);
			Assert.Equal(expectedLen, parent.Length);
			Assert.Equal((-1, -1), parent[0]);
		}
	}
}

