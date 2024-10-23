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
        //Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
        //Console.SetBufferSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
        Intro();

        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        ExpenseTracker expenseTracker = new ExpenseTracker();
        BudgetPlanner budgetPlanner = new BudgetPlanner(expenseTracker);
        FinancialReport financialReport = new FinancialReport();
        expenseTracker.SetBudgetPlanner(budgetPlanner);
        
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
                "5. Game                             .",
                "6. Lịch theo dõi thông tin chi tiêu .",
                "7. Thoát chương trình               ."
            };

            DrawCenteredBorder(menuOptions);


            var option = " ";
            option = InputWithBox("Chọn một tùy chọn: ", " ");


            switch (option)
            {
                case "1":
                    Console.Clear();
                    string[] balanceOptions = {
                        "1: Nhập thu nhập       .",
                        "2: Nhập khoản chi      .",
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
                            expenseTracker.EnterIncome();

                            if (TurnBack())
                            {
                                continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                            }

                            break;
                          
                        case "2":
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
                            if (expenseTracker.CanEnterIncome())
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("⚠ Bạn chưa nhập thu nhập cho tháng này. Vui lòng quay về menu chính và nhập thu nhập trước khi đặt ngân sách.");
                                Console.ResetColor();
                                Console.WriteLine("Nhấn phím bất kỳ để quay lại menu chính...");
                                Console.ReadKey(); // Đợi người dùng nhấn phím để quay lại menu chính
                                break;
                            }
                            expenseTracker.EnterExpense();
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
                    break;
                case "5":
                    Console.Clear(); 
                    string[] gameOptions = {
                        "1: MONEY - STOCKY  .",
                        "2: MONEY - MATCH   ."
                    };
                    DrawCenteredBorder(gameOptions);

                    var gameOption = InputWithBox("Chọn một tùy chọn:" , "");

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
                        Console.Clear();
                        continue; // Quay lại đầu vòng lặp, hiển thị menu chính
                    }
                    break;               
                case "6":
                    Console.Clear();
                    expenseTracker.CalendarTracker();
                    break;
                case "7":
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
        Console.WriteLine(" Nhấn ESC để quay lại menu chính hoặc nhấn phím bất kỳ để tiếp tục.");
        var keyInfo = Console.ReadKey(true);
        return keyInfo.Key == ConsoleKey.Escape;
    }





    // HÀM VẼ KHUNG VUÔNG VÀ CHỮ CĂN GIỮA 
    public static void DrawCenteredBorder(string[] content)
    {
        int consoleWidth = Console.WindowWidth;
        int contentWidth = content.Max(line => line.Length);
        int borderWidth = Math.Min(consoleWidth - 4, contentWidth + 4);

        // Hàm chia dòng khi nội dung quá dài so với chiều rộng console
        List<string> wrappedContent = new List<string>();
        foreach (string line in content)
        {
            if (line.Length > borderWidth - 4)
            {
                // Chia dòng nếu nội dung vượt quá chiều rộng viền
                for (int i = 0; i < line.Length; i += borderWidth - 4)
                {
                    wrappedContent.Add(line.Substring(i, Math.Min(borderWidth - 4, line.Length - i)));
                }
            }
            else
            {
                wrappedContent.Add(line);
            }
        }

        // Vẽ phần viền
        DrawLine('╔', '═', '╗', borderWidth, consoleWidth);

        // Vẽ nội dung (đã được bọc xuống dòng nếu cần)
        foreach (string line in wrappedContent)
        {
            int padding = (borderWidth - line.Length - 2) / 2;
            string paddedLine = new string(' ', padding) + line + new string(' ', borderWidth - line.Length - padding - 2);
            Console.WriteLine(new string(' ', (consoleWidth - borderWidth) / 2) + "║" + paddedLine + "║");
        }

        // Vẽ phần viền dưới
        DrawLine('╚', '═', '╝', borderWidth, consoleWidth);
    }

    private static void DrawLine(char start, char middle, char end, int width, int consoleWidth)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(start);
        sb.Append(new string(middle, width - 2));
        sb.Append(end);
        Console.WriteLine(new string(' ', (consoleWidth - width) / 2) + sb.ToString());
    }








    // HÀM TẠO HỘP NHẬP LIỆU CHO NGƯỜI DÙNG: GỒM 3 HÀM 
    public static string InputWithBox(string title, string prompt, int minSpaceAbove = 2)
    {
        int windowWidth = Console.WindowWidth;
        int boxWidth = Math.Max(prompt.Length + 4, 30); // Độ rộng khung ít nhất 30
        int boxHeight = 5; // Chiều cao ban đầu của khung
        int boxX = (windowWidth - boxWidth) / 2;

        // Đặt boxY để không đè lên các nội dung khác
        int boxY = Math.Max(minSpaceAbove + 2, Console.CursorTop + 3); // Đảm bảo có khoảng trống cho tiêu đề

        string userInput = "";  // Khởi tạo biến userInput với chuỗi rỗng
        bool isValid = false;

        while (!isValid)
        {
            // Đảm bảo không có ký tự nào bị chồng lên khung
            EnsureSpaceForBox(boxY, boxHeight + 2); // +2 để thêm khoảng trống dưới khung

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
            Console.SetCursorPosition(boxX + 2, boxY + boxHeight - 2); // Căn chỉnh để nhập vào giữa khung
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
        // Nếu con trỏ gần chạm cuối màn hình, tăng kích thước bộ đệm
        if (y + height >= Console.BufferHeight - 1)
        {
            Console.BufferHeight = Console.BufferHeight + height + 5;
        }

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


    // Hàm kiểm tra và đảm bảo có đủ khoảng trống để vẽ khung
    public static void EnsureSpaceForBox(int requiredBottomPosition, int boxHeight)
    {
        // Kiểm tra vị trí hiện tại của con trỏ
        int currentCursorTop = Console.CursorTop;

        // Tính toán vị trí cần in khung mới
        int neededPosition = requiredBottomPosition + boxHeight;

        // Nếu vị trí hiện tại của con trỏ gần vị trí cần in khung, thêm dòng trống
        if (currentCursorTop > neededPosition - 2)
        {
            // Thêm dòng trống để khung không bị chồng lên nội dung trước đó
            int linesToClear = currentCursorTop - (neededPosition - 2);
            Console.WriteLine(new string('\n', linesToClear));
        }

        // Kiểm tra nếu con trỏ console gần cuối màn hình
        if (neededPosition >= Console.BufferHeight - 1)
        {
            // Tăng kích thước của bộ đệm console nếu cần
            Console.BufferHeight = Console.BufferHeight + boxHeight + 5; // Mở rộng thêm không gian
        }
    }


    // INTRO MỞ ĐẦU BÀI
    static void Intro()
    {
       
        string[] content = {
    "                    ,,,,,,,        ,▄▄╬▓▓▓▓▄,                                ",
    "        ,▄╦▄▄@▄▄▓▓▓▓▓▓▒▒╣▒▒▒▓▓▄,  ▄▓▒╢╢╢╢▓▓╣▒▓▄                               ",
    "        ▓╢╢╢▒╢╢▓▓▓╢╢╢╢╢╢╢╢╢╢╢╢╢▒▓█▒╢╢╢╢╢╢╢╣╢╢╢▒▓,,▄▄▓▒▓▄▄▄,                   ",
    "        ▄▓▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▒▓▒╢╢╢╢╢╢╢▒▓▓▀▒▒╣╢╢╢╢▒▒█▓▒▒▓▓▄               ",
    "      ▄▓╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓╣╢╢╢╢▒▓▒▒▓▓▓╣╢╢╢╢╢▒▒▒╢╢╢╢╢╢▒▓             ",
    "     ▓▒▓╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╫▌╢╢▒▓▒╢▓▓▓▓▓╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▒▄           ",
    "    ▐▒╫╣▓╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢█▓▓▓▒╢╢╢╢╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▒▌         ",
    "    ▓╣╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢█▓▌╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓          ",
    "    ╙▌╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╣▒▒╣▓▓╣╢╢╢╢██╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓         ",
    "      ▀▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▒▒▒▓▓╢╢╢╢╢█▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▌         ",
    "        ▀▀▓▒▒▒╢╢╢╢╢╢╢▒▒▒▓▓█╣╢╢╢╢╢╢╢╫▌╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▌         ",
    "           '▀` `▀█▒▒▒▒▒▒▓█╣╢╢╢╢╢╢╢╢▓╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢█          ",
    "                 ▌╣╢╢╢╢╢╢▓▓▓▒▓▒╢╢╢╢▒╢╢╢╢╢▓▓▓▓▓▓▓▓▒▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢▒▀           ",
    "                  ▀▓▒▄▓╣╢╢╢╣▓╢╢╢╢╢╢╢╢╢╢╢╢▒▒╣╢▓▓▓▒▒▒▀█▓▒▒▒╢╢╢╣▒▒▓▀'            ",
    "                    ▀▓▓▓▒╢▒▓▒╢╢╢╢╢▒▒╢╢╢╢▒╣╢╢╢╢╫▓╢╢▒▓`   ```                   ",
    "                     ╙▌▀▀▀▐▌╢╢▒▓▒▓╜\"╩╣▒╩`╙▒╢╢╢╢▒▌`                           ",
    };

        string[] content2 =
        {
         "                      ▐  ╒▀╩▓╨  ,▄▄       ,▄╙╨▓╩▓▄                            ",
    "                       [ ▌       ▄▄      ,▄▄     ╙▄                           ",
    "                       ╙▀▄       ▌█      ▐▓█      ▐                           ",
    "                      ▄▐▀    ,,  ▀`       ▀  ,,    ▓                          ",
    "                     ╓▀█▄    ```    ▓▓▓▓     ```   ▄▌                         ",
    "                    ▄╖▓▓███▄▄                 ,▄▄████▄▄                       ",
    };
        string[] content3 =
        {
    "                   ▀▄▓▓▓▓▓██████▄▄▄▄▄, ▄▄▄▄▄██████▓▓▓▌╢╢▀\"²N&M--╕            ",
    "                   █▓▓▓▓▓▓▓▓▓▓███████▓████████▓▓▓▓▓▓▓█╢╢▌  Ñ▓W▐╙▀▌            ",
    "                 ╓█▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▌ UEH ▓▓█╣▓   ▒▐▓▄▄▄Γ            ",
    "                ╓▌▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█╬█M²═══╩▌æ╜             ",
    };

        string[] lines = {
"███╗   ███╗ ██████╗ ███╗   ██╗███████╗██╗   ██╗    ██████╗ ██╗   ██╗██████╗ ██████╗ ██╗   ██╗",
"████╗ ████║██╔═══██╗████╗  ██║██╔════╝╚██╗ ██╔╝    ██╔══██╗██║   ██║██╔══██╗██╔══██╗╚██╗ ██╔╝",
"██╔████╔██║██║   ██║██╔██╗ ██║█████╗   ╚████╔╝     ██████╔╝██║   ██║██║  ██║██║  ██║ ╚████╔╝ ",
"██║╚██╔╝██║██║   ██║██║╚██╗██║██╔══╝    ╚██╔╝      ██╔══██╗██║   ██║██║  ██║██║  ██║  ╚██╔╝  ",
"██║ ╚═╝ ██║╚██████╔╝██║ ╚████║███████╗   ██║       ██████╔╝╚██████╔╝██████╔╝██████╔╝   ██║   ",
"╚═╝     ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝   ╚═╝       ╚═════╝  ╚═════╝ ╚═════╝ ╚═════╝    ╚═╝   ",

                };
        Console.ForegroundColor = ConsoleColor.Green;
        foreach (string line in content)
        {
            Console.Write(line.PadLeft((Console.WindowWidth + line.Length) / 2).PadRight(Console.WindowWidth));
        }
        Console.ForegroundColor = ConsoleColor.White;
        foreach (string line in content2)
        {
            Console.Write(line.PadLeft((Console.WindowWidth + line.Length) / 2).PadRight(Console.WindowWidth));
        }
        Console.ForegroundColor = ConsoleColor.White;
        foreach (string line in content3)
        {
            Console.Write(line.PadLeft((Console.WindowWidth + line.Length) / 2).PadRight(Console.WindowWidth));
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        DrawCenteredBorder(lines);

        Console.ResetColor();

        // Hiển thị màn hình trong 4 giây
        System.Threading.Thread.Sleep(4000);
    }

}
