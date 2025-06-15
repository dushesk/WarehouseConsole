using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseConsole
{
    internal class FileWarehouseRepository : IWarehouseRepository
    {
        private readonly string _filePath;

        public FileWarehouseRepository(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        public void Save(IEnumerable<Pallet> pallets)
        {
            var lines = new List<string>();

            foreach (var pallet in pallets)
            {
                // Формат строки паллеты: ID|Width|Height|Depth|BoxCount
                lines.Add($"P|{pallet.Id}|{pallet.Width}|{pallet.Height}|{pallet.Depth}|{pallet.Boxes.Count}");

                foreach (var box in pallet.Boxes)
                {
                    // Формат строки коробки: B|ID|Width|Height|Depth|Weight|ProductionDate|ExpiryDate
                    lines.Add($"B|{box.Id}|{box.Width}|{box.Height}|{box.Depth}|{box.Weight}|" +
                             $"{box.ProductionDate?.ToString("yyyy-MM-dd")}|{box.ExpiryDate:yyyy-MM-dd}");
                }
            }

            File.WriteAllLines(_filePath, lines);
        }

        public List<Pallet> Load()
        {
            if (!File.Exists(_filePath))
                return new List<Pallet>();

            var pallets = new List<Pallet>();
            Pallet currentPallet = null;

            foreach (var line in File.ReadAllLines(_filePath))
            {
                var parts = line.Split('|');

                if (parts[0] == "P") // Паллета
                {
                    currentPallet = new Pallet(
                        int.Parse(parts[1]),
                        double.Parse(parts[2]),
                        double.Parse(parts[3]),
                        double.Parse(parts[4]));

                    pallets.Add(currentPallet);
                }
                else if (parts[0] == "B" && currentPallet != null) // Коробка
                {
                    var box = new Box(
                        int.Parse(parts[1]),
                        double.Parse(parts[2]),
                        double.Parse(parts[3]),
                        double.Parse(parts[4]),
                        double.Parse(parts[5]),
                        !string.IsNullOrEmpty(parts[6]) ? (DateTime?)DateTime.Parse(parts[6]) : null,
                        DateTime.Parse(parts[7]));

                    currentPallet.AddBox(box);
                }
            }

            return pallets;
        }
    }
}
