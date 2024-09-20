using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quanlychitieu
{
    public class Expense
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
}
