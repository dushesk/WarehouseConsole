using Xunit;
using WarehouseConsole;
using System;

namespace WarehouseConsole.Tests
{
    public class PalletTests
    {
        [Fact]
        public void Pallet_VolumeCalculation_IncludesBoxesAndOwnVolume()
        {
            // Arrange
            var pallet = new Pallet(1, 100, 100, 100);
            // Используем .Date для удаления времени
            var box1 = new Box(1, 10, 10, 10, 5, null, DateTime.Now.AddDays(100).Date);
            var box2 = new Box(2, 20, 20, 20, 5, null, DateTime.Now.AddDays(200).Date);

            pallet.AddBox(box1);
            pallet.AddBox(box2);

            var expectedVolume = (100 * 100 * 100) + box1.Volume + box2.Volume;

            // Act & Assert
            Assert.Equal(expectedVolume, pallet.Volume);
        }

        [Fact]
        public void Pallet_WeightCalculation_IncludesBoxesAndBaseWeight()
        {
            // Arrange
            var pallet = new Pallet(1, 100, 100, 100);
            // Используем .Date для удаления времени
            var box1 = new Box(1, 10, 10, 10, 5, null, DateTime.Now.AddDays(100).Date);
            var box2 = new Box(2, 20, 20, 20, 10, null, DateTime.Now.AddDays(200).Date);

            pallet.AddBox(box1);
            pallet.AddBox(box2);

            var expectedWeight = 30 + 5 + 10;

            // Act & Assert
            Assert.Equal(expectedWeight, pallet.Weight);
        }

        [Fact]
        public void Pallet_ExpiryDate_IsMinBoxExpiryDate()
        {
            // Arrange
            var pallet = new Pallet(1, 100, 100, 100);
            // Используем .Date для удаления времени
            var earlierDate = DateTime.Now.AddDays(50).Date;
            var laterDate = DateTime.Now.AddDays(100).Date;

            pallet.AddBox(new Box(1, 10, 10, 10, 5, null, laterDate));
            pallet.AddBox(new Box(2, 20, 20, 20, 5, null, earlierDate));

            // Act & Assert
            Assert.Equal(earlierDate, pallet.ExpiryDate);
        }

        [Fact]
        public void Pallet_AddBox_ThrowsWhenBoxTooLarge()
        {
            // Arrange
            var pallet = new Pallet(1, 10, 10, 10);
            // Используем .Date для удаления времени
            var box = new Box(1, 11, 5, 5, 5, null, DateTime.Now.AddDays(100).Date);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => pallet.AddBox(box));
        }
    }
}