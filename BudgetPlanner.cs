﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Quanlychitieu
{
    public class BudgetPlanner
    {
        private Dictionary<string, decimal> categoryBudgets;
        private ExpenseTracker expenseTracker;
        private readonly List<string> validCategories = new List<string>
        {
            "Ăn uống", "Đi lại", "Chi phí cố định", "Giải trí", "Giáo dục", "Mua sắm", "Khác"
        };

        internal BudgetPlanner(ExpenseTracker expenseTracker)
        {
            this.expenseTracker = expenseTracker;
            categoryBudgets = new Dictionary<string, decimal>();
        }

        public void SetBudget()
        {
            string input;
            do
            {
                Console.WriteLine("\nChọn danh mục để đặt ngân sách:");
                for (int i = 0; i < validCategories.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {validCategories[i]}");
                }

                int choice;
                // Vòng lặp để bắt buộc nhập đúng danh mục
                while (true)
                {
                    Console.Write("Nhập số tương ứng với danh mục: ");
                    if (int.TryParse(Console.ReadLine(), out choice) && choice >= 1 && choice <= validCategories.Count)
                    {
                        break; // Thoát khỏi vòng lặp nếu nhập hợp lệ
                    }
                    else
                    {
                        Console.WriteLine("Lựa chọn không hợp lệ. Vui lòng thử lại.");
                    }
                }

                string category = validCategories[choice - 1];
                decimal amount;
                // Vòng lặp để bắt buộc nhập đúng số tiền
                while (true)
                {
                    Console.Write($"Nhập số tiền ngân sách cho {category}: ");
                    if (decimal.TryParse(Console.ReadLine(), out amount) && amount >= 0)
                    {
                        break; // Thoát khỏi vòng lặp nếu nhập hợp lệ
                    }
                    else
                    {
                        Console.WriteLine("Số tiền không hợp lệ. Vui lòng nhập một số dương.");
                    }
                }

                // Lưu ngân sách vào danh mục
                categoryBudgets[category] = amount;
                Console.WriteLine($"Ngân sách cho {category} đã được đặt thành {amount:#,##0₫}");

                // Hỏi người dùng có muốn tiếp tục đặt ngân sách cho danh mục khác
                Console.Write("Bạn có muốn đặt ngân sách cho danh mục khác không? (y/n): ");
                input = Console.ReadLine()?.ToLower();
            } while (input == "y");
        }

        private void PlaywarningSound()
        {
            Console.Beep();
        }

        private void ShowMessage(string message)
        {
            Console.WriteLine($"Cảnh báo: {message}");
        }

        public void ShowBudgetStatus()
        {
            var expenses = expenseTracker.GetExpenses();

            Console.WriteLine("\n=== Tình trạng Ngân sách ===");
            foreach (var category in validCategories)
            {
                decimal budgetAmount = categoryBudgets.ContainsKey(category) ? categoryBudgets[category] : 0;
                decimal spent = expenses.ContainsKey(category) ? Math.Abs(expenses[category]) : 0;
                decimal remaining = budgetAmount - spent;
                decimal percentageUsed = budgetAmount > 0 ? (spent / budgetAmount) * 100 : 0;

                Console.WriteLine($"Danh mục: {category}");
                Console.WriteLine($"Ngân sách: {budgetAmount:#,##0₫}");
                Console.WriteLine($"Đã chi: -{spent:#,##0₫}");
                Console.WriteLine($"Còn lại: {remaining:#,##0₫}");
                Console.WriteLine($"Đã sử dụng: {percentageUsed:F1}%");

                if (remaining < 0)
                {
                    Console.WriteLine("Cảnh báo: Bạn đã vượt quá ngân sách!");
                    PlaywarningSound();
                }
                else if (percentageUsed > 80)
                {
                    Console.WriteLine("Cảnh báo: Bạn đã sử dụng hơn 80% ngân sách!");
                    PlaywarningSound();
                }

                Console.WriteLine();
            }
        }

        public void SuggestBudgetAdjustments()
        {
            var expenses = expenseTracker.GetExpenses();
            if (expenses == null || expenses.Count == 0 || categoryBudgets == null || categoryBudgets.Count == 0)
            {
                Console.WriteLine("Không có dữ liệu để phân tích.");
                return;
            }

            var totalBudget = categoryBudgets.Sum(b => b.Value);
            var totalExpenses = expenses.Sum(e => e.Value);

            Console.WriteLine("\n=== Đề xuất Điều chỉnh Ngân sách ===");

            if (totalExpenses > totalBudget)
            {
                Console.WriteLine("Bạn đang chi tiêu nhiều hơn tổng ngân sách. Hãy xem xét cắt giảm chi tiêu hoặc tăng ngân sách.");
            }

            foreach (var category in validCategories)
            {
                decimal spent = expenses.ContainsKey(category) ? expenses[category] : 0;
                decimal budgetAmount = categoryBudgets.ContainsKey(category) ? categoryBudgets[category] : 0;

                if (budgetAmount == 0)
                {
                    Console.WriteLine($"Đề xuất: Thiết lập ngân sách cho danh mục '{category}'. Chi tiêu hiện tại: {spent:#,##0₫}");
                }
                else if (spent > budgetAmount)
                {
                    decimal overspent = spent - budgetAmount;
                    Console.WriteLine($"Đề xuất: Tăng ngân sách cho '{category}' thêm {overspent:#,##0₫} hoặc cắt giảm chi tiêu.");
                }
                else if (spent < budgetAmount * 0.5m)
                {
                    Console.WriteLine($"Đề xuất: Xem xét giảm ngân sách cho '{category}' vì chi tiêu thấp hơn 50% ngân sách.");
                }
            }
        }
    }
}