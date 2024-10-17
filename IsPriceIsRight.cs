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
        static HttpClient client = new HttpClient();

        public static async Task BeginGameAsync()
        {
            // Thiết lập mã hóa console thành UTF-8
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Phát nhạc và di chuyển linh vật
            DisplayMascotWithMusic();

            // Chờ 13 giây trước khi hiển thị thông báo
            await Task.Delay(13000); // Changed from Thread.Sleep to await Task.Delay

            // Hiển thị yêu cầu chấp nhận nhiệm vụ
            Console.WriteLine("                                                            ══════════════════════════");
            Console.WriteLine("                                                            ║    CHẤP NHẬN NHIỆM VỤ  ║");
            Console.WriteLine("                                                            ══════════════════════════");
            Console.WriteLine("                                                       Nhấn 'Enter' để bắt đầu nhiệm vụ");
            Console.ReadLine(); // Chờ user nhấn 'Enter'

            // Sau khi nhấn Enter, hiển thị luật chơi
            Console.Clear(); // Xóa màn hình 
            string message = "                                                                    \n" +
                             " Bạn sẽ là Anh Củ Cải đi chợ mua đồ dùng cho các bạn Tân sinh viên. \n" +
                             " Hãy trả giá sao cho mua được giá hời từ gợi ý của các cô bán hàng. \n" +
                             " Lưu ý: Nếu bạn trả giá quá thấp, các cô sẽ không bán nữa.          \n" +
                             " Bạn mua thiếu đồ dùng cho các em sinh viên.                        \n" ;


            // Gọi hàm để hiển thị thông điệp
            DisplayMessageBox(message);

            // Thiết lập headers cho HTTP request
            var headers = new Dictionary<string, string>
        {
            { "User-Agent", "Mozilla/5.0 (Windows NT 6.3; Win64; x64; rv:83.0) Gecko/20100101 Firefox/83.0" },
            { "Accept", "application/json, text/plain, */*" },
            { "Accept-Language", "vi-VN,vi;q=0.8,en-US;q=0.5,en;q=0.3" },
            { "Referer", "https://tiki.vn/?src=header_tiki" },
            { "x-guest-token", "8jWSuIDBb2NGVzr6hsUZXpkP1FRin7lY" },
            { "Connection", "keep-alive" },
            { "TE", "Trailers" }
        };

            // Danh sách lưu trữ thông tin sản phẩm
            var productsInfo = new List<Dictionary<string, object>>();

            // Danh sách chứa các tasks tải dữ liệu sản phẩm song song
            var tasks = new List<Task<List<Dictionary<string, object>>>>();

            // Tạo 5 tasks để lấy dữ liệu từ 5 trang khác nhau
            for (int i = 1; i <= 5; i++)
            {
                tasks.Add(LoadProductsAsync(i, headers));
            }

            // Đợi tất cả các task hoàn thành và thu thập dữ liệu từ chúng
            var results = Task.WhenAll(tasks).Result;

            // Gộp tất cả các sản phẩm từ các tasks khác nhau
            foreach (var result in results)
            {
                productsInfo.AddRange(result);
            }

            // Bắt đầu trò chơi
            StartGame(productsInfo);

            Console.ReadKey();
        }
        static void DisplayMessageBox(string message)
        {
            // Tính toán chiều rộng cửa sổ console
            int windowWidth = Console.WindowWidth;

            // Tính chiều dài của thông điệp
            string[] messageLines = message.Split(new[] { '\n' }, StringSplitOptions.None);
            int maxLineLength = 0;

            foreach (string line in messageLines)
            {
                if (line.Length > maxLineLength)
                {
                    maxLineLength = line.Length;
                }
            }

            // Tạo khung
            string border = new string('═', maxLineLength + 3);

            // In ra khung và thông điệp
            Console.WriteLine(border.Pastel(Color.Teal));

            Console.WriteLine($"║ {messageLines[0].Pastel(Color.Orange).PadLeft((maxLineLength + 2 + messageLines[0].Length) / 2)}║".Pastel(Color.Teal));

            for (int i = 1; i < messageLines.Length; i++)
            {
                Console.WriteLine($"║ {messageLines[i].Pastel(Color.Orange).PadLeft((maxLineLength + 2 + messageLines[i].Length) / 2)}║".Pastel(Color.Teal));
            }

            Console.WriteLine(border.Pastel(Color.Teal));
        }
        // Hàm tải sản phẩm từ một trang cụ thể
        static async Task<List<Dictionary<string, object>>> LoadProductsAsync(int page, Dictionary<string, string> headers)
        {
            var productsInfo = new List<Dictionary<string, object>>();

            var queryParams = new Dictionary<string, string>
        {
            { "limit", "48" },
            { "include", "sale-attrs,badges,product_links,brand,category,stock_item,advertisement" },
            { "aggregations", "1" },
            { "trackity_id", "70e316b0-96f2-dbe1-a2ed-43ff60419991" },
            { "category", "1883" },
            { "page", page.ToString() },
            { "src", "c1883" },
            { "urlKey", "nha-cua-doi-song" }
        };

            // Gửi yêu cầu GET
            var response = await SendGetRequestAsync("https://tiki.vn/api/v2/products", headers, queryParams);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<dynamic>(jsonResponse).data;

                // Lưu trữ thông tin sản phẩm
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

        // Hàm bắt đầu trò chơi
        static void StartGame(List<Dictionary<string, object>> products)
        {
            Random rnd = new Random();
            int correctGuesses = 0;

            // Chọn ngẫu nhiên 3 sản phẩm từ danh sách sản phẩm
            var randomProducts = products.OrderBy(x => rnd.Next()).Take(3).ToList();

            Console.WriteLine("\n Hãy trả giá các sản phẩm sau:\n");

            decimal totalRealPrice = 0;
            decimal totalUserGuess = 0;
            int count = 1;

            foreach (var product in randomProducts)
            {
                string productName = product["name"].ToString();
                decimal productPrice = Convert.ToDecimal(product["price"]);
                totalRealPrice += productPrice;


                // Giá gợi ý từ "cô bán hàng"
                decimal suggestedPrice = productPrice * 2;
                Console.WriteLine($"Sản phẩm {count}: {productName}");
                Console.WriteLine($"Giá cô bán hàng đưa ra:{suggestedPrice} VND");
                Console.Write("Hãy trả giá đúng!");
                decimal userGuess = GetUserInput();
                Console.WriteLine();

                totalUserGuess += userGuess;
                count++;
            }

            // Kiểm tra tổng giá người dùng đoán
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


        // Hàm xử lý ngoại lệ 
        static decimal GetUserInput()
        {
            decimal number;
            while (true)
            {
                string input = Console.ReadLine();

                // Kiểm tra xem có thể chuyển đổi sang decimal hay không
                if (decimal.TryParse(input, out number))
                {
                    break; // Nếu thành công, thoát khỏi vòng lặp
                }
                else
                {
                    Console.WriteLine("Nhập liệu không hợp lệ, hãy nhập lại."); // Thông báo lỗi
                }
            }
            return number; // Trả về giá trị số hợp lệ
        }



        // Hàm phát nhạc chiến thắng
        static void CongratulateUser()
        {
            SoundPlayer chienThang = new SoundPlayer("Thankyou.wav");
            chienThang.Load();
            chienThang.Play();
            Console.WriteLine("Chúc mừng! Bạn đã trả giá khéo léo và mua được giá hời! 🎉🎉🎉");
            Console.WriteLine(@"

                         ,,,,,              ╓æ▓▓▓▓▓▓▄,
              ╓▄▓▓▓▓▓▓▓▓▓▓▓▓▓▓Wç     ,╦▓╣╢╢╣▓▓▓▓▓▓▓▄
          ╓@▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▄  ╔▒╢╢╢╢╢▓╢▓▓▓▓▓▓▓▓      ,,,
       ,@▓▓╣╢╢╣▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▄▒╢╢╢╢╢╢▓╢▓▓▓▒▒▓▓╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╣▓▓▄
     ╓╣╢╢╢╢╢╢╢╢▓▓▓▓▓▓▓▓▓▓▓╢╢╣▓▓▓▓▓▓▓▓╢╢╢╢╢╢▓╣╢▓▓▓▒▒▓▓╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╣▓▓▄
    ╣║╢╢╢╢╢╢╢╢╢╢╣▓▓▓▓▓▓▓▓╣╢╢╢╢╣▓▓▓▓▓▓▓╢╢╢╢╢▓╣▒▓▒▓▓╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▄
   ╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▓▓▓▒╢╢╢╫▓▓▒▓▓╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▓µ
  ║╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╣▓▓▓▓▓╢╢╢▓▒╢▓▓╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▓▄
  ║╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╣▓▓▓▓▒╢▒▒╢▓╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▓▓▌
  ╘╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▌║▒╢▓╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╣╣╣╢╢▓▓▓▓▓▌
   ╙╣╢╢╣╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╫▓▓▓▒╢╫╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▓▓▓▓▓▓▓▓▓▓▌
     ╙╝▒╣╣╣╢╢╢╫╬╣╣╣╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▓▓▓▓▓▓▓▓▓▓
                  ▄╢▓▓▓▓▓▓▓▒╢╣╣╢╢╢╢╢╢╢▓▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢▓▓▓▓▓▓▓▓▓▓▓▓▓
                 ▐▓▓▓▓▓▓▓▓▓▓▒▒╢╢╢╢╢╢╢╢▓╢╢╢╢╢╢╢╢▒▒▒╩╝╝╝╢╢╣╣╣╣╣╢╣▓▓▓▓▓▓▓▓▓▓▓▓
                 ╣▓▓▓▓▓▓▓▓▓▓▓▓▓╢╢╢╢╢╢╢╣╢╢╢╢╢╢╢╢╢╫▓▓▓▓,    '▀▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▀
                 ▀╢▓▓▓▓▀└╔▓▓▓▓▒╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╢╣▓▓▓▄        `└╙▀▀▀▀▀▀╙.
                         ╙▀▀▀▒╢╢╢╢╢╢╢▓╚╢╢╢╢╢▓▓╢╢╢╢╢▓▓▄
                            ╢╢╢╫╝'╙╨╜    ╙╜`   ╩╢╣╢╢▓▓▄
                           /╨╙'   r^          '═  '╙╙╙▀L
                          ╒                            ▒,
                                  💖         💖         └▒
               [ *`ⁿ.    ╛                              ▒▒    ' M`]
              ]`╙     ⌐ ▓█▄           ▐╦╥▄╦▓            └▒▄ ^    '`ƒ
              ┐        ▓▓▓▓▓▄          └╙▀            ,▄█▓▓▀       ;
               \        ╘▀▓▓▓▓▄                     ,█▓▓▓▀        ╒
                Y         ╙▓▓▓▓█                   ▄▓▓▓▓╘        ¿
                 ^         ▓▓▓▓▓█                 ▐▓▓▓▓▌        ╛
                   \       ▐▓▓▓▓▓▓▄▄▄▓████▓▄▄▄▄▄,,▓▓▓▓▓▌      ⌐
                           ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀

   
 ");
        }

        static void OverPriced()
        {
            SoundPlayer Overpriced = new SoundPlayer("Overpriced.wav");
            Overpriced.Load();
            Overpriced.Play();
            Console.WriteLine("Bạn mua mắc quá vậy! 😱");
        }
        static void Lack()
        {
            SoundPlayer lack = new SoundPlayer("Lack.wav");
            lack.Load();
            lack.Play();
            Console.WriteLine("Bạn mua thiếu hàng. 😔");
        }
        // Hàm phát nhạc và di chuyển linh vật

        static void DisplayMascotWithMusic()
        {
            SoundPlayer proudUEH = new SoundPlayer("proudUEHcombine.wav");
            proudUEH.Load();
            proudUEH.Play();
            DisplayMascot();
        }

        // Hàm hiển thị linh vật


        static void DisplayMascot()
        {
            Console.WriteLine(@"
                                                        ,,,,,,,        ,▄▄╬▓▓▓▓▄,
                                            ,▄╦▄▄@▄▄▓▓▓▓▓▓▒▒╣▒▒▒▓▓▄,  ▄▓▒╢╢╢╣▓▓╣▒▓▄
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
                                                      ▀▓▒▄▓╣╢╢╢╣▓╢╢╢╢╢╢╢╢╢╢╢╢╢▒▒╣╢▓▓▓▒▒▒▀█▓▒▒▒╢╢╢▒▒▓▀'
                                                        ▀▓▓▓▒╢▒▓▒╢╢╢╢╢▒▒╢╢╢╢▒╣╢╢╢╢╫▓╢╢▒▓   
                                                         ╙▌▀▀▀▐▌╢╢▒▓▒▓╜""╩╣▒╩╙▒╢╢╢╢▒▌  
                                                          ▐  ╒▀╩▓╨  ,▄▄       ,▄╙╨▓╩▓▄ 
                                                           [ ▌       ▄▄      ,▄▄     ▐ 
                                                           ╙▀▄      ▐▓█      ▐▓█      ▐ 
                                                          ▄▐▀    ,,  ▀       ▀  ,,    ▓
                                                         ╓▀█▄           ▓▓▓▓          ▄▌
                                                        ▄╖▓▓███▄▄                 ,▄▄████▄▄,,
                                                     , ▀▄▓▓▓▓▓██████▄▄▄▄▄, ▄▄▄▄▄██████▓▓▓▌╢╢▒▀
                                                       █▓▓▓▓▓▓▓▓▓▓███████▓████████▓▓▓▓▓▓▓█╢╢▌ º▓ ▐╙▀▌
                                                      █▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓█▓▓▓▓▓▌▌UEH ▓▓█╣▓   ▒▐▓▄▄▄Γ
                                                     ▌▓▓▓▓▓▓▓▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█╬█M²═══╩▌æ╜
                                                     ""▓▓▓▄▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▀▀
                                                      ▐,    ,▓█▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█
                                                        N▀▓ █▄█████▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓██████▌▀
                                                            ███████████▀▀▀▀▀▀███████████
                                                            ""▓░,,▀▀▀▀▀█      ▀█▀▀▀▀▀,,▄▀
                                                                                ''

");
        }



        // Hàm gửi yêu cầu GET với headers và query parameters
        static async Task<HttpResponseMessage> SendGetRequestAsync(string url, Dictionary<string, string> headers, Dictionary<string, string> queryParams)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{url}?{string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");

            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            return await client.SendAsync(request);
        }

    }
}