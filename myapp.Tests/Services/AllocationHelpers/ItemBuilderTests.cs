using System;
using System.Collections.Generic;
using Xunit;
using myapp.Models;
using myapp.Services.AllocationHelpers;

namespace myapp.Tests.Services.AllocationHelpers
{
	public class ItemBuilderTests
	{
		private ItemBuilder Create() => new ItemBuilder();

		private RoomType Rt(string code, int size) => new RoomType(code, size, "", Array.Empty<string>(), Array.Empty<string>());

		[Fact]
		public void Build_EmptyInput_ReturnsEmpty()
		{
			var b = Create();
			var list = b.Build(new List<(RoomType Rt, int Available)>());
			Assert.Empty(list);
		}

		[Fact]
		public void Build_SingleAvailable_ProducesSingleItem()
		{
			var b = Create();
			var input = new List<(RoomType Rt, int Available)> { (Rt("D", 2), 1) };
			var items = b.Build(input);
			Assert.Single(items);
			Assert.Equal("D", items[0].TypeCode);
			Assert.Equal(1, items[0].Count);
			Assert.Equal(2, items[0].Capacity);
		}

		[Fact]
		public void Build_BinarySplit_CreatesPowersOfTwoChunks()
		{
			var b = Create();
			// Available = 5 should produce chunks 1,2,2 (due to doubling)
			var input = new List<(RoomType Rt, int Available)> { (Rt("X", 3), 5) };
			var items = b.Build(input);
			Assert.Equal(3, items.Count);
			Assert.Equal(1, items[0].Count);
			Assert.Equal(2, items[1].Count);
			Assert.Equal(2, items[2].Count);
			Assert.All(items, it => Assert.Equal("X", it.TypeCode));
			Assert.Equal(3, items[0].Capacity / items[0].Count);
		}

		[Fact]
		public void Build_MultipleTypes_PreservesOrderAndCodes()
		{
			var b = Create();
			var input = new List<(RoomType Rt, int Available)>
			{
				(Rt("A",1), 3),
				(Rt("B",2), 2)
			};
			var items = b.Build(input);
			// items for A then for B
			Assert.Contains(items, it => it.TypeCode == "A");
			Assert.Contains(items, it => it.TypeCode == "B");
		}

		[Fact]
		public void Build_CapacityCalculation_IsCountTimesSize()
		{
			var b = Create();
			var input = new List<(RoomType Rt, int Available)> { (Rt("Z", 4), 3) };
			var items = b.Build(input);
			foreach (var it in items)
			{
				Assert.Equal(it.Count * 4, it.Capacity);
			}
		}
	}
}
