using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Admin.DTO
{

    public enum TipoPlanillaEnum
    {
        Nombrado,
        Contratado,
    }

    public class PayrollDTO
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string Period { get; set; }
    }

    public class PeriodPayroll
    {
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
