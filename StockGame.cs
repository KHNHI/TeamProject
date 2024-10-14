﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Text.Json;
//h
namespace Quanlychitieu
{
    internal class StockGame
    {
#nullable disable

        public class Company
        {
            public string Name { get; set; }
            public string Industry { get; set; }
            public decimal SharePrice { get; set; }
            public int NumberOfShares { get; set; }
            public string Description { get; set; }
        }

        public class Event
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string CompanyName { get; set; }
            public int Effect { get; set; }
        }

        private bool CloseRequested { get; set; } = false;
        private List<Company> Companies { get; set; } = null!;
        private List<Event> Events { get; set; } = null!;
        private Event CurrentEvent { get; set; } = null!;
        private decimal Money { get; set; }
        private const decimal LosingNetWorth = 2000.00m;
        private const decimal WinningNetWorth = 50000.00m;

        public void Run()
        {
            try
            {
                MainMenuScreen();
            }
            finally
            {
                Console.ResetColor();
                Console.CursorVisible = true;
            }
        }

        private void LoadEmbeddedResources()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();

            // In ra tất cả tên tài nguyên để kiểm tra
            foreach (string name in resourceNames)
            {
                Console.WriteLine($"Found resource: {name}");
            }

            try
            {
                using (Stream stream = assembly.GetManifestResourceStream("Quanlychitieu.Company.json"))
                {
                    if (stream == null)
                    {
                        throw new FileNotFoundException("Không tìm thấy file Company.json");
                    }
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string jsonContent = reader.ReadToEnd();
                        Companies = JsonSerializer.Deserialize<List<Company>>(jsonContent);
                    }
                }

