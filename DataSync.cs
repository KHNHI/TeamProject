using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quanlychitieu
{
    internal class DataSync
    {
        private ExpenseTracker tracker = new ExpenseTracker();

        public void HandleDataSync()
        {
            Console.WriteLine("1: Xuất dữ liệu ra CSV");
            Console.WriteLine("2: Nhập dữ liệu từ CSV");
            Console.Write("Chọn một tùy chọn: ");
            var option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    ExportToCSV(tracker.LoadExpenses(), "expenses.csv");
                    break;
                case "2":
                    var expenses = ImportFromCSV("expenses.csv");
                    Console.WriteLine("Dữ liệu nhập vào:");
                    foreach (var item in expenses)
                    {
                        Console.WriteLine($"{item.Key}: {item.Value:C}");
                    }
                    break;
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ.");
                    break;
            }
        }

        private void ExportToCSV(Dictionary<string, decimal> expenses, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                foreach (var expense in expenses)
                {
                    writer.WriteLine($"{expense.Key},{expense.Value}");
                }
            }
            Console.WriteLine("Dữ liệu đã được xuất ra CSV.");
        }

        private Dictionary<string, decimal> ImportFromCSV(string filePath)
        {
            var expenses = new Dictionary<string, decimal>();
            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    expenses[parts[0]] = decimal.Parse(parts[1]);
                }
            }
            return expenses;
        }
    }
}

