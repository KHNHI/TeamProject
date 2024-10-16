namespace Quanlychitieu
{
    internal class BudgetPlanner
    {
        private ExpenseTracker expenseTracker;
        private DataSync dataSync;
        public Dictionary<string, decimal> categoryBudgets { get;  set; }
        Dictionary<string, DateTime> categoryLastSetTimes = new Dictionary<string, DateTime>();

        private readonly string[] validCategories = new string[]
        {
               "Ăn uống", "Đi lại", "Chi phí cố định", "Giải trí", "Giáo dục", "Mua sắm", "Khác"
        };
        private DateTime lastBudgetSetTime;
        public BudgetPlanner(ExpenseTracker expenseTracker, DataSync dataSync)
        {
            this.expenseTracker = expenseTracker;
            this.dataSync = dataSync;
            categoryBudgets = new Dictionary<string, decimal>();
            LoadBudgets();
            //categoryBudgets = dataSync.LoadBudgetFromCSV();
            LoadLastCategoryBudgetSetTime();
            GetTotalBudget();
        }
        private void LoadBudgets()
        {
            var loadedBudgets = dataSync.LoadBudgetFromCSV();
            foreach (var category in validCategories)
            {
                if (loadedBudgets.ContainsKey(category))
                {
                    categoryBudgets[category] = loadedBudgets[category];
                }
                else
                {
                    categoryBudgets[category] = 0;
                }
            }
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
                 "    ********  ******* **********       ******   **     ** *******    ********    ******** *********    ",
                 "   **/////// /**///// ////**////      /*////** /**    /*/ **////**  **//////*/* /*//////  ///**///     ",
                 "  /**        /**         /**          /*   /** /**    /*/ **    /* ***      /// /**         /**        ",
                 "  /********  /*******    /**          /******  /**    /*/ **    /* /**          /*******    /**        ",
                 "  ////////*  /**////     /**          /*//// * /**    /*/ **    /* /**    ****  /**////     /**        ",
                 "        /*/  /**         /**          /*    /* /**    /*/ **    ** //**  ////*  /**         /**        ",
                 "   ******/   /*******    /**          /******* /*******/  *******/  /********// /*******    /**        ",
                 " ////////    ////////    /**          ///////   ///////// ///////   /////////   ////////    /**        ",
                                             
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
            dataSync.SaveBudgetToCSV(categoryBudgets);
            
           
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

        public void SuggestBudgetAdjustments()
        {
            var expenses = expenseTracker.GetExpenses();
            if (expenses == null || expenses.Count == 0 || categoryBudgets == null || categoryBudgets.Count == 0)
            {
                Console.WriteLine("Không có dữ liệu để phân tích.");
                return;
            }
            var totalBudget = GetTotalBudget();
            
            var totalExpenses = expenses.Sum(e => Math.Abs(e.Value));

            Console.WriteLine("\n=== Đề xuất Điều chỉnh Ngân sách ===");
            Console.WriteLine($"Tổng ngân sách: {totalBudget:#,##0₫}");
            Console.WriteLine($"Tổng chi tiêu: {totalExpenses:#,##0₫}");
            if (totalExpenses > totalBudget)
            {
                Console.WriteLine("Bạn đang chi tiêu nhiều hơn tổng ngân sách. Hãy xem xét cắt giảm chi tiêu hoặc tăng ngân sách.");
            }
            else
            {
                Console.WriteLine($"Tổng chi tiêu nằm trong ngân sách. Ngân sách tổng: {totalBudget:#,##0₫}, Chi tiêu tổng: {totalExpenses:#,##0₫}");
            }
            foreach (var category in validCategories)
            {
                decimal spent = expenses.ContainsKey(category) ? expenses[category] : 0;
                decimal budgetAmount = categoryBudgets.ContainsKey(category) ? categoryBudgets[category] : 0;

                if (budgetAmount == 0)
                {
                    Console.WriteLine($"Đề xuất: Thiết lập ngân sách cho danh mục '{category}'. Chi tiêu hiện tại: {spent:#,##0₫}");
                }
                else if (spent > budgetAmount)
                {
                    decimal overspent = spent - budgetAmount;
                    Console.WriteLine($"Đề xuất: Tăng ngân sách cho '{category}' thêm {overspent:#,##0₫} hoặc cắt giảm chi tiêu.");
                }
                else if (spent < budgetAmount * 0.5m)
                {
                    Console.WriteLine($"Đề xuất: Xem xét giảm ngân sách cho '{category}' vì chi tiêu thấp hơn 50% ngân sách.");
                }
            }
        }
    }
}
