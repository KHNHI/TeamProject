using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Newtonsoft.Json;
using static Quanlychitieu.StockGame;
using JsonSerializer = System.Text.Json.JsonSerializer;
using CsvHelper.Configuration;
using System.Reflection.Metadata;


namespace Quanlychitieu
{
    internal class StockGame
    {
#nullable disable   // Tắt tính năng kiểm tra null để đơn giản hóa xử lý giá trị null



        // Lớp Company đại diện cho thông tin về công ty trong trò chơi
        public class Company
        {
            public string Name { get; set; }  // Tên công ty
            public string Industry { get; set; }  // Ngành công nghiệp của công ty
            public decimal SharePrice { get; set; }  // Giá cổ phiếu hiện tại
            public int NumberOfShares { get; set; }  // Số lượng cổ phiếu công ty
            public string Description { get; set; }  // Mô tả công ty
        }

        // Lớp Event đại diện cho một sự kiện có thể ảnh hưởng đến công ty
        public class Event
        {
            public string Title { get; set; }  // Tiêu đề sự kiện
            public string Description { get; set; }  // Mô tả sự kiện
            public string CompanyName { get; set; }  // Tên công ty mà sự kiện ảnh hưởng
            public int Effect { get; set; }  // Tác động (phần trăm) của sự kiện đến giá cổ phiếu
        }

        // Kiểm tra xem người dùng có yêu cầu thoát game không
        private bool CloseRequested { get; set; } = false;

        // Danh sách các công ty có trong trò chơi
        private List<Company> Companies { get; set; } = null!;

        // Danh sách các sự kiện có thể xảy ra trong trò chơi
        private List<Event> Events { get; set; } = null!;

        // Sự kiện hiện tại đang xảy ra trong trò chơi
        private Event CurrentEvent { get; set; } = null!;

        // Số tiền hiện tại người chơi có
        private decimal Money { get; set; }

        // Giá trị tài sản tối thiểu khi thua game
        private const decimal LosingNetWorth = 2000.00m;

        // Giá trị tài sản tối đa để thắng game
        private const decimal WinningNetWorth = 4000.00m;

        // Hàm khởi động trò chơi
        public void Run()
        {
            try
            {
                InitializeDefaultData(); // Khởi tạo dữ liệu mặc định (công ty)
                MainMenuScreen(); // Hiển thị menu chính
            }
            finally
            {
                // Khôi phục màu sắc và hiển thị con trỏ khi thoát trò chơi
                Console.ResetColor();
                Console.CursorVisible = true;
            }
        }

       

        private void InitializeDefaultData()
        {
            // Load company data from a JSON file
            string jsonData = File.ReadAllText("Companies.json");
            Companies = JsonSerializer.Deserialize<List<Company>>(jsonData);

       

            if (Companies == null)
            {
                Console.WriteLine("Failed to load companies from JSON.");
                CloseRequested = true; // or handle it as needed
            }
        }

        private void MainMenuScreen()
        {
            while (!CloseRequested)
            {
                Console.Clear();

                StringBuilder prompt = new StringBuilder();
                prompt.AppendLine("\n███╗   ███╗ ██████╗ ███╗   ██╗███████╗██╗   ██╗              ███████╗████████╗ ██████╗  ██████╗██╗  ██╗██╗   ██╗    ");
                prompt.AppendLine("████╗ ████║██╔═══██╗████╗  ██║██╔════╝╚██╗ ██╔╝              ██╔════╝╚══██╔══╝██╔═══██╗██╔════╝██║ ██╔╝╚██╗ ██╔╝    ");
                prompt.AppendLine("██╔████╔██║██║   ██║██╔██╗ ██║█████╗   ╚████╔╝     █████╗    ███████╗   ██║   ██║   ██║██║     █████╔╝  ╚████╔╝     ");
                prompt.AppendLine("██║╚██╔╝██║██║   ██║██║╚██╗██║██╔══╝    ╚██╔╝      ╚════╝    ╚════██║   ██║   ██║   ██║██║     ██╔═██╗   ╚██╔╝      ");
                prompt.AppendLine("██║ ╚═╝ ██║╚██████╔╝██║ ╚████║███████╗   ██║                 ███████║   ██║   ╚██████╔╝╚██████╗██║  ██╗   ██║       ");
                prompt.AppendLine("╚═╝     ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝   ╚═╝                 ╚══════╝   ╚═╝    ╚═════╝  ╚═════╝╚═╝  ╚═╝   ╚═╝       ");

                prompt.AppendLine("\nBạn có thể thoát khỏi trò chơi bất cứ lúc nào bằng cách nhấn ESC.");
                prompt.AppendLine("Sử dụng phím mũi tên lên xuống và Enter để chọn một tùy chọn:");

                int selectedIndex = HandleMenuWithOptions(prompt.ToString(),
                    new string[] { "Chơi", "Thông tin", "Thoát" });

                switch (selectedIndex)
                {
                    case 0: IntroductionScreen(); break;
                    case 1: AboutInfoScreen(); break;
                    case 2: CloseRequested = true; break;
                }
            }
        }

