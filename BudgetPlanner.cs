namespace Quanlychitieu
{
     internal class BudgetPlanner
    {

        private ExpenseTracker expenseTracker;
        //private DataSync dataSync;
        private const string BUDGET_FILE = "budget1.csv"; // Tên file bạn muốn lưu
        private Dictionary<string, bool> categoryBudgetSet; // track việc nhập ngân sách cho danh mục
        private readonly string[] validCategories = new string[]
        {
               "Ăn uống", "Đi lại", "Chi phí cố định", "Giải trí", "Giáo dục", "Mua sắm", "Khác"
        };
        private DateTime lastBudgetSetTime;
        Dictionary<string, DateTime> categoryLastSetTimes = new Dictionary<string, DateTime>();
        public Dictionary<string, decimal> categoryBudgets { get; set; }

        public BudgetPlanner(ExpenseTracker expenseTracker)
        {
            categoryBudgets = new Dictionary<string, decimal>(); 
            categoryBudgetSet = new Dictionary<string, bool>(); 
            LoadBudgetFromCSV(); // Tải ngân sách khi khởi tạo
            this.expenseTracker = expenseTracker;
            LoadLastCategoryBudgetSetTime();
            GetTotalBudget();
        }
        public decimal GetTotalBudget()
        {
            return categoryBudgets.Values.Sum();

        }
        public decimal GetBudgetForCategory(string category)
        {

            return categoryBudgets.ContainsKey(category) ? categoryBudgets[category] : 0;
        }

         public void SetCategoryBudget()
        {
            int windowWidth = Console.WindowWidth;
            int originalTop = Console.CursorTop;
            string[] titleMoneyTidy =
            {

                         "███╗   ███╗ ██████╗ ███╗   ██╗███████╗██╗   ██╗              ████████╗██╗██████╗ ██╗   ██╗",
                         "████╗ ████║██╔═══██╗████╗  ██║██╔════╝╚██╗ ██╔╝              ╚══██╔══╝██║██╔══██╗╚██╗ ██╔╝",
                         "██╔████╔██║██║   ██║██╔██╗ ██║█████╗   ╚████╔╝     █████╗       ██║   ██║██║  ██║ ╚████╔╝ ",
                         "██║╚██╔╝██║██║   ██║██║╚██╗██║██╔══╝    ╚██╔╝      ╚════╝       ██║   ██║██║  ██║  ╚██╔╝  ",
                         "██║ ╚═╝ ██║╚██████╔╝██║ ╚████║███████╗   ██║                    ██║   ██║██████╔╝   ██║   ",
                         "╚═╝     ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝   ╚═╝                    ╚═╝   ╚═╝╚═════╝    ╚═╝   ",

            };
            Console.ForegroundColor = ConsoleColor.Yellow;
            Program.DrawCenteredBorder(titleMoneyTidy);
            Console.ResetColor();

            // khởi tạo categoryBudgetSet cho các danh mục trong validCategories
            foreach (var category in validCategories)
            {
                if (!categoryBudgetSet.ContainsKey(category))
                {
                    categoryBudgetSet[category] = false; // Not set yet
                }
            }
            //check xem ngân sách có lớn hơn thu nhập không
            

            if (CheckAllBudgetsSet())
            {
                // Nếu phương thức trả về true nghĩa là tất cả danh mục đều đẫ được đặt ngân sách
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Bạn đã nhập ngân sách cho tất cả các danh mục.");
                // Hiện tổng ngân sách, tiết kiệm tạm thời
                decimal totalBudget = GetTotalBudget();
                Console.WriteLine($"Tổng ngân sách của bạn là: {totalBudget:#,##0₫}");
                Console.WriteLine(expenseTracker.GetSavingsStatus()); // hiện trạng thái tiết kiệm
                Console.ResetColor();
                Console.WriteLine("Nhấn phím bất kỳ để quay lại menu chính...");
                Console.ReadKey(); //đợi người dùng nhấn phím
                return;
            }
 
            bool continueInput = true;
            while (continueInput)
            {
                Console.Clear();
                Console.SetCursorPosition(0, originalTop);
                Console.ForegroundColor = ConsoleColor.Yellow;
                // Redraw the title
                foreach (string line in titleMoneyTidy)
                {
                    int padding = (windowWidth - line.Length) / 2; // Tính toán khoảng cách cần thiết
                    Console.WriteLine(line.PadLeft(padding + line.Length)); // Căn giữa bằng cách thêm khoảng trắng
                };
                
                      // Dòng văn bản cần căn giữa
                Console.ForegroundColor= ConsoleColor.Green;
                string subtitle = "CHỌN DANH MỤC ĐỂ ĐẶT NGÂN SÁCH";
                // Tính toán khoảng cách cần thiết để căn giữa
                int paddingSubtitle = (windowWidth - subtitle.Length) / 2;

                // In dòng văn bản đã căn giữa
                Console.WriteLine(subtitle.PadLeft(paddingSubtitle + subtitle.Length));
                Console.ResetColor();
                // Kiểm tra tổng ngân sách trước khi yêu cầu nhập ngân sách
                if (!IsBudgetValid())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("⚠️ Tổng ngân sách không thể lớn hơn hoặc bằng tổng thu nhập. Không thể tiếp tục đặt ngân sách.");
                    Console.ResetColor();
                    Console.WriteLine("Nhấn phím bất kỳ để quay lại menu chính...");
                    Console.ReadKey(); // Đợi người dùng nhấn phím
                    break; // Kết thúc vòng lặp
                }
                // Hiện danh mục với số tương ứng
                for (int i = 0; i < validCategories.Length; i++)
                {
                    if (!categoryBudgetSet[validCategories[i]]) 
                    {
                        Console.WriteLine($"{i + 1}. {validCategories[i]}");
                    }
                }
           
                // Yêu cầu người dùng chọn danh mục nhập ngân sách hoặc nhấn esc để thoát
                Console.WriteLine("Chọn danh mục (1-7) để đặt ngân sách hoặc nhấn ESC để thoát: ");
                ConsoleKeyInfo keyInfo = Console.ReadKey(); // 
                if (keyInfo.Key == ConsoleKey.Escape) //kiểm tra nếu phím esc nhấn
                {
                    break; // thoát vòng lặp
                }
                Console.ReadKey();
                // Kiểm tra đầu vào hợp lệ và chuyển sang categoryIndex dạng số nguyên
                if (int.TryParse(keyInfo.KeyChar.ToString(), out int categoryIndex) && categoryIndex >= 1 && categoryIndex <= validCategories.Length)
                {
                    string selectedCategory = validCategories[categoryIndex - 1]; 
                    //kiểm tra danh mục chọn đã có ngân sách hay chưa
                    if (categoryBudgetSet[selectedCategory])
                    {
                        Console.WriteLine("\nNgân sách cho danh mục này đã được đặt. Vui lòng chọn danh mục khác.");
                        Console.ReadLine();
                        continue; 
                    }
                   

                    // Yêu cầu người dùng nhập ngân sách cho danh mục đã chọn. Nếu số tiền hợp lệ, lưu ngân sách và đánh dấu danh mục là đã được thiết lập.
                    // Nếu không hợp lệ, thông báo lỗi.
                    decimal budget;
                    Console.WriteLine($"\nNhập ngân sách cho {selectedCategory}: ");
                    if (decimal.TryParse(Console.ReadLine(), out budget) && budget >= 0)
                    {
                        // đặt ngân sách
                        categoryBudgets[selectedCategory] = budget;
                        categoryBudgetSet[selectedCategory] = true; // đánh dấu là đã đặt ngân sách
                        Console.WriteLine($"Ngân sách cho {selectedCategory} đã được đặt thành: {budget:#,##0₫}");
                        // Yêu cầu người dùng nhấn phím để tiếp tục
                        Console.WriteLine("Nhấn phím bất kỳ để tiếp tục...");
                        Console.ReadKey(); // Chờ người dùng nhấn phím
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("⚠ Số tiền không hợp lệ. Vui lòng nhập lại.");
                        Console.ResetColor();

                        Console.ReadLine();
                    }
                 

                }
                else
                {
                    Console.WriteLine("Danh mục không hợp lệ. Vui lòng thử lại.");
                }
               
            }
          
            // Lưu dữ liệu vào file csv sau khi đặt xong ngân sách
            SaveBudgetToCSV(categoryBudgets);
            SaveLastCategoryBudgetSetTime();
             }

        private bool IsBudgetValid()
        {
            decimal totalBudget = GetTotalBudget(); // Lấy tổng ngân sách
            decimal totalIncome = expenseTracker.TotalIncome; // Lấy tổng thu nhập từ ExpenseTracker
            return totalBudget < totalIncome; // Trả về true nếu tổng ngân sách nhỏ hơn tổng thu nhập
        }
        private void SaveBudgetToCSV(Dictionary<string, decimal> budgets)
        {
            using (StreamWriter writer = new StreamWriter(BUDGET_FILE))
            {
                // Ghi tiêu đề cho file CSV
                writer.WriteLine("Category,Budget");

                // Duyệt qua từng danh mục và ghi ngân sách vào file
                foreach (var category in budgets.Keys)
                {
                    writer.WriteLine($"{category},{budgets[category]:0.00}"); // Định dạng số với 2 chữ số thập phân
                }
            }
        }
        private void LoadBudgetFromCSV()
        {
            if (File.Exists(BUDGET_FILE))
            {
                using (StreamReader reader = new StreamReader(BUDGET_FILE))
                {
                    // Bỏ qua tiêu đề
                    string headerLine = reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        var parts = line.Split(',');

                        if (parts.Length == 2 && decimal.TryParse(parts[1], out decimal budget))
                        {
                            string category = parts[0].Trim();
                            categoryBudgets[category] = budget; // Thêm ngân sách vào dictionary
                            categoryBudgetSet[category] = true; // Mark as set
                        }
                    }
                }

                Console.WriteLine("Ngân sách đã được tải từ file CSV.");
            }
            else
            {
                Console.WriteLine("File ngân sách không tồn tại, khởi tạo ngân sách mới.");
            }
        }
        // Kiểm tra xem có tất cả các danh mục đã có ngân sách hay chưa
        private bool CheckAllBudgetsSet()
        {
            foreach (var category in validCategories)
            {
                if (!categoryBudgets.ContainsKey(category))
                {
                    return false; // Nếu có danh mục chưa có ngân sách
                }
            }
            return true; // Tất cả danh mục đã có ngân sách
        }

        private bool CanSetCategoryBudget(string category)
        { // Kiểm tra nếu categoryLastSetTimes không có danh mục đó => được phép đặt ngân sách mới
            if (!categoryLastSetTimes.ContainsKey(category))
            {
                return true;
            }
            // Kiểm tra thời gian hiện tại so với lần đặt ngân sách cuối cùng cho danh mục này
            DateTime lastSetTime = categoryLastSetTimes[category];
            DateTime nextAllowedSetTime = lastSetTime.AddMonths(1);

            // Nếu thời gian hiện tại >= thời gian được phép đặt tiếp, trả về true, ngược lại trả về false
            return DateTime.Now >= nextAllowedSetTime;
        }

        private void SaveLastCategoryBudgetSetTime()
        {
            File.WriteAllText("last_budget_set_time.txt", lastBudgetSetTime.ToString("o"));
        }

        private void LoadLastCategoryBudgetSetTime()
        {
            if (File.Exists("last_budget_set_time.txt"))
            {
                string dateText = File.ReadAllText("last_budget_set_time.txt");
                if (DateTime.TryParse(dateText, out DateTime savedTime))
                {
                    lastBudgetSetTime = savedTime;
                }
                else
                {
                    lastBudgetSetTime = DateTime.MinValue;
                }
            }
            else
            {
                lastBudgetSetTime = DateTime.MinValue;
            }
        }
       
        private void PlaywarningSound()
        {
            Console.Beep();
        }

        private void ShowMessage(string message)
        {
            Console.WriteLine($"Cảnh báo: {message}");
        }




        public void ShowBudgetStatus()
        {
            Console.Clear();
            var expenses = expenseTracker.GetExpenses();

            const int c0 = 20, c1 = 15, c2 = 15, c3 = 15;  // Định nghĩa độ dài của từng cột
            int tableWidth = c0 + c1 + c2 + c3 + 9;  // Chiều rộng của bảng (bao gồm ký tự phân cách)

            // Lấy chiều rộng của cửa sổ console và tính toán khoảng cách để căn giữa
            int windowWidth = Console.WindowWidth;
            int padding = Math.Max((windowWidth - tableWidth) / 2, 0);  // Số khoảng trắng cần chèn vào

            // Hàm hỗ trợ in dòng căn giữa
            void CenteredPrint(string text)
            {
                Console.WriteLine(new string(' ', padding) + text);
            }

            // Tiêu đề tình trạng ngân sách
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            string[] budgetStatuss = {" TÌNH TRẠNG NGÂN SÁCH "};
            Program.DrawCenteredBorder(budgetStatuss);
            Console.ResetColor();


            // In bảng theo kiểu tương tự với kí tự ASCII, có căn giữa
            CenteredPrint($"╔═{new string('═', c0)}═╤═{new string('═', c1)}═╤═{new string('═', c2)}═╤═{new string('═', c3)}═╗");
            CenteredPrint($"║ {"Danh mục",-c0} │ {"Ngân sách",c1} │ {"Đã chi",c2} │ {"Còn lại",c3} ║");
            CenteredPrint($"╟─{new string('─', c0)}─┼─{new string('─', c1)}─┼─{new string('─', c2)}─┼─{new string('─', c3)}─╢");

            foreach (var category in validCategories)
            {
                decimal budgetAmount = categoryBudgets.ContainsKey(category) ? categoryBudgets[category] : 0;
                decimal spent = expenses.ContainsKey(category) ? Math.Abs(expenses[category]) : 0;
                decimal remaining = budgetAmount - spent;
                decimal percentageUsed = budgetAmount > 0 ? (spent / budgetAmount) * 100 : 0;

                // Hiển thị thông tin từng danh mục dưới dạng bảng
                CenteredPrint($"║ {category,-c0} │ {budgetAmount,c1:#,##0₫} │ {spent,c2:#,##0₫} │ {remaining,c3:#,##0₫} ║");

                // Hiển thị cảnh báo nếu vượt quá ngân sách hoặc chi tiêu quá 80%
                if (remaining < 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    CenteredPrint($"║  {"",c3} {" Cảnh báo: Vượt quá ngân sách!"} {"",c0}       ║");
                    PlaywarningSound();
                    Console.ResetColor();
                }
                else if (percentageUsed > 80)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    CenteredPrint($"║ {"",c3} {" Cảnh báo: Đã sử dụng hơn 80% ngân sách!"} {"",c3}   ║");
                    PlaywarningSound();
                    Console.ResetColor();
                }

                CenteredPrint($"╟─{new string('─', c0)}─┼─{new string('─', c1)}─┼─{new string('─', c2)}─┼─{new string('─', c3)}─╢");
            }

            // Đóng khung dưới của bảng
            CenteredPrint($"╚═{new string('═', c0)}═╧═{new string('═', c1)}═╧═{new string('═', c2)}═╧═{new string('═', c3)}═╝");
            // Hiển thị tổng ngân sách, cũng căn giữa
            CenteredPrint($"\n                                                                            " +
        $" Tổng ngân sách: {GetTotalBudget():#,##0₫}");
        }


    }
}