                // Tạm thời bỏ qua phần Event.json
                // InitializeEvents();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tải tài nguyên: {ex.Message}");
                InitializeDefaultData();
            }
        }

        private void InitializeDefaultData()
        {
            Companies = new List<Company>
            {
                new Company { Name = "Vingroup", Industry = "Đa ngành", SharePrice = 100.00m, NumberOfShares = 0, Description = "Tập đoàn đa ngành hàng đầu Việt Nam" },
                new Company { Name = "Viettel", Industry = "Viễn thông", SharePrice = 50.00m, NumberOfShares = 0, Description = "Tập đoàn viễn thông và công nghệ thông tin lớn nhất Việt Nam" },
                new Company { Name = "VNPay", Industry = "Fintech", SharePrice = 80.00m, NumberOfShares = 0, Description = "Công ty công nghệ tài chính hàng đầu" },
                new Company { Name = "Masan Group", Industry = "Hàng tiêu dùng", SharePrice = 70.00m, NumberOfShares = 0, Description = "Tập đoàn kinh tế tư nhân hàng đầu Việt Nam" },
                new Company { Name = "FPT", Industry = "Công nghệ", SharePrice = 90.00m, NumberOfShares = 0, Description = "Công ty công nghệ thông tin và dịch vụ viễn thông lớn nhất Việt Nam" }
            };

            // Tạm thời bỏ qua phần Event
        }

        private void MainMenuScreen()
        {
            while (!CloseRequested)
            {
                StringBuilder prompt = new StringBuilder();
                prompt.AppendLine("\n     ██████╗ ██╗     ██╗ ██████╗  ██████╗ ██████╗  ██████╗ ██╗  ██╗   ██╗");
                prompt.AppendLine("    ██╔═══██╗██║     ██║██╔════╝ ██╔═══██╗██╔══██╗██╔═══██╗██║  ╚██╗ ██╔╝");
                prompt.AppendLine("    ██║   ██║██║     ██║██║  ███╗██║   ██║██████╔╝██║   ██║██║   ╚████╔╝ ");
                prompt.AppendLine("    ██║   ██║██║     ██║██║   ██║██║   ██║██╔═══╝ ██║   ██║██║    ╚██╔╝  ");
                prompt.AppendLine("    ╚██████╔╝███████╗██║╚██████╔╝╚██████╔╝██║     ╚██████╔╝███████╗██║   ");
                prompt.AppendLine("     ╚═════╝ ╚══════╝╚═╝ ╚═════╝  ╚═════╝ ╚═╝      ╚═════╝ ╚══════╝╚═╝   ");
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
            Money = 10000.00m;
            LoadEmbeddedResources();
        }

        private void GameLoop()
        {
            while (!CloseRequested && CalculateNetWorth() > LosingNetWorth && CalculateNetWorth() < WinningNetWorth)
            {
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
            // Tạo một sự kiện ngẫu nhiên
            string[] eventTypes = { "tích cực", "tiêu cực", "trung lập" };
            string eventType = eventTypes[Random.Shared.Next(eventTypes.Length)];

            // Chọn một công ty ngẫu nhiên
            Company affectedCompany = Companies[Random.Shared.Next(Companies.Count)];

            // Tạo hiệu ứng ngẫu nhiên (từ -10% đến +10%)
            int effect = Random.Shared.Next(-10, 11);

            // Tạo mô tả sự kiện
            string eventDescription = GenerateEventDescription(affectedCompany.Name, eventType, effect);

            // Áp dụng hiệu ứng
            affectedCompany.SharePrice += affectedCompany.SharePrice * effect / 100;

            StringBuilder prompt = RenderCompanyStocksTable();
            prompt.AppendLine();
            prompt.AppendLine($"Tin tức thị trường: {eventDescription}");
            prompt.AppendLine();
            prompt.Append("Nhấn phím bất kỳ để tiếp tục...");

            Console.Clear();
            Console.Write(prompt);
            Console.CursorVisible = false;
            CloseRequested = CloseRequested || Console.ReadKey(true).Key == ConsoleKey.Escape;
        }

        private string GenerateEventDescription(string companyName, string eventType, int effect)
        {
            string direction = effect > 0 ? "tăng" : "giảm";
            string intensity = Math.Abs(effect) > 5 ? "mạnh" : "nhẹ";

            switch (eventType)
            {
                case "tích cực":
                    return $"{companyName} vừa công bố kết quả kinh doanh vượt kỳ vọng. Giá cổ phiếu {direction} {intensity}.";
                case "tiêu cực":
                    return $"{companyName} gặp khó khăn trong hoạt động kinh doanh. Giá cổ phiếu {direction} {intensity}.";
                default:
                    return $"{companyName} có một số thay đổi nhỏ trong hoạt động. Giá cổ phiếu {direction} {intensity}.";
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

        private void IntroductionScreen()
        {
            Console.Clear();
            Console.WriteLine(@"
    ┌────────────────────────────────────────────────────────────────────────────────┐
    │ Kính gửi CEO,                                                                  │
    │                                                                                │
    │ Chào mừng bạn đến với Oligopoly!                                               │
    │                                                                                │
    │ Thay mặt hội đồng quản trị của Oligopoly Investments, chúng tôi xin chúc mừng  │
    │ bạn đã trở thành CEO mới của chúng tôi. Chúng tôi tin tưởng rằng bạn sẽ đưa    │
    │ công ty của chúng ta lên tầm cao mới về thành công và đổi mới. Với tư cách là  │
    │ CEO, bạn giờ đây có quyền truy cập vào phần mềm nội bộ độc quyền của chúng tôi │
    │ có tên là Oligopoly, nơi bạn có thể theo dõi tin tức mới nhất từ các công ty   │
    │ hàng đầu và mua bán cổ phiếu của họ. Phần mềm này sẽ giúp bạn có lợi thế cạnh  │
    │ tranh và hỗ trợ bạn đưa ra các quyết định quan trọng cho công ty chúng ta. Để  │
    │ truy cập chương trình, chỉ cần nhấn nút ở cuối email này. Chúng tôi mong được  │
    │ làm việc với bạn và hỗ trợ bạn trong vai trò mới của mình.                     │
    │                                                                                │
    │ Trân trọng,                                                                    │
    │ Hội đồng quản trị Oligopoly Investments                                        │
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
            Console.WriteLine(@"
    ┌────────────────────────────────────────────────────────────────────────────────┐
    │ Kính gửi CEO,                                                                  │
    │                                                                                │
    │ Thay mặt hội đồng quản trị của Oligopoly Investments, chúng tôi xin bày tỏ     │
    │ lòng biết ơn và thấu hiểu đối với quyết định rời khỏi vị trí của bạn. Bạn đã   │
    │ là một nhà lãnh đạo xuất sắc và một chiến lược gia tầm nhìn, người đã chơi     │
    │ thị trường chứng khoán một cách khéo léo và tăng ngân sách của chúng ta lên    │
    │ gấp năm lần. Chúng tôi tự hào về những thành tích của bạn và chúc bạn mọi      │
    │ điều tốt đẹp nhất trong những nỗ lực tương lai. Như một dấu hiệu của sự        │
    │ đánh giá cao của chúng tôi, chúng tôi vui mừng thông báo rằng công ty sẽ       │
    │ trả cho bạn một khoản thưởng 1 tỷ đồng. Bạn xứng đáng nhận được phần thưởng    │
    │ này cho sự làm việc chăm chỉ và cống hiến của mình. Chúng tôi hy vọng bạn      │
    │ sẽ tận hưởng nó và nhớ đến chúng tôi một cách trìu mến. Cảm ơn bạn vì          │
    │ những đóng góp và dịch vụ của bạn cho Oligopoly Investments.                   │
    │ Chúng tôi sẽ nhớ bạn.                                                          │
    │                                                                                │
    │ Trân trọng,                                                                    │
    │ Hội đồng quản trị của Oligopoly Investments                                    │
    └────────────────────────────────────────────────────────────────────────────────┘
");
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
    │ và bị sa thải khỏi công ty, có hiệu lực ngay lập tức. Hội đồng quản trị của    │
    │ Oligopoly Investments đã quyết định thực hiện hành động này vì bạn đã chi tiêu │
    │ hết ngân sách được phân bổ cho bạn, và khoản đầu tư của bạn hóa ra không có    │
    │ lợi nhuận cho công ty. Chúng tôi đánh giá cao sự phục vụ của bạn và chúc bạn   │
    │ mọi điều tốt đẹp nhất trong những nỗ lực tương lai của bạn.                    │
    │                                                                                │
    │ Trân trọng,                                                                    │
    │ Hội đồng quản trị của Oligopoly Investments                                    │
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
    | Trò chơi này được tạo bởi Semion Medvedev (Fuinny) và được Việt hóa bởi [Tên của bạn]
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
            Events = new List<Event>
            {
                // Vingroup
                new Event { Title = "Vingroup ra mắt mẫu xe điện mới", Description = "VinFast, công ty con của Vingroup, vừa ra mắt mẫu xe điện mới với công nghệ tiên tiến.", CompanyName = "Vingroup", Effect = 8 },
                new Event { Title = "Vingroup mở rộng chuỗi bệnh viện", Description = "Vinmec, hệ thống y tế của Vingroup, công bố kế hoạch mở thêm nhiều bệnh viện trên cả nước.", CompanyName = "Vingroup", Effect = 5 },
                new Event { Title = "Vingroup gặp khó khăn trong dự án bất động sản", Description = "Một dự án bất động sản lớn của Vingroup bị trì hoãn do vấn đề pháp lý.", CompanyName = "Vingroup", Effect = -7 },

                // Viettel
                new Event { Title = "Viettel triển khai 5G trên toàn quốc", Description = "Viettel công bố kế hoạch triển khai mạng 5G trên toàn quốc trong năm nay.", CompanyName = "Viettel", Effect = 10 },
                new Event { Title = "Viettel giành được hợp đồng quốc phòng lớn", Description = "Viettel ký kết hợp đồng cung cấp giải pháp an ninh mạng cho Bộ Quốc phòng.", CompanyName = "Viettel", Effect = 12 },
                new Event { Title = "Viettel gặp sự cố mạng lưới", Description = "Mạng di động của Viettel gặp sự cố trên diện rộng, ảnh hưởng đến nhiều khách hàng.", CompanyName = "Viettel", Effect = -8 },

                // VNPay
                new Event { Title = "VNPay hợp tác với chuỗi bán lẻ lớn", Description = "VNPay ký kết thỏa thuận hợp tác với một trong những chuỗi siêu thị lớn nhất Việt Nam.", CompanyName = "VNPay", Effect = 9 },
                new Event { Title = "VNPay ra mắt tính năng thanh toán mới", Description = "VNPay giới thiệu tính năng thanh toán bằng nhận diện khuôn mặt, nâng cao trải nghiệm người dùng.", CompanyName = "VNPay", Effect = 7 },
                new Event { Title = "VNPay bị điều tra về bảo mật dữ liệu", Description = "Cơ quan chức năng mở cuộc điều tra về cách VNPay xử lý dữ liệu cá nhân của khách hàng.", CompanyName = "VNPay", Effect = -10 },

                // Masan Group
                new Event { Title = "Masan mua lại chuỗi cửa hàng tiện lợi", Description = "Masan Group thông báo mua lại một chuỗi cửa hàng tiện lợi lớn, mở rộng mạng lưới bán lẻ.", CompanyName = "Masan Group", Effect = 11 },
                new Event { Title = "Masan ra mắt sản phẩm thực phẩm mới", Description = "Masan Consumer, công ty con của Masan Group, giới thiệu dòng sản phẩm thực phẩm chức năng mới.", CompanyName = "Masan Group", Effect = 6 },
                new Event { Title = "Masan gặp khó khăn trong hoạt động khai thác", Description = "Masan High-Tech Materials báo cáo sản lượng khai thác giảm do điều kiện thị trường không thuận lợi.", CompanyName = "Masan Group", Effect = -9 },

                // FPT
                new Event { Title = "FPT ký hợp đồng outsourcing lớn", Description = "FPT Software ký kết hợp đồng phát triển phần mềm trị giá hàng trăm triệu đô la với đối tác nước ngoài.", CompanyName = "FPT", Effect = 13 },
                new Event { Title = "FPT mở rộng đầu tư vào AI và IoT", Description = "FPT công bố kế hoạch đầu tư mạnh mẽ vào nghiên cứu và phát triển AI và IoT.", CompanyName = "FPT", Effect = 8 },
                new Event { Title = "FPT Education gặp khó khăn trong tuyển sinh", Description = "Hệ thống giáo dục của FPT báo cáo số lượng sinh viên đăng ký giảm trong năm học mới.", CompanyName = "FPT", Effect = -6 }
            };
        }
    }
}