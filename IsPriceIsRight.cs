using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using System.Globalization;
using CsvHelper;
using Pastel;
using System.Media;
using System.Threading;
using System.Drawing;


namespace Quanlychitieu
{
    internal class IsPriceIsRight
    { 
        // Khởi tạo một đối tượng HttpClient để thực hiện các yêu cầu HTTP
        static HttpClient client = new HttpClient();

        // Hàm bắt đầu trò chơi, được gọi khi game bắt đầu
        public static async Task BeginGameAsync()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Gọi hàm hiển thị linh vật và phát nhạc nền
            DisplayMascotWithMusic();

            // Chờ 13 giây trước khi hiển thị thông báo
            await Task.Delay(13000); // // Sử dụng await Task.Delay thay vì Thread.Sleep để tránh chặn luồng

            // Tiêu đề chính của trò chơi được hiển thị
            string[] title =
      {
"███╗   ███╗ ██████╗ ███╗   ██╗███████╗██╗   ██╗              ███╗   ███╗ █████╗ ████████╗ ██████╗██╗  ██╗  ",
"████╗ ████║██╔═══██╗████╗  ██║██╔════╝╚██╗ ██╔╝              ████╗ ████║██╔══██╗╚══██╔══╝██╔════╝██║  ██║  ",
"██╔████╔██║██║   ██║██╔██╗ ██║█████╗   ╚████╔╝     █████╗    ██╔████╔██║███████║   ██║   ██║     ███████║  ",
"██║╚██╔╝██║██║   ██║██║╚██╗██║██╔══╝    ╚██╔╝      ╚════╝    ██║╚██╔╝██║██╔══██║   ██║   ██║     ██╔══██║  ",
"██║ ╚═╝ ██║╚██████╔╝██║ ╚████║███████╗   ██║                 ██║ ╚═╝ ██║██║  ██║   ██║   ╚██████╗██║  ██║  ",
"╚═╝     ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝   ╚═╝                 ╚═╝     ╚═╝╚═╝  ╚═╝   ╚═╝    ╚═════╝╚═╝  ╚═╝  ",
      };
            // Đặt màu chữ thành màu vàng cho tiêu đề
            Console.ForegroundColor = ConsoleColor.Yellow;
            // Gọi hàm DrawCenteredBorder() đã định nghĩa ở class Program
            Program.DrawCenteredBorder(title);

           
            Console.WriteLine("                                                            Nhấn phím bất kì để bắt đầu nhiệm vụ");
            Console.ReadKey(); // Chờ user nhấn 'Enter'

            // Sau khi nhấn Enter, xóa màn hình và hiển thị luật chơi
            Console.Clear();  
             

            string[] messsage =
    {
"  Bạn sẽ là Anh Củ Cải đi chợ mua đồ dùng cho các bạn Tân sinh viên.",
"  Hãy trả giá sao cho mua được giá hời từ gợi ý của các cô bán hàng.",
      };
            // Đặt màu chữ thành màu xanh lơ cho thông điệp hướng dẫn
            Console.ForegroundColor = ConsoleColor.Cyan;
            Program.DrawCenteredBorder(messsage);


            // Hiển thị thông báo cảnh báo cho người chơi
            string[] warningg =
    {
"  Lưu ý: Nếu bạn trả giá quá thấp, các cô sẽ không bán nữa.         ",
"  Bạn mua thiếu đồ dùng cho các em sinh viên.                       ",
      };
            // Đặt màu chữ thành màu đỏ để nhấn mạnh cảnh báo
            Console.ForegroundColor = ConsoleColor.Red;
            Program.DrawCenteredBorder(warningg);

            // Đặt lại màu chữ về mặc định
            Console.ResetColor();


            // Phát nhạc background
            SoundPlayer Background = new SoundPlayer("loto.wav");
            Background.Load(); // Tải file nhạc
            Background.Play();  // Phát nhạc một lần
            Background.PlayLooping();  // Lặp lại nhạc


            // Thiết lập headers 
            var headers = new Dictionary<string, string>
        {
            { "User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Mobile Safari/537.36" },
            { "Accept", "application/json, text/plain, */*" },
            { "Accept-Language", "en-US,en;q=0.9,vi;q=0.8" },
            { "Referer", "https://tiki.vn/nha-cua-doi-song/c1883" },
            { "x-guest-token", "U3bu08MpC9HcGmaK1RdJTsND6Ix7yZrl" },
         
        };

