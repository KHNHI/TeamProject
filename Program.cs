using Quanlychitieu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
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
            Console.WriteLine("\n=============  ỨNG DỤNG QUẢN LÍ THU CHI =============\n");
            Console.WriteLine("1: Nhập biến động số dư");
            Console.WriteLine("2. Đặt ngân sách");
            Console.WriteLine("3. Xem tình trạng ngân sách");
            Console.WriteLine("4. Xem đề xuất điều chỉnh ngân sách");
            Console.WriteLine("5: Xem báo cáo tài chính");
            Console.WriteLine("6: Xuất/nhập dữ liệu");
            Console.WriteLine("7: Thoát chương trình");
            Console.Write("Chọn một tùy chọn: ");
            var option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    Console.Clear();
                    Console.WriteLine("1: Nhập khoản chi");
                    Console.WriteLine("2: Nhập khoản thu");
                    Console.Write("Chọn một tùy chọn: ");
                    var balanceOption = Console.ReadLine();

                    switch (balanceOption)
                    {
                        case "1":
                            Console.Clear();
                            Console.WriteLine("Chọn danh mục chi tiêu:");
                            Console.WriteLine("1. Ăn uống");
                            Console.WriteLine("2. Đi lại");
                            Console.WriteLine("3. Chi phí cố định (nhà ở, điện, nước, wifi...)");
                            Console.WriteLine("4. Giải trí");
                            Console.WriteLine("5. Giáo dục");
                            Console.WriteLine("6. Mua sắm");
                            Console.WriteLine("7. Khác");
                            Console.Write("Chọn danh mục: ");
                            var expenseCategory = Console.ReadLine();
                            if (!string.IsNullOrEmpty(expenseCategory))
                            {
                                expenseTracker.EnterExpense(expenseCategory);
                            }
                            else
                            {
                                Console.WriteLine("Danh mục không hợp lệ.");
                            }
                            break;
                        case "2":
                            Console.Clear();
                            Console.WriteLine("Chọn danh mục thu nhập:");
                            Console.WriteLine("1. Lương");
                            Console.WriteLine("2. Thưởng");
                            Console.WriteLine("3. Đầu tư");
                            Console.WriteLine("4. Tiết kiệm");
                            Console.Write("Chọn danh mục: ");
                            var incomeCategory = Console.ReadLine();
                            if (!string.IsNullOrEmpty(incomeCategory))
                            {
                                expenseTracker.EnterIncome(incomeCategory);
                            }
                            else
                            {
                                Console.WriteLine("Danh mục không hợp lệ.");
                            }
                            break;
                        default:
                            Console.WriteLine("Lựa chọn không hợp lệ.");
                            break;
                    }
                    break;
                case "2":
                    Console.Clear();
                    budgetPlanner.SetBudget();
                    break;
                case "3":
                    Console.Clear();
                    budgetPlanner.ShowBudgetStatus();
                    break;
                case "4":
                    Console.Clear();
                    budgetPlanner.SuggestBudgetAdjustments();
                    break;
                case "5":
                    Console.Clear();
                    financialReport.ShowReport(expenseTracker);
                    Console.WriteLine("Báo cáo tài chính đã hoàn thành.");
                    Console.WriteLine("Nhấn Enter để tiếp tục...");
                    Console.ReadLine();
                    break;
                   
                case "6":
                    Console.Clear();
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
