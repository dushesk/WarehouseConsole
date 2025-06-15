using System;
using System.Collections.Generic;
using System.Linq;

namespace WarehouseConsole
{
    public class WarehouseService
    {
        private readonly IWarehouseRepository _repository;
        private readonly List<Pallet> _pallets;

        public WarehouseService(IWarehouseRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _pallets = _repository.Load() ?? new List<Pallet>(); // Загружаем данные при создании
        }

        public IReadOnlyCollection<Pallet> Pallets => _pallets.AsReadOnly();

        public Pallet AddPallet(double width, double height, double depth)
        {
            var pallet = new Pallet(width, height, depth);
            AddExistingPallet(pallet);
            return pallet;
        }

        public void AddExistingPallet(Pallet pallet)
        {
            if (pallet == null)
                throw new ArgumentNullException(nameof(pallet), "Паллета не может быть null");

            if (_pallets.Any(p => p.Id == pallet.Id))
                throw new ArgumentException($"Паллета с ID {pallet.Id} уже существует на складе");

            _pallets.Add(pallet);
        }

        public bool RemovePallet(int palletId)
        {
            var palletToRemove = _pallets.FirstOrDefault(p => p.Id == palletId);
            return palletToRemove != null && _pallets.Remove(palletToRemove);
        }

        // Сгруппировать все паллеты по сроку годности, отсортировать по возрастанию срока годности,
        // в каждой группе отсортировать паллеты по весу
        public IEnumerable<IGrouping<DateTime?, Pallet>> GetPalletsGroupedByExpiry()
        {
            return _pallets
                .OrderBy(p => p.ExpiryDate)
                .ThenBy(p => p.Weight)
                .GroupBy(p => p.ExpiryDate);
        }

        // 3 паллеты, которые содержат коробки с наибольшим сроком годности, отсортированные по возрастанию объема
        public IEnumerable<Pallet> GetTopPalletsByBoxExpiry(int count = 3)
        {
            return _pallets
                .Where(p => p.Boxes.Any())
                .OrderByDescending(p => p.Boxes.Max(b => b.ExpiryDate))
                .Take(count)
                .OrderBy(p => p.Volume);
        }

        public Pallet GetPalletById(int palletId)
        {
            return _pallets.FirstOrDefault(p => p.Id == palletId);
        }

        public IEnumerable<Pallet> GetAllPalletsSorted()
        {
            return _pallets
                .OrderBy(p => p.ExpiryDate)
                .ThenBy(p => p.Weight);
        }

        public Box AddBoxToPallet(int palletId, double width, double height, double depth,
                                double weight, DateTime? productionDate = null, DateTime? expiryDate = null)
        {
            var pallet = GetPalletById(palletId) ??
                throw new KeyNotFoundException($"Паллета с ID {palletId} не найдена");

            var box = new Box(width, height, depth, weight, productionDate, expiryDate);
            pallet.AddBox(box);
            return box;
        }

        public bool RemoveBoxFromPallet(int palletId, int boxId)
        {
            var pallet = GetPalletById(palletId);
            return pallet?.RemoveBox(boxId) ?? false;
        }

        public void SaveToFile(string path = "warehouse.txt")
        {
            _repository.Save(_pallets);
        }

        public void ReloadData()
        {
            var loadedPallets = _repository.Load();
            if (loadedPallets != null)
            {
                _pallets.Clear();
                _pallets.AddRange(loadedPallets);
            }
        }
    }
}