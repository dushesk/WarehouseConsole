using System;
using System.Collections.Generic;
using System.Linq;

namespace WarehouseConsole
{
    public class Pallet
    {
        private static int _lastId = 0;

        public int Id { get; }
        public double Width { get; }
        public double Height { get; }
        public double Depth { get; }
        public List<Box> Boxes { get; } = new List<Box>();

        public double Weight => Boxes.Sum(b => b.Weight) + 30;
        public double Volume => Boxes.Sum(b => b.Volume) + (Width * Height * Depth);
        public DateTime? ExpiryDate => Boxes.Count > 0 ? (DateTime?)Boxes.Min(b => b.ExpiryDate) : null;

        // Конструктор с автоматической генерацией ID
        public Pallet(double width, double height, double depth)
            : this(GenerateNewId(), width, height, depth) { }

        // Конструктор с явным указанием ID
        public Pallet(int id, double width, double height, double depth)
        {
            ValidateId(id);
            ValidateDimensions(width, height, depth);

            // Установка свойств
            Id = id;
            Width = width;
            Height = height;
            Depth = depth;

            if (id > _lastId)
            {
                _lastId = id;
            }
        }

        private static int GenerateNewId()
        {
            return ++_lastId;
        }

        private static void ValidateId(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id),
                    "ID паллеты должен быть положительным числом");
        }

        private static void ValidateDimensions(double width, double height, double depth)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width),
                    "Ширина паллеты должна быть положительной");

            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height),
                    "Высота паллеты должна быть положительной");

            if (depth <= 0)
                throw new ArgumentOutOfRangeException(nameof(depth),
                    "Глубина паллеты должна быть положительной");
        }

        private void ValidateBoxDimensions(Box box)
        {
            if (box.Width > Width)
                throw new InvalidOperationException(
                    $"Ширина коробки ({box.Width}) превышает ширину паллеты ({Width})");

            if (box.Depth > Depth)
                throw new InvalidOperationException(
                    $"Глубина коробки ({box.Depth}) превышает глубину паллеты ({Depth})");
        }

        public void AddBox(Box box)
        {
            if (box == null)
                throw new ArgumentNullException(nameof(box), "Коробка не может быть null");

            ValidateBoxDimensions(box);

            Boxes.Add(box);
        }

        public bool RemoveBox(int boxId)
        {
            var boxToRemove = Boxes.FirstOrDefault(b => b.Id == boxId);
            return boxToRemove != null && Boxes.Remove(boxToRemove);
        }
    }
}