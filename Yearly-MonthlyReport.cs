using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quanlychitieu
{
    internal class Yearly_MonthlyReport
    {
        private double[,] expensesData; // Mảng 2 chiều để lưu dữ liệu chi tiêu
        private string[] categories = { "Ăn uống", "Đi lại", "Chi phí cố định", "Giải trí", "Giáo dục", "Mua sắm", "Khác" };

        public void FinancialReport()
        {
            expensesData = new double[12, categories.Length]; // 12 tháng và 7 danh mục chi tiêu
        }

        // Nhận dữ liệu từ ExpenseTracker và lưu vào mảng expensesData
        public void CollectData(ExpenseTracker expenseTracker)
        {
            Dictionary<string, Dictionary<int, double>> expenses = expenseTracker.GetMonthlyExpenses();

            foreach (var category in expenses)
            {
                string categoryName = category.Key;
                Dictionary<int, double> monthlyExpenses = category.Value;

                int categoryIndex = Array.IndexOf(categories, categoryName); // Tìm vị trí danh mục trong mảng categories

                if (categoryIndex >= 0) // Nếu danh mục hợp lệ
                {
                    foreach (var monthExpense in monthlyExpenses)
                    {
                        int month = monthExpense.Key - 1; // Lưu ý: tháng bắt đầu từ 1 nên trừ đi 1 để dùng cho mảng
                        expensesData[month, categoryIndex] += monthExpense.Value;
                    }
                }
            }
        }

        // Hiển thị báo cáo theo tháng
        public void ShowMonthlyReport(int month)
        {
            if (month < 1 || month > 12)
            {
                Console.WriteLine("Tháng không hợp lệ.");
                return;
            }

            Console.WriteLine($"\nBáo cáo chi tiêu cho tháng {month}:\n");
            month--; // Trừ 1 để phù hợp với chỉ số mảng

            for (int category = 0; category < categories.Length; category++)
            {
                double amount = expensesData[month, category];
                Console.WriteLine($"{categories[category]}: {amount:#,##0₫}");
            }

            Console.WriteLine("--------------------------------------------");
        }

        // Hiển thị báo cáo tổng kết cả năm
        public void ShowYearlyReport()
        {
            Console.WriteLine("\nBáo cáo chi tiêu cả năm:\n");

            double[] yearlyTotals = new double[categories.Length];

            // Cộng tổng chi tiêu từng danh mục trong 12 tháng
            for (int month = 0; month < 12; month++)
            {
                for (int category = 0; category < categories.Length; category++)
                {
                    yearlyTotals[category] += expensesData[month, category];
                }
            }

            for (int category = 0; category < categories.Length; category++)
            {
                Console.WriteLine($"{categories[category]}: {yearlyTotals[category]:#,##0₫}");
            }

            Console.WriteLine("--------------------------------------------");
        }
    }
}
