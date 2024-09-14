using System;
using System.Linq;

namespace Quanlychitieu
{
    class FinancialReport
    {
        public void ShowReport(ExpenseTracker expenseTracker)
        {
            Console.WriteLine("=== Báo cáo Tài chính ===");
            ShowTextReport(expenseTracker);
            ShowASCIIChart(expenseTracker);
        }

        private void ShowTextReport(ExpenseTracker expenseTracker)
        {
            var expenses = expenseTracker.GetExpenses();
            var totalExpense = expenses.Sum(e => e.Value);

            Console.WriteLine("\nBáo cáo dạng văn bản:");
            Console.WriteLine($"Tổng chi tiêu: {totalExpense:C}");
            Console.WriteLine("\nChi tiêu theo danh mục:");
            foreach (var category in expenses.OrderByDescending(e => e.Value))
            {
                var percentage = (category.Value / totalExpense) * 100;
                Console.WriteLine($"{category.Key,-15} {category.Value,10:C} ({percentage,5:F1}%)");
            }
        }

        private void ShowASCIIChart(ExpenseTracker expenseTracker)
        {
            var expenses = expenseTracker.GetExpenses();

            Console.WriteLine("\nBiểu đồ ASCII chi tiêu theo danh mục:");
            if (expenses.Any())
            {
                var maxExpense = expenses.Max(e => e.Value);
                foreach (var category in expenses.OrderByDescending(e => e.Value))
                {
                    var barLength = (int)((category.Value / maxExpense) * 20);  // Độ dài tối đa của thanh là 20 ký tự
                    var bar = new string('█', barLength);
                    Console.WriteLine($"{category.Key,-15} {bar,-20} {category.Value:C}");
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("Không có dữ liệu chi tiêu để hiển thị.");
            }
        }
    }
}