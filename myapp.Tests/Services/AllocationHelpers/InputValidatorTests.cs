using System;
using Xunit;
using myapp.Services.AllocationHelpers;
using myapp.Models;
using myapp.Services.Interfaces;
using System.Collections.Generic;

namespace myapp.Tests.Services.AllocationHelpers
{
    public class InputValidatorTests
    {
        private InputValidator Create() => new InputValidator();

        [Fact]
        public void Validate_ReturnsFalse_WhenHotelIsNull()
        {
            var v = Create();
            var (ok, err) = v.Validate(null, 1);
            Assert.False(ok);
            Assert.NotNull(err);
            Assert.False(err.Success);
            Assert.Equal("Unknown hotel", err.ErrorMessage);
        }

        [Fact]
        public void Validate_ReturnsFalse_WhenNumPeopleIsZero()
        {
            var v = Create();
            var hotel = new Hotel("H","N", Array.Empty<RoomType>(), Array.Empty<Room>());
            var (ok, err) = v.Validate(hotel, 0);
            Assert.False(ok);
            Assert.NotNull(err);
            Assert.Equal("numPeople must be > 0", err.ErrorMessage);
        }

        [Fact]
        public void Validate_ReturnsFalse_WhenNumPeopleIsNegative()
        {
            var v = Create();
            var hotel = new Hotel("H","N", Array.Empty<RoomType>(), Array.Empty<Room>());
            var (ok, err) = v.Validate(hotel, -3);
            Assert.False(ok);
            Assert.NotNull(err);
            Assert.Equal("numPeople must be > 0", err.ErrorMessage);
        }

        [Fact]
        public void Validate_ReturnsTrue_ForValidInputs()
        {
            var v = Create();
            var hotel = new Hotel("H","N", new[] { new RoomType("T", 2, "", Array.Empty<string>(), Array.Empty<string>()) }, new[] { new Room("R1", "T") });
            var (ok, err) = v.Validate(hotel, 2);
            Assert.True(ok);
            Assert.Null(err);
        }
    }
}
