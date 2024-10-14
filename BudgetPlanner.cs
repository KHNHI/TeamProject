namespace Quanlychitieu
{
    internal class BudgetPlanner
    {
        private ExpenseTracker expenseTracker;
        private DataSync dataSync;
        private Dictionary<string, decimal> categoryBudgets;
        private readonly string[] validCategories = new string[]
        {
     "Ăn uống", "Đi lại", "Chi phí cố định", "Giải trí", "Giáo dục", "Mua sắm", "Khác"
        };

        public BudgetPlanner(ExpenseTracker expenseTracker, DataSync dataSync)
        {
            this.expenseTracker = expenseTracker;
            this.dataSync = dataSync;
            categoryBudgets = dataSync.LoadBudgetFromCSV();
        }

        public void SetBudget()
        {

            string input;
            List<string> remainingCategories = new List<string>(validCategories);
            HashSet<string> enteredCategories = new HashSet<string>();
            int choice;
            bool continueInput = true;
            do
            {
                Console.WriteLine("\nChọn danh mục để đặt ngân sách:");
                int index = 1;
                if (remainingCategories.Count == 0)
                {
                    Console.WriteLine("Bạn đã nhập ngân sách cho tất cả các danh mục.");
                    break;
                }
                for (int i = 0; i < validCategories.Length; i++)
                {
                    if (remainingCategories.Contains(validCategories[i]))
                    {
                        Console.WriteLine($"{i + 1}. {validCategories[i]}");
                    }

                }

                // Vòng lặp để bắt buộc nhập đúng danh mục
                while (true)
                {
                    Console.Write("Nhập số tương ứng với danh mục: ");
                    if (int.TryParse(Console.ReadLine(), out choice) && choice >= 1 && choice <= validCategories.Length)
                    {
                        string selectedCategory = validCategories[choice - 1];
                        if (enteredCategories.Contains(selectedCategory))
                        {
                            Console.WriteLine("Bạn đã nhập ngân sách cho danh mục này. Vui lòng chọn danh mục khác.");
                        }
                        else if (remainingCategories.Contains(selectedCategory))
                        {
                            enteredCategories.Add(selectedCategory);
                            remainingCategories.Remove(selectedCategory);
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Danh mục này không còn trong danh sách. Vui lòng chọn danh mục khác.");
                        }


                    }
                    else
                    {
                        Console.WriteLine("Lựa chọn không hợp lệ. Vui lòng thử lại.");
                    }
                }

                //string category = validCategories[choice - 1];
                decimal amount;
                // Vòng lặp để bắt buộc nhập đúng số tiền
                while (true)
                {
                    Console.Write($"Nhập số tiền ngân sách cho {validCategories[choice - 1]}: ");
                    if (decimal.TryParse(Console.ReadLine(), out amount) && amount >= 0)
                    {
                        break; // Thoát khỏi vòng lặp nếu nhập hợp lệ
                    }
                    else
                    {
                        Console.WriteLine("Số tiền không hợp lệ. Vui lòng nhập một số dương.");
                    }
                }

                // Lưu ngân sách vào danh mục
                categoryBudgets[validCategories[choice - 1]] = amount;
                Console.WriteLine($"Ngân sách cho {validCategories[choice - 1]} đã được đặt thành {amount:#,##0₫}");


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
                        Console.WriteLine("Bạn vui lòng nhập hết tất cả danh mục trên.");
                        Console.ResetColor();
                        continueInput = true;
                    }
                    else
                    {
                        Console.WriteLine("Vui lòng nhập đúng ký tự.");
                    }


                }
                else
                {
                    Console.WriteLine("Tất cả danh mục đã được đặt ngân sách.");
                    continueInput = false;
                }


            } while (continueInput);

            // Lưu dữ liệu vào file csv sau khi đặt xong ngân sách
            dataSync.SaveBudgetToCSV(categoryBudgets);
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

            var totalBudget = categoryBudgets.Sum(b => b.Value);
            var totalExpenses = expenses.Sum(e => e.Value);

            Console.WriteLine("\n=== Đề xuất Điều chỉnh Ngân sách ===");

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
