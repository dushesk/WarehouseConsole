using Xunit;
using WarehouseConsole;
using System;
using System.Linq;
using System.Collections.Generic;

namespace WarehouseConsole.Tests
{
    public class WarehouseServiceTests
    {
        private readonly TestWarehouseRepository _testRepo;
        private readonly WarehouseService _service;

        private Box CreateTestBox() => new Box(10, 10, 10, 5, DateTime.Today, null);

        public WarehouseServiceTests()
        {
            _testRepo = new TestWarehouseRepository();
            _service = new WarehouseService(_testRepo);
        }

        [Fact]
        public void Constructor_ShouldLoadPalletsFromRepository()
        {
            // Arrange & Act (выполняется в конструкторе)

            // Assert
            Assert.Equal(4, _service.Pallets.Count);
        }

        [Fact]
        public void AddPallet_ShouldAddNewPallet()
        {
            // Act
            var newPallet = _service.AddPallet(200, 200, 200);

            // Assert
            Assert.Equal(5, _service.Pallets.Count);
            Assert.Contains(newPallet, _service.Pallets);
        }

        [Fact]
        public void AddExistingPallet_ShouldThrowIfPalletExists()
        {
            // Arrange
            var existingPallet = _service.Pallets.First();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.AddExistingPallet(existingPallet));
        }

        [Fact]
        public void RemovePallet_ShouldRemoveExistingPallet()
        {
            // Arrange
            var palletToRemove = _service.Pallets.First();

            // Act
            var result = _service.RemovePallet(palletToRemove.Id);

            // Assert
            Assert.True(result);
            Assert.Equal(3, _service.Pallets.Count);
            Assert.DoesNotContain(palletToRemove, _service.Pallets);
        }

