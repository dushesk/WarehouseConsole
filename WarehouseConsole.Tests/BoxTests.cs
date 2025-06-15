using Xunit;
using WarehouseConsole;
using System;

namespace WarehouseConsole.Tests
{
    public class BoxTests
    {
        // Тесты на создание коробки
        [Fact]
        public void Constructor_ValidParameters_CreatesBox()
        {
            // Arrange & Act
            var box = new Box(1, 10, 10, 10, 5, DateTime.Today, null);

            // Assert
            Assert.Equal(1, box.Id);
            Assert.Equal(10, box.Width);
            Assert.Equal(1000, box.Volume);
        }

        // Тесты на валидацию ID
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_InvalidId_ThrowsException(int invalidId)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Box(invalidId, 10, 10, 10, 5, DateTime.Today, null));
        }

        // Тесты на валидацию размеров
        [Theory]
        [InlineData(0, 10, 10, 5)]
        [InlineData(10, 0, 10, 5)]
        [InlineData(10, 10, 0, 5)]
        [InlineData(10, 10, 10, 0)]
        public void Constructor_InvalidDimensions_ThrowsException(double w, double h, double d, double weight)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Box(1, w, h, d, weight, DateTime.Today, null));
        }

        // Тесты на даты
        [Fact]
        public void Constructor_WithProductionDate_CalculatesExpiryDate()
        {
            // Arrange
            var productionDate = new DateTime(2023, 1, 1);
            var expectedExpiry = new DateTime(2023, 4, 11); // +100 дней

            // Act
            var box = new Box(1, 10, 10, 10, 5, productionDate, null);

            // Assert
            Assert.Equal(productionDate, box.ProductionDate);
            Assert.Equal(expectedExpiry, box.ExpiryDate);
        }

        [Fact]
        public void Constructor_WithExpiryDate_UsesProvidedDate()
        {
            // Arrange
            var expiryDate = new DateTime(2023, 5, 1);

            // Act
            var box = new Box(1, 10, 10, 10, 5, null, expiryDate);

            // Assert
            Assert.Null(box.ProductionDate);
            Assert.Equal(expiryDate, box.ExpiryDate);
        }

        [Fact]
        public void Constructor_WithTimeInDates_ThrowsException()
        {
            // Arrange
            var dateWithTime = new DateTime(2023, 1, 1, 10, 0, 0);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new Box(1, 10, 10, 10, 5, dateWithTime, null));

            Assert.Throws<ArgumentException>(() =>
                new Box(1, 10, 10, 10, 5, null, dateWithTime));
        }

        // Тест на автоматическую генерацию ID
        [Fact]
        public void Constructor_WithoutId_GeneratesSequentialIds()
        {
            // Сбросим статический счетчик
            var lastIdField = typeof(Box).GetField("_lastId",
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.NonPublic);
            lastIdField.SetValue(null, 0);

            // Act
            var box1 = new Box(10, 10, 10, 5, DateTime.Today, null);
            var box2 = new Box(10, 10, 10, 5, DateTime.Today, null);

            // Assert
            Assert.Equal(1, box1.Id);
            Assert.Equal(2, box2.Id);
        }
    }
}