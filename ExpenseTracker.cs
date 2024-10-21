using Newtonsoft.Json;

namespace Quanlychitieu
{
    internal class ExpenseTracker
    {
        private Dictionary<string, decimal> expenses = new Dictionary<string, decimal>();
        private List<Expense> expenseList = new List<Expense>();

        private string filePath = "expenses.json";
        private Dictionary<string, Dictionary<int, double>> monthlyExpenses;
        private int currentMonth = DateTime.Now.Month;
        //private DataSync dataSync;
        private BudgetPlanner budgetPlanner;
        private string categoryChoice;
        private decimal savings; // Private field to track savings
        private string[] validCategories =
       {
            "Ăn uống",
            "Đi lại",
            "Chi phí cố định",
            "Giải trí",
            "Giáo dục",
            "Mua sắm",
            "Khác"
        };

        public decimal TotalIncome { get; private set; } = 0;//Tổng thu nhập
        public decimal TotalBudget => budgetPlanner.GetTotalBudget();
        public decimal TotalExpenses => GetTotalExpenses();//Tổng chi tiêu
        public decimal Savings => TotalIncome - TotalBudget;
        private bool incomeEnteredThisMonth = false;//Kiểm tra xem đã nhập thu nhập cho tháng này chưa 
        private DateTime lastIncomeEntryTime;// Thời gian nhập thu gần nhất 

        public class Expense // Nested class
        {
            public string Category { get; set; }
            public decimal Amount { get; set; }
            public DateTime Date { get; set; }

