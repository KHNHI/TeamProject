namespace Quanlychitieu
{
     internal class BudgetPlanner
    {

        private ExpenseTracker expenseTracker;
        //private DataSync dataSync;
        private const string BUDGET_FILE = "budget1.csv"; // Tên file bạn muốn lưu
        public Dictionary<string, decimal> categoryBudgets { get;  set; }
        Dictionary<string, DateTime> categoryLastSetTimes = new Dictionary<string, DateTime>();
        private Dictionary<string, bool> categoryBudgetSet; // Tracks if a budget is set for each category
        private HashSet<string> enteredCategories; // Khai báo biến thành viên
        private List<string> remainingCategories; // Khai báo biến thành viên
        private readonly string[] validCategories = new string[]
        {
               "Ăn uống", "Đi lại", "Chi phí cố định", "Giải trí", "Giáo dục", "Mua sắm", "Khác"
        };
        private DateTime lastBudgetSetTime;
        public BudgetPlanner(ExpenseTracker expenseTracker)
        {
            enteredCategories = new HashSet<string>(); // Khởi tạo HashSet
            remainingCategories = new List<string>(); // Khởi tạo List
            categoryBudgets = new Dictionary<string, decimal>(); // Khởi tạo ở đây
            categoryBudgetSet = new Dictionary<string, bool>(); // Initialize the budget set tracking dictionary
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

            List<string> remainingCategories = new List<string>(validCategories);
            HashSet<string> enteredCategories = new HashSet<string>();
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

//            string[] titleMoneyTidy =
//     {

//"███╗   ███╗ ██████╗ ███╗   ██╗███████╗██╗   ██╗              ████████╗██╗██████╗ ██╗   ██╗",
//"████╗ ████║██╔═══██╗████╗  ██║██╔════╝╚██╗ ██╔╝              ╚══██╔══╝██║██╔══██╗╚██╗ ██╔╝",
//"██╔████╔██║██║   ██║██╔██╗ ██║█████╗   ╚████╔╝     █████╗       ██║   ██║██║  ██║ ╚████╔╝ ",
//"██║╚██╔╝██║██║   ██║██║╚██╗██║██╔══╝    ╚██╔╝      ╚════╝       ██║   ██║██║  ██║  ╚██╔╝  ",
//"██║ ╚═╝ ██║╚██████╔╝██║ ╚████║███████╗   ██║                    ██║   ██║██████╔╝   ██║   ",
//"╚═╝     ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝   ╚═╝                    ╚═╝   ╚═╝╚═════╝    ╚═╝   ",



//      };
            //Console.ForegroundColor = ConsoleColor.Yellow;
            //Program.DrawCenteredBorder(titleMoneyTidy);

            //Console.ForegroundColor = ConsoleColor.Yellow;
            //foreach (string line in title)
            //{
            //    int padding = (windowWidth - line.Length) / 2; // Tính toán căn giữa
            //    Console.WriteLine(line.PadLeft(padding + line.Length));
            //}
            Console.ResetColor();


            // khởi tạo categoryBudgetSet cho các danh mục trong validCategories
            foreach (var category in validCategories)
            {
                if (!categoryBudgetSet.ContainsKey(category))
                {
                    categoryBudgetSet[category] = false; // Not set yet
                }
            }
            if (CheckAllBudgetsSet())
            {
                // Nếu phương thức trả về true nghĩa là tất cả danh mục đều đẫ được đặt ngân sách
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Bạn đã nhập ngân sách cho tất cả các danh mục.");
                // Hiện tổng ngân sách, tiết kiệm tạm thời
                decimal totalBudget = GetTotalBudget();
                Console.WriteLine($"Tổng ngân sách của bạn là: {totalBudget:#,##0₫}");
                Console.WriteLine(expenseTracker.GetSavingsStatus()); // Display savings status
                Console.ResetColor();
                Console.WriteLine("Nhấn phím bất kỳ để quay lại menu chính...");
                Console.ReadKey(); // Wait for user to press a key to return to the main menu
                return;
            }

            bool continueInput = true;
            while (continueInput)
            {
                Console.Clear();
                Console.SetCursorPosition(0, originalTop);

                // Redraw the title



                //Console.ForegroundColor = ConsoleColor.Yellow;

                //foreach (string line in titleMoneyTidy)
                //{
                //   Console.WriteLine(line);
                //}
                //Console.ResetColor();




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








                Console.WriteLine("                                         CHỌN DANH MỤC ĐỂ ĐẶT NGÂN SÁCH");

                // Display categories with numbers
                for (int i = 0; i < validCategories.Length; i++)
                {
                    if (!categoryBudgetSet[validCategories[i]]) 
                    {
                        Console.WriteLine($"{i + 1}. {validCategories[i]}");
                    }
                }
                // Yêu cầu người dùng chọn danh mục nhập ngân sách hoặc nhấn esc để thoát
                Console.WriteLine("Chọn danh mục (1-7) để đặt ngân sách hoặc nhấn ESC để thoát: ");
                ConsoleKeyInfo keyInfo = Console.ReadKey(); // true to not display the pressed key
                if (keyInfo.Key == ConsoleKey.Escape) // Check if the Esc key is pressed
                {
                    break; // Exit the loop
                }
                Console.ReadKey();
                // Kiểm tra đầu vào hợp lệ và chuyển sang categoryIndex
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
                        // Set the budget for the selected category
                        categoryBudgets[selectedCategory] = budget;
                        categoryBudgetSet[selectedCategory] = true; // Mark as set
                        Console.WriteLine($"Ngân sách cho {selectedCategory} đã được đặt thành: {budget:#,##0₫}");
                        // Yêu cầu người dùng nhấn phím để tiếp tục
                        Console.WriteLine("Nhấn phím bất kỳ để tiếp tục...");
                        Console.ReadKey(); // Chờ người dùng nhấn phím
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        /*Console.BackgroundColor = ConsoleColor.Red;*/
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


        public void SaveBudgetToCSV(Dictionary<string, decimal> budgets)
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
        public void LoadBudgetFromCSV()
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
        public bool CheckAllBudgetsSet()
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

            Console.WriteLine("\n=== TÌNH TRẠNG NGÂN SÁCH ===");
            Console.WriteLine($"┌{new string('-', 70)}┐");
            Console.WriteLine($"| {"Danh mục",-20} | {"Ngân sách",-15} | {"Đã chi",-15} | {"Còn lại",-15} |");
            Console.WriteLine($"├{new string('-', 70)}┤");

            foreach (var category in validCategories)
            {
                decimal budgetAmount = categoryBudgets.ContainsKey(category) ? categoryBudgets[category] : 0;
                decimal spent = expenses.ContainsKey(category) ? Math.Abs(expenses[category]) : 0;
                decimal remaining = budgetAmount - spent;
                decimal percentageUsed = budgetAmount > 0 ? (spent / budgetAmount) * 100 : 0;
                // Hiển thị thông tin từng danh mục dưới dạng bảng
                Console.WriteLine($"| {category,-20} | {budgetAmount,15:#,##0₫} | {spent,15:#,##0₫} | {remaining,15:#,##0₫} |");

                // Hiển thị cảnh báo nếu vượt quá ngân sách hoặc chi tiêu quá 80%
                if (remaining < 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("|" + new string(' ', 24) + " Cảnh báo: Vượt quá ngân sách!" + new string(' ', 24) + "|");
                    PlaywarningSound();
                    Console.ResetColor();
                }
                else if (percentageUsed > 80)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("|" + new string(' ', 24) + " Cảnh báo: Đã sử dụng hơn 80% ngân sách!" + new string(' ', 24) + "|");
                    PlaywarningSound();
                    Console.ResetColor();
                }


                Console.WriteLine($"├{new string('─', 70)}┤");
            }
            Console.WriteLine($"└{new string('─', 70)}┘");
            Console.WriteLine($"Tổng ngân sách: {GetTotalBudget():#,##0₫}");
        }
        //Console.Clear();
        //    var expenses = expenseTracker.GetExpenses();
            
        //    Console.WriteLine("\n=== TÌNH TRẠNG NGÂN SÁCH  ===");
        //    Console.WriteLine($"Tổng ngân sách: {GetTotalBudget():#,##0₫}");
        //    foreach (var category in validCategories)
        //    {
        //        decimal budgetAmount = categoryBudgets.ContainsKey(category) ? categoryBudgets[category] : 0;
        //        decimal spent = expenses.ContainsKey(category) ? Math.Abs(expenses[category]) : 0;
        //        decimal remaining = budgetAmount - spent;
        //        decimal percentageUsed = budgetAmount > 0 ? (spent / budgetAmount) * 100 : 0;

        //        Console.WriteLine($"Danh mục: {category}");
        //        Console.WriteLine($"Ngân sách: {budgetAmount:#,##0₫}");
        //        Console.WriteLine($"Đã chi: -{spent:#,##0₫}");
        //        Console.WriteLine($"Còn lại: {remaining:#,##0₫}");
        //        Console.WriteLine($"Đã sử dụng: {percentageUsed:F1}%");

        //        if (remaining < 0)
        //        {
        //            Console.WriteLine("Cảnh báo: Bạn đã vượt quá ngân sách!");
        //            PlaywarningSound();
        //        }
        //        else if (percentageUsed > 80)
        //        {
        //            Console.WriteLine("Cảnh báo: Bạn đã sử dụng hơn 80% ngân sách!");
        //            PlaywarningSound();
        //        }
        //        Console.WriteLine();
        //    }
     }
}
