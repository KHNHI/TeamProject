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
                var maxBarLength = 20; // Độ dài tối đa của thanh là 20 ký tự
                var tableWidth = maxBarLength + 25;

                // Vẽ đường viền trên
                Console.WriteLine("+" + new string('-', tableWidth ) + "+");

                foreach (var category in expenses.OrderByDescending(e => e.Value))
                {
                    /*Console.WriteLine("__" + new string('_', tableWidth ) + "__");*/
                    var barLength = (int)((category.Value / maxExpense) * maxBarLength);
                    var bar = new string('█', barLength); // Thay '█' bằng '#'
                    
                    // Hiển thị từng hàng với khung trái và phải
                    Console.WriteLine($"  {category.Key,-10} {bar,-22} {category.Value,10:C} ");
                    
                }

                // Vẽ đường viền dưới
                Console.WriteLine("+" + new string('-', tableWidth - 2) + "+");
            }
            else
            {
                Console.WriteLine("Không có dữ liệu chi tiêu để hiển thị.");
            }
        }
    }
}