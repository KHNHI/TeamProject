using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Quanlychitieu
{
    internal class ExpenseTracker
    {
        private Dictionary<string, decimal> expenses = new Dictionary<string, decimal>();
        private string filePath = "expenses.json";

        public void EnterExpense()
        {
            Console.Write("Nhập danh mục chi tiêu (ăn uống, giải trí, học tập, di chuyển,...): ");
            string category = Console.ReadLine();
            Console.Write("Nhập số tiền: ");
            if (decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                if (expenses.ContainsKey(category))
                    expenses[category] += amount;
                else
                    expenses[category] = amount;

                SaveExpenses();
                Console.WriteLine("Chi tiêu đã được ghi lại.");
            }
            else
            {
                Console.WriteLine("Số tiền không hợp lệ.");
            }
        }

        private void SaveExpenses()
        {
            var json = JsonConvert.SerializeObject(expenses);
            File.WriteAllText(filePath, json);
        }

        public Dictionary<string, decimal> LoadExpenses()
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                expenses = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(json) ?? new Dictionary<string, decimal>();
            }
            return expenses;
        }

        public Dictionary<string, decimal> GetExpenses()
        {
            return expenses;
        }
    }
}