﻿using System;
using System.Linq;
using System.Globalization;
//3
namespace Quanlychitieu
{
    class FinancialReport
    {
        private int consoleWidth;

        public FinancialReport()
        {
            consoleWidth = Console.WindowWidth;
        }

        private void CenterPrint(string text)
        {
            Console.WriteLine(string.Format("{0," + ((consoleWidth / 2) + (text.Length / 2)) + "}", text));
        }

        private void CenterPrintLine(string text)
        {
            int padding = (consoleWidth - text.Length) / 2;
            string paddedText = new string(' ', padding) + text + new string(' ', padding);
            Console.WriteLine(paddedText);
        }

        public void ShowReport(ExpenseTracker expenseTracker)
        {
            // Hiển thị tiêu đề "FINANCIAL REPORT" không có dấu "=" ở hai bên
            CenterPrintLine("═══════════════════════════════════════════════════════════════════════════════");
            CenterPrintLine("╔═╗╦╔╗╔╔═╗╔╗╔╔═╗╦╔═╗╦    ╦═╗╔═╗╔═╗╔═╗╦═╗╔╦╗");
            CenterPrintLine("╠╣ ║║║║╠═╣║║║║  ║╠═╣║    ╠╦╝║╣ ╠═╝║ ║╠╦╝ ║ ");
            CenterPrintLine("╚  ╩╝╚╝╩ ╩╝╚╝╚═╝╩╩ ╩╩═╝  ╩╚═╚═╝╩  ╚═╝╩╚═ ╩ ");
            CenterPrintLine("═══════════════════════════════════════════════════════════════════════════════");

            ShowTextReport(expenseTracker);
            ShowExpenseChart(expenseTracker.GetExpenses());
        }

        private void ShowTextReport(ExpenseTracker expenseTracker)
        {
            var transactions = expenseTracker.GetExpenses();
            var expenses = new Dictionary<string, decimal>();
            var income = new Dictionary<string, decimal>();

            // Phân loại giao dịch dựa trên danh mục
            foreach (var transaction in transactions)
            {
                if (transaction.Key == "Thu nhập")
                {
                    income[transaction.Key] = transaction.Value;
                }
                else
                {
                    expenses[transaction.Key] = Math.Abs(transaction.Value);
                }
            }

            var totalExpense = expenses.Sum(e => e.Value);
            var totalIncome = income.Sum(e => e.Value);
            var accountBalance = totalIncome - totalExpense;

            CenterPrintLine(" BÁO CÁO DẠNG VĂN BẢN \n");
            CenterPrint($"Tổng chi tiêu: {totalExpense:#,##0₫}");
            CenterPrint($"Tổng thu nhập: {totalIncome:#,##0₫}");
            CenterPrint($"Số dư tài khoản: {accountBalance:#,##0₫}");

            CenterPrintLine(" BÁO CÁO THEO DANH MỤC \n");
            CenterPrint("Chi tiết chi tiêu:");
            ShowCategoryDetails(expenses, totalExpense);

            CenterPrint("Chi tiết thu nhập:");
            ShowCategoryDetails(income, totalIncome);
        }

        private void ShowCategoryDetails(Dictionary<string, decimal> data, decimal total)
        {
            foreach (var category in data.OrderByDescending(e => e.Value))
            {
                var percentage = (category.Value / total) * 100;
                string line = $"{category.Key,-15} {category.Value,10:#,##0₫} ({percentage,5:F1}%)";
                CenterPrint(line);
            }
        }

        private void ShowExpenseChart(Dictionary<string, decimal> transactions)
        {
            var expenseCategories = new[] { "Ăn uống", "Đi lại", "Chi phí cố định", "Giải trí", "Giáo dục", "Mua sắm", "Khác" };
            var expenses = expenseCategories.ToDictionary(category => category,
                category => transactions.TryGetValue(category, out decimal value) ? Math.Abs(value) : 0);

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            CenterPrintLine(" Biểu đồ ASCII chi tiêu ");
            if (expenses.Any(e => e.Value > 0))
            {
                var maxExpense = expenses.Max(e => e.Value);
                var maxBarLength = 40; // Tăng độ dài tối đa của thanh
                var totalWidth = 71; // Tăng tổng chiều rộng của biểu đồ

                CenterPrintLine("┌" + new string('─', totalWidth - 2) + "┐");
                foreach (var expense in expenses)
                {
                    int barLength = maxExpense > 0 ? (int)((expense.Value / maxExpense) * maxBarLength) : 0;
                    var bar = new string('█', barLength);
                    var line = $"│ {expense.Key,-15} {bar,-40} {expense.Value,10:#,##0₫} │";
                    CenterPrint(line);

                    // Thêm dòng trống giữa các danh mục
                    if (expense.Key != expenseCategories.Last())
                    {
                        CenterPrint("│" + new string(' ', totalWidth - 2) + "│");
                    }
                }
                CenterPrintLine("└" + new string('─', totalWidth - 2) + "┘");
            }
            else
            {
                var totalWidth = 71;
                CenterPrintLine("┌" + new string('─', totalWidth - 2) + "┐");
                CenterPrint("│ Không có dữ liệu chi tiêu" + new string(' ', totalWidth - 30) + "│");
                CenterPrintLine("└" + new string('─', totalWidth - 2) + "┘");
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