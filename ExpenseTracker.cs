﻿using Newtonsoft.Json;

namespace Quanlychitieu
{
    internal class ExpenseTracker
    {
        private Dictionary<string, decimal> expenses = new Dictionary<string, decimal>();
        private string filePath = "expenses.json";

        public void EnterExpense(string categoryChoice)
        {
            if (string.IsNullOrEmpty(categoryChoice))
            {
                Console.WriteLine("Danh mục không hợp lệ.");
                return;
            }

            string category = GetExpenseCategory(categoryChoice);
            EnterTransaction(category, isExpense: true);
        }

        public void EnterIncome(string categoryChoice)
        {
            if (string.IsNullOrEmpty(categoryChoice))
            {
                Console.WriteLine("Danh mục không hợp lệ.");
                return;
            }

            string category = GetIncomeCategory(categoryChoice);
            EnterTransaction(category, isExpense: false);
        }

        private void EnterTransaction(string category, bool isExpense)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.Write("Nhập số tiền: ");
            if (decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                if (isExpense)
                    amount = -amount; // Make expense amounts negative

                if (expenses.ContainsKey(category))
                    expenses[category] += amount;
                else
                    expenses[category] = amount;

                SaveExpenses();
                string transactionType = isExpense ? "chi tiêu" : "thu nhập";
                Console.WriteLine($"Đã lưu {transactionType}: {Math.Abs(amount)} vào danh mục '{category}' vào lúc {timestamp}.");
            }
            else
            {
                Console.WriteLine("Số tiền không hợp lệ.");
            }
        }

        private string GetExpenseCategory(string choice)
        {
            switch (choice)
            {
                case "1": return "Ăn uống";
                case "2": return "Đi lại";
                case "3": return "Chi phí cố định";
                case "4": return "Giải trí";
                case "5": return "Giáo dục";
                case "6": return "Mua sắm";
                case "7": return "Khác";
                default: return "Khác";
            }
        }

        private string GetIncomeCategory(string choice)
        {
            switch (choice)
            {
                case "1": return "Lương";
                case "2": return "Thưởng";
                case "3": return "Đầu tư";
                case "4": return "Tiết kiệm";
                default: return "Khác";
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