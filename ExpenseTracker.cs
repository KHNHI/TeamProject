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
        public ExpenseTracker()
        {
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

        public void EnterExpense()
        {
            while (true)
            {
                try
                {
                    // Kiểm tra tổng chi tiêu so với tổng thu nhập
                    if (GetTotalExpenses() >= TotalIncome)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("⚠️ Bạn không thể nhập thêm chi tiêu vì tổng chi tiêu đã bằng hoặc vượt quá tổng thu nhập.");
                        Console.ReadLine();
                        Console.ResetColor();
                        return; // Kết thúc phương thức nếu điều kiện đúng
                    }

                    Console.WriteLine("Nhấn phím tương ứng để chọn danh mục chi tiêu (hoặc nhấn ESC để quay lại menu chính): ");
                    ConsoleKeyInfo keyInfo = Console.ReadKey();

                    // Kiểm tra người dùng có nhấn ESC để thoát
                    if (keyInfo.Key == ConsoleKey.Escape) break;

                    Console.WriteLine();
                    if (int.TryParse(keyInfo.KeyChar.ToString(), out int categoryIndex) && categoryIndex >= 1 && categoryIndex <= validCategories.Length)
                    {
                        string category = validCategories[categoryIndex - 1];
                        if (!expenses.ContainsKey(category)) expenses[category] = 0;

                        decimal budgetForCategory = budgetPlanner.GetBudgetForCategory(category);
                        decimal amountSpent = expenses[category];
                        ShowExpenseInfo(category, budgetForCategory, amountSpent);

                        // Kiểm tra ngân sách cho danh mục
                        if (budgetForCategory <= 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Chưa có ngân sách cho danh mục '{category}'. Vui lòng đặt ngân sách trước khi nhập chi tiêu.");
                            Console.Write($"Bạn có muốn đặt ngân sách ngay không? (y/n): ");
                            if (Console.ReadLine()?.ToLower() == "y")
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

                        // Nhập số tiền chi tiêu
                        Console.Write("Nhập số tiền chi tiêu: ");
                        if (decimal.TryParse(Console.ReadLine(), out decimal amount) && amount > 0)
                        {

                            // Ghi lại thời gian giao dịch
                            DateTime transactionTime = DateTime.Now;

                            // Thêm chi tiêu vào danh sách
                            Expense expense = new Expense(category, amount, transactionTime);
                            expenseList.Add(expense); // Thêm chi tiêu vào danh sách
                            UpdateExpensesAndMonthlyExpenses(expense);

                            // Tính toán tổng chi tiêu và ngân sách
                            decimal totalExpenses = GetTotalExpenses(); // lấy tổng chi tiêu
                            decimal totalBudget = TotalBudget; // tổng ngân sách từ budgetPlanner
                            decimal totalIncome = TotalIncome; // tổng thu nhập

                            // trạng thái của savings dựa trên tổng chi tiêu và ngân sách
                            if (totalExpenses <= totalBudget)
                            {
                                savings = totalIncome - totalBudget; // Tiết kiệm khi trong ngân sách
                            }
                            else
                            {
                                savings = totalIncome - totalExpenses; // Tiết kiệm khi vượt ngân sách
                                decimal overspending = totalExpenses - totalBudget; // Tính toán số tiền vượt ngân sách
                                Console.WriteLine($"Bạn đã chi tiêu vượt tổng ngân sách. Số tiền vượt ngân sách ({overspending:#,##0₫}) đã được trừ vào tiết kiệm tạm thời.");
                            }

                            // Lưu chi tiêu, hiện thông báo
                            SaveExpenses();
                            Console.WriteLine($"Đã lưu chi tiêu: {Math.Abs(amount)} vào danh mục '{category}' vào lúc {transactionTime}");
                            Console.WriteLine($"Số tiền bằng chữ: {ConvertNumberToWords((long)Math.Abs(amount))}");
                        }
                        else
                        {
                            Console.WriteLine("Số tiền không hợp lệ.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Danh mục không hợp lệ. Vui lòng thử lại.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Đã xảy ra lỗi: " + ex.Message);
                }
            }
        }
        private void UpdateExpensesAndMonthlyExpenses(Expense expense)
        {
            // Cập nhật expenses
            if (expenses.ContainsKey(expense.Category))
            {
                expenses[expense.Category] += expense.Amount;
            }
            else
            {
                expenses[expense.Category] = expense.Amount;
            }

            // Cập nhật monthlyExpenses
            int month = expense.Date.Month;
            if (!monthlyExpenses.ContainsKey(expense.Category))
            {
                monthlyExpenses[expense.Category] = new Dictionary<int, double>();
            }
            if (!monthlyExpenses[expense.Category].ContainsKey(month))
            {
                monthlyExpenses[expense.Category][month] = 0;
            }
            monthlyExpenses[expense.Category][month] += (double)expense.Amount;
        }
        private void SaveExpenses()
        {
            var json = JsonConvert.SerializeObject(expenseList, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        private void LoadExpenses()
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                expenseList = JsonConvert.DeserializeObject<List<Expense>>(json) ?? new List<Expense>();

                // Cập nhật expenses và monthlyExpenses từ expenseList
                expenses.Clear(); // Xóa dữ liệu cũ
                monthlyExpenses.Clear(); // Xóa dữ liệu cũ

                foreach (var expense in expenseList)
                {
                    UpdateExpensesAndMonthlyExpenses(expense);
                }
            }
            else
            {
                expenseList = new List<Expense>(); // Khởi tạo danh sách rỗng nếu tệp không tồn tại
            }
        }
    

        private void ShowExpenseInfo(string category, decimal budgetForCategory, decimal amountSpent)
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
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Khoản thu nhập đã được nhập cho tháng này. Không thể nhập lại.");
                    Console.ResetColor();
                    return;
                }
            }
            string[] enterIncomee = { " NHẬP THU NHẬP " };
            Console.ForegroundColor = ConsoleColor.Yellow;
            Program.DrawCenteredBorder(enterIncomee);
            Console.ResetColor();

            string? incomeInput;//có thể nhận giá trị null
            decimal amount = 0;
            while (true)
            {
                incomeInput = Console.ReadLine();
                if (string.IsNullOrEmpty(incomeInput) || !decimal.TryParse(incomeInput, out amount))//Kiểm tra đầu vào, cập nhật vào biến amount nếu hợp lệ
                {
                    Console.WriteLine("Số tiền không hợp lệ, vui lòng nhập lại");
                }
                else
                {
                    break; //Thoát vòng lặp nếu đầu vào hợp lệ
                }
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Đã thêm khoản thu nhập: {amount:#,##0₫}");
            Console.ResetColor();
            TotalIncome += amount;
            savings += amount;
            incomeEnteredThisMonth = true;
            lastIncomeEntryTime = DateTime.Now;
            SaveIncomeEntryTime();
            SaveTotalIncome();
            SaveIncomeEnteredStatus();
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

         public bool CanEnterIncome()
        {
            if (DateTime.Now >= lastIncomeEntryTime.AddMonths(1))
            {
                incomeEnteredThisMonth = false;
            }
            return !incomeEnteredThisMonth;
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
        public Dictionary<string, Dictionary<int, double>> GetMonthlyExpenses()
        {
            return monthlyExpenses;
        }
       
        public Dictionary<string, decimal> GetExpenses()
        {
            return expenses;
        }

        public decimal GetTotalExpenses()
        {
          return expenseList.Sum(expense => expense.Amount);
        }
        //Tình trạng tiết kiệm
        public string GetSavingsStatus()
        {
            decimal totalExpenses = GetTotalExpenses();
            decimal totalBudget = TotalBudget;
            decimal totalIncome = TotalIncome;
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
                return $" Tiết kiệm tạm thời: {savings:#,##0₫}";
            }
            else if (savings < 0)
            {
                return $" Đã vượt ngân sách: {-savings:#,##0₫}";
            }
            else
            {
                return "Không có tiết kiệm tạm thời.";
            }
        }

        // FUNCTION IN RA SỐ TIỀN BẰNG CHỮ 
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
        private void LoadMockExpenses()
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





        // LỰA CHỌN XEM CALENDAR 
    
        private int selectedYear = 0;                                            // Khai báo biến kiểu int và khởi tạo nó với giá trị mặc định là 0. Mục tiêu: Lưu trữ năm hiện tại mà người dùng chọn.
        private int selectedMonth = 0;                                           // Khai báo biến kiểu int và khởi tạo giá trị mặc định là 0. Mục tiêu: Lưu trữ tháng hiện tại người dùng chọn.
        private int selectedRow = 0;                                             //Lưu trữ hàng của ngày được chọn.
        private int selectedCol = 0;                                             // Lưu trữ cột của ngày được chọn.
        private int[,] calendarTracker = new int[6, 7];                          // Đây là mảng 2 chiều với 6 hàng và 7 cột:6 hàng tương ung các tuần trong tháng ( trong 1 tháng nhiều nhất 6 tuần); 7 cột tương ứng các ngày trong tuần.
        private int windowWidth = Console.WindowWidth;                           // Khai báo biên kiểu int và lấy giá trị chiều rộng hiện tại của cửa sổ console và lưu trữ vào biến windowWidth.

        public void CalendarTracker()
        {

            Console.Clear();
            TitleIntroMoneymory();
            Console.WriteLine("Bạn nhấn phím bất kì để tiếp tục.");
            Console.ReadKey();
            Console.Clear();
            TitleCalendar();
            HandleInput();
        }   
        
                                        
        public void HandleInput()                                                                // Định nghĩa phương thức HandleInput để xử lý đầu vào từ người dùng.                                                           
        {
            bool isSelectingDay = false;                                                         // Thêm biến để theo dõi trạng thái chọn ngày
            bool move = true;                                                                    // Biến điều khiển vòng lặp chính, cho phép tiếp tục nhận đầu vào từ người dùng.

            while (move)                                                                         // Bắt đầu vòng lặp, sẽ tiếp tục cho đến khi biến move được đặt thành false.
            {
                Console.Clear();                                                                 // Xóa toàn bộ nội dung hiện tại trên console để chuẩn bị cho giao diện mới.
                DrawHeader();                                                                    // Gọi phương thức để vẽ tiêu đề lịch.
                DrawCalendarBox();                                                               // Gọi phương thức để vẽ hộp lịch.
                FillCalendar();                                                                  // Gọi phương thức để điền thông tin vào lịch.
                DrawCalendarHeader();                                                            // Gọi phương thức để vẽ tiêu đề của lịch.
                DrawOptions();                                                                   // Gọi phương thức để vẽ các tùy chọn điều hướng cho người dùng.

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);                                  // Đọc phím người dùng nhấn mà không hiển thị nó trên console.
                switch (keyInfo.Key)                                                             // Bắt đầu một câu lệnh switch để xử lý các phím khác nhau.
                {
                    case ConsoleKey.UpArrow:
                        if (isSelectingDay)                                                      // Nếu người dùng đang chọn ngày.
                        {
                            MoveSelection(-1, 0);                                                // Di chuyển lựa chọn lên một hàng.
                        }
                        else                                                                     // Nếu không đang chọn ngày.
                        {
                            selectedYear++;                                                      // Tăng năm lên 1.                                           
                        }
                        break;
                    case ConsoleKey.DownArrow:                                                   // Nếu phím xuống được nhấn.
                        if (isSelectingDay)
                        { 
                            MoveSelection(1, 0);                                                 // Di chuyển xuống nếu đang chọn ngày
                        }
                        else
                        {
                            selectedYear--;                                                      // Giảm năm nếu không chọn ngày
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        if (isSelectingDay)
                        {
                            MoveSelection(0, -1);                                                // Di chuyển sang trái nếu đang chọn ngày
                        }
                        else
                        {
                            selectedMonth--;                                                    // Giảm tháng nếu không chọn ngày
                            if (selectedMonth < 1)
                            {
                                selectedMonth = 12;                                             // Quay về tháng 12
                                selectedYear--;                                                 // Giảm năm
                            }
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (isSelectingDay)
                        {
                            MoveSelection(0, 1);                                              // Di chuyển sang phải nếu đang chọn ngày
                        }
                        else
                        {
                            selectedMonth++;                                                 // Tăng tháng nếu không chọn ngày
                            if (selectedMonth > 12)
                            {
                                selectedMonth = 1;                                           // Quay về tháng 1
                                selectedYear++;                                              // Tăng năm
                            }
                        }
                        break;
                    case ConsoleKey.Enter:                                                  // Nếu chọn ngày
                        if (isSelectingDay) 
                        {
                            ShowDayInfo();                                                  // Hiển thị thông tin ngày được chọn
                        }
                        else
                        {
                            isSelectingDay = true;

                        }
                        break;
                    case ConsoleKey.Delete:                                                 // Nếu muốn chọn tháng và năm 
                        if (isSelectingDay)
                        {
                            isSelectingDay = false;
                        }

                        break;
                    case ConsoleKey.Escape:
                        move = false;                                                       // Thoát vòng lặp
                        Console.WriteLine("Nhấn phím ESC để thoát:");
                        break;
                }

            }
        } 
        private void TitleIntroMoneymory()                                                            // Khai báo  để tạo và hiển thị intro. Hàm này tạo và hiển thị các biểu tượng, hình ảnh ASCII với màu sắc thay đổi để làm phần tiêu đề.
        {
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

            ConsoleColor defaultForeground = Console.ForegroundColor;            // Biến này lưu trữ màu chữ mặc định để có thể phục hồi lại sau khi in tiêu đề.
            ConsoleColor defaultBackground = Console.BackgroundColor;            //  Biến này lưu trữ màu nền mặc định để có thể phục hồi lại sau khi in tiêu đề.

            foreach (var line in titleIntroCalendar)                             // Vòng lặp sẽ thực hiện cho từng dòng trong titleIntroCalendar.
            {

                int padding = (windowWidth - line.Length) / 2;                   // Tính toán khoảng cách cần thêm vào bên trái để căn giữa dòng trong cửa sổ console.
                                                                                 // Ví dụ sử dụng màu khác nhau cho từng dòng
                if (line.Contains("\u263B"))                                     // Nếu chứa biểu tượng mặt cười."\u263A\u263A\u263A", ☻☻☻ (Mặt cười)
                {
                    Console.ForegroundColor = ConsoleColor.Green;                // Màu chữ xanh
                                                                                 
                }
                else if (line.Contains("\u2665"))                                // Nếu chứa biểu tượng trái tim."\u2665\u2665\u2665",  ♥♥♥ (Trái tim) 
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;               // Màu chữ vàng
                                                                                 
                }
                else if (line.Contains("*"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;             // Màu chữ Xám tối

                }
                else if (line.Contains("♦"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;                // Màu chữ xanh
                }
                else
                {
                                                                                 // Màu mặc định
                    Console.ForegroundColor = defaultForeground;
                    Console.BackgroundColor = defaultBackground;
                }

                                                                                 
                Console.WriteLine(line.PadLeft(padding + line.Length));          //n dòng đã được căn giữa. Sử dụng PadLeft để thêm khoảng trống bên trái.


            }
                                                                                 // Khôi phục lại màu mặc định sau khi in xong
            Console.ForegroundColor = defaultForeground;
            Console.BackgroundColor = defaultBackground;
        }

        private void TitleCalendar()                                                                  // Hiển thị tên tiêu đề chính của lịch
        {
            string[] titleCalendar =
            {
                "███╗   ███╗ ██████╗ ███╗   ██╗███████╗██╗   ██╗███╗   ███╗ ██████╗ ██████╗ ██╗   ██╗",
                "████╗ ████║██╔═══██╗████╗  ██║██╔════╝╚██╗ ██╔╝████╗ ████║██╔═══██╗██╔══██╗╚██╗ ██╔╝",
                "██╔████╔██║██║   ██║██╔██╗ ██║█████╗   ╚████╔╝ ██╔████╔██║██║   ██║██████╔╝ ╚████╔╝ ",
                "██║╚██╔╝██║██║   ██║██║╚██╗██║██╔══╝    ╚██╔╝  ██║╚██╔╝██║██║   ██║██╔══██╗  ╚██╔╝  ",
                "██║ ╚═╝ ██║╚██████╔╝██║ ╚████║███████╗   ██║   ██║ ╚═╝ ██║╚██████╔╝██║  ██║   ██║   ",
                "╚═╝     ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝   ╚═╝   ╚═╝     ╚═╝ ╚═════╝ ╚═╝  ╚═╝   ╚═╝   "

            };                                                            // Tạo mảng 1 chiều để chứa các dòng chữ ASCII để tạo thành tiêu đề lịch.
            Console.ForegroundColor = ConsoleColor.Yellow;                                          // Đặt màu sắc cho chữ 
            Program.DrawCenteredBorder(titleCalendar);                                              // Gọi phương thức từ lớp Program để hiển thị tiêu đề viền ở giữa màn hình.
            Console.ResetColor();                                                                   // Đặt lại màu sắc.
            DrawInputBox();                                                                         //Hiển thị phần nhập liệu
            GetUserInput();
        }
     
        private void DrawInputBox()
        {
                int windowHeight = Console.WindowHeight;                                             // Khai báo và lưu giá trị chiều cao của cửa sổ hiện tại vào biến windowHeight
                int boxWidth = 40;                                                                   // Khai báo hộp nhập liệu có chiều rộng là 40
                int boxHeight = 10;                                                                  // Khai báo hộp nhập liệu có chiều cao là 10
                int yearBoxX = (windowWidth / 2) - boxWidth - 2;                                     // Tính toán tọa độ X cho hộp nhập liệu năm 
                int monthBoxX = (windowWidth / 2) + 2;                                               // Tính toán tọa độ X cho hộp nhập liệu tháng
                int boxY = 10;                                                                       // Hộp nhập liệu sẽ hiện thị ở dòng thứ 10

                DrawBox(yearBoxX, boxY, boxWidth, boxHeight, "Nhập năm bạn muốn xem:");              // Gọi phương thức DrawBox để vẽ hộp nhập liệu cho năm

                DrawBox(monthBoxX, boxY, boxWidth, boxHeight, "Nhập tháng bạn muốn xem:");           // Gọi phương thức DrawBox để vẽ hộp nhập liệu cho tháng



        }
        private void GetUserInput()
        {
            int yearBoxX = (Console.WindowWidth / 2) - 40;                                                                 // Khởi tạo tọa độ nhập tháng và năm
            int monthBoxX = (Console.WindowWidth / 2) + 2;                                                                
            int inputY = 14;                                                                                               // //Vị trí Y là inputY để con trỏ nằm trên dòng thứ 14.

            Console.SetCursorPosition(yearBoxX + 9, inputY);                                                               //Đặt ví trị con trỏ cho năm, Vị trí X là yearBoxX + 9 để con trỏ bắt đầu cách khung 9 ký tự nhằm căn chỉnh đẹp mắt.
                                                                                                                                                                                        
                                                    
            while (!int.TryParse(Console.ReadLine(), out selectedYear) || selectedYear < 1)                                 //Vòng lặp để kiểm tra nhập năm hợp lệ. Đọc dữ liệu từ người dùng và cố gắng chuyển nó sang kiểu số nguyên.
                                                                                                                            // Nếu không thể chuyển đổi (ví dụ, người dùng nhập chữ thay vì số), nó trả về false.
                                                                                                                            // selectedYear < 1: Kiểm tra xem năm có nhỏ hơn 1 không, vì năm không thể là số âm hoặc bằng 0.
                                                                                                                            //Nếu bất kỳ điều kiện nào không hợp lệ, vòng lặp tiếp tục cho đến khi người dùng nhập đúng.
            {
                Console.SetCursorPosition(yearBoxX + 9, inputY);                                                            //Đặt lại con trỏ đến vị trí bắt đầu để người dùng có thể nhập năm mới.
                ClearCurrentLine(yearBoxX + 9, inputY,24);                                                                  // Xóa dòng hiện tại tại vị trí con trỏ để chuẩn bị cho việc hiển thị thông báo mới.Xóa đúng 24 ký tự.
                Console.Write("Vui lòng nhập năm hợp lệ.");                                                                 // In ra thông báo
                System.Threading.Thread.Sleep(1000);                                                                        // Tạm dừng chương trình 1 s để người dùng đọc.
                ClearCurrentLine(yearBoxX + 9, inputY,24);
            }
            Console.ResetColor();                                                                                           // Đặt lại màu sắc mặc định
            Console.SetCursorPosition(monthBoxX + 12, inputY);
            while (!int.TryParse(Console.ReadLine(), out selectedMonth) || selectedMonth < 1 || selectedMonth > 12)         //Vòng lặp để kiểm tra nhập tháng hợp lệ. Tương tự như năm 
            {
                Console.SetCursorPosition(monthBoxX + 12, inputY);
                ClearCurrentLine(monthBoxX + 12, inputY,24);
                Console.Write("Vui lòng nhập tháng hợp lệ (1-12).");
                System.Threading.Thread.Sleep(1000);
                ClearCurrentLine(monthBoxX + 12, inputY,24);
              
            }
            Console.ResetColor()
        }
        private void DrawBox(int x, int y, int width, int height, string title)                        
        {

            Console.ForegroundColor = ConsoleColor.DarkYellow;                                      // Đặt màu chữ thành vàng đậm để vẽ viền 
          
            Console.SetCursorPosition(x, y);                                                        // Đặt Đặt con trỏ tại vị trí (x, y) để bắt đầu vẽ đường viền trên cùng của hộp.
            Console.Write("╔" + new string('═', width - 2) + "╗");                                  // Vẽ đường viền trên cùng bằng ký tự "╔" (góc trên bên trái), một dãy "═" (đường ngang) có chiều dài là width - 2, và kết thúc bằng ký tự "╗" (góc trên bên phải).

            Console.ForegroundColor = ConsoleColor.Yellow;                                          //Thay đổi màu chữ thành màu vàng sáng để hiển thị tiêu đề.                          
            Console.SetCursorPosition(x + 9, y);                                                    //Đặt con trỏ tại vị trí (x + 9, y) để căn chỉnh tiêu đề ở giữa hộp. Số 9 được chọn để căn tiêu đề vào bên trong hộp một cách cân đối.
            Console.Write(title);                                                                   // In ra tiêu đề
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkYellow;

            for (int i = 1; i < height - 1; i++)                                                    // Vẽ các cạnh dọc của hộp.                                                                                          
            {                                                                                       //  Lặp qua các dòng từ 1 đến height - 2 để vẽ các cạnh dọc của hộp.
                Console.SetCursorPosition(x, y + i);                                                // Đặt con trỏ tại vị trí (x, y + i) để vẽ cạnh của hộp trên mỗi dòng.
                Console.Write("║" + new string(' ', width - 2) + "║");                              //Vẽ một cạnh dọc với ký tự "║" (cạnh dọc bên trái), một dãy khoảng trắng có chiều dài là width - 2, và kết thúc bằng ký tự "║" (cạnh dọc bên phải).
            }

                                                                                                     // Vẽ đường viền dưới cùng
            Console.SetCursorPosition(x, y + height - 1);                                            //Đặt con trỏ tại vị trí để vẽ đường viền dưới cùng của hộp (dòng cuối cùng của hộp).
            Console.Write("╚" + new string('═', width - 2) + "╝");                                   //Vẽ đường viền dưới cùng với ký tự "╚" (góc dưới bên trái), một dãy "═" (đường ngang) có chiều dài width - 2, và kết thúc bằng ký tự "╝" (góc dưới bên phải).
            Console.ResetColor();
        }
        private void ClearCurrentLine(int startX, int startY, int length)                             //xóa nội dung hiện tại trên một dòng cụ thể tại vị trí xác định trong console. 
        {
            Console.SetCursorPosition(startX, startY);                                               //Đặt vị trí con trỏ (cursor) tại tọa độ (startX, startY) trong console.
                                                                                                     //Đây là vị trí bắt đầu của dòng mà bạn muốn xóa.
                                                                                                     //startX: Vị trí ngang(cột) trong console.
                                                                                                     //startY: Vị trí dọc(dòng) trong console. length: số lượng ký tự muốn xóa 
            Console.Write(new string(' ', length));                                                  // Tạo ra một chuỗi rỗng gồm các khoảng trắng có độ dài bằng length.                                


            Console.SetCursorPosition(startX, startY);                                               //Sau khi xóa nội dung, con trỏ được đặt lại về vị trí startX, startY để người dùng có thể nhập lại thông tin mới tại vị trí đó.

        }
        private void DrawHeader()                                                                   //Hàm này có nhiệm vụ hiển thị tiêu đề lịch của năm đã chọn (selectedYear) ở phần trên cùng của console.
        {
            Console.Clear();
            string titleYear ="               𝑪𝑨𝑳𝑬𝑵𝑫𝑨𝑹 𝑶𝑭 𝑻𝑯𝑬 𝒀𝑬𝑨𝑹   " +  selectedYear ;               // Tạo chuỗi tiêu đè
            int leftPadding = (windowWidth - titleYear.Length ) / 2;                               //Tính khoảng cách cần đệm để căn giữa
            Console.SetCursorPosition(leftPadding, 2);                                             // Đặt vị trí con trỏ ở hàng thứ 2 và căn giữa theo chiều ngang
            Console.ForegroundColor = ConsoleColor.Cyan;                                           // Đặt màu sắc cho chữ
            Console.WriteLine(titleYear);                                                          //In ra tiêu đè
            Console.ResetColor();                                                                  // Đặt lại màu sắc mặc định

        }
        private void FillCalendar()                                                                  // hàm này được sử dụng để điền thông tin về ngày tháng vào một cấu trúc dữ liệu của lịch.
        {
            int day = DateTime.DaysInMonth(selectedYear, selectedMonth);                             // Sử dụng phương thức DaysInMonth để lấy số lượng ngày trong tháng được chọn và kết quả lưu vào biến day.
            int firstDayOfMonth = (int)new DateTime(selectedYear, selectedMonth, 1).DayOfWeek;       //Tạo một đối tượng DateTime cho ngày đầu tiên của tháng và chuyển đổi ngày đó sang số nguyên, đại diện cho ngày trong tuần      
                                                                                                     //(0 = Chủ nhật, 1 = Thứ hai, ..., 6 = Thứ bảy). Kết quả được lưu vào firstDayOfMonth.
            if (firstDayOfMonth == 0)                                                                //Nếu ngày đầu tiên của tháng là Chủ nhật (0), chuyển đổi giá trị đó thành 7 để dễ dàng xử lý trong vòng lặp,
                                                                                                     //vì chúng ta sẽ dùng 7 để đại diện cho Chủ nhật trong mảng lịch.
            {
                firstDayOfMonth = 7;
            }
                                                      

            int currentDay = 1;                                                                      //Khởi tạo biến currentDay với giá trị 1, nó sẽ dùng để theo dõi ngày hiện tại mà chúng ta đang điền vào lịch.
            for (int i = 0; i < calendarTracker.GetLength(0); i++)                                  //Bắt đầu vòng lặp qua từng hàng của mảng calendarTracker. GetLength(0) trả về số lượng hàng của mảng.
            {
                for (int j = 0; j < calendarTracker.GetLength(1); j++)                              //Bắt đầu vòng lặp qua từng cột của mảng calendarTracker. GetLength(1) trả về số lượng cột của mảng.
                {
                    if (i == 0 && j < firstDayOfMonth - 1)                                          //Kiểm tra nếu đang ở hàng đầu tiên (i == 0) và cột hiện tại nhỏ hơn ngày đầu tiên của tháng (đã điều chỉnh với firstDayOfMonth - 1), nghĩa là chưa đến ngày bắt đầu của tháng.
                    {
                        calendarTracker[i, j] = 0;                                                  //Nếu điều kiện trên đúng, đặt giá trị của ô hiện tại trong calendarTracker thành 0, nghĩa là ô này không có ngày.

                    }
                    else if (currentDay <= day)                                                      //Nếu không phải ô trống và vẫn còn ngày để điền vào lịch (ngày hiện tại nhỏ hơn hoặc bằng số ngày trong tháng).
                    {
                        calendarTracker[i, j] = currentDay;                                          //Gán giá trị của currentDay vào ô hiện tại trong calendarTracker.
                        currentDay++;                                                                //Tăng currentDay lên 1 để chuyển sang ngày tiếp theo.
                    }
                    else
                    {
                        calendarTracker[i, j] = 0;                                                   // Đặt ô này là 0, nghĩa là ô này không có ngày.
                    }
                }
            }
        }
        private void DrawCalendarBox()                                                                    //Hàm này có nhiệm vụ vẽ một khung cho lịch trong cửa sổ console, bao gồm các đường viền và tên tháng hiện tại.
        {          
            int boxWidth = 80;                                                                            // Đặt chiều rộng của khung lịch là 80
            int boxHeight = 16;                                                                           // Đặt chiều cao của khung lịch là 15
            int startX = (windowWidth - boxWidth) / 2;                                            // Tính toán vị trí X để bắt đầu căn giữa khung
            int startY = 4;                                                                               // Đặt vị trí Y bắt đầu cho khung lịch 
            Console.ForegroundColor = ConsoleColor.Yellow;                                                // Đặt màu chữ thành vàng
            Console.SetCursorPosition(startX, startY);                                                    // Đặt con trỏ tại vị trí (startX, startY) 
            Console.Write("╔" + new string('═', boxWidth) + "╗");                                         //Vẽ cạnh trên của khung 

            for (int i = 0; i < boxHeight; i++)                                                           //Vòng lặp để vẽ các hàng bên trong khung
            {
                Console.SetCursorPosition(startX, startY + i + 1);                                        // Đặt con trỏ tại hàng hiện tại
                Console.Write("║" + new string(' ', boxWidth) + "║");                                     //Vẽ các hàng bên trong khung
            }

            Console.SetCursorPosition(startX, startY + boxHeight + 1);                                    // Đặt con trỏ tại hàng dưới cùng của khung
            Console.Write("╚" + new string('═', boxWidth) + "╝");                                         // Vẽ cạnh dưới của khung
            Console.ResetColor();                                                                         // Đặt lại màu chữ về mặc định

            Console.ForegroundColor = ConsoleColor.Cyan;                                                  // Đặt màu chữ thành cyan    
            string monthName = new DateTime(selectedYear, selectedMonth, 1).ToString("MMMM");             //Lấy tên tháng hiện tại
            string monthDisplay = $" {monthName} {selectedYear} ";                                        // Tạo chuỗi hiển thị cho tháng và năm
            Console.SetCursorPosition(startX + (boxWidth - monthDisplay.Length) / 2, startY + 1);         // Đặt con trỏ vào giữa khung
            Console.WriteLine(monthDisplay);                                                              // In ra
            Console.ResetColor();                                                                         // Đặt lại màu chữ về mặc định
        }
        private void DrawOptions()                                                                          //hàm DrawOptions() có chức năng hiển thị hướng dẫn cho người dùng trong một ứng dụng console. 
        {
            string options = "[Up/Down: Thay đổi năm] [Left/Right: Thay đổi tháng] [Esc: Exit] " +             // Khai báo chuỗi options chứa các hướng dẫn cho người dùng
                "[Enter: Chọn ngày] [Delete: Chọn tháng và năm] ";
            Console.SetCursorPosition((windowWidth - options.Length) / 2, Console.WindowHeight - 2);                   //Tính toán vị trí để căn giữa chuỗi options trong cửa sổ console
            Console.ForegroundColor = ConsoleColor.Yellow;                                                     //Đặt màu chữ thành vàng để nổi bật
            Console.WriteLine(options);                                                                        // In ra
            Console.ResetColor();                                                                              // Đặt lại màu mặc định
        }
        private void DrawCalendarHeader()                                                                              // Hàm chức năng hiển thị tiêu đề của lịch và các ngày, thứ trong tuần:
        {   Console.ForegroundColor = ConsoleColor.Yellow;                                                                // Đặt màu chữ thành màu vàng
            string[] dayNames = { "Thứ hai ", "Thứ ba ", "Thứ tư ", "Thứ năm ", "Thứ sáu ", "Thứ bảy ", "Chủ nhật " };    // Khai báo mảng tên các ngày trong tuần
            int startX = (Console.WindowWidth - 55 - dayNames.Length) / 2;                                                // Tính toán vị trí bắt đầu để căn giữa tiêu đề
            int startY = 7;                                                                                               // Xác định vị trí dọc để in tiêu đề thứ ..

            Console.SetCursorPosition(startX, startY);                                                                    // Đặt con trỏ đến vị trí đã tính toán
            foreach (var day in dayNames)                                                                                 // Lặp qua từng ngày trong mảng dayNames
            {
                Console.Write(day.PadRight(10));                                                                          // In tên ngày và đệm thêm khoảng trắng bên phải
            }
            for (int i = 0; i < calendarTracker.GetLength(0); i++)                                                        // Lặp qua các hàng của lịch
            {
                Console.SetCursorPosition(startX, startY + i * 2 + 2);                                                    // Đặt vị trí con trỏ cho mỗi hàng,khoảng cách giữa các số liền kề theo đường dọc
                for (int j = 0; j < calendarTracker.GetLength(1); j++)                                                    // Lặp qua các cột của lịch
                {
                    if (calendarTracker[i, j] > 0)                                                                        // Kiểm tra nếu có ngày hợp lệ
                    {
                        if ( i == selectedRow && j == selectedCol)                                                        // Nếu hàng và cột hiện tại là ô đã chọn
                        {
                            Console.BackgroundColor = ConsoleColor.DarkBlue;                                              // Đặt màu nền thành xanh đậm cho ô đã chọn

                        }
                        if (j == 6)                                                                                       // Chủ nhật màu đỏ
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                        }
                        else                                                                                              // Các ngày còn lại màu trắng
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                        }

                        Console.Write(calendarTracker[i, j].ToString("D2").PadRight(10));                                 // In ngày, định dạng là 2 chữ số và đệm khoảng trắng bên phải
                    }
                    else
                    {
                        Console.Write(" ".PadRight(10));                                                                  // Nếu không có ngày hợp lệ, in khoảng trắng
                    }

                    Console.ResetColor();
                }
                Console.Write('\n');
            }

        }
        private void MoveSelection(int rowChange, int colChange)             // người dùng di chuyển lựa chọn trong lịch bằng cách cập nhật hàng và cột dựa trên các thay đổi được cung cấp.                                        

        {
            int newRow = selectedRow + rowChange;                            // Tính toán hàng mới bằng cách cộng với thay đổi hàng.
            int newCol = selectedCol + colChange;                            // Tính toán cột mới bằng cách cộng với thay đổi cột.
            if (newRow >= 0 && newRow < calendarTracker.GetLength(0) &&      // Kiểm tra xem hàng mới và cột mới có nằm trong giới hạn hợp lệ và ô đó có giá trị lớn hơn 0 hay không.
                newCol >= 0 && newCol < calendarTracker.GetLength(1) &&      // Hàng mới không âm và không vượt quá số hàng của mảng. Cột mới không âm và không vượt quá số cột của mảng.
                calendarTracker[newRow, newCol] > 0)                         // Ô mới có giá trị lớn hơn 0 (ngày hợp lệ).                  
            {
                selectedRow = newRow;                                        // Cập nhật hàng đã chọn thành hàng mới.
                selectedCol = newCol;                                        // Cập nhật cột đã chọn thành cột mới
            }
            DrawHeader();                                                    // Gọi phương thức để vẽ lại tiêu đề lịch.
            DrawCalendarHeader();                                            // Gọi phương thức để vẽ lại phần tiêu đề của lịch.

        }
        public void ShowDayInfo()                                                                                                           //iển thị thông tin chi tiêu cho ngày đã chọn.                                                                                                         
        {
            int selectedDay = calendarTracker[selectedRow, selectedCol];                                                                    //Lấy ngày đã chọn từ mảng calendarTracker dựa trên hàng và cột đã chọn.
            if (selectedDay >= 0)                                                                                                           // Kiểm tra nếu ngày hợp lệ
            {
                Console.Clear();                                                                                                            // Xóa toàn bộ nội dung đang hiển thị
                Console.WriteLine($"Nhật ký chi tiêu cho ngày {selectedDay}/{selectedMonth}/{selectedYear}:");                              //Hiển thị tiêu đề với thông tin ngày, tháng và năm.

                                                                                                                                            // Lọc các chi tiêu cho ngày đã chọn
                var expensesForSelectedDay = expenseList.Where(expense =>                                                                   // Lọc ra danh sách chi tiêu dựa trên ngày, tháng và năm.
                    expense.Date.Day == selectedDay &&                                                                                      // So sánh ngày của chi tiêu với ngày đã chọn.
                    expense.Date.Month == selectedMonth &&                                                                                  // So sánh tháng của chi tiêu với tháng đã chọn.
                    expense.Date.Year == selectedYear)
                    .ToList();                                                                                                              // Chuyển đổi kết quả lọc thành danh sách.                                                                         // So sánh năm của chi tiêu với năm đã chọn.

                                                                                                                                            // Nhóm các chi tiêu theo danh mục và tính tổng số tiền cho mỗi danh mục
                var groupedExpenses = expensesForSelectedDay
                    .GroupBy(expense => expense.Category)                                                                                   //Nhóm chi tiêu theo danh mục.
                    .Select(group => new                                                                                                    // Tạo một đối tượng mới cho mỗi nhóm.
                    {
                        Category = group.Key,                                                                                               // Lấy tên danh mục từ nhóm.
                        TotalAmount = group.Sum(expense => expense.Amount)                                                                  // Tính tổng số tiền cho danh mục này.
                    }).ToList();                                                                                                            // Chuyển đổi kết quả nhóm thành danh sách.

                if (groupedExpenses.Any())                                                                                                  // Kiểm tra xem có chi tiêu nào đã được nhóm không.
                {
                    foreach (var groupedExpense in groupedExpenses)                                                                         // Duyệt qua từng nhóm chi tiêu.
                    {
                        Console.WriteLine($"Danh mục: {groupedExpense.Category}, Tổng số tiền: {groupedExpense.TotalAmount:#,##0₫}");       // Hiển thị thông tin danh mục và tổng số tiền với định dạng tiền tệ.
                    }
                }
                else
                {
                    Console.WriteLine("Không có chi tiêu nào ghi nhận cho ngày này.");                                                      // Nếu không có chi tiêu nào được ghi nhận. 
                }

                Console.WriteLine("\nNhấn phím bất kỳ để quay lại.");                                                                       // Thông báo người dùng nhấn phím để quay lại.
                Console.ReadKey();                                                                                                          // Chờ người dùng nhấn phím bất kỳ.
               

                DrawHeader();                                                                                                                // Gọi phương thức để vẽ lại tiêu đề lịch.
                DrawCalendarHeader();                                                                                                        // Gọi phương thức để vẽ lại phần tiêu đề của lịch.
            }
        }
    }

}