            public Expense(string category, decimal amount, DateTime date)
            {
                Category = category;
                Amount = amount;
                Date = date;
            }
        }
        public ExpenseTracker()//BudgetPlanner budgetPlanner)
        {
            this.budgetPlanner = budgetPlanner;
            monthlyExpenses = new Dictionary<string, Dictionary<int, double>>();
            LoadExpenses();
            LoadMockExpenses();
            LoadIncomeEntryTime();
            LoadTotalIncome();
            LoadIncomeEnteredStatus();
        }
        public void SetBudgetPlanner(BudgetPlanner planner)
        {
            budgetPlanner = planner; // Thiết lập mối quan hệ sau khi khởi tạo
        }
        //Kiểm tra người dùng đã nhập khoản thu nhập trong tháng này chưa 
        public bool CanEnterIncome()
        {
            if (DateTime.Now >= lastIncomeEntryTime.AddMonths(1))
            {
                incomeEnteredThisMonth = false;
            }
            return !incomeEnteredThisMonth;
        }
        //private decimal GetOverspending()
        //{
        //    return TotalExpenses > TotalBudget ? TotalExpenses - TotalBudget : 0;
        //}
        private string GetExpenseCategory()
        {
            if (categoryChoice == null)
            {
                return "Khác"; // Giá trị mặc định khi categoryChoice là null
            }
            switch (categoryChoice)
            {
                case "1": return "Ăn uống";
                case "2": return "Đi lại";
                case "3": return "Chi phí cố định";
                case "4": return "Giải trí";
                case "5": return "Giáo dục";
                case "6": return "Mua sắm";
                case "7": return "Khác";
                default: return "Khác";
            }
        }
        public void EnterExpense()
        {
            while (true) 
            {
                try
                {
                    decimal totalIncome = TotalIncome; // Lấy tổng thu nhập
                    decimal totalExpenses = GetTotalExpenses(); // Lấy tổng chi tiêu
                    // Kiểm tra xem tổng chi tiêu có bằng tổng thu nhập không
                    if (totalExpenses >= totalIncome)
                    {
                        Console.ForegroundColor = ConsoleColor.Red; // Đổi màu chữ thành đỏ
                        Console.WriteLine("⚠️ Bạn không thể nhập thêm chi tiêu vì tổng chi tiêu đã bằng hoặc vượt quá tổng thu nhập.");
                        Console.ReadLine();
                        Console.ResetColor(); // Đặt lại màu về mặc định
                        return; // Kết thúc phương thức nếu điều kiện đúng
                    }
                    Console.WriteLine("Nhấn phím tương ứng để chọn danh mục chi tiêu (hoặc nhấn ESC để quay lại menu chính): ");
                    //Đọc phím
                    ConsoleKeyInfo keyInfo = Console.ReadKey();
                    // Kiểm tra người dùng có nhấn ESC để thoát
                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        break; //Thoát vòng lặp về menu chính
                    }
                    Console.WriteLine();
                    // Chuyển phím nhấn thành index
                    if (int.TryParse(keyInfo.KeyChar.ToString(), out int categoryIndex) && categoryIndex >= 1 && categoryIndex <= validCategories.Length)
                    {
                        string category = validCategories[categoryIndex - 1]; // Lấy danh mục dựa trên dữ liệu đàu vào của người dùng
                        if (!expenses.ContainsKey(category))
                        {
                            expenses[category] = 0; // Khởi tạo giá trị cho danh mục
                        }
                       
                        decimal budgetForCategory = budgetPlanner.GetBudgetForCategory(category);
                        decimal amountSpent = expenses[category];
                        ShowExpenseInfo(category,budgetForCategory,amountSpent);
                        if (budgetForCategory <= 0)
                        {   Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Chưa có ngân sách cho danh mục '{category}'. Vui lòng đặt ngân sách trước khi nhập chi tiêu.");
                            Console.Write($"Bạn có muốn đặt ngân sách ngay không? (y/n): ");
                            var input = Console.ReadLine();
                            if (input?.ToLower() == "y")
                            {
                                Console.Clear();
                                try
                                {
                                    budgetPlanner.SetCategoryBudget();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Đã xảy ra lỗi khi gọi SetCategoryBudget: " + ex.Message);
                                }
                            }
                            continue; 
                        }

                        Console.Write("Nhập số tiền chi tiêu: ");
                        if (decimal.TryParse(Console.ReadLine(), out decimal amount) && amount > 0)
                        {
                            EnterTransaction(category, amount, true);
                        }
                        else
                        {
                            Console.WriteLine("Số tiền không hợp lệ.");
                        }
                        if (expenses.ContainsKey(category))
                        {
                            amountSpent = expenses[category];
                            ShowExpenseInfo(category, budgetForCategory, amountSpent);
                           // Console.WriteLine($"Số tiền bạn đã chi tiêu cho {category} là {Math.Abs(expenses[category]).ToString("#,##0₫")}");
                        }
                        else
                        {
                            Console.WriteLine($"Chưa có chi tiêu nào cho danh mục '{category}'.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Danh mục không hợp lệ. Vui lòng thử lại.");
                    }

                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine("Lỗi: Một giá trị null đã được tìm thấy. " + ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Đã xảy ra lỗi: " + ex.Message);
                }
            }
        }
        public void ShowExpenseInfo(string category, decimal budgetForCategory, decimal amountSpent)
        {
           
            string header = $"Danh mục: {category}";
            string budgetInfo = $"Ngân sách đã đặt cho '{category}': {budgetForCategory:#,##0₫}";
            string expenseInfo = $"Số tiền đã chi tiêu cho '{category}': {Math.Abs(amountSpent):#,##0₫}";

           
            int maxLength = Math.Max(Math.Max(header.Length, budgetInfo.Length), expenseInfo.Length) + 4;

            // Tạo khung trang trí
            string topBorder = "╔" + new string('═', maxLength) + "╗";
            string bottomBorder = "╚" + new string('═', maxLength) + "╝";

            // Hiển thị thông tin trong khung
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(topBorder);
            Console.WriteLine("║ " + header.PadRight(maxLength - 2) + " ║");
            Console.WriteLine("║ " + budgetInfo.PadRight(maxLength - 2) + " ║");
            Console.WriteLine("║ " + expenseInfo.PadRight(maxLength - 2) + " ║");
            Console.WriteLine(bottomBorder);
            Console.ResetColor();
        }
        private void EnterTransaction(string category, decimal amount, bool isExpense)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            // Thêm chi tiêu vào danh sách nếu là chi tiêu
            if (isExpense)
            {
                Expense expense = new Expense(category, amount, DateTime.Now);
                expenseList.Add(expense); // Thêm chi tiêu vào danh sách
            }

            int categoryIndex = Array.IndexOf(validCategories, category);
            // Kiểm tra và khởi tạo danh mục chi tiêu trong monthlyExpenses
            if (!monthlyExpenses.ContainsKey(category))
            {
                monthlyExpenses[category] = new Dictionary<int, double>();
            }
            // Khởi tạo chi tiêu cho tháng hiện tại nếu chưa có
            if (!monthlyExpenses[category].ContainsKey(month))
            {
                monthlyExpenses[category][month] = 0;
            }
            // Cập nhật chi tiêu hàng tháng
            monthlyExpenses[category][month] += (double)amount;
            // Cập nhật tổng chi tiêu của từng danh mục
            if (expenses.ContainsKey(category))
            {
                expenses[category] += amount;
            }
            else
            {
                expenses[category] = amount;
            }
            // Tính toán tổng chi tiêu và ngân sách
            decimal totalExpenses = GetTotalExpenses();// lấy tổng chi tiêu
            decimal totalBudget = TotalBudget;// tổng ngân sách từ budgetPlanner
            decimal totalIncome = TotalIncome; // tổng thu nhập
            // trạng thái của savings dựa trên tổng chi tiêu và ngân sách
            if (totalExpenses <= totalBudget)
            {
                savings = totalIncome - totalBudget; // Savings when within budget
            }
            else
            {
                savings = totalIncome - totalExpenses; // Savings when exceeding budget
                decimal overspending = totalExpenses - totalBudget; // Calculate overspending
                Console.WriteLine($"Bạn đã chi tiêu vượt tổng ngân sách. Số tiền vượt ngân sách ({overspending:#,##0₫}) đã được trừ vào tiết kiệm tạm thời.");
            }
            //lưu chi tiêu, hiện thông báo
            SaveExpenses();
            string transactionType = isExpense ? "chi tiêu" : "thu nhập";
            Console.WriteLine($"Đã lưu {transactionType}: {Math.Abs(amount)} vào danh mục '{category}' vào lúc {timestamp}.");
            Console.WriteLine($"Số tiền bằng chữ: {ConvertNumberToWords((long)Math.Abs(amount))}");
        }
        public void EnterIncome()
        {
            if (incomeEnteredThisMonth)
            {
                if (DateTime.Now >= lastIncomeEntryTime.AddMonths(1))
                {
                    incomeEnteredThisMonth = false;
                }
                else
                {
                    Console.WriteLine("Khoản thu nhập đã được nhập cho tháng này. Không thể nhập lại.");
                    return;
                }
            }
            DisplayCenteredMessageInBox("Nhập số tiền thu nhập:");//Tạo khung cho box nhập liệu
            decimal amount = 0;
            while (true)
            {
                string? incomeInput = Console.ReadLine();
                if (string.IsNullOrEmpty(incomeInput) || !decimal.TryParse(incomeInput, out amount))//Kiểm tra đầu vào
                {
                    Console.WriteLine("Số tiền không hợp lệ, vui lòng nhập lại");
                }
                else
                {
                    break; //Thoát vòng lặp nếu đầu vào hợp lệ
                }
            }
            Console.WriteLine($"Đã thêm khoản thu nhập: {amount:#,##0₫}");
            TotalIncome += amount;
            savings += amount;
            incomeEnteredThisMonth = true;
            lastIncomeEntryTime = DateTime.Now;
            expenses["Thu nhập"] = TotalIncome;
            SaveExpenses();
            SaveIncomeEntryTime();
            SaveTotalIncome();
            SaveIncomeEnteredStatus();
        }
        public void DisplayCenteredMessageInBox(string message)
        {
            // Độ dài tối thiểu của khung
            Console.ForegroundColor = ConsoleColor.Yellow;
            int boxWidth = Math.Max(message.Length + 4, 40); // Đảm bảo khung đủ rộng, ít nhất là 40 ký tự
            string topBorder = "╔" + new string('═', boxWidth) + "╗";
            string bottomBorder = "╚" + new string('═', boxWidth) + "╝";

            // Tính toán vị trí của thông điệp ở giữa khung
            int padding = (boxWidth - message.Length) / 2;
            string paddedMessage = new string(' ', padding) + message + new string(' ', boxWidth - message.Length - padding);

            // Hiển thị khung và thông điệp
            Console.WriteLine(topBorder);
            Console.WriteLine("║" + paddedMessage + "║");
            Console.WriteLine(bottomBorder);
            Console.ResetColor();
        }
        private void SaveTotalIncome()
        {
            File.WriteAllText("total_income.txt", TotalIncome.ToString("F2")); // Lưu tổng thu nhập với 2 chữ số thập phân
        }
        private void LoadTotalIncome()
        {
            if (File.Exists("total_income.txt"))
            {
                string incomeText = File.ReadAllText("total_income.txt");
                if (decimal.TryParse(incomeText, out decimal savedIncome))
                {
                    TotalIncome = savedIncome;
                }
            }
            else
            {
                TotalIncome = 0; // Nếu chưa có file, thì tổng thu nhập là 0
            }
        }
        private void SaveIncomeEnteredStatus()//Tệp lưu trạng thái của thu nhập
        {
            File.WriteAllText("income_status.txt", incomeEnteredThisMonth.ToString());
        }
        private void LoadIncomeEnteredStatus()//tải trạng thái khi khởi động
        {
            if (File.Exists("income_status.txt"))
            {
                string statusText = File.ReadAllText("income_status.txt");
                incomeEnteredThisMonth = bool.TryParse(statusText, out bool savedStatus) && savedStatus;
            }
            else
            {
                incomeEnteredThisMonth = false; // Nếu không có tệp, đặt giá trị mặc định là false
            }
        }
        public void NotifyIncomeEntry()
        {
            if (DateTime.Now >= lastIncomeEntryTime.AddMonths(1))
            {
                Console.WriteLine("Đã qua 1 tháng, bạn cần nhập khoản thu nhập mới.");
            }
            else
            {
                TimeSpan timeRemaining = lastIncomeEntryTime.AddMonths(1) - DateTime.Now;
                Console.WriteLine($"Bạn có thể nhập thu nhập mới sau {timeRemaining.Days} ngày nữa.");
            }
            lastIncomeEntryTime = DateTime.Now;
        }
        private void SaveIncomeEntryTime()
        {
            File.WriteAllText("income_entry_time.txt", lastIncomeEntryTime.ToString("o")); // Lưu thời gian theo định dạng ISO
        }
        private void LoadIncomeEntryTime()
        {
            if (File.Exists("income_entry_time.txt"))
            {
                string dateText = File.ReadAllText("income_entry_time.txt");
                if (DateTime.TryParse(dateText, out DateTime savedTime))
                {
                    lastIncomeEntryTime = savedTime;
                }
                else
                {
                    lastIncomeEntryTime = DateTime.MinValue; // Nếu không đọc được, mặc định là chưa nhập
                }
            }
            else
            {
                lastIncomeEntryTime = DateTime.MinValue; // Nếu chưa có file, chưa nhập thu nhập lần nào
            }
        }

       
        int month = DateTime.Now.Month;



