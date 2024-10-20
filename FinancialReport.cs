using System;
using System.Linq;
using System.Globalization;

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
            var transactions = expenseTracker.GetExpenses();
            var expenses = new Dictionary<string, decimal>();
            var income = new Dictionary<string, decimal>();
            decimal totalBudget = expenseTracker.TotalBudget; // Lấy tổng ngân sách
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


            string[] textReport = { " BÁO CÁO TỔNG QUÁT " };
            Program.DrawCenteredBorder( textReport );

            CenterPrint($"Tổng chi tiêu: {totalExpense:#,##0₫}");
            CenterPrint($"Tổng thu nhập: {totalIncome:#,##0₫}");
            CenterPrint("────────────────────────");
            CenterPrint($"Số dư tài khoản: {accountBalance:#,##0₫}");
            CenterPrint($"Tổng ngân sách: {totalBudget:#,##0₫}"); // Hiển thị tổng ngân sách
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

            string[] bieuDoASCII = {" BIỂU ĐỒ ASCII CHI TIÊU "};
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
            };
            Program.DrawCenteredBorder(chosseTypeReport);





            //Console.WriteLine("Chọn loại báo cáo tài chính:");
            //Console.WriteLine("1:Báo cáo tài chính tháng hiện tại");
            //Console.WriteLine("2:Thống kê theo tháng");
            //Console.WriteLine("3:Tổng chi tiêu trong năm");


           // Console.Write("Chọn một tùy chọn: ");

            var choice = Program.InputWithBox("Chọn một tùy chọn: "," ");


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

            Console.WriteLine("\nBiểu đồ chi tiêu tổng theo tháng:");

            foreach (var month in monthlyTotals)
            {
                double totalExpense = month.Value.Values.Sum(); // Tính tổng chi tiêu cho tháng
                int barLength = (int)((totalExpense / maxExpense) * maxBarLength); // Chuẩn hóa chiều dài thanh
                string bar = new string('█', barLength); // Tạo thanh biểu đồ bằng ký tự '█'

                // In ra biểu đồ với số liệu rõ ràng
                Console.WriteLine($"Tháng {month.Key,-2}: {bar,-50} {totalExpense,15:#,##0₫}");
            }
        }
    }
}