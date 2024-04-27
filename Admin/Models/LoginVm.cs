using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Admin.Models
{
    public class LoginVm
    {
        public string Usuario { get; set; }
        public string Clave { get; set; }
    }
    
    public class LoginPatientVm
    {
        public string Dni { get; set; }
        public DateTime IssueDate { get; set; }
    }
}
