using Quanlychitieu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
namespace Quanlychitieu;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        ExpenseTracker expenseTracker = new ExpenseTracker();
        BudgetPlanner budgetPlanner = new BudgetPlanner(expenseTracker, new DataSync());
        FinancialReport financialReport = new FinancialReport();
        DataSync dataSync = new DataSync();
        expenseTracker.LoadExpenses();
        expenseTracker.LoadMockExpenses();



        while (true)
        {
            Console.Clear();
            int consoleWidth = Console.WindowWidth;
            string[] lines = {
                "╔╗ ╦ ╦╔╦╗╔═╗╔═╗╔╦╗  ╔╦╗╦═╗╔═╗╔═╗╦╔═╔═╗╦═╗    ╔═╗╔═╗╔═╗",
                "╠╩╗║ ║ ║║║ ╦║╣  ║    ║ ╠╦╝╠═╣║  ╠╩╗║╣ ╠╦╝    ╠═╣╠═╝╠═╝",
                "╚═╝╚═╝═╩╝╚═╝╚═╝ ╩    ╩ ╩╚═╩ ╩╚═╝╩ ╩╚═╝╩╚═    ╩ ╩╩  ╩  "
            };

            foreach (string line in lines)
            {
                int padding = (consoleWidth - line.Length) / 2;
                Console.WriteLine(line.PadLeft(padding + line.Length));
            }


            Console.WriteLine("1: Nhập biến động số dư");
            Console.WriteLine("2. Đặt ngân sách");
            Console.WriteLine("3. Xem tình trạng ngân sách");
            Console.WriteLine("4. Xem đề xuất điều chỉnh ngân sách");
            Console.WriteLine("5: Xem báo cáo tài chính");
            Console.WriteLine("6: Xuất/nhập dữ liệu");
            Console.WriteLine("7: Xem tình trạng tiết kiệm");
            Console.WriteLine("8: Thoát chương trình");
            Console.WriteLine("9: Chơi StockGame");

            Console.Write("Chọn một tùy chọn: ");
            var option = Console.ReadLine();
            var keyInfo = Console.ReadKey();

            switch (option)
            {
                case "1":
                    Console.Clear();
                    Console.WriteLine("1: Nhập khoản chi");
                    Console.WriteLine("2: Nhập khoản thu");
                    Console.WriteLine("3: Nhập khoản ngân sách");
                    Console.WriteLine("4: Quay lại menu chính");
                    Console.Write("Chọn một tùy chọn: ");
                    var balanceOption = Console.ReadLine();

                    switch (balanceOption)
                    {
                        case "1":
                            Console.Clear();
                            Console.WriteLine("Chọn danh mục chi tiêu:");
                            Console.WriteLine("1. Ăn uống");
                            Console.WriteLine("2. Đi lại");
                            Console.WriteLine("3. Chi phí cố định (nhà ở, điện, nước, wifi...)");
                            Console.WriteLine("4. Giải trí");
                            Console.WriteLine("5. Giáo dục");
                            Console.WriteLine("6. Mua sắm");
                            Console.WriteLine("7. Khác");

                            Console.WriteLine("Nhấn ESC để quay lại menu chính hoặc nhấn phím bất kỳ để tiếp tục chọn danh mục chi tiêu");
                            
                            keyInfo = Console.ReadKey(true);
                            if (keyInfo.Key == ConsoleKey.Escape)
                            {
                                continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                            }

                            Console.Write("Chọn danh mục: ");
                            var expenseCategory = Console.ReadLine();
                            if (!string.IsNullOrEmpty(expenseCategory))
                            {
                                expenseTracker.EnterExpense(expenseCategory);
                            }
                            else
                            {
                                Console.WriteLine("Danh mục không hợp lệ.");
                            }

                            Console.WriteLine("Nhấn ESC để quay lại menu chính hoặc nhấn phím bất kỳ để tiếp tục.");
                            keyInfo = Console.ReadKey(true);
                            if (keyInfo.Key == ConsoleKey.Escape)
                            {
                                continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                            }

                            break;
                        case "2":
                            Console.Clear();
                            if (!expenseTracker.CanEnterIncone()) // Kiểm tra nếu đã nhập thu nhập trong tháng này
                            {
                                Console.WriteLine("Bạn đã nhập thu nhập cho tháng này. Không thể nhập lại.");
                                Console.WriteLine("Nhấn ESC để quay lại menu chính");
                                keyInfo = Console.ReadKey(true);
                                if (keyInfo.Key == ConsoleKey.Escape)
                                {
                                    continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                                }
                            }
                            Console.Write("Nhập số tiền thu nhập: ");
                            string incomeInput = Console.ReadLine();
                            if (decimal.TryParse(incomeInput, out decimal amount))
                            {
                                expenseTracker.EnterIncome(amount);
                                Console.WriteLine($"Đã thêm khoản thu nhập: {amount:#,##0₫}");
                                expenseTracker.NotifyIncomeEntry();
                            }
                            else
                            {
                                Console.WriteLine("Số tiền không hợp lệ.");
                            }
                            Console.WriteLine("Nhấn ESC để quay lại menu chính hoặc nhấn phím bất kỳ để tiếp tục.");
                            keyInfo = Console.ReadKey(true);
                            if (keyInfo.Key == ConsoleKey.Escape)
                            {
                                continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                            }
                            break;
                        case "3":
                            Console.Clear();
                            if (expenseTracker.TotalIncome == 0) // Kiểm tra nếu chưa có thu nhập
                            {
                                Console.WriteLine("Bạn cần nhập khoản thu nhập trước khi thiết lập ngân sách.");
                                Console.WriteLine("Nhấn ESC để quay lại menu chính hoặc nhấn phím bất kỳ để tiếp tục.");
                                keyInfo = Console.ReadKey(true);
                                if (keyInfo.Key == ConsoleKey.Escape)
                                {
                                    continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                                }
                            }
                            Console.WriteLine("Nhập số tiền cho khoản ngân sách cố định:");
                            string incomebudget = Console.ReadLine();
                            if (decimal.TryParse(incomebudget, out decimal budget))
                            {
                                expenseTracker.SetBudget(budget);
                                Console.WriteLine($"Đã thêm ngân sách: {budget:#,##0đ}");
                            }
                            else
                            {
                                Console.WriteLine("Số tiền không hợp lệ.");
                            }
                            Console.WriteLine("Nhấn ESC để quay lại menu chính hoặc nhấn phím bất kỳ để tiếp tục.");
                            keyInfo = Console.ReadKey(true);
                            if (keyInfo.Key == ConsoleKey.Escape)
                            {
                                continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                            }
                            break;
                        case "4":
                            continue;
                        default:
                            Console.WriteLine("Lựa chọn không hợp lệ.");
                            Console.WriteLine("Nhấn ESC để quay lại menu chính hoặc nhấn phím bất kỳ để tiếp tục.");
                            keyInfo = Console.ReadKey(true);
                            if (keyInfo.Key == ConsoleKey.Escape)
                            {
                                continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                            }
                            break;
                    }
                    break;
                case "2":
                    Console.Clear();
                    budgetPlanner.SetBudget();
                    Console.WriteLine("Nhấn ESC để quay lại menu chính hoặc nhấn phím bất kỳ để tiếp tục.");
                    keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                    }
                    break;
                case "3":
                    Console.Clear();
                    budgetPlanner.ShowBudgetStatus();
                    Console.WriteLine("Nhấn ESC để quay lại menu chính hoặc nhấn phím bất kỳ để tiếp tục.");
                    keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                    }
                    break;
                case "4":
                    Console.Clear();
                    budgetPlanner.SuggestBudgetAdjustments();
                    Console.WriteLine("Nhấn ESC để quay lại menu chính hoặc nhấn phím bất kỳ để tiếp tục.");
                    keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                    }
                    break;
                case "5":
                    Console.Clear();

                    expenseTracker.EnterIncome(500000);
                    expenseTracker.SetBudget(500000);
                    Console.WriteLine("Nhấn ESC để quay lại menu chính hoặc nhấn phím bất kỳ để tiếp tục.");
                    keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                    }
                    //financialReport.ShowReport(expenseTracker);
                    financialReport.ShowFinancialReport(expenseTracker);
                    Console.WriteLine("Báo cáo tài chính đã hoàn thành.");
                    Console.WriteLine("Nhấn ESC để quay lại menu chính hoặc nhấn phím bất kỳ để tiếp tục.");
                    keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                    }
                    break;

                case "6":
                    Console.Clear();
                    dataSync.HandleDataSync();
                    Console.WriteLine("Nhấn ESC để quay lại menu chính hoặc nhấn phím bất kỳ để tiếp tục.");
                    keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                    }
                    break;
                case "7":
                    Console.Clear();
                    Console.WriteLine(expenseTracker.GetSavingsStatus());
                    Console.WriteLine("Nhấn ESC để quay lại menu chính hoặc nhấn phím bất kỳ để tiếp tục.");
                    keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                    }
                    break;
                case "8":
                    return;
                case "9":
                    StockGame game = new StockGame();
                    game.Run();
                    Console.WriteLine("Nhấn ESC để quay lại menu chính hoặc nhấn phím bất kỳ để tiếp tục.");
                    keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                    }
                    break;
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ. Vui lòng chọn lại.");
                    break;

            }
            
        }

    }
}
