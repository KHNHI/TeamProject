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
            // get: Được dùng để truy xuất (đọc) giá trị của một thuộc tính.
           //  set: Được dùng để gán(ghi) giá trị cho một thuộc tính.

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
            public int Effect { get; set; }  // Tác động của sự kiện đến giá cổ phiếu
        }

        // Kiểm tra xem người dùng có yêu cầu thoát game không
        private bool CloseRequested { get; set; } = false;

        // Danh sách các công ty có trong trò chơi
        private List<Company> Companies { get; set; } = null!;

        // Danh sách các sự kiện có thể xảy ra trong trò chơi
        private List<Event> Events { get; set; } = null!;

        // Số tiền hiện tại người chơi có
        private decimal Money { get; set; }

        // Giá trị tài sản tối thiểu khi thua game
        private const decimal LosingNetWorth = 2000.00m;

        // Giá trị tài sản tối đa để thắng game
        private const decimal WinningNetWorth = 4000.00m;

        // Hàm khởi động trò chơi
        public void Run()
        {
            // dùng try-finally để đảm bảo rằng ngay cả khi InitializeDefaultData() và  MainMenuScreen() lỗi
            // màu sắc console và sự hiển thị của con trỏ sẽ được khôi phục về trạng thái ban đầu
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
            // Tải dữ liệu công ty từ file JSON 
            string jsonData = File.ReadAllText("Companies.json");
            //chuyển đổi (deserialize) chuỗi JSON thành một danh sách các đối tượng Company
            Companies = JsonSerializer.Deserialize<List<Company>>(jsonData);


            // Kiểm tra việc tải dữ liệu có thành công hay không
            if (Companies == null)
            {
                Console.WriteLine(" Tải dữ liệu từ file Companies JSON đã thất bại.");
                // yêu cầu đóng ứng dụng
                CloseRequested = true;
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

                //hiển thị menu và lấy chỉ số của tùy chọn mà người dùng đã chọn.
                int selectedIndex = HandleMenuWithOptions(prompt.ToString(),
                    new string[] { "Chơi", "Thông tin", "Thoát" });

                // dùng switch để xử lý lựa chọn của người dùng
                switch (selectedIndex)
                {
                    case 0: IntroductionScreen(); break;
                    case 1: AboutInfoScreen(); break;
                    case 2: CloseRequested = true; break;
                }
            }
        }

        // HÀM KHỞI TẠO TRÒ CHƠI SAU KHI IN IntroductionScreen()
        private void InitializeGame()
        {
            // Số tiền vốn ban đầu của người chơi là 3000
            Money = 3000.00m;
            InitializeDefaultData();
            InitializeEvents(); 
        }

        private void GameLoop()
        {
            // Vòng lặp chính của trò chơi
            while (!CloseRequested && CalculateNetWorth() > LosingNetWorth && CalculateNetWorth() < WinningNetWorth)
            {
                // Hiển thị bảng cổ phiếu công ty và lấy lựa chọn của người chơi
                int selectedIndex = HandleMenuWithOptions(RenderCompanyStocksTable().ToString(),
                    new string[] { "Đợi Thay Đổi Thị Trường", "Mua", "Bán", "Thông Tin Về Các Công Ty" });

                // Xử lý lựa chọn của người chơi
                switch (selectedIndex) 
                {
                    case 0: EventScreen(); break;
                    case 1: BuyOrSellStockScreen(true); break;  // Gọi hàm để mua cổ phiếu  
                    case 2: BuyOrSellStockScreen(false); break;  // Gọi hàm để bán cổ phiếu
                    case 3: CompanyDetailsScreen(); break;  //// Hiển thị thông tin chi tiết về các công ty
                }
            }

            if (CalculateNetWorth() >= WinningNetWorth) // Nếu tài sản ròng đạt đủ điểm thắng
            {
                PlayerWinsScreen();
            }
            else if (CalculateNetWorth() <= LosingNetWorth) // Nếu tài sản ròng đạt đủ điểm thua
            {
                PlayerLosesScreen();
            }
        }

        private void EventScreen()
        {
            // Ngẫu nhiên chọn một sự kiện
            if (Events.Count > 0) // Kiểm tra xem có sự kiện nào trong danh sách không
            {
                Event randomEvent = Events[Random.Shared.Next(Events.Count)]; // Chọn một sự kiện ngẫu nhiên từ danh sách sự kiện

                // Áp dụng tác động của sự kiện lên công ty tương ứng
                Company affectedCompany = Companies.FirstOrDefault(c => c.Name == randomEvent.CompanyName);// Tìm công ty bị ảnh hưởng dựa trên tên công ty trong sự kiện
                if (affectedCompany != null) // Nếu công ty bị ảnh hưởng được tìm thấy
                {
                    // Cập nhật giá cổ phiếu của công ty dựa trên tác động của sự kiện
                    affectedCompany.SharePrice += affectedCompany.SharePrice * randomEvent.Effect / 100;
                }

                // Hiển thị bảng cổ phiếu công ty
                StringBuilder prompt = RenderCompanyStocksTable();
                prompt.AppendLine();
                prompt.AppendLine($"Tin tức thị trường: {randomEvent.Description}");// Thêm mô tả sự kiện vào thông báo
                prompt.AppendLine();
                prompt.Append("Nhấn phím bất kỳ để tiếp tục...");

                Console.Clear();
                Console.Write(prompt);
                Console.CursorVisible = false; // Ẩn con trỏ chuột
                CloseRequested = CloseRequested || Console.ReadKey(true).Key == ConsoleKey.Escape;
            }
        }

        // Phương thức riêng BuyOrSellStockScreen, xử lý giao diện mua/bán cổ phiếu
        private void BuyOrSellStockScreen(bool isBuying)
        {
            // Khởi tạo mảng lưu số lượng cổ phiếu cho từng công ty
            int[] numberOfShares = new int[Companies.Count];
            int index = 0; // Chỉ số của công ty hiện tại
            ConsoleKey key; // Biến lưu phím nhấn

            do
            {
                Console.Clear();
                // Hiển thị bảng cổ phiếu công ty
                Console.WriteLine(RenderCompanyStocksTable());
                Console.WriteLine();

                // Hướng dẫn người dùng về cách sử dụng phím
                Console.WriteLine($"Sử dụng phím mũi tên để di chuyển, ←→ để thay đổi số lượng cổ phiếu muốn {(isBuying ? "mua" : "bán")}:");
                Console.WriteLine("Nhấn Enter để xác nhận hoặc Esc để hủy.");

                // Hiển thị danh sách các công ty và số lượng cổ phiếu của từng công ty
                for (int i = 0; i < Companies.Count; i++)
                {
                    // Đổi màu nền cho công ty đang được chọn
                    if (i == index)
                    {
                        Console.BackgroundColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    // Hiển thị tên công ty và số lượng cổ phiếu
                    Console.WriteLine($"[{(i == index ? "*" : " ")}] {numberOfShares[i],3} {Companies[i].Name}");
                    Console.ResetColor(); // Khôi phục màu mặc định
                }

                // Tính toán tổng chi phí cho giao dịch
                decimal cost = numberOfShares.Select((shares, i) => shares * Companies[i].SharePrice).Sum();
                Console.WriteLine();
                // Hiển thị chi phí hoặc thu về tương ứng
                Console.WriteLine($"{(isBuying ? "Chi phí" : "Thu về")}: {cost:C}");

                Console.CursorVisible = false; // Ẩn con trỏ
                key = Console.ReadKey(true).Key; // Đọc phím nhấn mà không hiển thị lên màn hình
                switch (key)
                {
                    // Điều khiển di chuyển lên
                    case ConsoleKey.UpArrow:
                        index = (index == 0) ? Companies.Count - 1 : index - 1; // Về cuối nếu đang ở đầu
                        break;
                    // Điều khiển di chuyển xuống
                    case ConsoleKey.DownArrow:
                        index = (index == Companies.Count - 1) ? 0 : index + 1; // Về đầu nếu đang ở cuối
                        break;
                    // Điều khiển giảm số lượng cổ phiếu
                    case ConsoleKey.LeftArrow:
                        if (numberOfShares[index] > 0) // Không cho giảm nếu số lượng bằng 0
                        {
                            numberOfShares[index]--;
                        }
                        break;
                    // Điều khiển tăng số lượng cổ phiếu
                    case ConsoleKey.RightArrow:
                        if (isBuying) // Nếu đang mua
                        {
                            // Kiểm tra xem có đủ tiền để mua không
                            if (Money - cost >= Companies[index].SharePrice)
                            {
                                numberOfShares[index]++;
                            }
                        }
                        else // Nếu đang bán
                        {
                            // Kiểm tra xem có đủ cổ phiếu để bán không
                            if (numberOfShares[index] < Companies[index].NumberOfShares)
                            {
                                numberOfShares[index]++;
                            }
                        }
                        break;
                    // Nếu nhấn Esc, thoát khỏi phương thức
                    case ConsoleKey.Escape:
                        return;
                }
            } while (key != ConsoleKey.Enter); // Tiếp tục cho đến khi nhấn Enter

            // Xử lý giao dịch mua/bán cổ phiếu
            for (int i = 0; i < Companies.Count; i++)
            {
                if (isBuying) // Nếu đang mua
                {
                    Money -= numberOfShares[i] * Companies[i].SharePrice; // Giảm tiền
                    Companies[i].NumberOfShares += numberOfShares[i]; // Tăng số lượng cổ phiếu
                }
                else // Nếu đang bán
                {
                    Money += numberOfShares[i] * Companies[i].SharePrice; // Tăng tiền
                    Companies[i].NumberOfShares -= numberOfShares[i]; // Giảm số lượng cổ phiếu
                }
            }

            
            Console.Clear();
            // Hiển thị lại bảng cổ phiếu
            Console.WriteLine(RenderCompanyStocksTable());
            Console.WriteLine($"Số lượng cổ phiếu của bạn đã được cập nhật.");
            Console.WriteLine();
            
            Console.Write("Nhấn phím bất kỳ để tiếp tục...");
            Console.ReadKey(true); // Đọc phím nhấn
        }



        // Phương thức  RenderCompanyStocksTable, trả về một chuỗi kiểu StringBuilder
        private StringBuilder RenderCompanyStocksTable()
        {
            // Định nghĩa chiều rộng của các cột
            const int c0 = 30, c1 = 15, c2 = 15, c3 = 10;

            // Khởi tạo một đối tượng StringBuilder để xây dựng bảng
            StringBuilder gameView = new StringBuilder();

            // Thêm dòng tiêu đề cho bảng với các ký tự vẽ viền
            gameView.AppendLine($"╔═{new string('═', c0)}═╤═{new string('═', c1)}═╤═{new string('═', c2)}═╤═{new string('═', c3)}═╗");
            gameView.AppendLine($"║ {"Công ty",-c0} │ {"Ngành",c1} │ {"Giá cổ phiếu",c2} │ {"Bạn có",c3} ║");
            gameView.AppendLine($"╟─{new string('─', c0)}─┼─{new string('─', c1)}─┼─{new string('─', c2)}─┼─{new string('─', c3)}─╢");

            // Lặp qua danh sách các công ty và thêm thông tin của mỗi công ty vào bảng
            foreach (Company company in Companies)
            {
                gameView.AppendLine($"║ {company.Name,-c0} │ {company.Industry,c1} │ {company.SharePrice,c2:C2} │ {company.NumberOfShares,c3} ║");
            }

            // Thêm dòng cuối của bảng
            gameView.AppendLine($"╚═{new string('═', c0)}═╧═{new string('═', c1)}═╧═{new string('═', c2)}═╧═{new string('═', c3)}═╝");

            // Thêm thông tin về tiền của người dùng và giá trị tài sản ròng
            gameView.AppendLine($"Tiền của bạn: {Money:C2}    Giá trị tài sản ròng: {CalculateNetWorth():C2}");

            // Trả về bảng đã được xây dựng
            return gameView;
        }


        private void CompanyDetailsScreen()
        {
            Console.Clear(); 

            // Duyệt qua từng công ty trong danh sách các công ty
            foreach (Company company in Companies)
            {
                // Hiển thị tên và mô tả của công ty
                Console.WriteLine($"{company.Name} - {company.Description}");
                Console.WriteLine(); 
            }
            Program.TurnBack(); // Gọi hàm TurnBack để quay lại menu trước
        }

        private int HandleMenuWithOptions(string prompt, string[] options)
        {
            // Khởi tạo chỉ số cho tùy chọn hiện tại
            int index = 0;
            ConsoleKey key = default; // Khởi tạo biến key để lưu phím nhấn

           // Vòng lặp sẽ tiếp tục chạy cho đến khi người dùng yêu cầu thoát hoặc nhấn phím Enter
            while (!CloseRequested && key != ConsoleKey.Enter)
            {
                Console.Clear(); 
                Console.WriteLine(prompt); 

                // Duyệt qua từng tùy chọn và hiển thị chúng
                for (int i = 0; i < options.Length; i++)
                {
                    string currentOption = options[i]; // Lấy tùy chọn hiện tại

                    // Kiểm tra xem tùy chọn hiện tại có phải là tùy chọn được chọn không
                    if (i == index)
                    {
                        // Đổi màu nền và màu chữ cho tùy chọn đang được chọn
                        Console.BackgroundColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($"[*] {currentOption}"); // Hiển thị tùy chọn đã chọn
                        Console.ResetColor(); // Đặt lại màu sắc về mặc định
                    }
                    else
                    {
                        Console.WriteLine($"[ ] {currentOption}"); // Hiển thị tùy chọn chưa chọn
                    }
                }

                Console.CursorVisible = false; // Ẩn con trỏ khi nhập
                key = Console.ReadKey(true).Key; // Đọc phím nhấn từ người dùng
               
                // Xử lý các phím nhấn
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        // Di chuyển lên tùy chọn trước đó
                        index = (index == 0) ? options.Length - 1 : index - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        // Di chuyển xuống tùy chọn tiếp theo
                        index = (index == options.Length - 1) ? 0 : index + 1;
                        break;
                    case ConsoleKey.Escape:
                        // Nếu nhấn phím Escape, yêu cầu thoát
                        CloseRequested = true;
                        break;
                }
            }
            return index; // Trả về chỉ số tùy chọn đã được chọn
        }


        private decimal CalculateNetWorth()
        {
            // Khởi tạo biến netWorth với giá trị ban đầu là số tiền hiện có của người chơi
            decimal netWorth = Money;
            // Duyệt qua từng công ty trong danh sách Companies
            foreach (Company company in Companies)
            {
                // Cộng giá trị cổ phiếu của công ty (giá cổ phiếu nhân với số lượng cổ phiếu) vào tổng tài sản ròng
                netWorth += company.SharePrice * company.NumberOfShares;
            }
            // Trả về tổng tài sản ròng
            return netWorth;
        }


        private void InitializeEvents()
        {
            string jsonFilePath = "events.json"; // Đường dẫn đến file JSON 
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

            Program.TurnBack();
            
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

            Program.TurnBack();
            
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
            Program.TurnBack();
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

            Program.TurnBack();

        }


    }
}