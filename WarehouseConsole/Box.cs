using System;

namespace WarehouseConsole
{
    public class Box
    {
        private const int SHELF_LIFE_DAYS = 100;
        private static int _lastId = 0;

        public int Id { get; }
        public double Width { get; }
        public double Height { get; }
        public double Depth { get; }
        public double Weight { get; }
        public DateTime? ProductionDate { get; }
        public DateTime ExpiryDate { get; }
        public double Volume => Width * Height * Depth;

        // Конструктор с автоматической генерацией ID
        public Box(double width, double height, double depth, double weight,
                 DateTime? productionDate = null, DateTime? expiryDate = null)
            : this(GenerateNewId(), width, height, depth, weight, productionDate, expiryDate)
        {
        }

        // Конструктор с явным указанием ID
        public Box(int id, double width, double height, double depth, double weight,
                 DateTime? productionDate = null, DateTime? expiryDate = null)
        {
            ValidateId(id);
            ValidateDimensions(width, height, depth, weight);

            // Валидация и установка дат
            var dates = ValidateAndCalculateDates(productionDate, expiryDate);

            // Установка свойств
            Id = id;
            Width = width;
            Height = height;
            Depth = depth;
            Weight = weight;
            ProductionDate = dates.productionDate;
            ExpiryDate = dates.expiryDate;

            // Обновляем последний использованный ID
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

        private void ValidateDimensions(double width, double height, double depth, double weight)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Ширина должна быть положительной");
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), "Высота должна быть положительной");
            if (depth <= 0)
                throw new ArgumentOutOfRangeException(nameof(depth), "Глубина должна быть положительной");
            if (weight <= 0)
                throw new ArgumentOutOfRangeException(nameof(weight), "Вес должен быть положительным");
        }

        private (DateTime? productionDate, DateTime expiryDate) ValidateAndCalculateDates(
    DateTime? productionDate, DateTime? expiryDate)
        {
            // Проверка что указана только одна дата
            if (productionDate.HasValue && expiryDate.HasValue)
                throw new ArgumentException("Должна быть указана только одна дата: либо производства, либо годности");

            if (!productionDate.HasValue && !expiryDate.HasValue)
                throw new ArgumentException("Должна быть указана хотя бы одна дата: либо производства, либо годности");

            // Проверка формата дат (без времени)
            if (productionDate.HasValue && productionDate.Value.TimeOfDay != TimeSpan.Zero)
                throw new ArgumentException("Дата производства должна быть указана без времени");

            if (expiryDate.HasValue && expiryDate.Value.TimeOfDay != TimeSpan.Zero)
                throw new ArgumentException("Срок годности должен быть указан без времени");

            // Расчет дат
            if (productionDate.HasValue)
            {
                var expiry = productionDate.Value.AddDays(SHELF_LIFE_DAYS);
                return (productionDate.Value, expiry.Date); // .Date для гарантии отсутствия времени
            }
            else
            {
                return (null, expiryDate.Value.Date); // .Date для гарантии отсутствия времени
            }
        }
    }
}