using System;
using System.Linq;
using System.Globalization;
using System.Reflection.Metadata;

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

            string[] titleFinancialReport =
   {

"███████╗██╗███╗   ██╗            ██████╗ ███████╗██████╗  ██████╗ ██████╗ ████████╗",
"██╔════╝██║████╗  ██║            ██╔══██╗██╔════╝██╔══██╗██╔═══██╗██╔══██╗╚══██╔══╝",
"█████╗  ██║██╔██╗ ██║   █████╗   ██████╔╝█████╗  ██████╔╝██║   ██║██████╔╝   ██║   ",
"██╔══╝  ██║██║╚██╗██║   ╚════╝   ██╔══██╗██╔══╝  ██╔═══╝ ██║   ██║██╔══██╗   ██║   ",
"██║     ██║██║ ╚████║            ██║  ██║███████╗██║     ╚██████╔╝██║  ██║   ██║   ",
"╚═╝     ╚═╝╚═╝  ╚═══╝            ╚═╝  ╚═╝╚══════╝╚═╝      ╚═════╝ ╚═╝  ╚═╝   ╚═╝   ",



      };
            Console.ForegroundColor = ConsoleColor.Yellow;
            Program.DrawCenteredBorder(titleFinancialReport);
            Console.ResetColor();

            ShowTextReport(expenseTracker);
            ShowExpenseChart(expenseTracker.GetExpenses());
        }

        private void ShowTextReport(ExpenseTracker expenseTracker)
        {
           
            var expenses = expenseTracker.GetExpenses();
            decimal totalBudget = expenseTracker.TotalBudget; // Lấy tổng ngân sách
       
        

            var totalExpense = expenseTracker.TotalExpenses;
            var totalIncome = expenseTracker.TotalIncome;
            var accountBalance = totalIncome - totalExpense;
            var temptSaving = expenseTracker.GetSavingsStatus();

            string[] textReport = { " BÁO CÁO TỔNG QUÁT " };
            Program.DrawCenteredBorder(textReport);

            CenterPrint($"Tổng chi tiêu: {totalExpense:#,##0₫}");
            CenterPrint($"Tổng thu nhập: {totalIncome:#,##0₫}");
            CenterPrint("────────────────────────");
            CenterPrint($"Số dư tài khoản: {accountBalance:#,##0₫}");
            CenterPrint($"Tổng ngân sách: {totalBudget:#,##0₫}");
            CenterPrint($" {temptSaving:#,##0₫}");
            string[] CategotiesReport = { " BÁO CÁO CHI TIẾT CHI TIÊU " };
            Program.DrawCenteredBorder(CategotiesReport);

            CenterPrint("Chi tiết chi tiêu:");
            ShowCategoryDetails(expenses, totalExpense);
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

            string[] bieuDoASCII = { " BIỂU ĐỒ ASCII CHI TIÊU " };
            Program.DrawCenteredBorder(bieuDoASCII);

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
            string[] chosseTypeReport = new string[] {
            "     CHỌN LOẠI BÁO CÁO TÀI CHÍNH        ",
            "    ─────────────────────────────       ",
            "1. Báo cáo tài chính tháng hiện tại  ",
            "2. Thống kê theo tháng               ",
            "3. Tổng chi tiêu trong năm           ",
            "4. Quay về menu chính                ",

            };
            Program.DrawCenteredBorder(chosseTypeReport);
            var choice = Program.InputWithBox("Chọn một tùy chọn: ", " ");
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
                    DrawYearlyExpenseChart(expenseTracker);
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ.");
                    break;
            }
            Console.WriteLine("Nhấn ESC để quay về");
            ConsoleKeyInfo key = Console.ReadKey();
            if (key.Key == ConsoleKey.Escape)
            {
                return;
            }
        }




        private void ShowMonthlyReport(ExpenseTracker expenseTracker)
        {

            var monthlyTotals = expenseTracker.GetMonthlyTotals();


            string[] title = { " BÁO CÁO CHI TIÊU THEO THÁNG ",
            "─────────────────────────────"};
            Program.DrawCenteredBorder(title);

            foreach (var month in monthlyTotals)
            {
                List<string> reportLines = new List<string>();
                reportLines.Add($"Tháng {month.Key}:");
                foreach (var category in month.Value)
                {
                    reportLines.Add($"  {category.Key}: {category.Value:#,##0₫}");
                }
                reportLines.Add(""); // Thêm dòng trống giữa các tháng
                Program.DrawCenteredBorder(reportLines.ToArray());
            }

            // Vẽ khung bao quanh báo cáo
            string[] name = { "BIỂU ĐỒ TỔNG CHI TIÊU THEO THÁNG" };
            Program.DrawCenteredBorder(name);
            // Vẽ biểu đồ tổng chi tiêu
            DrawTotalExpenseChart(expenseTracker);
        }

        private void DrawTotalExpenseChart(ExpenseTracker expenseTracker)
        {
            var monthlyTotals = expenseTracker.GetMonthlyTotals(); // Lấy tổng chi tiêu mỗi tháng
            int maxBarLength = 50; // Độ dài tối đa của thanh biểu đồ
            double maxExpense = 0; // Biến để lưu chi tiêu lớn nhất

            // Tìm giá trị chi tiêu lớn nhất để chuẩn hóa chiều dài thanh
            foreach (var month in monthlyTotals)
            {
                double totalExpense = month.Value.Values.Sum();
                if (totalExpense > maxExpense)
                {
                    maxExpense = totalExpense;
                }
            }


            foreach (var month in monthlyTotals)
            {
                double totalExpense = month.Value.Values.Sum(); // Tính tổng chi tiêu cho tháng
                int barLength = (int)((totalExpense / maxExpense) * maxBarLength); // Chuẩn hóa chiều dài thanh
                string bar = new string('█', barLength); // Tạo thanh biểu đồ bằng ký tự '█'

                // Tạo chuỗi cho mỗi dòng của biểu đồ
                string line = $"Tháng {month.Key,-2}: {bar,-50} {totalExpense,15:#,##0₫}";

                // In ra biểu đồ với số liệu rõ ràng và căn giữa
                CenterPrintLine(line);
                Console.WriteLine();
            }
        }


        // VẼ BIỂU ĐỒ CHI TIÊU THEO NĂM 
        private void DrawYearlyExpenseChart(ExpenseTracker expenseTracker)
        {
        // Lấy dữ liệu tổng chi tiêu hàng tháng và chuyển sang tổng theo năm
         var monthlyTotals = expenseTracker.GetMonthlyTotals();
         var yearlyTotals = new Dictionary<string, double>();

         // Tổng hợp dữ liệu chi tiêu theo năm dựa trên các danh mục
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

         // Độ dài tối đa của thanh biểu đồ
         int maxBarLength = 50;  // Độ dài thanh biểu đồ
         double maxExpense = yearlyTotals.Values.Max(); // Tìm giá trị chi tiêu lớn nhất

         // In tiêu đề cho biểu đồ
         string[] title = { " BÁO CÁO CHI TIÊU TRONG NĂM " };
         Program.DrawCenteredBorder(title);

         // Tổng chiều rộng của biểu đồ và thanh phân cách
         var totalWidth = 90; // Tăng tổng chiều rộng của biểu đồ

         // Vẽ khung trên của biểu đồ
         CenterPrintLine("┌" + new string('─', totalWidth - 2) + "┐");

         // Vẽ biểu đồ cho từng danh mục chi tiêu
         foreach (var category in yearlyTotals)
         {
            // Tính toán tổng chi tiêu cho từng danh mục
            double totalExpense = category.Value;

            // Tính chiều dài của thanh biểu đồ dựa trên tỷ lệ so với giá trị chi tiêu lớn nhất
            int barLength = (int)((totalExpense / maxExpense) * maxBarLength);
            string bar = new string('█', barLength); // Tạo thanh biểu đồ với ký tự '█'

            // Định dạng chuỗi in ra: tên danh mục, thanh biểu đồ và giá trị chi tiêu
            string line = $"│ {category.Key,-15} {bar,-50} {totalExpense,15:#,##0₫} │";

            // Sử dụng hàm CenterPrintLine để căn giữa và in ra biểu đồ
            CenterPrint(line);

            // Thêm dấu phân cách giữa các danh mục nếu không phải danh mục cuối cùng
            if (category.Key != yearlyTotals.Keys.Last())
            {
                CenterPrint("│" + new string(' ', totalWidth - 2) + "│");
            }
         }

        // Vẽ khung dưới của biểu đồ
        CenterPrintLine("└" + new string('─', totalWidth - 2) + "┘");
        }
    }
}