        [Fact]
        public void RemovePallet_ShouldReturnFalseForNonExistingPallet()
        {
            // Act
            var result = _service.RemovePallet(-1); // Несуществующий ID

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetPalletById_ShouldReturnCorrectPallet()
        {
            // Arrange
            var expectedPallet = _service.Pallets.First();

            // Act
            var actualPallet = _service.GetPalletById(expectedPallet.Id);

            // Assert
            Assert.Equal(expectedPallet, actualPallet);
        }

        [Fact]
        public void GetPalletById_ShouldReturnNullForNonExistingPallet()
        {
            // Act
            var result = _service.GetPalletById(-1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void AddBoxToPallet_ShouldAddBoxToPallet()
        {
            // Arrange
            var pallet = _service.Pallets.First();
            int initialBoxCount = pallet.Boxes.Count;
            var testBox = CreateTestBox();

            // Act
            var newBox = _service.AddBoxToPallet(
                pallet.Id,
                testBox.Width,
                testBox.Height,
                testBox.Depth,
                testBox.Weight,
                testBox.ProductionDate);

            // Assert
            Assert.Equal(initialBoxCount + 1, pallet.Boxes.Count);
            Assert.Contains(newBox, pallet.Boxes);
        }

        [Fact]
        public void AddBoxToPallet_ShouldThrowForNonExistingPallet()
        {
            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() =>
                _service.AddBoxToPallet(-1, 50, 50, 50, 10));
        }

        [Fact]
        public void RemoveBoxFromPallet_ShouldRemoveBox()
        {
            // Arrange
            var pallet = _service.Pallets.First();
            var boxToRemove = pallet.Boxes.First();
            int initialCount = pallet.Boxes.Count;

            // Act
            var result = _service.RemoveBoxFromPallet(pallet.Id, boxToRemove.Id);

            // Assert
            Assert.True(result);
            Assert.Equal(initialCount - 1, pallet.Boxes.Count);
            Assert.DoesNotContain(boxToRemove, pallet.Boxes);
        }

        [Fact]
        public void RemoveBoxFromPallet_ShouldReturnFalseForNonExistingBox()
        {
            // Arrange
            var pallet = _service.Pallets.First();

            // Act
            var result = _service.RemoveBoxFromPallet(pallet.Id, -1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetAllPalletsSorted_ShouldReturnCorrectOrder()
        {
            // Act
            var sortedPallets = _service.GetAllPalletsSorted().ToList();

            // Assert
            // Проверяем сортировку по ExpiryDate (по возрастанию)
            for (int i = 0; i < sortedPallets.Count - 1; i++)
            {
                Assert.True(sortedPallets[i].ExpiryDate <= sortedPallets[i + 1].ExpiryDate);

                // Если даты равны, проверяем сортировку по весу
                if (sortedPallets[i].ExpiryDate == sortedPallets[i + 1].ExpiryDate)
                {
                    Assert.True(sortedPallets[i].Weight <= sortedPallets[i + 1].Weight);
                }
            }
        }

        [Fact]
        public void ReloadData_ShouldResetPallets()
        {
            // Arrange
            var initialCount = _service.Pallets.Count;
            _service.AddPallet(300, 300, 300);

            // Act
            _service.ReloadData();

            // Assert
            Assert.Equal(initialCount, _service.Pallets.Count);
        }

        [Fact]
        public void SaveToFile_ShouldCallRepositorySave()
        {
            // Arrange
            bool saveWasCalled = false;
            _testRepo.SaveAction = _ => saveWasCalled = true;

            // Act
            _service.SaveToFile();

            // Assert
            Assert.True(saveWasCalled);
        }

        [Fact]
        public void GetPalletsGroupedByExpiry_ShouldReturnGroupsOrderedByExpiry()
        {
            // Arrange
            var repo = new TestWarehouseRepository();
            var service = new WarehouseService(repo);

            // Act
            var result = service.GetPalletsGroupedByExpiry().ToList();

            // Assert
            Assert.Equal(2, result.Count); // Должно быть 2 группы
            Assert.True(result[0].Key < result[1].Key); // Группы отсортированы по возрастанию срока
        }

        [Fact]
        public void GetPalletsGroupedByExpiry_ShouldSortPalletsByWeightInGroup()
        {
            // Arrange
            var repo = new TestWarehouseRepository();
            var service = new WarehouseService(repo);

            // Act
            var result = service.GetPalletsGroupedByExpiry().ToList();

            // Assert
            foreach (var group in result)
            {
                var pallets = group.ToList();
                for (int i = 0; i < pallets.Count - 1; i++)
                {
                    Assert.True(pallets[i].Weight <= pallets[i + 1].Weight);
                }
            }
        }

        [Fact]
        public void GetPalletsGroupedByExpiry_ShouldIncludeAllPallets()
        {
            // Arrange
            var repo = new TestWarehouseRepository();
            var service = new WarehouseService(repo);
            var totalPallets = service.Pallets.Count;

            // Act
            var result = service.GetPalletsGroupedByExpiry().ToList();
            var palletsInGroups = result.Sum(g => g.Count());

            // Assert
            Assert.Equal(totalPallets, palletsInGroups);
        }

        [Fact]
        public void GetPalletsGroupedByExpiry_ShouldHandleEmptyPallets()
        {
            // Arrange
            var repo = new EmptyWarehouseRepository();
            var service = new WarehouseService(repo);

            // Act
            var result = service.GetPalletsGroupedByExpiry().ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetTopPalletsByBoxExpiry_ShouldReturnCorrectCount()
        {
            // Arrange
            var repo = new TestWarehouseRepository();
            var service = new WarehouseService(repo);

            // Act
            var resultDefault = service.GetTopPalletsByBoxExpiry().ToList();
            var resultCustom = service.GetTopPalletsByBoxExpiry(2).ToList();

            // Assert
            Assert.Equal(3, resultDefault.Count);
            Assert.Equal(2, resultCustom.Count);
        }

        [Fact]
        public void GetTopPalletsByBoxExpiry_ShouldFilterPalletsWithoutBoxes()
        {
            // Arrange
            var repo = new TestWarehouseRepository();
            var service = new WarehouseService(repo);
            service.AddExistingPallet(new Pallet(100, 100, 100)); // Паллета без коробок

            // Act
            var result = service.GetTopPalletsByBoxExpiry(10).ToList();

            // Assert
            Assert.All(result, p => Assert.NotEmpty(p.Boxes));
        }

        [Fact]
        public void GetTopPalletsByBoxExpiry_ShouldOrderByMaxExpiryAscending()
        {
            // Arrange
            var repo = new TestWarehouseRepository();
            var service = new WarehouseService(repo);

            // Act
            var result = service.GetTopPalletsByBoxExpiry().ToList();

            // Assert
            for (int i = 0; i < result.Count - 1; i++)
            {
                var currentMax = result[i].Boxes.Max(b => b.ExpiryDate);
                var nextMax = result[i + 1].Boxes.Max(b => b.ExpiryDate);
                Assert.True(currentMax <= nextMax);
            }
        }

        [Fact]
        public void GetTopPalletsByBoxExpiry_ShouldSortByVolumeForSameExpiry()
        {
            // Arrange
            var repo = new TestWarehouseRepository();
            var service = new WarehouseService(repo);

            // Добавляем паллеты с одинаковым сроком годности
            var pallet1 = new Pallet(200, 200, 200);
            pallet1.AddBox(new Box(1, 10, 10, 10, 5, null, new DateTime(2023, 8, 1)));
            service.AddExistingPallet(pallet1);

            var pallet2 = new Pallet(150, 150, 150);
            pallet2.AddBox(new Box(2, 10, 10, 10, 5, null, new DateTime(2023, 8, 1)));
            service.AddExistingPallet(pallet2);

            // Act
            var result = service.GetTopPalletsByBoxExpiry(4).ToList();
            var sameExpiryGroup = result.Where(p => p.Boxes.Any(b => b.ExpiryDate == new DateTime(2023, 8, 1))).ToList();

            // Assert
            if (sameExpiryGroup.Count > 1)
            {
                Assert.True(sameExpiryGroup[0].Volume <= sameExpiryGroup[1].Volume);
            }
        }

        [Fact]
        public void GetTopPalletsByBoxExpiry_ShouldReturnEmptyWhenNoBoxes()
        {
            // Arrange
            var repo = new EmptyWarehouseRepository();
            var service = new WarehouseService(repo);

            // Act
            var result = service.GetTopPalletsByBoxExpiry().ToList();

            // Assert
            Assert.Empty(result);
        }
    }

    public class TestWarehouseRepository : IWarehouseRepository
    {
        public Action<IEnumerable<Pallet>> SaveAction { get; set; }

        public List<Pallet> Load()
        {
            // Создаем тестовые данные с разными сроками годности и весами
            var pallets = new List<Pallet>();

            // Группа 1 (ранние сроки)
            var pallet1 = new Pallet(1, 100, 100, 100);
            pallet1.AddBox(new Box(1, 10, 10, 10, 5, null, new DateTime(2023, 6, 1)));
            pallets.Add(pallet1);

            var pallet2 = new Pallet(2, 100, 100, 100);
            pallet2.AddBox(new Box(2, 20, 20, 20, 10, null, new DateTime(2023, 6, 1)));
            pallets.Add(pallet2);

            // Группа 2 (поздние сроки)
            var pallet3 = new Pallet(3, 100, 100, 100);
            pallet3.AddBox(new Box(3, 15, 15, 15, 7, null, new DateTime(2023, 7, 1)));
            pallets.Add(pallet3);

            var pallet4 = new Pallet(4, 100, 100, 100);
            pallet4.AddBox(new Box(4, 25, 25, 25, 12, null, new DateTime(2023, 7, 1)));
            pallets.Add(pallet4);

            return pallets;
        }

        public void Save(IEnumerable<Pallet> pallets)
        {
            SaveAction?.Invoke(pallets);
        }
    }

    public class EmptyWarehouseRepository : IWarehouseRepository
    {
        public List<Pallet> Load() => new List<Pallet>();
        public void Save(IEnumerable<Pallet> pallets) { }
    }
}