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

            //bool continueInput = true;
            int windowWidth = Console.WindowWidth;
            int originalTop = Console.CursorTop;


            Console.ResetColor();

            // Initialize categoryBudgetSet for valid categories
            foreach (var category in validCategories)
            {
                if (!categoryBudgetSet.ContainsKey(category))
                {
                    categoryBudgetSet[category] = false; // Not set yet
                }
            }
            if (CheckAllBudgetsSet())
            {
                // If the method returns true, it means budgets have been set for all categories
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Bạn đã nhập ngân sách cho tất cả các danh mục.");
                Console.ResetColor();

                // Display total budget and savings status
                decimal totalBudget = GetTotalBudget();
                Console.WriteLine($"Tổng ngân sách của bạn là: {totalBudget:#,##0₫}");
                Console.WriteLine(expenseTracker.GetSavingsStatus()); // Display savings status
                Console.WriteLine("Nhấn phím bất kỳ để quay lại menu chính...");
                Console.ReadKey(); // Wait for user to press a key to return to the main menu
                return;
            }

            bool continueInput = true;
            while (continueInput)
            {
                Console.Clear();
                Console.SetCursorPosition(0, originalTop);



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


                string[] SetBudgetCateories = { "CHỌN DANH MỤC ĐỂ ĐẶT NGÂN SÁCH"};
                Program.DrawCenteredBorder(SetBudgetCateories);
              


                // Display categories with numbers
                for (int i = 0; i < validCategories.Length; i++)
                {
                    if (!categoryBudgetSet[validCategories[i]]) // Only show categories that haven't been budgeted
                    {
                        Console.WriteLine($"{i + 1}. {validCategories[i]}");
                    }
                }
                // Prompt user to enter a category number or exit
                Console.WriteLine("Chọn danh mục (1-7) để đặt ngân sách hoặc nhấn ESC để thoát: ");
                ConsoleKeyInfo keyInfo = Console.ReadKey(); // true to not display the pressed key
                if (keyInfo.Key == ConsoleKey.Escape) // Check if the Esc key is pressed
                {
                    break; // Exit the loop
                }
                Console.ReadKey();
                // Validate input and convert to an index
                if (int.TryParse(keyInfo.KeyChar.ToString(), out int categoryIndex) && categoryIndex >= 1 && categoryIndex <= validCategories.Length)
                {
                    string selectedCategory = validCategories[categoryIndex - 1]; // Get the category based on user input

                    // Check if the budget for this category has already been set
                    if (categoryBudgetSet[selectedCategory])
                    {
                        Console.WriteLine("\nNgân sách cho danh mục này đã được đặt. Vui lòng chọn danh mục khác.");
                        Console.ReadLine();
                        continue; // Skip to the next iteration
                    }
                    // Prompt for budget amount
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

            // Console.WriteLine("Ngân sách đã được lưu vào file CSV.");
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
        // Kiểm tra xem có tất cả các danh mục đã có ngân sách hay không

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
            string[] budgettStatus = {"TÌNH TRẠNG NGÂN SÁCH"};
            Program.DrawCenteredBorder(budgettStatus);
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
                    CenteredPrint($"║ {"",-c0} │ {"",-c1} │ {"",-c2} │ {" Cảnh báo: Vượt quá ngân sách!",c3} ║");
                    PlaywarningSound();
                    Console.ResetColor();
                }
                else if (percentageUsed > 80)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    CenteredPrint($"║ {"",-c0} │ {"",-c1} │ {"",-c2} │ {" Cảnh báo: Đã sử dụng hơn 80% ngân sách!",c3} ║");
                    PlaywarningSound();
                    Console.ResetColor();
                }

                CenteredPrint($"╟─{new string('─', c0)}─┼─{new string('─', c1)}─┼─{new string('─', c2)}─┼─{new string('─', c3)}─╢");
            }

            // Đóng khung dưới của bảng
            CenteredPrint($"╚═{new string('═', c0)}═╧═{new string('═', c1)}═╧═{new string('═', c2)}═╧═{new string('═', c3)}═╝");

            // Hiển thị tổng ngân sách, cũng căn giữa

            CenteredPrint($"\n                                                                  " +
                $"Tổng ngân sách: {GetTotalBudget():#,##0₫}");
        }



          
    }
}
