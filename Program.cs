using Quanlychitieu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
namespace Quanlychitieu;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        ExpenseTracker expenseTracker = new ExpenseTracker();
        BudgetPlanner budgetPlanner = new BudgetPlanner(expenseTracker);
        FinancialReport financialReport = new FinancialReport();
        DataSync dataSync = new DataSync();
        DataSecurity dataSecurity = new DataSecurity();
        Notification notification = new Notification();
        expenseTracker.LoadExpenses();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Ứng dụng Quản lý Chi tiêu ===");
            Console.WriteLine("1: Nhập chi tiêu mới");
            Console.WriteLine("2: Xem báo cáo tài chính");
            Console.WriteLine("3. Đặt ngân sách");
            Console.WriteLine("4. Xem tình trạng ngân sách");
            Console.WriteLine("5. Xem đề xuất điều chỉnh ngân sách");
            Console.WriteLine("6: Xuất/nhập dữ liệu");
            Console.WriteLine("7: Thoát chương trình");
            Console.Write("Chọn một tùy chọn: ");
            var option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    expenseTracker.EnterExpense();
                    break;
                case "2":
                    Console.WriteLine("Đang chuẩn bị báo cáo tài chính...");
                    financialReport.ShowReport(expenseTracker);
                    Console.WriteLine("Báo cáo tài chính đã hoàn thành.");
                    Console.WriteLine("Nhấn Enter để tiếp tục...");
                    Console.ReadLine();
                    break;
                case "3":
                    Console.Write("Nhập danh mục: ");
                    string category = Console.ReadLine();
                    Console.Write("Nhập số tiền ngân sách: ");
                    if (decimal.TryParse(Console.ReadLine(), out decimal amount))
                    {
                        budgetPlanner.SetBudget(category, amount);
                    }
                    else
                    {
                        Console.WriteLine("Số tiền không hợp lệ.");
                    }
                    break;
                case "4":
                    budgetPlanner.ShowBudgetStatus();
                    break;
                case "5":
                    budgetPlanner.SuggestBudgetAdjustments();
                    break;
                case "6":
                    dataSync.HandleDataSync();
                    break;
                case "7":
                    return;
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ. Vui lòng chọn lại.");
                    break;
            }
            Console.ReadLine();
        }
    
    }
}
