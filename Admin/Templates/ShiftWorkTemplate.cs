using System;
using System.Collections.Generic;
using Domain.Models;
using Org.BouncyCastle.Asn1;

namespace Admin.Templates
{
    public class ShiftWorkTemplate
    {
        public string Structure { get; set; }
        public string Month { get; set; }
        public int Year { get; set; }
        public int HeaderHeight { get; set; }
        public string Type { get; set; }
        public List<ShiftWorkTemplateApprovals> Approvals { get; set; }
        public List<ShiftWorkTemplateEstablishment> Establishments { get; set; }
        public List<ShiftWorkTemplateDay> Days { get; set; }
        public List<ShiftWorkTemplateTurn> Turns { get; set; }
        public List<ShiftWorkTemplateLevel> Levels { get; set; }
        public List<ShiftWorkTemplateSignature> Signatures { get; set; }
        public string QrCode { get; set; }
        public string SearchUrl { get; set; }
        public string FileName { get; set; }
        
    }

    public class ShiftWorkTemplateEstablishment
    {
        public int Id { get; set; }
        public string Denomination { get; set; }
        public List<ShiftWorkTemplateCategory> Categories { get; set; }
        //public List<ShiftWorkTemplateEmployee> Employees { get; set; }
    }

    public class ShiftWorkTemplateCategory
    {
        public string Denomination { get; set; }
        public List<ShiftWorkTemplateEmployee> Employees { get; set; }
    }

    public class ShiftWorkTemplateDay
    {
        public int Day { get; set; }
        public string Abbr { get; set; }
    }
    
    public class ShiftWorkTemplateEmployee
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string FullName { get; set; }
        public string LaboralCondition { get; set; }
        public List<ShiftWorkTemplateDetails> Details {get; set; }
        public int TotalHours { get; set; }
    }

    public class ShiftWorkTemplateDetails
    {
        public string Abbreviation { get; set; }
        public string BackgroundColor { get; set; }
    }

    public class ShiftWorkTemplateTurn
    {
        public string Denomination { get; set; }
        public string Description { get; set; }
    }

    public class ShiftWorkTemplateLevel
    {
        public string Level { get; set; }
        public string Denomination { get; set; }
    }

    public class ShiftWorkTemplateApprovals
    {
        public string Header { get; set; }
        public string Approver { get; set; }
        public string Date { get; set; }
    }

    public class ShiftWorkTemplateSignature
    {
        public string Denomination { get; set; }
        public string Dashes { get; set; }
    }
}