        private void InitializeGame()
        {
            Money = 3000.00m;
            InitializeDefaultData();
            //LoadEmbeddedResources();
            InitializeEvents(); // Ensure events are initialized
        }

        private void GameLoop()
        {
            while (!CloseRequested && CalculateNetWorth() > LosingNetWorth && CalculateNetWorth() < WinningNetWorth)
            {
                // Randomly trigger an event
                if (Random.Shared.Next(0, 5) == 0) // 20% chance to trigger an event
                {
                    EventScreen();
                }

                int selectedIndex = HandleMenuWithOptions(RenderCompanyStocksTable().ToString(),
                    new string[] { "Đợi Thay Đổi Thị Trường", "Mua", "Bán", "Thông Tin Về Các Công Ty" });

                switch (selectedIndex)
                {
                    case 0: EventScreen(); break;
                    case 1: BuyOrSellStockScreen(true); break;
                    case 2: BuyOrSellStockScreen(false); break;
                    case 3: CompanyDetailsScreen(); break;
                }
            }

            if (CalculateNetWorth() >= WinningNetWorth)
            {
                PlayerWinsScreen();
            }
            else if (CalculateNetWorth() <= LosingNetWorth)
            {
                PlayerLosesScreen();
            }
        }

        private void EventScreen()
        {
            // Randomly select an event
            if (Events.Count > 0)
            {
                Event randomEvent = Events[Random.Shared.Next(Events.Count)];

                // Apply the event effect to the corresponding company
                Company affectedCompany = Companies.FirstOrDefault(c => c.Name == randomEvent.CompanyName);
                if (affectedCompany != null)
                {
                    affectedCompany.SharePrice += affectedCompany.SharePrice * randomEvent.Effect / 100;
                }

                StringBuilder prompt = RenderCompanyStocksTable();
                prompt.AppendLine();
                prompt.AppendLine($"Tin tức thị trường: {randomEvent.Description}");
                prompt.AppendLine();
                prompt.Append("Nhấn phím bất kỳ để tiếp tục...");

                Console.Clear();
                Console.Write(prompt);
                Console.CursorVisible = false;
                CloseRequested = CloseRequested || Console.ReadKey(true).Key == ConsoleKey.Escape;
            }
        }

