using Newtonsoft.Json;

namespace Quanlychitieu
{
    internal class ExpenseTracker
    {
        private Dictionary<string, decimal> expenses = new Dictionary<string, decimal>();
        private string filePath = "expenses.json";

        public decimal TotalIncome { get; private set; } = 0;
        public decimal TotalBudget { get; private set; } = 0;
        public decimal TotalExpenses => GetTotalExpenses();

        public decimal Savings => TotalIncome - TotalBudget  + GetOverspending();


        private decimal GetOverspending()
        {
            // If total expenses exceed the budget, calculate overspending
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
            //expenses["Thu nhập"] = expenses.GetValueOrDefault("Thu nhập", 0) + amount;
            //SaveExpenses();

            TotalIncome += amount; // Cập nhật tổng thu nhập
            expenses["Thu nhập"] = TotalIncome; // Lưu thu nhập vào dictionary
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
                if (expenses.ContainsKey(category))
                    expenses[category] += amount;
                else
                    expenses[category] = amount;

                ArrayMoney[month - 1, categoryIndex] += (double)amount;

                SaveExpenses();
                string transactionType = isExpense ? "chi tiêu" : "thu nhập";
                Console.WriteLine($"Đã lưu {transactionType}: {Math.Abs(amount)} vào danh mục '{category}' vào lúc {timestamp}.");
                CheckOverspending();
            }
            else
            {
                Console.WriteLine("Số tiền không hợp lệ.");
            }
        }
        private void CheckOverspending()
        {
            if (TotalExpenses > TotalBudget)
            {
                decimal overspending = TotalExpenses - TotalBudget;
                Console.WriteLine($"Bạn đang chi tiêu vượt mức dự tính: {overspending:#,##0₫}. Nếu tiếp tục chi tiêu như vậy, bạn có thể không có khoản tiết kiệm trong tháng nay.");
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

        private string GetIncomeCategory(string choice)
        {
            switch (choice)
            {
                case "1": return "Lương";
                case "2": return "Thưởng";
                case "3": return "Đầu tư";
                case "4": return "Tiết kiệm";
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
        // Lấy tổng chi tiêu
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
    }
}