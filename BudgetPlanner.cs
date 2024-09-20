
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quanlychitieu
{
    public class BudgetPlanner
    {
        private Dictionary<string, decimal> categoryBudgets;
        private ExpenseTracker expenseTracker;

        internal BudgetPlanner (ExpenseTracker expenseTracker)
        {
            this.expenseTracker = expenseTracker;
            categoryBudgets = new Dictionary<string, decimal>();
        }

        public void SetBudget(string category, decimal amount)
        {
            categoryBudgets[category] = amount;
            Console.WriteLine($"Ngân sách cho {category} đã được đặt thành {amount:#,##0₫}");
        }

        public void ShowBudgetStatus()
        {
            var expenses = expenseTracker.GetExpenses();

            Console.WriteLine("\n=== Tình trạng Ngân sách ===");
            foreach (var budget in categoryBudgets)
            {
                string category = budget.Key;
                decimal budgetAmount = budget.Value;
                decimal spent = expenses.ContainsKey(category) ? expenses[category] : 0;
                decimal remaining = budgetAmount - spent;
                decimal percentageUsed = (spent / budgetAmount) * 100;
              

                Console.WriteLine($"Danh mục: {category}");
                Console.WriteLine($"Ngân sách: {budgetAmount:#,##0₫}");
                Console.WriteLine($"Đã chi: {spent:#,##0₫}");
                Console.WriteLine($"Còn lại: {remaining:#,##0₫}");
                Console.WriteLine($"Đã sử dụng: {percentageUsed:F1}%");

                if (remaining < 0)
                {
                    Console.WriteLine("Cảnh báo: Bạn đã vượt quá ngân sách!");
                }
                else if (percentageUsed > 80)
                {
                    Console.WriteLine("Cảnh báo: Bạn đã sử dụng hơn 80% ngân sách!");
                }

                Console.WriteLine();
            }
        }

        public void SuggestBudgetAdjustments()
        {
            var expenses = expenseTracker.GetExpenses();
            var totalBudget = categoryBudgets.Sum(b => b.Value);
            var totalExpenses = expenses.Sum(e => e.Value);

            Console.WriteLine("\n=== Đề xuất Điều chỉnh Ngân sách ===");

            if (totalExpenses > totalBudget)
            {
                Console.WriteLine("Bạn đang chi tiêu nhiều hơn tổng ngân sách. Hãy xem xét cắt giảm chi tiêu hoặc tăng ngân sách.");
            }

            foreach (var expense in expenses)
            {
                string category = expense.Key;
                decimal spent = expense.Value;

                if (!categoryBudgets.ContainsKey(category))
                {
                    Console.WriteLine($"Đề xuất: Thiết lập ngân sách cho danh mục '{category}'. Chi tiêu hiện tại: {spent:#,##0₫}");
                }
                else if (spent > categoryBudgets[category])
                {
                    decimal overspent = spent - categoryBudgets[category];
                    Console.WriteLine($"Đề xuất: Tăng ngân sách cho '{category}' thêm {overspent:#,##0₫} hoặc cắt giảm chi tiêu.");
                }
            }

            var unusedBudgets = categoryBudgets.Where(b => !expenses.ContainsKey(b.Key) || expenses[b.Key] < b.Value * 0.5m);
            foreach (var budget in unusedBudgets)
            {
                Console.WriteLine($"Đề xuất: Xem xét giảm ngân sách cho '{budget.Key}' vì chi tiêu thấp hơn 50% ngân sách.");
            }
        }
    }
}