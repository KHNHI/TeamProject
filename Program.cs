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


            Console.ForegroundColor = ConsoleColor.Yellow;
            DrawCenteredBorder(lines);
            Console.ResetColor();
            Console.WriteLine();

            string[] menuOptions = {
                "1. Nhập biến động số dư             .",
                "2. Đặt ngân sách                    .",
                "3. Xem tình trạng ngân sách         .",
                "4. Xem đề xuất điều chỉnh ngân sách .",
                "5. Xem báo cáo tài chính            .",
                "6. Xuất/nhập dữ liệu                .",
                "7. Xem tình trạng tiết kiệm         .",
                "8. Game Tài chính                   .",
                "9. Thoát chương trình               ."
            };

            DrawCenteredBorder(menuOptions);



            Console.Write("Chọn một tùy chọn: ");
            var option = Console.ReadLine();
            var keyInfo = Console.ReadKey();

            switch (option)
            {
                case "1":
                    Console.Clear();
                    string[] balanceOptions = {
                        "1. Nhập khoản chi       .",
                        "2. Nhập khoản thu       .",
                        "3. Nhập khoản ngân sách .",
                        "4. Quay lại menu chính  ."
                    };
                    DrawCenteredBorder(balanceOptions);

                    Console.Write("Chọn một tùy chọn: ");
                    var balanceOption = Console.ReadLine();

                    switch (balanceOption)
                    {
                        case "1":
                            Console.Clear();
                            Console.WriteLine("Chọn danh mục chi tiêu:");
                            string[] expenseCategories = {
                                "1. Ăn uống                                 .",
                                "2. Đi lại                                  .",
                                "3. Chi phí cố định (nhà ở, điện, nước,...) .",
                                "4. Giải trí                                .",
                                "5. Giáo dục                                .",
                                "6. Mua sắm                                 .",
                                "7. Khác                                    ."
                            };
                            DrawCenteredBorder(expenseCategories);

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

                            if (TurnBack())
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

                            if (TurnBack())
                            {
                                continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                            }

                            break;
                        case "3":
                            Console.Clear();
                            if (expenseTracker.TotalIncome == 0) // Kiểm tra nếu chưa có thu nhập
                            {
                                Console.WriteLine("Bạn cần nhập khoản thu nhập trước khi thiết lập ngân sách.");
                                if (TurnBack())
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
                                Console.WriteLine("S tiền không hợp l.");
                            }

                            if (TurnBack())
                            {
                                continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                            }

                            break;
                        case "4":
                            continue;
                        default:
                            Console.WriteLine("Lựa chọn không hợp lệ.");

                            if (TurnBack())
                            {
                                continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                            }

                            break;
                    }
                    break;
                case "2":
                    Console.Clear();
                    budgetPlanner.SetBudget();

                    if (TurnBack())
                    {
                        continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                    }

                    break;
                case "3":
                    Console.Clear();
                    budgetPlanner.ShowBudgetStatus();

                    if (TurnBack())
                    {
                        continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                    }

                    break;
                case "4":
                    Console.Clear();
                    budgetPlanner.SuggestBudgetAdjustments();

                    if (TurnBack())
                    {
                        continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                    }

                    break;
                case "5":
                    Console.Clear();

                    expenseTracker.EnterIncome(500000);
                    expenseTracker.SetBudget(500000);

                    if (TurnBack())
                    {
                        continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                    }

                    //financialReport.ShowReport(expenseTracker);
                    financialReport.ShowFinancialReport(expenseTracker);
                    Console.WriteLine("Báo cáo tài chính đã hoàn thành.");

                    if (TurnBack())
                    {
                        continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                    }

                    break;

                case "6":
                    Console.Clear();
                    dataSync.HandleDataSync();

                    if (TurnBack())
                    {
                        continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                    }

                    break;
                case "7":
                    Console.Clear();
                    Console.WriteLine(expenseTracker.GetSavingsStatus());

                    if (TurnBack())
                    {
                        continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                    }

                    break;
                case "8":
                    Console.Clear(); // Clear the console

                    // Present the user with two options
                    string[] gameOptions = {
                        "1: Chơi StockGame   .",
                        "2: Hãy trả giá đúng ."
                    };
                    DrawCenteredBorder(gameOptions);

                    Console.Write("Chọn một tùy chọn: ");
                    var gameOption = Console.ReadLine();

                    switch (gameOption)
                    {
                        case "1":
                            StockGame stockGame = new StockGame();
                            stockGame.Run();
                            break;
                        case "2":
                            // Call the static method directly on the class
                            // Use 'Wait' to block the calling thread
                            IsPriceIsRight.BeginGameAsync().Wait();
                            break;
                        default:
                            Console.WriteLine("Lựa chọn không hợp lệ.");
                            break;
                    }

                    if (TurnBack())
                    {
                        continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                    }
                    break;
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ. Vui lòng chọn lại.");
                    break;
                    return;
                case "9":
                    return;

            }

        }



    }

    static bool TurnBack()
    {
        Console.WriteLine("Nhấn ESC để quay lại menu chính hoặc nhấn phím bất kỳ để tiếp tục.");
        var keyInfo = Console.ReadKey(true);
        return keyInfo.Key == ConsoleKey.Escape;
    }

    static void DrawCenteredBorder(string[] content)
    {
        int consoleWidth = Console.WindowWidth;
        int contentWidth = content.Max(line => line.Length);
        int borderWidth = Math.Min(consoleWidth - 4, contentWidth + 4);

        char[,] border = new char[3, borderWidth];

        // Top border
        border[0, 0] = '╔';
        border[0, borderWidth - 1] = '╗';
        for (int i = 1; i < borderWidth - 1; i++)
            border[0, i] = '═';

        // Middle border
        border[1, 0] = '║';
        border[1, borderWidth - 1] = '║';
        for (int i = 1; i < borderWidth - 1; i++)
            border[1, i] = ' ';

        // Bottom border
        border[2, 0] = '╚';
        border[2, borderWidth - 1] = '╝';
        for (int i = 1; i < borderWidth - 1; i++)
            border[2, i] = '═';

        // Draw top border
        Console.WriteLine(new string(' ', (consoleWidth - borderWidth) / 2) + new string(Enumerable.Range(0, borderWidth).Select(i => border[0, i]).ToArray()));

        // Draw content
        foreach (string line in content)
        {
            int padding = (borderWidth - line.Length - 2) / 2;
            string paddedLine = new string(' ', padding) + line + new string(' ', borderWidth - line.Length - padding - 2);
            Console.WriteLine(new string(' ', (consoleWidth - borderWidth) / 2) + "║" + paddedLine + "║");
        }

        // Draw bottom border
        Console.WriteLine(new string(' ', (consoleWidth - borderWidth) / 2) + new string(Enumerable.Range(0, borderWidth).Select(i => border[2, i]).ToArray()));
    }






}
