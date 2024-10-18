namespace Quanlychitieu
{
     internal class BudgetPlanner
    {

        private ExpenseTracker expenseTracker;
        //private DataSync dataSync;
        private const string BUDGET_FILE = "budget1.csv"; // Tên file bạn muốn lưu
        public Dictionary<string, decimal> categoryBudgets { get;  set; }
        Dictionary<string, DateTime> categoryLastSetTimes = new Dictionary<string, DateTime>();

        private readonly string[] validCategories = new string[]
        {
               "Ăn uống", "Đi lại", "Chi phí cố định", "Giải trí", "Giáo dục", "Mua sắm", "Khác"
        };
        private DateTime lastBudgetSetTime;
        public BudgetPlanner(ExpenseTracker expenseTracker)
        {
            categoryBudgets = new Dictionary<string, decimal>(); // Khởi tạo ở đây
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
            
            bool continueInput = true;
            int windowWidth = Console.WindowWidth;


            string[] title =
     {
"  ███╗   ███╗ ██████╗ ███╗   ███╗███████╗██╗   ██╗████████╗██╗██████╗ ██╗   ██╗",
"  ████╗ ████║██╔═══██╗████╗ ████║██╔════╝╚██╗ ██╔╝╚══██╔══╝██║██╔══██╗╚██╗ ██╔╝",
"  ██╔████╔██║██║   ██║██╔████╔██║█████╗   ╚████╔╝    ██║   ██║██║  ██║ ╚████╔╝ ",
"  ██║╚██╔╝██║██║   ██║██║╚██╔╝██║██╔══╝    ╚██╔╝     ██║   ██║██║  ██║  ╚██╔╝  ",
"  ██║ ╚═╝ ██║╚██████╔╝██║ ╚═╝ ██║███████╗   ██║      ██║   ██║██████╔╝   ██║   ",
"  ╚═╝     ╚═╝ ╚═════╝ ╚═╝     ╚═╝╚══════╝   ╚═╝      ╚═╝   ╚═╝╚═════╝    ╚═╝   ",



      };

          

            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (string line in title)
            {
                int padding = (windowWidth - line.Length) / 2; // Tính toán căn giữa
                Console.WriteLine(line.PadLeft(padding + line.Length));
            }
            Console.ResetColor();
          
            do
            {
               
                if (remainingCategories.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Bạn đã nhập ngân sách cho tất cả các danh mục.");
                    Console.ResetColor();
                    break;
                }
                string categoryTitle = "CHỌN DANH MỤC ĐỂ ĐẶT NGÂN SÁCH";
                int padding = (windowWidth - categoryTitle.Length) / 2;

                Console.WriteLine("" + categoryTitle.PadLeft(padding + categoryTitle.Length).PadRight(windowWidth - 2) + "║");
                for (int i = 0; i < validCategories.Length; i++)
                {
                    if (remainingCategories.Contains(validCategories[i]))
                    {
                        Console.WriteLine($"{i + 1}. {validCategories[i]}");
                    }

                }

                int choice;
                while (true)
                {
                    Console.Write("Nhập số tương ứng với danh mục: ");
                    if (int.TryParse(Console.ReadLine(), out choice) && choice >= 1 && choice <= validCategories.Length)
                    {
                        string selectedCategory = validCategories[choice - 1];
                        if (!CanSetCategoryBudget(selectedCategory))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("╔══════════════════════════════════════════╗");
                            Console.WriteLine("║ ⚠ Bạn đã nhập ngân sách cho danh mục  này. Bạn chỉ có thể đặt 1 lần mỗi tháng..║");
                            Console.WriteLine("╚══════════════════════════════════════════╝");
                            Console.ResetColor();
                        }
                        else if (remainingCategories.Contains(selectedCategory))
                        {
                            enteredCategories.Add(selectedCategory);
                            remainingCategories.Remove(selectedCategory);
                            break;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("⚠ Danh mục này không còn trong danh sách. Vui lòng chọn danh mục khác.");
                            Console.ResetColor();
                        }


                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("⚠ Lựa chọn không hợp lệ. Vui lòng thử lại.");
                        Console.ResetColor();
                    }
                }

                decimal budget; 
              
                while (true)
                {
                    Console.Write($"Nhập số tiền ngân sách cho {validCategories[choice - 1]}: ");
                    if (decimal.TryParse(Console.ReadLine(), out budget) && budget >= 0)
                    {
                        categoryBudgets[validCategories[choice - 1]] = budget;
                        categoryLastSetTimes[validCategories[choice - 1]] = DateTime.Now;
                        Console.WriteLine($"✔ Ngân sách cho {validCategories[choice - 1]} đã được đặt {budget:#,##0₫}");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Số tiền không hợp lệ. Vui lòng nhập một số dương.");
                    }
                }

                if (remainingCategories.Count > 0)
                {
                    Console.WriteLine($"Bạn còn {remainingCategories.Count} danh mục chưa nhập ngân sách");

                    Console.Write("Bạn có muốn tiếp tục nhập ngân sách cho các danh mục còn lại? (y/n): ");
                    string input2 = Console.ReadLine()?.ToLower() ?? "n";
                    if (input2 == "y")
                    {
                        continueInput = true; // Đặt biến kiểm soát thành false nếu người dùng không muốn tiếp tục
                    }
                    else if (input2 == "n")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("╔══════════════════════════════════════════╗");
                        Console.WriteLine("║ LƯU Ý:                                   ║");
                        Console.WriteLine("║ Bạn chỉ nên nhập chi tiêu cho những      ║");
                        Console.WriteLine("║ danh mục đã đặt ngân sách (khuyến khích  ║");
                        Console.WriteLine("║ nên đặt ngân sách cho 1 lần).            ║");
                        Console.WriteLine("╚══════════════════════════════════════════╝");
                        Console.ResetColor();

                        Console.WriteLine("Nhấn phím 'c' để tiếp tục đặt ngân sách hoặc nhấn bất kỳ phím để thoát");
                        string input3 = Console.ReadLine()?.ToLower() ?? "x";
                        if (input3 == "c")
                        {
                            continueInput = true;
                        }
                        else
                        {
                            continueInput = false;
                            break;
                        }

                       

                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Vui lòng nhập đúng ký tự (y/n/c)");
                        Console.ResetColor();
                        continueInput = true;
                    }


                }
                else
                {
                    Console.WriteLine("Tất cả danh mục đã được đặt ngân sách.");
                    continueInput = false;
                }


            } while (continueInput);
            
            //lastBudgetSetTime = DateTime.Now;
            SaveLastCategoryBudgetSetTime();

            // Lưu dữ liệu vào file csv sau khi đặt xong ngân sách
            SaveBudgetToCSV(categoryBudgets);
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
        {
            if (categoryLastSetTimes.ContainsKey(category))
            {
                // Kiểm tra xem có đủ một tháng kể từ lần đặt ngân sách trước hay chưa
                return DateTime.Now >= categoryLastSetTimes[category].AddMonths(1);
            }

            // Nếu danh mục chưa có ngân sách trước đó, cho phép đặt ngân sách
            return true;
          
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
            var expenses = expenseTracker.GetExpenses();

            Console.WriteLine("\n=== Tình trạng Ngân sách ===");
            Console.WriteLine($"Tổng ngân sách: {GetTotalBudget():#,##0₫}");
            foreach (var category in validCategories)
            {
                decimal budgetAmount = categoryBudgets.ContainsKey(category) ? categoryBudgets[category] : 0;
                decimal spent = expenses.ContainsKey(category) ? Math.Abs(expenses[category]) : 0;
                decimal remaining = budgetAmount - spent;
                decimal percentageUsed = budgetAmount > 0 ? (spent / budgetAmount) * 100 : 0;

                Console.WriteLine($"Danh mục: {category}");
                Console.WriteLine($"Ngân sách: {budgetAmount:#,##0₫}");
                Console.WriteLine($"Đã chi: -{spent:#,##0₫}");
                Console.WriteLine($"Còn lại: {remaining:#,##0₫}");
                Console.WriteLine($"Đã sử dụng: {percentageUsed:F1}%");

                if (remaining < 0)
                {
                    Console.WriteLine("Cảnh báo: Bạn đã vượt quá ngân sách!");
                    PlaywarningSound();
                }
                else if (percentageUsed > 80)
                {
                    Console.WriteLine("Cảnh báo: Bạn đã sử dụng hơn 80% ngân sách!");
                    PlaywarningSound();
                }
                Console.WriteLine();
            }
        }

    
    }
}
