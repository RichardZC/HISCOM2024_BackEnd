using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Admin.Templates
{
    public class PayrollTemplate
    {
        public string Vacancy { get; set; }
        public int Year { get; set; }
        public string Month { get; set; }
        public string RUCHRA { get; set; }
        public string Occupation { get; set; }
        public string LaboralCondition { get; set; }
        public string DNI { get; set; }
        public string FullName { get; set; }
        public string BirthDate { get; set; }
        public string AfpCard { get; set; }
        public string AfpDate { get; set; }
        public List<PayrollSalary> Income { get; set; }
        public List<PayrollSalary> Expenses { get; set; }
        public List<PayrollSalary> Contributions { get; set; }
        public decimal? TotalIncome { get; set; }
        public decimal? TotalExpenses { get; set; }
        public decimal? Liquid { get; set; }
        public string QrCode { get; set; }
        public string SearchUrl { get; set; }
        public string FileName { get; set; }
    }


    public class PayrollSalary
    {
        public string Key { get; set; }
        public decimal? Value { get; set; }
    }


}
