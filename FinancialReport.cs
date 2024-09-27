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
        public void ShowFinancialReport(ExpenseTracker expenseTracker)
        {
            Console.WriteLine("Chọn loại báo cáo tài chính:");
            Console.WriteLine("1:Báo cáo tài chính tháng hiện tại");
            Console.WriteLine("2:Thống kê theo tháng");
            Console.WriteLine("3:Tổng chi tiêu trong năm");
            Console.Write("Chọn một tùy chọn: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Clear();
                    ShowReport(expenseTracker);
                    break;
                case "2":
                    Console.Clear();
                    ShowMonthlyReport(expenseTracker);
                    break;
                case "3":
                    Console.Clear();
                    ShowYearlyReport(expenseTracker);
                    break;
                    
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ.");
                    break;
            }
        }


        public void ShowMonthlyReport(ExpenseTracker expenseTracker)
        {
            var monthlyTotals = expenseTracker.GetMonthlyTotals();

            Console.WriteLine("\nBáo cáo tài chính theo tháng:");
            foreach (var month in monthlyTotals)
            {
                Console.WriteLine($"Tháng {month.Key}:");
                foreach (var category in month.Value)
                {
                    Console.WriteLine($"  {category.Key}: {category.Value:#,##0₫}");
                }
                Console.WriteLine(); // Thêm dòng trống giữa các tháng
            }
            DrawTotalExpenseChart(expenseTracker);
        }

        public void ShowYearlyReport(ExpenseTracker expenseTracker)
        {
            var monthlyTotals = expenseTracker.GetMonthlyTotals();
            var yearlyTotals = new Dictionary<string, double>();

            foreach (var month in monthlyTotals)
            {
                foreach (var category in month.Value)
                {
                    if (!yearlyTotals.ContainsKey(category.Key))
                    {
                        yearlyTotals[category.Key] = 0;
                    }
                    yearlyTotals[category.Key] += category.Value;
                }
            }

            Console.WriteLine("\nBáo cáo tài chính theo năm:");
            foreach (var category in yearlyTotals)
            {
                Console.WriteLine($"  {category.Key}: {category.Value:#,##0₫}");
            }
        }

        public void DrawTotalExpenseChart(ExpenseTracker expenseTracker)
        {
            var monthlyTotals = expenseTracker.GetMonthlyTotals(); // Sử dụng GetMonthlyTotals

            Console.WriteLine("\nBiểu đồ chi tiêu tổng theo tháng:");
            foreach (var month in monthlyTotals)
            {
                double totalExpense = month.Value.Values.Sum(); // Tính tổng chi tiêu cho tháng
                int barLength = (int)(totalExpense / 10000); // Điều chỉnh tỷ lệ cho chiều dài thanh
                string bar = new string('█', barLength);
                Console.WriteLine($"Tháng {month.Key}: {bar} ({totalExpense:#,##0₫})");
            }
        }
    }
}