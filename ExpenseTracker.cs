using Newtonsoft.Json;

namespace Quanlychitieu
{
    internal class ExpenseTracker
    {
        private Dictionary<string, decimal> expenses = new Dictionary<string, decimal>();
        private string filePath = "expenses.json";
        private Dictionary<string, Dictionary<int, double>> monthlyExpenses;

        public decimal TotalIncome { get; private set; } = 0;
        public decimal TotalBudget { get; private set; } = 0;
        public decimal TotalExpenses => GetTotalExpenses();
        public decimal Savings => TotalIncome - TotalBudget + GetOverspending();

        public ExpenseTracker()
        {
            monthlyExpenses = new Dictionary<string, Dictionary<int, double>>();
        }

        private decimal GetOverspending()
        {
            return TotalExpenses > TotalBudget ? TotalExpenses - TotalBudget : 0;
        }

        public void SetBudget(decimal budget)
        {
            TotalBudget = budget;
        }

        public void EnterExpense(string categoryChoice)
        {
            if (string.IsNullOrEmpty(categoryChoice))
            {
                Console.WriteLine("Danh mục không hợp lệ.");
                return;
            }

            string category = GetExpenseCategory(categoryChoice);
            EnterTransaction(category, isExpense: true);
        }

        public void EnterIncome(decimal amount)
        {
            TotalIncome += amount;
            expenses["Thu nhập"] = TotalIncome;
            SaveExpenses();
        }

        double[,] ArrayMoney = new double[12, 7]; // 12 tháng, 7 danh mục chi tiêu
        string[] categories = { "Ăn uống", "Đi lại", "Chi phí cố định", "Giải trí", "Giáo dục", "Mua sắm", "Khác" };

        int month = DateTime.Now.Month;

        private void EnterTransaction(string category, bool isExpense)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.Write("Nhập số tiền: ");

            int categoryIndex = Array.IndexOf(categories, category);

            if (decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                if (!monthlyExpenses.ContainsKey(category))
                {
                    monthlyExpenses[category] = new Dictionary<int, double>();
                }

                if (!monthlyExpenses[category].ContainsKey(month))
                {
                    monthlyExpenses[category][month] = 0;
                }

                monthlyExpenses[category][month] += (double)amount;

                if (expenses.ContainsKey(category))
                {
                    expenses[category] += amount;
                }
                else
                {
                    expenses[category] = amount;
                }

                SaveExpenses();

                string transactionType = isExpense ? "chi tiêu" : "thu nhập";
                Console.WriteLine($"Đã lưu {transactionType}: {Math.Abs(amount)} vào danh mục '{category}' vào lúc {timestamp}.");
                Console.WriteLine($"Số tiền bằng chữ: {ConvertNumberToWords((long)Math.Abs(amount))}");
                CheckOverspending();
            }
            else
            {
                Console.WriteLine("Số tiền không hợp lệ.");
            }
        }

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
            if (Savings >= 0)
            {
                return $"Tiết kiệm: {Savings:#,##0₫}";
            }
            else
            {
                return $"Chi tiêu vượt mức: {-Savings:#,##0₫}";
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
            if (File.Exists(filePath))
            {
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
            else
            {
                Console.WriteLine("Tệp mock_expenses.json không tồn tại.");
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
        

    }
}