            // Danh sách lưu trữ thông tin sản phẩm
            var productsInfo = new List<Dictionary<string, object>>();

            // Danh sách chứa các tasks tải dữ liệu sản phẩm song song
            var tasks = new List<Task>();

            // Tạo 5 tasks để lấy dữ liệu từ 5 trang khác nhau
            for (int i = 1; i <= 5; i++)
            {
                // Sử dụng Task.Run để chạy mỗi tác vụ trên một luồng riêng
                tasks.Add(Task.Run(async () =>
                {
                    var result = await LoadProductsAsync(i, headers);
                    // Khóa để đảm bảo rằng productsInfo được cập nhật một cách an toàn khi đa luồng
                    lock (productsInfo)
                    {
                        productsInfo.AddRange(result);
                    }
                }));
            }


            // Đợi tất cả các task hoàn thành
            Task.WaitAll(tasks.ToArray());

            // Bắt đầu trò chơi
            StartGame(productsInfo);

            Console.ReadKey();
            

        }

        // Hàm tải sản phẩm từ một trang cụ thể
        static async Task<List<Dictionary<string, object>>> LoadProductsAsync(int page, Dictionary<string, string> headers)
        {
            var productsInfo = new List<Dictionary<string, object>>();

            // Tạo các tham số query để gửi kèm yêu cầu HTTP
            var queryParams = new Dictionary<string, string>
        {
            { "limit", "40" }, // Giới hạn số sản phẩm trên mỗi trang
            { "include", "advertisement" }, // Bao gồm các thuộc tính cần thiết
            { "aggregations", "2" },
            { "trackity_id", "817823f9-f82f-d7b5-1536-a6b336989bd2" }, 
            { "category", "1883" },  // ID danh mục sản phẩm
            { "page", page.ToString() }, // Số trang
            { "urlKey", "nha-cua-doi-song" }
        };

            // Gửi yêu cầu GET để lấy dữ liệu sản phẩm
            var response = await SendGetRequestAsync("https://tiki.vn/api/v2/products", headers, queryParams);

            // Nếu yêu cầu thành công, phân tích dữ liệu JSON trả về
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<dynamic>(jsonResponse).data;

                // Lưu trữ thông tin các sản phẩm vào danh sách
                foreach (var record in products)
                {
                    // Lấy ID, tên và giá sản phẩm
                    var productId = record.id.ToString();
                    var productName = record.name.ToString();
                    var productPrice = record.price.ToString();

                    productsInfo.Add(new Dictionary<string, object>
                {
                    { "id", productId },
                    { "name", productName },
                    { "price", productPrice }
                });
                }
            }
            return productsInfo;
        }



        // Hàm gửi yêu cầu GET với headers và query parameters
        static async Task<HttpResponseMessage> SendGetRequestAsync(string url, Dictionary<string, string> headers, Dictionary<string, string> queryParams)
        {
            // Tạo yêu cầu HTTP với URL và các tham số query
            var request = new HttpRequestMessage(HttpMethod.Get, $"{url}?{string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");

            // Thêm các headers vào yêu cầu
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            // Gửi yêu cầu và trả về phản hồi
            return await client.SendAsync(request);
        }



        // Hàm bắt đầu trò chơi
        static void StartGame(List<Dictionary<string, object>> products)
        {
            Random rnd = new Random();
            int correctGuesses = 0;

            // Chọn ngẫu nhiên 3 sản phẩm từ danh sách sản phẩm
            var randomProducts = products.OrderBy(x => rnd.Next()).Take(3).ToList();

            Console.WriteLine("\n                                                          " +
                "💸💸💸💸💸 HÃY TRẢ GIÁ 💸💸💸💸💸\n");

            decimal totalRealPrice = 0;
            decimal totalUserGuess = 0;
            int count = 1;

            // Vòng lặp để xử lý từng sản phẩm
            foreach (var product in randomProducts)
            {
                string productName = product["name"].ToString();
                decimal productPrice = Convert.ToDecimal(product["price"]);
                totalRealPrice += productPrice;


                // Giá cô bán hàng đưa ra là gấp đôi giá thực tế
                decimal suggestedPrice = productPrice * 2;
                string[] suggestedPriceArray = new string[] { suggestedPrice.ToString() };

                string[] hintt =
                {
                    $"Sản phẩm {count}: {productName}",
                    $"Giá cô bán hàng đưa ra: {suggestedPrice} VND",
                };
                Program.DrawCenteredBorder(hintt);


                // Yêu cầu người chơi nhập giá đoán
                var userGuessStr = Program.InputWithBox("\n                                                                " +
                    "     Hãy trả giá đúng! ", " ");

              
                decimal userGuess = Convert.ToDecimal(userGuessStr);
                Console.WriteLine();


                totalUserGuess += userGuess;
                count++;
            }

            // Kiểm tra tổng giá người chơi đoán so với giá thực
            decimal minAcceptablePrice = totalRealPrice * 0.7M;  // 70% tổng giá
            decimal maxAcceptablePrice = totalRealPrice * 1.0M;  // 100% tổng giá

            if (totalUserGuess >= minAcceptablePrice && totalUserGuess <= maxAcceptablePrice)
            {
                CongratulateUser();
            }
            else if (totalUserGuess > totalRealPrice)
            {
                OverPriced();
            }
            else
            {
                Lack();
            }
        }





        // HÀM PHÁT ÂM THANH VÀ MASCOT CHIẾN THẮNG 
        static void CongratulateUser()
        {
            Console.Clear();
            SoundPlayer chienThang = new SoundPlayer("Thankyou.wav");
            chienThang.Load();
            chienThang.Play();
            Console.WriteLine("                                                    " +
                "Chúc mừng! Bạn đã trả giá khéo léo và mua được giá hời! 🎉🎉🎉");
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
"                   l       ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀                           ",  };

            Program.DrawCenteredBorder(winnn);

            Console.ForegroundColor = ConsoleColor.Cyan;
            string[] youwin =
            {
                "   ██╗   ██╗ ██████╗ ██╗   ██╗    ██╗    ██╗██╗███╗   ██╗    ",
                "   ╚██╗ ██╔╝██╔═══██╗██║   ██║    ██║    ██║██║████╗  ██║    ",
                "    ╚████╔╝ ██║   ██║██║   ██║    ██║ █╗ ██║██║██╔██╗ ██║    ",
                "     ╚██╔╝  ██║   ██║██║   ██║    ██║███╗██║██║██║╚██╗██║    ",
                "      ██║   ╚██████╔╝╚██████╔╝    ╚███╔███╔╝██║██║ ╚████║    ",
                "      ╚═╝    ╚═════╝  ╚═════╝      ╚══╝╚══╝ ╚═╝╚═╝  ╚═══╝    ",
            };
            Program.DrawCenteredBorder(youwin);
            Console.ResetColor();
        }

        static void OverPriced()
        {
            Console.Clear();
            SoundPlayer Overpriced = new SoundPlayer("Overpriced.wav");
            Overpriced.Load();
            Overpriced.Play();
            string[] losee =
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
"     ╙╝▒╣╣╣╢╢╢╫╬╣╣╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓   ",
"                  ▄╢▓▓▓▓▓▓▓▒╢╣╣╢╢╢╢╢╢╢▓▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▓▓▓▓▓▓▓▓▓▓   ",
"                 ▐▓▓▓▓▓▓▓▓▓▓▒▒╢╢╢╢╢╢╢╢▓╢╢╢╢╢╢╢╢▒▒▒╩╝╝╝╢╢╣╣╣╣╣╢╣▓▓▓▓▓▓▓▓▓▓▓▓       ",
"                 ╣▓▓▓▓▓▓▓▓▓▓▓▓▓╢╢╢╢╢╢╢╣╢╢╢╢╢╢╢╢╢╫▓▓▓▓,    '▀▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▀      ",
"                 ▀╢▓▓▓▓▀└╔▓▓▓▓▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╣▓▓▓▄        `└╙▀▀▀▀▀▀╙.           ",
"                         ╙▀▀▀▒╢╢╢╢╢╢╢▓╚╢╢╢╢╢▓▓╢╢╢╢╢▓▓▄                            ",
"                            ╢╢╢╫╝'╙╨╜    ╙╜`   ╩╢╣╢╢▓▓▄                           ",
"                           /╨╙'   r^          '═  '╙╙╙▀L                          ",
"                          ╒                            ▒,                         ",
"                                  ▐▓█         ▐▓█      └▒                         ",
"               [ *`ⁿ.    ╛         💧         💧        ▒▒    ' M`]              ",
"              ]`╙     ⌐ ▓█▄        💧 ▐╦╥▄╦▓  💧        └▒▄ ^    '`ƒ              ",
"              ┐        ▓▓▓▓▓▄          └╙▀            ,▄█▓▓▀       ;              ",
"               j         ╘▀▓▓▓▓▄                     ,█▓▓▓▀        ╒              ",
"                Y          ╙▓▓▓▓█                   ▄▓▓▓▓╘        ¿               ",
"                 ^          ▓▓▓▓▓█                 ▐▓▓▓▓▌        ╛                ",
"                  l         ▐▓▓▓▓▓▓▄▄▄▓████▓▄▄▄▄▄,,▓▓▓▓▓▌      ⌐                  ",
"                   l       ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀                          ",
         };
            Program.DrawCenteredBorder(losee);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            string[] youlose =
            {
                " ██╗   ██╗ ██████╗ ██╗   ██╗    ██╗      ██████╗ ███████╗███████╗ ",
                " ╚██╗ ██╔╝██╔═══██╗██║   ██║    ██║     ██╔═══██╗██╔════╝██╔════╝ ",
                "  ╚████╔╝ ██║   ██║██║   ██║    ██║     ██║   ██║███████╗█████╗   ",
                "   ╚██╔╝  ██║   ██║██║   ██║    ██║     ██║   ██║╚════██║██╔══╝   ",
                "    ██║   ╚██████╔╝╚██████╔╝    ███████╗╚██████╔╝███████║███████╗ ",
                "    ╚═╝    ╚═════╝  ╚═════╝     ╚══════╝ ╚═════╝ ╚══════╝╚══════╝ ",
            };
            Program.DrawCenteredBorder(youlose);
            Console.ResetColor();
        }




        static void Lack()
        {
            Console.Clear();
            SoundPlayer lack = new SoundPlayer("Lack.wav");
            lack.Load();
            lack.Play();
            string[] losee =
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
"     ╙╝▒╣╣╣╢╢╢╫╬╣╣╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓   ",
"                  ▄╢▓▓▓▓▓▓▓▒╢╣╣╢╢╢╢╢╢╢▓▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▓▓▓▓▓▓▓▓▓▓   ",
"                 ▐▓▓▓▓▓▓▓▓▓▓▒▒╢╢╢╢╢╢╢╢▓╢╢╢╢╢╢╢╢▒▒▒╩╝╝╝╢╢╣╣╣╣╣╢╣▓▓▓▓▓▓▓▓▓▓▓▓       ",
"                 ╣▓▓▓▓▓▓▓▓▓▓▓▓▓╢╢╢╢╢╢╢╣╢╢╢╢╢╢╢╢╢╫▓▓▓▓,    '▀▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▀      ",
"                 ▀╢▓▓▓▓▀└╔▓▓▓▓▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╣▓▓▓▄        `└╙▀▀▀▀▀▀╙.           ",
"                         ╙▀▀▀▒╢╢╢╢╢╢╢▓╚╢╢╢╢╢▓▓╢╢╢╢╢▓▓▄                            ",
"                            ╢╢╢╫╝'╙╨╜    ╙╜`   ╩╢╣╢╢▓▓▄                           ",
"                           /╨╙'   r^          '═  '╙╙╙▀L                          ",
"                          ╒                            ▒,                         ",
"                                  ▐▓█         ▐▓█      └▒                         ",
"               [ *`ⁿ.    ╛         💧         💧        ▒▒    ' M`]               ",
"              ]`╙     ⌐ ▓█▄        💧 ▐╦╥▄╦▓  💧        └▒▄ ^    '`ƒ              ",
"              ┐        ▓▓▓▓▓▄      💧  └╙▀    💧      ,▄█▓▓▀       ;              ",
"               j         ╘▀▓▓▓▓▄                     ,█▓▓▓▀        ╒              ",
"                Y          ╙▓▓▓▓█                   ▄▓▓▓▓╘        ¿               ",
"                 ^          ▓▓▓▓▓█                 ▐▓▓▓▓▌        ╛                ",
"                  l         ▐▓▓▓▓▓▓▄▄▄▓████▓▄▄▄▄▄,,▓▓▓▓▓▌      ⌐                  ",
"                   l       ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀                          ",
 };

            Program.DrawCenteredBorder(losee);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            string[] youlose =
            {   " ██╗   ██╗ ██████╗ ██╗   ██╗    ██╗      ██████╗ ███████╗███████╗  ",
                " ╚██╗ ██╔╝██╔═══██╗██║   ██║    ██║     ██╔═══██╗██╔════╝██╔════╝  ",
                "  ╚████╔╝ ██║   ██║██║   ██║    ██║     ██║   ██║███████╗█████╗    ",
                "   ╚██╔╝  ██║   ██║██║   ██║    ██║     ██║   ██║╚════██║██╔══╝    ",
                "    ██║   ╚██████╔╝╚██████╔╝    ███████╗╚██████╔╝███████║███████╗  ",
                "    ╚═╝    ╚═════╝  ╚═════╝     ╚══════╝ ╚═════╝ ╚══════╝╚══════╝  ",
            };
            Program.DrawCenteredBorder(youlose);
            Console.ResetColor();
        }

            

        // HÀM PHÁT NHẠC UEH MỞ ĐẦU TRÒ CHƠI VÀ IN MASCOT
            static void DisplayMascotWithMusic()
            {
            SoundPlayer proudUEH = new SoundPlayer("proudUEHcombine.wav");
            proudUEH.Load();
            proudUEH.Play();

            Console.Clear();

            Console.WriteLine(@"
                                                             ,,,,,,,        ,▄▄╬▓▓▓▓▄,
                                                  ,▄╦▄▄▄▄▓▓▓▓▓▓▒▒╣▒▒▒▓▓▄,  ▄▓▒╢╢╢╣▓▓╣▒▓▄
                                                 ▓╢╢╢▒╢╢▓▓▓╢╢╢╢╢╢╢╢╢╢╢╢╢╢▒▓█▒╢╢╢╢╢╢╢╣╢╢╢▒▓,,▄▄▓▒▓▄▄▄,
                                                 ▄▓▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▒▓▒╢╢╢╢╢╢▒▓▓▀▒▒╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▒▄
                                               ▄▓╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢█▓▌╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓
                                              ▓▒▓╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢█▓▌╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓
                                             ▐▒╫╣▓╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢█▓▓▌╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓
                                             ▓╣╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢█▓▌╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓
                                             ╙▌╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢██╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓
                                               ▀▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▒▒▒▓▓╢╢╢╢██▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▌
                                                 ▀▀▓▒▒▒╢╢╢╢╢╢╢╢╢▒▒▒▓▓█╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▌
                                                    '▀ ▀█▒▒▒▒▒▒▓█╣╢╢╢╢╢╢╢╢▓╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▒▀
                                                          ▌╣╢╢╢╢╢▓▓▓▒▓▒╢╢╢╢▒╢╢╢╢▓▓▓▓▓▓▓▓▒▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢▒▀
                                                           ▀▓▒▄▓╣╢╢╢╣▓╢╢╢╢╢╢╢╢╢╢╢╢╢▒▒╣╢▓▓▓▒▒▒▀█▓▒▒▒╢▒▒▓▀
                                                             ▀▓▓▓▒╢▒▓▒╢╢╢╢╢▒▒╢╢╢╢▒╣╢╢╢╢╫▓╢╢▒▓   
                                                              ╙▌▀▀▀▐▌╢╢▒▓▒▓╜▒▒╩╣▒╩╙▒╢╢╢╢▒▌  
                                                                  ╒▀╩▓╨  ,▄▄       ,▄╙╨▓╩▓▄ 
                                                                  ▌       ▄▄      ,▄▄     ▐ 
                                                                 ▀▄      ▐▓█      ▐▓█      ▐ 
                                                                ▄▐▀    ,, ▀        ▀  ,,    ▓
                                                               ╓▀█▄           ▓▓▓▓          ▄▌
                                                             ▄╖▓▓███▄▄                 ,▄▄████▄▄
                                                            ▀▄▓▓▓▓▓██████▄▄▄▄▄, ▄▄▄▄▄██████▓▓▓▓▓▀
                                                            █▓▓▓▓▓▓▓▓▓▓███████▓████████▓▓▓▓▓▓▓█▓▓▌ 
                                                           █▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓█▓▓▓▓▓▌▌UEH ▓▓█▓▓▓▓▓█   
                                                          ▌▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓██▓▓▓▓█
                                                             ▓▓▓▄▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▓▓▓▀▀
                                                            ▐,▓▓▓ ,▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█ ▓▓▓█
                                                              ▀▓ █▄█████▓▓░▓▓▓▓▓▓▓▓▓▓▓██████▌▀█
                                                                 ███████████▀▀▀▀▀▀███████████
                                                                   ▓░,▀▀▀▀▀█      ▀█▀▀▀▀▀▀▓░
                                                                   ▓░▓░▓░▓░▓      ▓░▓░▓░▓░▓░                 
             ");

            }


    }
}