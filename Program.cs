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
        BudgetPlanner budgetPlanner = new BudgetPlanner(expenseTracker);
        FinancialReport financialReport = new FinancialReport();
        expenseTracker.LoadExpenses();
        expenseTracker.LoadMockExpenses();
        budgetPlanner.LoadBudgetFromCSV();
        expenseTracker.SetBudgetPlanner(budgetPlanner);
        budgetPlanner.GetTotalBudget();
        while (true)
        {
            Console.Clear();
            int consoleWidth = Console.WindowWidth;
            string[] lines =  {
"███╗   ███╗ ██████╗ ███╗   ██╗███████╗██╗   ██╗    ██████╗ ██╗   ██╗██████╗ ██████╗ ██╗   ██╗",
"████╗ ████║██╔═══██╗████╗  ██║██╔════╝╚██╗ ██╔╝    ██╔══██╗██║   ██║██╔══██╗██╔══██╗╚██╗ ██╔╝",
"██╔████╔██║██║   ██║██╔██╗ ██║█████╗   ╚████╔╝     ██████╔╝██║   ██║██║  ██║██║  ██║ ╚████╔╝ ",
"██║╚██╔╝██║██║   ██║██║╚██╗██║██╔══╝    ╚██╔╝      ██╔══██╗██║   ██║██║  ██║██║  ██║  ╚██╔╝  ",
"██║ ╚═╝ ██║╚██████╔╝██║ ╚████║███████╗   ██║       ██████╔╝╚██████╔╝██████╔╝██████╔╝   ██║   ",
"╚═╝     ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝   ╚═╝       ╚═════╝  ╚═════╝ ╚═════╝ ╚═════╝    ╚═╝   ",
             
                };



            Console.ForegroundColor = ConsoleColor.Yellow;
            DrawCenteredBorder(lines);
            Console.ResetColor();
            Console.WriteLine();

            string[] menuOptions = {
                "1. Nhập biến động số dư             .",
                "2. Đặt ngân sách                    .",
                "3. Xem tình trạng ngân sách         .",
                "4. Xem báo cáo tài chính            .",
                "5. Xem tình trạng tiết kiệm         .",
                "6. Game Tài chính                   .",
                "7. Lịch theo dõi thông tin chi tiêu .",
                "8. Thoát chương trình               ."
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
                        "1: Nhập khoản chi      .",
                        "2: Nhập khoản thu      .",
                        "3: Quay lại menu chính ."
                    };
                    DrawCenteredBorder(balanceOptions);

                    //Console.Write("Chọn một tùy chọn: ");



                     var balanceOption = "      ";  
                     balanceOption = InputWithBox("Chọn một tùy chọn: "," ");
                              

                    switch (balanceOption)
                    {
                        case "1":
                            Console.Clear();

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
                            expenseTracker.EnterExpense();
                            break;
                        case "2":
                            Console.Clear();
                            expenseTracker.EnterIncome();

                            if (TurnBack())
                            {
                                continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                            }

                            break;
                        case "3":
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
                    if (expenseTracker.CanEnterIncome())
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("⚠ Bạn chưa nhập thu nhập cho tháng này. Vui lòng quay về menu chính và nhập thu nhập trước khi đặt ngân sách.");
                        Console.ResetColor();
                        Console.WriteLine("Nhấn phím bất kỳ để quay lại menu chính...");
                        Console.ReadKey(); // Đợi người dùng nhấn phím để quay lại menu chính
                        break;
                    }
                    else
                    {
                        budgetPlanner.SetCategoryBudget();
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
                    financialReport.ShowFinancialReport(expenseTracker);
                    Console.WriteLine("Báo cáo tài chính đã hoàn thành.");
                    if (TurnBack())
                    {
                        continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                    }

                    break;
                case "5":
                    Console.Clear();
                    Console.WriteLine(expenseTracker.GetSavingsStatus());
                    if (TurnBack())
                    {
                        continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                    }
                    break;
                case "6":
                    Console.Clear(); 
                    string[] gameOptions = {
                        "1: MONEY - STOCKY  .",
                        "2: MONEY - MATCH   ."
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
                case "7":
                    
                    expenseTracker.CalendarTracker();
                    break;
                case "8":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ. Vui lòng chọn lại.");
                    break;
                    


            }

        }
    }
    static bool TurnBack()
    {
        Console.WriteLine("Nhấn ESC để quay lại menu chính hoặc nhấn phím bất kỳ để tiếp tục.");
        var keyInfo = Console.ReadKey(true);
        return keyInfo.Key == ConsoleKey.Escape;
    }
    public static void DrawCenteredBorder(string[] content)
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


    // Hàm tao hộp Nhập input 
    public static string InputWithBox(string title, string prompt)
    {
        int windowWidth = Console.WindowWidth;
        int boxWidth = Math.Max(prompt.Length + 4, 30); // Độ rộng khung ít nhất 30
        int boxHeight = 5; // Chiều cao ban đầu của khung
        int boxX = (windowWidth - boxWidth) / 2;
        int boxY = 10;

        string userInput = "";  // Khởi tạo biến userInput với chuỗi rỗng
        bool isValid = false;

        while (!isValid)
        {
            // Xóa màn hình trước khi in lại khung
            

            // In tiêu đề
            Console.SetCursorPosition((windowWidth - title.Length) / 2, boxY - 2);
            Console.WriteLine(title);

            // Nếu dữ liệu không hợp lệ, hiển thị thông báo lỗi
            if (!string.IsNullOrEmpty(userInput) && !int.TryParse(userInput, out _))
            {
                boxHeight = 6; // Tăng chiều cao khung thêm 1 dòng cho thông báo lỗi
                DrawBox(boxX, boxY, boxWidth, boxHeight, "  Nhập liệu không hợp lệ.");
            }
            else
            {
                // Vẽ khung thông thường khi chưa có lỗi
                DrawBox(boxX, boxY, boxWidth, boxHeight, prompt);
            }

            // Đặt con trỏ vào vị trí nhập dữ liệu
            Console.SetCursorPosition(boxX + 2, boxY + 3); // Căn chỉnh để nhập vào giữa khung
            userInput = Console.ReadLine();

            // Kiểm tra nếu nhập đúng số
            if (int.TryParse(userInput, out _))
            {
                isValid = true; // Nếu nhập hợp lệ, thoát vòng lặp
            }
        }

        return userInput; // Trả về dữ liệu hợp lệ
    }

    public static void DrawBox(int x, int y, int width, int height, string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;

        // Vẽ đường viền trên
        Console.SetCursorPosition(x, y);
        Console.Write("╔" + new string('═', width - 2) + "╗");

        // Vẽ dòng chứa thông báo
        Console.SetCursorPosition(x, y + 1);
        Console.Write("║" + message.PadRight(width - 2) + "║");

        // Vẽ dòng trống trong khung để nhập liệu
        for (int i = 2; i < height - 1; i++)
        {
            Console.SetCursorPosition(x, y + i);
            Console.Write("║" + new string(' ', width - 2) + "║");
        }

        // Vẽ đường viền dưới
        Console.SetCursorPosition(x, y + height - 1);
        Console.Write("╚" + new string('═', width - 2) + "╝");

        Console.ResetColor();
    }


}
