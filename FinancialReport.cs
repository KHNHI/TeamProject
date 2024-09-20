using System;
using System.Linq;
using System.Globalization;

namespace Quanlychitieu
{
    class FinancialReport
    {
        public void ShowReport(ExpenseTracker expenseTracker)
        {
            Console.WriteLine("=========== BÁO CÁO TÀI CHÍNH ===========");
            ShowTextReport(expenseTracker);
            ShowASCIIChart(expenseTracker);
        }

        private void ShowTextReport(ExpenseTracker expenseTracker)
        {
            var transactions = expenseTracker.GetExpenses();
            var expenses = transactions.Where(e => e.Value < 0).ToDictionary(e => e.Key, e => Math.Abs(e.Value));
            var income = transactions.Where(e => e.Value > 0).ToDictionary(e => e.Key, e => e.Value);

            var totalExpense = expenses.Sum(e => e.Value);
            var totalIncome = income.Sum(e => e.Value);
            var accountBalance = totalIncome - totalExpense;

            Console.WriteLine("\n==== BÁO CÁO DẠNG VĂN BẢN ====\n");
            Console.WriteLine($"Tổng chi tiêu: {totalExpense:#,##0₫}");
            Console.WriteLine($"Tổng thu nhập: {totalIncome:#,##0₫}");
            Console.WriteLine($"Số dư tài khoản: {accountBalance:#,##0₫}");

            Console.WriteLine("\n==== BÁO CÁO THEO DANH MỤC ====");
            Console.WriteLine("\nChi tiết chi tiêu:");
            ShowCategoryDetails(expenses, totalExpense);

            Console.WriteLine("\nChi tiết thu nhập:");
            ShowCategoryDetails(income, totalIncome);
        }

        private void ShowCategoryDetails(Dictionary<string, decimal> data, decimal total)
        {
            foreach (var category in data.OrderByDescending(e => e.Value))
            {
                var percentage = (category.Value / total) * 100;
                Console.WriteLine($"{category.Key,-15} {category.Value:#,##0₫} ({percentage,5:F1}%)");
            }
        }

        private void ShowASCIIChart(ExpenseTracker expenseTracker)
        {
            var transactions = expenseTracker.GetExpenses();
            var income = transactions.Where(t => t.Value > 0).ToDictionary(t => t.Key, t => t.Value);
            var expenses = transactions.Where(t => t.Value < 0).ToDictionary(t => t.Key, t => Math.Abs(t.Value));

            Console.WriteLine("\nBiểu đồ ASCII thu nhập:");
            DrawChart(income);

            Console.WriteLine("\nBiểu đồ ASCII chi tiêu:");
            DrawChart(expenses);
        }

        private void DrawChart(Dictionary<string, decimal> data)
        {
            if (data.Any())
            {
                var maxValue = data.Max(e => e.Value);
                var maxBarLength = 20; // dài tối đa của thanh là 20 ký tự
                var tableWidth = maxBarLength + 25;

                // Vẽ đường viền trên
                Console.WriteLine("+" + new string('-', tableWidth) + "+");

                foreach (var item in data.OrderByDescending(e => e.Value))
                {
                    Console.WriteLine();
                    var barLength = (int)((item.Value / maxValue) * maxBarLength);
                    var bar = new string('█', barLength);

                    // Hiển thị từng hàng với khung trái và phải
                    Console.WriteLine($"| {item.Key,-10} {bar,-22} {item.Value:#,##0₫} |");
                }

                // Vẽ đường viền dưới
                Console.WriteLine("+" + new string('-', tableWidth) + "+");
            }
            else
            {
                Console.WriteLine("Không có dữ liệu để hiển thị.");
            }
        }
    }
}