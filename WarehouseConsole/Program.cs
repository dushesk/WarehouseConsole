using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WarehouseConsole
{
    internal class Program
    {
        private static WarehouseService _warehouseService;

        static void Main(string[] args)
        {
            // Инициализируем репозиторий и сервис
            IWarehouseRepository repository = new FileWarehouseRepository("warehouse.txt");
            _warehouseService = new WarehouseService(repository);

            Console.WriteLine("=== Система управления складом ===");

            while (true)
            {
                DisplayMenu();
                var choice = Console.ReadLine();

                try
                {
                    ProcessUserChoice(choice);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                    Console.WriteLine("Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        static void DisplayMenu()
        {
            Console.Clear();
            Console.WriteLine("\nМеню:");
            Console.WriteLine("1. Добавить паллету");
            Console.WriteLine("2. Удалить паллету");
            Console.WriteLine("3. Добавить коробку на паллету");
            Console.WriteLine("4. Удалить коробку с паллеты");
            Console.WriteLine("5. Просмотреть все паллеты");
            Console.WriteLine("6. Сгруппировать паллеты по сроку годности");
            Console.WriteLine("7. Показать топ-3 паллеты с наибольшим сроком годности коробок");
            Console.WriteLine("8. Загрузить данные из файла");
            Console.WriteLine("9. Сохранить данные в файл");
            Console.WriteLine("0. Выход");
            Console.Write("Выберите действие: ");
        }

        static void ProcessUserChoice(string choice)
        {
            switch (choice)
            {
                case "1":
                    AddPallet();
                    break;
                case "2":
                    RemovePallet();
                    break;
                case "3":
                    AddBox();
                    break;
                case "4":
                    RemoveBox();
                    break;
                case "5":
                    DisplayAllPallets();
                    break;
                case "6":
                    DisplayPalletsGroupedByExpiry();
                    break;
                case "7":
                    DisplayTopPallets();
                    break;
                case "8":
                    LoadFromFile();
                    break;
                case "9":
                    SaveToFile();
                    break;
                case "0":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Неверный ввод. Попробуйте снова.");
                    break;
            }
        }

        static void AddPallet()
        {
            Console.WriteLine("\nДобавление новой паллеты:");
            double width = ReadDouble("Введите ширину: ");
            double height = ReadDouble("Введите высоту: ");
            double depth = ReadDouble("Введите глубину: ");

            var pallet = _warehouseService.AddPallet(width, height, depth);
            Console.WriteLine($"Паллета добавлена. ID: {pallet.Id}");
            WaitForUser();
        }

        static void RemovePallet()
        {
            Console.WriteLine("\nУдаление паллеты:");
            int id = ReadInt("Введите ID паллеты: ");

            if (_warehouseService.RemovePallet(id))
                Console.WriteLine("Паллета удалена");
            else
                Console.WriteLine("Паллета не найдена");

            WaitForUser();
        }

        static void AddBox()
        {
            Console.WriteLine("\nДобавление коробки на паллету:");
            int palletId = ReadInt("Введите ID паллеты: ");

            Console.WriteLine("Введите параметры коробки:");
            double width = ReadDouble("Ширина: ");
            double height = ReadDouble("Высота: ");
            double depth = ReadDouble("Глубина: ");
            double weight = ReadDouble("Вес: ");

            Console.WriteLine("Укажите дату производства (Enter чтобы пропустить) или срок годности:");
            Console.Write("Дата производства (гггг-мм-дд): ");
            var productionDateInput = Console.ReadLine();

            DateTime? productionDate = null;
            DateTime? expiryDate = null;

            if (!string.IsNullOrEmpty(productionDateInput))
            {
                productionDate = DateTime.Parse(productionDateInput);
            }
            else
            {
                Console.Write("Срок годности (гггг-мм-дд): ");
                expiryDate = DateTime.Parse(Console.ReadLine());
            }

            var box = _warehouseService.AddBoxToPallet(palletId, width, height, depth, weight, productionDate, expiryDate);
            Console.WriteLine($"Коробка добавлена. ID: {box.Id}");
            WaitForUser();
        }

        static void RemoveBox()
        {
            Console.WriteLine("\nУдаление коробки:");
            int palletId = ReadInt("Введите ID паллеты: ");
            int boxId = ReadInt("Введите ID коробки: ");

            if (_warehouseService.RemoveBoxFromPallet(palletId, boxId))
                Console.WriteLine("Коробка удалена");
            else
                Console.WriteLine("Коробка не найдена");

            WaitForUser();
        }

        static void DisplayAllPallets()
        {
            Console.WriteLine("\nСписок всех паллет:");
            var pallets = _warehouseService.GetAllPalletsSorted();

            if (!pallets.Any())
            {
                Console.WriteLine("На складе нет паллет");
            }
            else
            {
                foreach (var pallet in pallets)
                {
                    PrintPalletDetails(pallet);
                }
            }

            WaitForUser();
        }

        static void DisplayPalletsGroupedByExpiry()
        {
            Console.WriteLine("\nПаллеты, сгруппированные по сроку годности:");
            var groups = _warehouseService.GetPalletsGroupedByExpiry();

            foreach (var group in groups)
            {
                Console.WriteLine($"\n--- Срок годности: {group.Key?.ToString("yyyy-MM-dd") ?? "Нет данных"} ---");
                foreach (var pallet in group.OrderBy(p => p.Weight))
                {
                    PrintPalletDetails(pallet);
                }
            }

            WaitForUser();
        }

        // 3 паллеты, которые содержат коробки с наибольшим сроком годности, отсортированные по возрастанию объема
        static void DisplayTopPallets()
        {
            Console.WriteLine("\nТоп-3 паллеты с наибольшим сроком годности коробок:");
            var topPallets = _warehouseService.GetTopPalletsByBoxExpiry();

            if (!topPallets.Any())
            {
                Console.WriteLine("Нет паллет с коробками");
            }
            else
            {
                foreach (var pallet in topPallets)
                {
                    PrintPalletDetails(pallet);
                }
            }

            WaitForUser();
        }

        static void LoadFromFile()
        {
            Console.Write("\nВведите путь к файлу (по умолчанию warehouse.txt): ");
            var path = Console.ReadLine();
            path = string.IsNullOrWhiteSpace(path) ? "warehouse.txt" : path;

            _warehouseService = new WarehouseService(new FileWarehouseRepository(path));
            Console.WriteLine("Данные успешно загружены");
            WaitForUser();
        }

        static void SaveToFile()
        {
            Console.Write("\nВведите путь к файлу (по умолчанию warehouse.txt): ");
            var path = Console.ReadLine();
            path = string.IsNullOrWhiteSpace(path) ? "warehouse.txt" : path;

            _warehouseService.SaveToFile(path);
            Console.WriteLine("Данные успешно сохранены");
            WaitForUser();
        }

        static void PrintPalletDetails(Pallet pallet)
        {
            Console.WriteLine($"\nПаллета ID: {pallet.Id}");
            Console.WriteLine($"Размеры: {pallet.Width}x{pallet.Height}x{pallet.Depth}");
            Console.WriteLine($"Вес: {pallet.Weight}, Объем: {pallet.Volume}");
            Console.WriteLine($"Срок годности: {pallet.ExpiryDate?.ToString("yyyy-MM-dd") ?? "Нет данных"}");

            if (pallet.Boxes.Any())
            {
                Console.WriteLine("Содержит коробки:");
                foreach (var box in pallet.Boxes)
                {
                    Console.WriteLine($"  ID: {box.Id}, Размер: {box.Width}x{box.Height}x{box.Depth}, " +
                                    $"Вес: {box.Weight}, Срок: {box.ExpiryDate:yyyy-MM-dd}");
                }
            }
            else
            {
                Console.WriteLine("Не содержит коробок");
            }
        }

        static int ReadInt(string prompt)
        {
            Console.Write(prompt);
            return int.Parse(Console.ReadLine());
        }

        static double ReadDouble(string prompt)
        {
            Console.Write(prompt);
            return double.Parse(Console.ReadLine());
        }

        static void WaitForUser()
        {
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }
}