        public Dictionary<string, Dictionary<int, double>> GetMonthlyExpenses()
        {
            return monthlyExpenses;
        }

        private void CheckOverspending()
        {
            if (TotalExpenses > TotalBudget)
            {
                decimal overspending = TotalExpenses - TotalBudget;
                Console.WriteLine($"Bạn đang chi tiêu vượt mức dự tính: {overspending:#,##0₫}.");
            }
        }


        private string GetExpenseCategory(string choice)
        {
            switch (choice)
            {
                case "1": return "Ăn uống";
                case "2": return "Đi lại";
                case "3": return "Chi phí cố định";
                case "4": return "Giải trí";
                case "5": return "Giáo dục";
                case "6": return "Mua sắm";
                case "7": return "Khác";
                default: return "Khác";
            }
        }

        private void SaveExpenses()
        {
            var json = JsonConvert.SerializeObject(expenses);
            File.WriteAllText(filePath, json);
        }

        public Dictionary<string, decimal> LoadExpenses()
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                expenses = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(json) ?? new Dictionary<string, decimal>();
            }
            return expenses;
        }

        public Dictionary<string, decimal> GetExpenses()
        {
            return expenses;
        }

        private decimal GetTotalExpenses()
        {
            decimal total = 0;
            foreach (var expense in expenses)
            {
                if (expense.Key != "Thu nhập")
                {
                    total += expense.Value;
                }
            }
            return total;
        }
        public string GetSavingsStatus()
        {
            decimal totalExpenses = GetTotalExpenses(); // Assuming this method returns the total expenses
            decimal totalBudget = TotalBudget; // Assuming TotalBudget is a property that returns the total budget
            decimal totalIncome = TotalIncome; // Assuming TotalIncome is a property that returns the total income

            decimal savings;

            // Calculate savings based on the conditions
            if (totalExpenses <= totalBudget)
            {
                savings = totalIncome - totalBudget; // Savings when within budget
            }
            else
            {
                savings = totalIncome - totalExpenses; // Savings when exceeding budget
            }

            // Return the savings status message
            if (savings > 0)
            {
                return $"Bạn có tiết kiệm tạm thời: {savings:#,##0₫}";
            }
            else if (savings < 0)
            {
                return $"Bạn đã vượt ngân sách: {-savings:#,##0₫}";
            }
            else
            {
                return "Bạn không có tiết kiệm tạm thời.";
            }
        }

        private string ConvertNumberToWords(long number)
        {
            if (number == 0) return "không đồng";

            string[] units = { "", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín", "mười", "mười một", "mười hai", "mười ba", "mười bốn", "mười lăm", "mười sáu", "mười bảy", "mười tám", "mười chín" };
            string[] tens = { "", "", "hai mươi", "ba mươi", "bốn mươi", "năm mươi", "sáu mươi", "bảy mươi", "tám mươi", "chín mươi" };
            string[] scales = { "", "nghìn", "triệu", "tỷ" };

            string result = "";
            int scaleIndex = 0;

            while (number > 0)
            {
                int group = (int)(number % 1000);
                if (group > 0)
                {
                    string groupWords = ConvertGroupToWords(group);
                    if (scaleIndex > 0 && !string.IsNullOrEmpty(groupWords))
                    {
                        result = groupWords + " " + scales[scaleIndex] + ", " + result;
                    }
                    else
                    {
                        result = groupWords + result;
                    }
                }
                number /= 1000;
                scaleIndex++;
            }

            result = result.Trim().TrimEnd(',');
            return char.ToUpper(result[0]) + result.Substring(1) + " đồng";
        }

        private string ConvertGroupToWords(int group)
        {
            string[] units = { "", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín", "mười", "mười một", "mười hai", "mười ba", "mười bốn", "mười lăm", "mười sáu", "mười bảy", "mười tám", "mười chín" };
            string[] tens = { "", "", "hai mươi", "ba mươi", "bốn mươi", "năm mươi", "sáu mươi", "bảy mươi", "tám mươi", "chín mươi" };

            string result = "";

            int hundreds = group / 100;
            int tensAndUnits = group % 100;

            if (hundreds > 0)
            {
                result += units[hundreds] + " trăm ";
                if (tensAndUnits > 0 && tensAndUnits < 10)
                {
                    result += "lẻ ";
                }
            }

            if (tensAndUnits >= 20)
            {
                int tensDigit = tensAndUnits / 10;
                int unitsDigit = tensAndUnits % 10;
                result += tens[tensDigit] + " ";
                if (unitsDigit > 0)
                {
                    if (unitsDigit == 1)
                    {
                        result += "mốt";
                    }
                    else if (unitsDigit == 5)
                    {
                        result += "lăm";
                    }
                    else
                    {
                        result += units[unitsDigit];
                    }
                }
            }
            else if (tensAndUnits > 0)
            {
                result += units[tensAndUnits];
            }

            return result.Trim();
        }


        //=====Đây là phần dữ liệu giả lập để thống kê các chi tiêu của những tháng trước được load từ file "mock_expenses.json"=====
        public void LoadMockExpenses()
        {
            string filePath = "mock_expenses.json"; // Đường dẫn đến tệp JSON

            // Kiểm tra nếu tệp không tồn tại, tạo tệp giả lập
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Tệp mock_expenses.json không tồn tại. Đang tạo dữ liệu giả lập...");
                CreateMockExpenses(filePath); // Tạo dữ liệu giả lập
            }

            // Sau khi đảm bảo tệp tồn tại, tiến hành đọc dữ liệu
            string jsonData = File.ReadAllText(filePath);
            var mockExpenses = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<int, double>>>(jsonData);

            if (mockExpenses != null)
            {
                foreach (var category in mockExpenses)
                {
                    string categoryName = category.Key;
                    Dictionary<int, double> monthlyData = category.Value;

                    if (!monthlyExpenses.ContainsKey(categoryName))
                    {
                        monthlyExpenses[categoryName] = new Dictionary<int, double>();
                    }

                    foreach (var monthData in monthlyData)
                    {
                        monthlyExpenses[categoryName][monthData.Key] = monthData.Value;
                    }
                }
            }
        }

        // Phương thức tạo dữ liệu giả lập và ghi vào file JSON
        private void CreateMockExpenses(string filePath)
        {
            // Tạo đối tượng giả lập cho các danh mục chi tiêu
            var mockExpenses = new Dictionary<string, Dictionary<int, double>>
        {
            { "Ăn uống", new Dictionary<int, double> { { 1, 1500000 }, { 2, 1700000 }, { 3, 1600000 }, { 4, 1800000 }, { 5, 1900000 } } },
            { "Đi lại", new Dictionary<int, double> { { 1, 500000 }, { 2, 600000 }, { 3, 550000 }, { 4, 700000 }, { 5, 650000 } } },
            { "Chi phí cố định", new Dictionary<int, double> { { 1, 2500000 }, { 2, 2500000 }, { 3, 2500000 }, { 4, 2500000 }, { 5, 2500000 } } },
            { "Giải trí", new Dictionary<int, double> { { 1, 300000 }, { 2, 400000 }, { 3, 350000 }, { 4, 450000 }, { 5, 500000 } } },
            { "Giáo dục", new Dictionary<int, double> { { 1, 1200000 }, { 2, 1300000 }, { 3, 1250000 }, { 4, 1400000 }, { 5, 1350000 } } },
            { "Mua sắm", new Dictionary<int, double> { { 1, 1000000 }, { 2, 1200000 }, { 3, 1100000 }, { 4, 1500000 }, { 5, 1300000 } } },
            { "Khác", new Dictionary<int, double> { { 1, 200000 }, { 2, 300000 }, { 3, 250000 }, { 4, 400000 }, { 5, 350000 } } }
        };

            // Chuyển đổi đối tượng thành chuỗi JSON
            string json = JsonConvert.SerializeObject(mockExpenses, Formatting.Indented);

            // Ghi chuỗi JSON vào file
            try
            {
                File.WriteAllText(filePath, json);
                Console.WriteLine($"Tạo tệp JSON thành công tại: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi ghi tệp: {ex.Message}");
            }
        }

        public void ShowExpenses()
        {
            if (monthlyExpenses.Count == 0)
            {
                Console.WriteLine("Không có dữ liệu chi tiêu nào để hiển thị.");
                return;
            }

            Console.WriteLine("\nDữ liệu chi tiêu các tháng:");
            foreach (var category in monthlyExpenses)
            {
                Console.WriteLine($"Danh mục: {category.Key}");
                foreach (var monthExpense in category.Value)
                {
                    Console.WriteLine($"Tháng {monthExpense.Key}: {monthExpense.Value:#,##0₫}");
                }
                Console.WriteLine(); // Thêm dòng trống giữa các danh mục
            }
        }

        public Dictionary<int, Dictionary<string, double>> GetMonthlyTotals()
        {
            var monthlyTotals = new Dictionary<int, Dictionary<string, double>>();

            foreach (var category in monthlyExpenses)
            {
                foreach (var month in category.Value)
                {
                    int monthKey = month.Key;
                    double amount = month.Value;

                    if (!monthlyTotals.ContainsKey(monthKey))
                    {
                        monthlyTotals[monthKey] = new Dictionary<string, double>();
                    }

                    if (!monthlyTotals[monthKey].ContainsKey(category.Key))
                    {
                        monthlyTotals[monthKey][category.Key] = 0;
                    }

                    monthlyTotals[monthKey][category.Key] += amount;
                }
            }

            return monthlyTotals;
        }

        /// <summary>
        /// //////////////////////////
        /// </summary>
        public void ShowExpenseChart()
        {
            Console.WriteLine("\nBiểu đồ chi tiêu:");
            decimal maxExpense = expenses.Where(e => e.Key != "Thu nhập").Max(e => e.Value);
            int chartWidth = 50; // Độ rộng tối đa của biểu đồ

            foreach (var category in expenses)
            {
                if (category.Key != "Thu nhập")
                {
                    int barLength = (int)((category.Value / maxExpense) * chartWidth);
                    Console.WriteLine($"{category.Key.PadRight(20)} | {new string('#', barLength)} {category.Value:#,##0₫}");
                }
            }
        }

       



        static int selectedYear = new int();
        static int selectedMonth = new int();
        static int[,] calendarTracker = new int[6, 7];
        static int selectedRow = 0;
        static int selectedCol = 0;
        static int windowWidth = Console.WindowWidth;
        
        public void CalendarTracker()
        {
            
           
            
            Console.Clear();
            TitleIntroMemory();
            Console.WriteLine("Bạn nhấn phím bất kì để tiếp tục.");
            Console.ReadKey();
            Console.Clear();
            
            static void TitleIntroMemory()
            {
                string[] titleMoneymory =
                {

                    "                       ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬                   ",
                    "                                                                                  ",
                    "         ██    ██ ██████ ██    █ ██████ █    █ ██    ██ ██████ ██████ █    █      ",
                    "         █ █  █ █ █    █ ███   █ █      ██  ██ █ █  ███ █    █ █    █ █    █      ",
                    "         █  ██  █ █    █ █ ██  █ █       █  █  █  ██  █ █    █ █    █  █  █       ",
                    "         █  ██  █ █    █ █  █  █ █       ████  █  ██  █ █    █ █    █  █  █       ",
                    "         █      █ █    █ █   █ █ ██████   ██   █      █ █    █ ██████  ████       ",
                    "         █      █ █    █ █   █ █ █        ██   █      █ █    █ ██       ██        ",
                    "         █      █ █    █ █    ██ █        ██   █      █ █    █ █ █      ██        ",
                    "         █      █ █    █ █    ██ █        ██   █      █ █    █ █  █     ██        ",
                    "         █      █ ██████ █     █ ██████   ██   █      █ ██████ █   █    ██        ",
                    "                                                                                  ",
                    "                       ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬                   "



                };
                Console.ForegroundColor = ConsoleColor.Yellow;
                foreach (var word in titleMoneymory)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    int padding = (windowWidth - word.Length) / 2;
                    Console.WriteLine(word.PadLeft(padding + word.Length));


                }
                Console.ResetColor();
                Console.WriteLine();
                string[] titleIconCalendar =
                {
            "\u263A\u263A\u263A",  // ☻☻☻ (Mặt cười)
            "\u2665\u2665\u2665",  // ♥♥♥ (Trái tim)
            "This is a smiley face: \u263A",
            "This is a heart: \u2665"
                };

                string[] titleIntroCalendar =
                {





                                            "  ☻☻☻☻☻☻☻                            ",
                                             "☻☻☻☻☻☻☻☻☻☻☻              •☻☻☻☻☻☻☻☻☻ ",
                                           "☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻           •☻☻☻☻☻☻☻☻☻☻☻☻☻",
                                          "☻##############☻☻        •☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻",
                                         "☻☻☻☻☻%##########☻☻☻     •☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻",
                                        "☻☻%%☻%###########☻☻☻    •☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻",
                                       ":☻☻☻###############☻☻☻  •☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻",
                                       "#☻☻#######☻☻######☻☻☻☻ ☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻",
                                       "☻☻☻#####%☻☻☻####%☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻#☻####%☻☻☻☻",
                                       "☻☻☻☻####☻☻☻###☻☻☻☻☻☻☻☻☻%☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻##########%☻☻",
                                       "*☻☻☻####☻☻☻#☻☻☻☻☻☻%##☻##%☻☻☻☻☻☻☻☻☻☻☻☻☻%%%##############☻☻.",
                       ":-= +****##+=-.-☻☻☻%##%☻☻☻☻☻☻☻☻%###########%####%#####################*☻☻ ",
                    ".+#☻%%#########%#%☻#☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻#######################################%☻☻",
                 ".=%%%#############☻☻☻%##%%☻☻☻☻☻☻☻☻☻☻☻#######################################%☻☻☻ ",
               ":#☻%###################%%☻☻%##%☻☻☻☻☻%##################%%☻☻☻%%%##%%%#%%######%%%☻☻ ",
              "+☻#########################%☻☻☻###%☻☻☻#################☻☻☻☻☻%%%%%%%%☻%☻☻☻☻☻☻%%##☻☻ ",
             "#☻#############################%☻%###☻☻☻%#############%☻☻☻##############☻☻☻☻☻☻☻☻☻☻  ",
            "☻☻################################%☻%##☻%☻☻%###########☻☻☻##############☻☻☻☻☻☻☻☻☻☻☻☻##  ",
           "%☻☻%######################################%☻☻%##########%################%☻☻☻☻☻☻☻☻☻☻☻☻☻## ",
           "☻☻☻☻☻☻☻%%###############################☻###☻☻%############################☻☻☻☻☻☻☻☻☻☻%☻☻☻# ",
           "☻☻☻☻☻☻☻☻☻☻##################################%☻☻#######################☻☻#######%%%#####%☻## ",
           "☻☻☻☻☻☻☻☻☻☻☻##################################%☻##############☻%%#####%☻☻☻###############%☻☻ ",
           "☻☻☻☻☻☻☻☻☻☻☻☻############################☻####################%☻☻#####☻☻☻☻☻###############%☻# ",
            "☻☻☻☻☻☻☻☻☻☻☻##################################################%☻%###☻☻☻☻☻☻☻###############☻☻ ",
             "☻☻☻☻☻☻☻☻☻###################%#########☻#%##☻###########☻%###%###☻☻☻☻☻☻☻☻☻##############☻# ",
              " ☻☻☻☻☻☻☻☻##############☻☻☻☻☻☻##########%☻☻☻ ☻#☻%#######☻☻%#####☻☻☻☻☻☻☻☻☻☻☻☻##########%☻# ",
               " ☻☻☻☻☻☻############%%☻☻☻☻☻☻####%%#####☻☻☻   ☻#%#######%☻%###☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻#########: ",
                  " ☻☻☻☻☻%##########%☻☻☻%%####%☻%####☻☻☻     ☻%##########☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻          ",
                     "  ☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻%%☻☻☻☻☻☻☻☻        ☻☻☻☻☻=+=#☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻☻         ",
                    "                      ☻☻☻    ██████████████     ██████████████ ☻☻☻☻☻☻☻☻☻☻☻        ",
                    "                        ☻    █ ███ ██████████████ ███ ████████  ☻☻☻☻☻☻☻☻☻☻        ",
                    "                        ☻    ██ ███ ██████████████ ███ ███████   ☻☻☻☻☻☻☻☻☻        ",
                    "                        ☻    ███ ███ ██████████████ ███ ██████     ☻☻☻☻☻☻☻        ",
                    "                        ☻    ████ ███ ██████   █████ ███ █████                    ",
                    "                        ☻     ████ ███ █████   ██████ ███ ███                     ",
                    "                      ☻☻☻      ████ ███ ███     ██████ ███ █          ☻☻☻         ",
                    "                     ☻  ☻☻      ██████████       ██████████ ☻        ☻   ☻        ",
                    "                    ☻    ♥♥                                ♥♥☻       ☻   ☻        ",
                    "                   ☻      ♥♥             ♥♥♥♥♥♥♥♥         ♥♥  ☻      ☻   ☻        ",
                    "                  ☻        ♥♥             ♥    ♥         ♥♥    ☻    ☻    ☻        ",
                    "                 ☻          ♥♥             ♥♥♥♥         ♥♥     ☻   ☻     ☻☻☻☻☻☻   ",
                    "             ███████         ♥♥                        ♥♥      ☻☻██            ☻  ",
                    "    ▬▬▬▬▬▬▬▬☻       ☻         ♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥       ☻☻██            ☻  ",
                    "   ▐ 💰    ☻         ☻         ♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥        ☻☻██    ☻☻☻☻☻☻☻    ",
                    "   ▐      ☻           ☻                                        ☻☻██            ☻  ",
                    " •♦♦♦♦♦♦♦☻   ☻      ♦♦♦♦•       █     █ ██████  █    █         ☻☻██            ☻  ",
                    " ♦      ☻   ☻    ☻  ☻   ♦       █     █ █       █    █         ☻☻██     ☻☻☻☻☻☻☻   ",
                    " ♦      ☻   ☻   ☻   ☻   ♦       █     █ █       █    █         ☻   ☻           ☻  ",
                    " ♦  ♦♦♦♦☻   ☻   ☻   ☻♦♦♦♦♦♦♦    █     █ ██████  ██████         ☻    ☻          ☻  ",
                    " ♦      ☻   ☻   ☻   ☻    ♦•♦    █     █ █       █    █         ☻     ☻☻☻☻☻☻☻☻☻☻   ",
                    " ♦       ☻☻☻ ☻  ☻   ☻    ♦•♦    █     █ █       █    █        ☻                   ",
                    " ♦            ☻☻ ☻☻☻     ♦•♦    ███████ ██████  █    █       ☻                    ",
                    " ♦   ♥♥♥♥♥ ♥♥♥♥     ♦♦♦♦♦♦•♦                                ☻                     ",
                    " ♦   ♥   ♥   ♥♥    ♦     ♦•♦☻♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥☻                      ",
                    " ♦   ♥   ♥   ♥♥   ♦ ☻☻   ♦•♦☻           ☻☻☻☻☻☻☻           ☻                       ",
                    " ♦   ♥   ♥ ♥♥♥♥   ♦ ☻☻   ♦•♦ ☻          ☻     ☻          ☻                        ",
                    " ♦   ♥   ♥   ♥♥    ♦     ♦•♦  ☻         ☻     ☻         ☻                         ",
                    " ♦   ♥   ♥   ♥♥     ♦♦♦♦♦♦•♦   ☻       ☻       ☻       ☻                          ",
                    " ♦   ♥♥♥♥♥ ♥♥♥♥          ♦•♦    ☻☻☻☻☻☻☻         ☻☻☻☻☻☻☻                           ",
                    " ♦                       ♦•♦                                                      ",
                    " ♦                       ♦•♦                                                      ",
                    " •♦♦♦♦♦♦♦♦♦♦♦♦♦♦♦♦♦♦♦♦♦♦♦♦♦♦                                                      ",
                };
            
                ConsoleColor defaultForeground = Console.ForegroundColor;
                ConsoleColor defaultBackground = Console.BackgroundColor;
                
                foreach (var line in titleIntroCalendar)
                {

                    int padding = (windowWidth - line.Length) / 2;
                    // Ví dụ sử dụng màu khác nhau cho từng dòng


                    if (line.Contains("\u263B")) // Nếu chứa biểu tượng mặt cười
                    {
                        Console.ForegroundColor = ConsoleColor.Green; // Màu chữ vàng
                                                                      // Nền đen
                    }
                    else if (line.Contains("\u2665")) // Nếu chứa biểu tượng trái tim
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow; // Màu chữ đỏ
                                                                       // Nền trắng
                    }
                    else if (line.Contains("*"))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;

                    }
                    else if (line.Contains("♦"))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }


                    else
                    {
                        // Màu mặc định
                        Console.ForegroundColor = defaultForeground;
                        Console.BackgroundColor = defaultBackground;
                    }

                    // In dòng với màu đã chọn
                    Console.WriteLine(line.PadLeft(padding + line.Length));


                }
                // Khôi phục lại màu mặc định sau khi in xong
                Console.ForegroundColor = defaultForeground;
                Console.BackgroundColor = defaultBackground;
            }



            string[] titleCalendar =
            {
                "███╗   ███╗ ██████╗ ███╗   ██╗███████╗██╗   ██╗███╗   ███╗ ██████╗ ██████╗ ██╗   ██╗",
                "████╗ ████║██╔═══██╗████╗  ██║██╔════╝╚██╗ ██╔╝████╗ ████║██╔═══██╗██╔══██╗╚██╗ ██╔╝",
                "██╔████╔██║██║   ██║██╔██╗ ██║█████╗   ╚████╔╝ ██╔████╔██║██║   ██║██████╔╝ ╚████╔╝ ",
                "██║╚██╔╝██║██║   ██║██║╚██╗██║██╔══╝    ╚██╔╝  ██║╚██╔╝██║██║   ██║██╔══██╗  ╚██╔╝  ",
                "██║ ╚═╝ ██║╚██████╔╝██║ ╚████║███████╗   ██║   ██║ ╚═╝ ██║╚██████╔╝██║  ██║   ██║   ",
                "╚═╝     ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝   ╚═╝   ╚═╝     ╚═╝ ╚═════╝ ╚═╝  ╚═╝   ╚═╝   "

            };
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (var line in titleCalendar)
            {
                int padding = (windowWidth - line.Length) / 2;
                Console.WriteLine(line.PadLeft(padding + line.Length));
            }
            Console.ResetColor();

            DrawInputBox();
            GetUserInput();

            static void DrawInputBox()
            {
                int windowWidth = Console.WindowWidth;
                int windowHeight = Console.WindowHeight;


                int boxWidth = 40;
                int boxHeight = 10;
                int yearBoxX = (windowWidth / 2) - boxWidth - 2;
                int monthBoxX = (windowWidth / 2) + 2;
                int boxY = 10;


                DrawBox(yearBoxX, boxY, boxWidth, boxHeight, "Nhập năm bạn muốn xem:");


                DrawBox(monthBoxX, boxY, boxWidth, boxHeight, "Nhập tháng bạn muốn xem:");


            }
            
            static void GetUserInput()
            {
                int yearBoxX = (Console.WindowWidth / 2) - 40;
                int inputY = 14;
                Console.SetCursorPosition(yearBoxX + 9, inputY);
                //Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(yearBoxX + 9, inputY); // Vị trí nhập trong khung năm
                while (!int.TryParse(Console.ReadLine(), out selectedYear) || selectedYear < 1)
                {
                    Console.SetCursorPosition(yearBoxX + 9, inputY);
                   ClearCurrentLine(yearBoxX + 9, inputY);
                   Console.SetCursorPosition(yearBoxX + 9, inputY);
                   Console.Write("Vui lòng nhập năm hợp lệ.");
                    System.Threading.Thread.Sleep(1000);
                    ClearCurrentLine(yearBoxX +9, inputY);
                    Console.SetCursorPosition(yearBoxX + 9, inputY);
                }
                Console.ResetColor();   
                // Input cho tháng
                int monthBoxX = (Console.WindowWidth / 2) + 2;
                
                Console.SetCursorPosition(monthBoxX + 12, inputY ); 
               // Console.ForegroundColor = ConsoleColor.White;
                while (!int.TryParse(Console.ReadLine(), out selectedMonth) || selectedMonth < 1 || selectedMonth > 12)
                {
                    Console.SetCursorPosition(monthBoxX + 12, inputY);
                    ClearCurrentLine(monthBoxX +12,inputY);
                   
                    Console.Write("Vui lòng nhập tháng hợp lệ (1-12).");
                    System.Threading.Thread.Sleep(1000);
                    ClearCurrentLine(monthBoxX+12, inputY);
                    Console.SetCursorPosition(monthBoxX + 12, inputY);
                    //ClearCurrentLine();
                    
                }
                Console.ResetColor();
            }
            static void DrawBox(int x, int y, int width, int height, string title)
            {

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                // Draw top border
                Console.SetCursorPosition(x, y);
                Console.Write("╔" + new string('═', width - 2) + "╗");

                // Draw title
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.SetCursorPosition(x + 9, y);
                Console.Write(title);
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.DarkYellow;

                // Draw side borders
                for (int i = 1; i < height - 1; i++)
                {
                    Console.SetCursorPosition(x, y + i);
                    Console.Write("║" + new string(' ', width - 2) + "║");
                }

                // Draw bottom border
                Console.SetCursorPosition(x, y + height - 1);
                Console.Write("╚" + new string('═', width - 2) + "╝");
                Console.ResetColor();
            }
            static void ClearCurrentLine(int startX, int startY)
            {
                Console.SetCursorPosition(startX, startY);

              
                Console.Write(new string(' ', Console.WindowWidth - startX));

                
                Console.SetCursorPosition(startX, startY);

                //int currentLineCursor = Console.CursorTop;
                //Console.SetCursorPosition(0, currentLineCursor);
                //Console.Write(new string(' ', Console.WindowWidth));
                //Console.SetCursorPosition(Console.CursorLeft, currentLineCursor);
            }

            bool isSelectingDay = false; // Thêm biến để theo dõi trạng thái chọn ngày
            bool move = true;
            while (move)
            {
                Console.Clear();
                DrawHeader();
                DrawCalendarBox();
                FillCalendar();
                DrawCalendar();
                DrawOptions();

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (isSelectingDay)
                        {
                            MoveSelection(-1, 0); 
                        }
                        else
                        {
                            selectedYear++; // Tăng năm nếu không chọn ngày
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (isSelectingDay)
                        {
                            MoveSelection(1, 0); // Di chuyển xuống nếu đang chọn ngày
                        }
                        else
                        {
                            selectedYear--; // Giảm năm nếu không chọn ngày
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        if (isSelectingDay)
                        {
                            MoveSelection(0, -1); // Di chuyển sang trái nếu đang chọn ngày
                        }
                        else
                        {
                            selectedMonth--; // Giảm tháng nếu không chọn ngày
                            if (selectedMonth < 1)
                            {
                                selectedMonth = 12; // Quay về tháng 12
                                selectedYear--; // Giảm năm
                            }
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (isSelectingDay)
                        {
                            MoveSelection(0, 1); // Di chuyển sang phải nếu đang chọn ngày
                        }
                        else
                        {
                            selectedMonth++; // Tăng tháng nếu không chọn ngày
                            if (selectedMonth > 12)
                            {
                                selectedMonth = 1; // Quay về tháng 1
                                selectedYear++; // Tăng năm
                            }
                        }
                        break;
                    case ConsoleKey.Enter: // Nếu chọn ngày
                        if (isSelectingDay)
                        {
                            ShowDayInfo(); // Hiển thị thông tin ngày được chọn
                        }
                        else 
                        {
                            isSelectingDay = true;
                            
                        }
                        break;
                    case ConsoleKey.Delete:// Nếu muốn chọn tháng và năm 
                        if (isSelectingDay)
                        {
                            isSelectingDay = false;
                        }

                        break;
                    case ConsoleKey.Escape:
                        move = false; // Thoát vòng lặp
                        Console.WriteLine("Nhấn phím ESC để thoát:");
                        break;
                }

                //if (isSelectingDay)
                //{
                //    MoveSelection(0, 0); // Cập nhật vị trí chọn ngày
                //}
            }

           
        }
        static void DrawHeader()
        {
            Console.Clear();
            string titleYear =  "                    𝑪𝑨𝑳𝑬𝑵𝑫𝑨𝑹 𝑶𝑭 𝑻𝑯𝑬 𝒀𝑬𝑨𝑹 " + selectedYear ;


                
                         


            Console.SetCursorPosition((Console.WindowWidth - titleYear.Length ) / 2, 2);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(titleYear);
            Console.ResetColor();

        }
        static void FillCalendar()
        {
            int day = DateTime.DaysInMonth(selectedYear, selectedMonth);
            int firstDayOfMonth = (int)new DateTime(selectedYear, selectedMonth, 1).DayOfWeek;
            if (firstDayOfMonth == 0) firstDayOfMonth = 7; // Make Sunday the last column

            int currentDay = 1;
            for (int i = 0; i < calendarTracker.GetLength(0); i++)
            {
                for (int j = 0; j < calendarTracker.GetLength(1); j++)
                {
                    if (i == 0 && j < firstDayOfMonth - 1)
                    {
                        calendarTracker[i, j] = 0;
                    }
                    else if (currentDay <= day)
                    {
                        calendarTracker[i, j] = currentDay;
                        currentDay++;
                    }
                    else
                    {
                        calendarTracker[i, j] = 0;
                    }
                }
            }
        }
        static void DrawCalendarBox()
        {
            // Drawing a box to contain the calendar
            int boxWidth = 80;
            int boxHeight = 16;
            int startX = (Console.WindowWidth - boxWidth) / 2;
            int startY = 3;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.SetCursorPosition(startX, startY);
            Console.Write("╔" + new string('═', boxWidth) + "╗");

            for (int i = 0; i < boxHeight; i++)
            {
                Console.SetCursorPosition(startX, startY + i + 1);
                Console.Write("║" + new string(' ', boxWidth) + "║");
            }

            Console.SetCursorPosition(startX, startY + boxHeight + 1);
            Console.Write("╚" + new string('═', boxWidth) + "╝");
            Console.ResetColor();

            // Display current month inside the box
            Console.ForegroundColor = ConsoleColor.Cyan;
            string monthName = new DateTime(selectedYear, selectedMonth, 1).ToString("MMMM");
            string monthDisplay = $" {monthName} {selectedYear} ";
            Console.SetCursorPosition(startX + (boxWidth - monthDisplay.Length) / 2, startY + 1);
            Console.WriteLine(monthDisplay);
            Console.ResetColor();
        }
        static void DrawOptions()
        {
            // Drawing bottom options
            string options = "[Up/Down: Thay đổi năm] [Left/Right: Thay đổi tháng] [Esc: Exit] " +
                "[Enter: Chọn ngày] [Delete: Chọn tháng và năm] ";
            Console.SetCursorPosition((Console.WindowWidth - options.Length) / 2, Console.WindowHeight - 2);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(options);
            Console.ResetColor();
        }
        static void DrawCalendar()
        {   Console.ForegroundColor = ConsoleColor.Yellow;
            string[] dayNames = { "Thứ hai ", "Thứ ba ", "Thứ tư ", "Thứ năm ", "Thứ sáu ", "Thứ bảy ", "Chủ nhật " };
            int startX = (Console.WindowWidth - 55 - dayNames.Length) / 2 ;
            int startY = 6;
            

            

            Console.SetCursorPosition(startX, startY);
            foreach (var day in dayNames)
            {
                Console.Write(day.PadRight(10));
            }
            for (int i = 0; i < calendarTracker.GetLength(0); i++)
            {
                Console.SetCursorPosition(startX, startY + i * 2 + 2); // khoảng cách giữa các số liền kề theo đường dọc ,
                for (int j = 0; j < calendarTracker.GetLength(1); j++)
                {
                    if (calendarTracker[i, j] > 0)
                    {
                        if ( i == selectedRow && j == selectedCol )
                        {
                            Console.BackgroundColor = ConsoleColor.DarkBlue;
                            
                        }
                        if (j == 6) // Sunday in red
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                        }

                        Console.Write(calendarTracker[i, j].ToString("D2").PadRight(10));
                    }
                    else
                    {
                        Console.Write(" ".PadRight(10));
                    }

                    Console.ResetColor();
                }
                Console.Write('\n');
            }

        }
        static void MoveSelection(int rowChange, int colChange)

        {
            int newRow = selectedRow + rowChange;
            int newCol = selectedCol + colChange;
            if (newRow >= 0 && newRow < calendarTracker.GetLength(0) && newCol >= 0 && newCol < calendarTracker.GetLength(1) && calendarTracker[newRow, newCol] > 0)
            {
                selectedRow = newRow;
                selectedCol = newCol;
            }
            DrawHeader();
            DrawCalendar();

        }
        public void ShowDayInfo()
        {
            int selectedDay = calendarTracker[selectedRow, selectedCol];
            if (selectedDay >= 0) // Kiểm tra nếu ngày hợp lệ
            {
                Console.Clear();
                Console.WriteLine($"Nhật ký chi tiêu cho ngày {selectedDay}/{selectedMonth}/{selectedYear}:");

                // Hiển thị thông tin chi tiêu cho ngày đã chọn
                var expensesForSelectedDay = expenseList.Where(expense =>
                    expense.Date.Day == selectedDay &&
                    expense.Date.Month == selectedMonth &&
                    expense.Date.Year == selectedYear).ToList();

                if (expensesForSelectedDay.Any())
                {
                    foreach (var expense in expensesForSelectedDay)
                    {
                        Console.WriteLine($"Danh mục: {expense.Category}, Số tiền: {expense.Amount:#,##0₫}, Ngày: {expense.Date}");
                    }
                }
                else
                {
                    Console.WriteLine("Không có chi tiêu nào ghi nhận cho ngày này.");
                }

                // Tùy chọn để người dùng quay lại
                Console.WriteLine("\nNhấn phím bất kỳ để quay lại.");
                Console.ReadKey();

                // Hiển thị lại lịch sau khi xem thông tin chi tiêu
                DrawHeader();
                DrawCalendar();
            }
        }

       
    }

}