        private void BuyOrSellStockScreen(bool isBuying)
        {
            int[] numberOfShares = new int[Companies.Count];
            int index = 0;
            ConsoleKey key;
            do
            {
                Console.Clear();
                Console.WriteLine(RenderCompanyStocksTable());
                Console.WriteLine();
                Console.WriteLine($"Sử dụng phím mũi tên để di chuyển, ←→ để thay đổi số lượng cổ phiếu muốn {(isBuying ? "mua" : "bán")}:");
                Console.WriteLine("Nhấn Enter để xác nhận hoặc Esc để hủy.");

                for (int i = 0; i < Companies.Count; i++)
                {
                    if (i == index)
                    {
                        Console.BackgroundColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    Console.WriteLine($"[{(i == index ? "*" : " ")}] {numberOfShares[i],3} {Companies[i].Name}");
                    Console.ResetColor();
                }

                decimal cost = numberOfShares.Select((shares, i) => shares * Companies[i].SharePrice).Sum();
                Console.WriteLine();
                Console.WriteLine($"{(isBuying ? "Chi phí" : "Thu về")}: {cost:C}");

                Console.CursorVisible = false;
                key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        index = (index == 0) ? Companies.Count - 1 : index - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        index = (index == Companies.Count - 1) ? 0 : index + 1;
                        break;
                    case ConsoleKey.LeftArrow:
                        if (numberOfShares[index] > 0)
                        {
                            numberOfShares[index]--;
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (isBuying)
                        {
                            if (Money - cost >= Companies[index].SharePrice)
                            {
                                numberOfShares[index]++;
                            }
                        }
                        else
                        {
                            if (numberOfShares[index] < Companies[index].NumberOfShares)
                            {
                                numberOfShares[index]++;
                            }
                        }
                        break;
                    case ConsoleKey.Escape:
                        return;
                }
            } while (key != ConsoleKey.Enter);

            // Xử lý mua/bán cổ phiếu
            for (int i = 0; i < Companies.Count; i++)
            {
                if (isBuying)
                {
                    Money -= numberOfShares[i] * Companies[i].SharePrice;
                    Companies[i].NumberOfShares += numberOfShares[i];
                }
                else
                {
                    Money += numberOfShares[i] * Companies[i].SharePrice;
                    Companies[i].NumberOfShares -= numberOfShares[i];
                }
            }

            Console.Clear();
            Console.WriteLine(RenderCompanyStocksTable());
            Console.WriteLine($"Số lượng cổ phiếu của bạn đã được cập nhật.");
            Console.WriteLine();
            Console.Write("Nhấn phím bất kỳ để tiếp tục...");
            Console.ReadKey(true);
        }

        private int GetMaxShares(bool isBuying, int index, decimal currentCost)
        {
            if (isBuying)
            {
                return (int)((Money - currentCost) / Companies[index].SharePrice);
            }
            else
            {
                return Companies[index].NumberOfShares;
            }
        }

        private void CompanyDetailsScreen()
        {
            Console.Clear();
            foreach (Company company in Companies)
            {
                Console.WriteLine($"{company.Name} - {company.Description}");
                Console.WriteLine();
            }
            Console.Write("Nhấn phím bất kỳ để thoát menu...");
            Console.CursorVisible = false;
            CloseRequested = CloseRequested || Console.ReadKey(true).Key == ConsoleKey.Escape;
        }


        private int HandleMenuWithOptions(string prompt, string[] options)
        {
            int index = 0;
            ConsoleKey key = default;
            while (!CloseRequested && key != ConsoleKey.Enter)
            {
                Console.Clear();
                Console.WriteLine(prompt);
                for (int i = 0; i < options.Length; i++)
                {
                    string currentOption = options[i];
                    if (i == index)
                    {
                        Console.BackgroundColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($"[*] {currentOption}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"[ ] {currentOption}");
                    }
                }
                Console.CursorVisible = false;
                key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow: index = (index == 0) ? options.Length - 1 : index - 1; break;
                    case ConsoleKey.DownArrow: index = (index == options.Length - 1) ? 0 : index + 1; break;
                    case ConsoleKey.Escape: CloseRequested = true; break;
                }
            }
            return index;
        }

        private decimal CalculateNetWorth()
        {
            decimal netWorth = Money;
            foreach (Company company in Companies)
            {
                netWorth += company.SharePrice * company.NumberOfShares;
            }
            return netWorth;
        }



        private void InitializeEvents()
        {
            string jsonFilePath = "events.json"; // Đường dẫn đến file JSON của bạn
            if (File.Exists(jsonFilePath))
            {
                // Đọc nội dung của file JSON
                string jsonContent = File.ReadAllText(jsonFilePath);

                // Deserialize JSON thành danh sách các sự kiện
                Events = JsonSerializer.Deserialize<List<Event>>(jsonContent);
            }
            else
            {
                Console.WriteLine("File JSON không tồn tại!");
                Events = new List<Event>();  // Khởi tạo danh sách rỗng nếu file không tồn tại
            }
        }

        private void IntroductionScreen()
        {
            Console.Clear();
            Console.WriteLine(@"
    ┌────────────────────────────────────────────────────────────────────────────────┐
    │ Kính gửi CEO,                                                                  │
    │                                                                                │
    │ Chào mừng bạn đến với Stocky!                                                  │
    │                                                                                │
    │ Thay mặt hội đồng quản trị của Stocky Investments, chúng tôi xin chúc mừng     │
    │ bạn đã trở thành CEO mới của chúng tôi. Với tư cách là CEO, bạn giờ đây có     │
    │ quyền truy cập vào phần mềm nội bộ độc quyền của chúng tôi có tên là Stocky,   │
    │ nơi bạn có thể theo dõi tin tức mới nhất từ các công ty hàng đầu và mua bán    │
    │ cổ phiếu của họ.                                                               │
    │                                                                                │
    │ Trân trọng,                                                                    │
    │ Hội đồng quản trị Stocky Investments                                           │
    └────────────────────────────────────────────────────────────────────────────────┘
");
            Console.Write("Nhấn phím bất kỳ để tiếp tục...");
            Console.CursorVisible = false;
            CloseRequested = CloseRequested || Console.ReadKey(true).Key == ConsoleKey.Escape;
            InitializeGame();
            GameLoop();
        }

        private void PlayerWinsScreen()
        {
            Console.Clear();

            string[] winnn =
           {


"                         ,,,,,            ▓▓▓▓▓▓▓▓▓▄,                             ",
"              ╓▄▓▓▓▓▓▓▓▓▓▓▓▓▓▓       ,╦▓╣╢╢╣▓▓▓▓▓▓▓▄                              ",
"          ╓@▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▄  ╔▒╢╢╢╢╢▓╢▓▓▓▓▓▓▓▓      ,,,                    ",
"       ,@▓▓╣╢╢╣▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▄▒╢╢╢╢╢╢▓╢▓▓▓▒▒▓▓╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╣▓▓▄           ",
"     ╓╣╢╢╢╢╢╢╢╢▓▓▓▓▓▓▓▓▓▓▓╢╢╣▓▓▓▓▓▓▓▓╢╢╢╢╢╢▓╣╢▓▓▓▒▒▓▓╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╣▓▓▄         ",
"    ╣║╢╢╢╢╢╢╢╢╢╢╣▓▓▓▓▓▓▓▓╣╢╢╢╢╣▓▓▓▓▓▓▓╢╢╢╢╢▓╣▒▓▒▓▓╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▄        ",
"   ╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▓▓▓▒╢╢╢╫▓▓▒▓▓╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▓       ",
"  ║╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╣▓▓▓▓▓╢╢╢▓▒╢▓▓╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▓▄     ",
"  ║╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╣▓▓▓▓▒╢▒▒╢▓╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▓▓▌    ",
"  ╘╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▌║▒╢▓╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╣╣╣╢╢▓▓▓▓▓▌    ",
"   ╙╣╢╢╣╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╫▓▓▓▒╢╫╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▓▓▓▓▓▓▓▓▓▓▌   ",
"     ╙╝▒╣╣╣╢╢╢╫╬╣╣╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▓▓▓▓▓▓▓▓▓▓        ",
"                  ▄╢▓▓▓▓▓▓▓▒╢╣╣╢╢╢╢╢╢╢▓▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▓▓▓▓▓▓▓▓▓▓   ",
"                 ▐▓▓▓▓▓▓▓▓▓▓▒▒╢╢╢╢╢╢╢╢▓╢╢╢╢╢╢╢╢▒▒▒╩╝╝╝╢╢╣╣╣╣╣╢╣▓▓▓▓▓▓▓▓▓▓▓▓       ",
"                 ╣▓▓▓▓▓▓▓▓▓▓▓▓▓╢╢╢╢╢╢╢╣╢╢╢╢╢╢╢╢╢╫▓▓▓▓,    '▀▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▀      ",
"                 ▀╢▓▓▓▓▀└╔▓▓▓▓▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╣▓▓▓▄        `└╙▀▀▀▀▀▀╙.           ",
"                         ╙▀▀▀▒╢╢╢╢╢╢╢▓╚╢╢╢╢╢▓▓╢╢╢╢╢▓▓▄                            ",
"                            ╢╢╢╫╝'╙╨╜    ╙╜`   ╩╢╣╢╢▓▓▄                           ",
"                           /╨╙'   r^          '═  '╙╙╙▀L                          ",
"                          ╒                            ▒,                         ",
"                                  💖         💖        └▒                        ",
"               [ *`ⁿ.    ╛                              ▒▒    ' M`]               ",
"              ]`╙     ⌐ ▓█▄           ▐╦╥▄╦▓            └▒▄ ^    '`ƒ              ",
"              ┐        ▓▓▓▓▓▄          └╙▀            ,▄█▓▓▀       ;              ",
"               j         ╘▀▓▓▓▓▄                     ,█▓▓▓▀        ╒               ",
"                Y          ╙▓▓▓▓█                   ▄▓▓▓▓╘        ¿                ",
"                 ^          ▓▓▓▓▓█                 ▐▓▓▓▓▌        ╛                 ",
"                  l         ▐▓▓▓▓▓▓▄▄▄▓████▓▄▄▄▄▄,,▓▓▓▓▓▌      ⌐                   ",
"                   l       ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀                          ",


            };

            Program.DrawCenteredBorder(winnn);
            Console.ForegroundColor = ConsoleColor.Cyan;
            string[] youwin =
            {
                " Kính gửi CEO,                                                                  ",
                "                                                                                ",
                " Thay mặt hội đồng quản trị Stocky Investments, chúng tôi xin bày tỏ lòng biết  ",
                " ơn đối với bạn. Bạn đã là một nhà lãnh đạo xuất sắc và một chiến lược gia tầm  ",
                " nhìn, người đã chơi thị trường chứng khoán một cách khéo léo. Cảm ơn bạn vì    ",
                " những đóng góp và dịch vụ của bạn cho Stocky Investments.                      ",
                "                                                                                ",
                " Trân trọng,                                                                    ",
                " Hội đồng quản trị của Stocky Investments                                       ",
                
            };
            Program.DrawCenteredBorder(youwin);
            Console.ResetColor();

            Console.WriteLine($"Giá trị tài sản ròng của bạn đã vượt quá {WinningNetWorth:C}.");
            Console.WriteLine("\nBạn đã chiến thắng! Xin chúc mừng!");
            Console.WriteLine("\nNhấn phím bất kỳ (trừ ENTER) để tiếp tục...");
            ConsoleKey key;
            do
            {
                Console.CursorVisible = false;
                key = Console.ReadKey(true).Key;
                CloseRequested = CloseRequested || key == ConsoleKey.Escape;
            } while (!CloseRequested && key == ConsoleKey.Enter);
        }



        private void PlayerLosesScreen()
        {
            Console.Clear();
            Console.WriteLine(@"
    ┌────────────────────────────────────────────────────────────────────────────────┐
    │ Kính gửi cựu CEO,                                                              │
    │                                                                                │
    │ Chúng tôi rất tiếc phải thông báo rằng bạn đang bị loại khỏi vị trí CEO        │
    │ và bị sa thải khỏi công ty. Khoản đầu tư của bạn không có lợi nhuận cho công   │
    │ ty. Chúng tôi đánh giá cao sự phục vụ của bạn và chúc bạn mọi điều tốt đẹp     │
    │ nhất trong những nỗ lực tương lai của bạn.                                     │
    │                                                                                │
    │ Trân trọng,                                                                    │
    │ Hội đồng quản trị của Stocky Investments                                       │
    └────────────────────────────────────────────────────────────────────────────────┘
");
            Console.WriteLine($"Giá trị tài sản ròng của bạn đã giảm xuống dưới {LosingNetWorth:C}.");
            Console.WriteLine("\nBạn đã thua! Hãy thử lại lần sau...");
            Console.WriteLine("\nNhấn phím bất kỳ (trừ ENTER) để tiếp tục...");
            ConsoleKey key;
            do
            {
                Console.CursorVisible = false;
                key = Console.ReadKey(true).Key;
                CloseRequested = CloseRequested || key == ConsoleKey.Escape;
            } while (!CloseRequested && key == ConsoleKey.Enter);
        }

        private void AboutInfoScreen()
        {
            Console.Clear();
            Console.WriteLine(@"
    |
    | CẢM ƠN!
    |
    | Thật sự, cảm ơn bạn đã dành thời gian để chơi trò chơi console đơn giản này. Điều đó có ý nghĩa rất lớn.
    |
    | Trò chơi này được tạo bởi Semion Medvedev (Fuinny) và được Việt hóa bởi [Minh Thư, Anh Thư, Khải Nhi,Thảo Vy]
    |
");
            Console.WriteLine("\nNhấn phím bất kỳ để tiếp tục...");
            Console.CursorVisible = false;
            CloseRequested = CloseRequested || Console.ReadKey(true).Key == ConsoleKey.Escape;
        }

        private StringBuilder RenderCompanyStocksTable()
        {
            const int c0 = 30, c1 = 15, c2 = 15, c3 = 10;

            StringBuilder gameView = new StringBuilder();
            gameView.AppendLine($"╔═{new string('═', c0)}═╤═{new string('═', c1)}═╤═{new string('═', c2)}═╤═{new string('═', c3)}═╗");
            gameView.AppendLine($"║ {"Công ty",-c0} │ {"Ngành",c1} │ {"Giá cổ phiếu",c2} │ {"Bạn có",c3} ║");
            gameView.AppendLine($"╟─{new string('─', c0)}─┼─{new string('─', c1)}─┼─{new string('─', c2)}─┼─{new string('─', c3)}─╢");
            foreach (Company company in Companies)
            {
                gameView.AppendLine($"║ {company.Name,-c0} │ {company.Industry,c1} │ {company.SharePrice,c2:C2} │ {company.NumberOfShares,c3} ║");
            }
            gameView.AppendLine($"╚═{new string('═', c0)}═╧═{new string('═', c1)}═╧═{new string('═', c2)}═╧═{new string('═', c3)}═╝");
            gameView.AppendLine($"Tiền của bạn: {Money:C2}    Giá trị tài sản ròng: {CalculateNetWorth():C2}");
            return gameView;
        }

    }
}