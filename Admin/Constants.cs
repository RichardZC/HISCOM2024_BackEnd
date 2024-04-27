using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Admin
{
    public interface IConstants
    {
        public string DayLimitKey {get;}
        public string ShiftWorkQRKey { get; }
        public string PayrollQRKey { get; }
        public string Storage { get; }
        public string ImagePath { get; }
        public string ShiftWorkPath { get; }
        public string PayrollPath { get; set; }
        public string ClinicalExamPath { get; set; }
        public string ElasticUrl { get; set; }
        public string HiscomFrontEndUrl { get; }

    }
    public class Constants: IConstants
    {

        public string DayLimitKey { get; set; }
        public string ShiftWorkQRKey { get; set; }
        public string PayrollQRKey { get; set; }
        public string Storage { get; set; }
        public string ImagePath { get; set; }
        public string ShiftWorkPath { get; set; }
        public string PayrollPath { get; set; }
        public string ClinicalExamPath { get; set; }
        public string ElasticUrl { get; set; }
        public string HiscomFrontEndUrl { get; set; }
    }
}
