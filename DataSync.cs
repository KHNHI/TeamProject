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
        
        private const string BUDGET_FILE = "budget.csv";

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
                        Console.WriteLine($"{item.Key}: {item.Value:#,##0₫}");
                    }
                    break;
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ.");
                    break;
            }
        }

        public void SaveBudgetToCSV(Dictionary<string, decimal> budgets)
        {
            using (var writer = new StreamWriter(BUDGET_FILE))
            {
                foreach (var category in budgets)
                {
                    writer.WriteLine($"{category.Key},{category.Value}");
                }
            }
            ExportToCSVbudget(budgets, BUDGET_FILE);
           
           
        }
        private void ExportToCSVbudget(Dictionary<string, decimal> data, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                foreach (var item in data)
                {
                    writer.WriteLine($"{item.Key},{item.Value}");
                }
            }
        }

       
        public Dictionary<string, decimal> LoadBudgetFromCSV()
        {
            var budgets = new Dictionary<string, decimal>();
            if (File.Exists(BUDGET_FILE))
            {
                var lines = File.ReadAllLines(BUDGET_FILE);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 2 && decimal.TryParse(parts[1], out decimal amount))
                    {
                        budgets[parts[0]] = amount;
                    }
                }
               
            }
            return budgets;
          
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
            //Console.WriteLine("Dữ liệu đã được xuất ra CSV.");
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

