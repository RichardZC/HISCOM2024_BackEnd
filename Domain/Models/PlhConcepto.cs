using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class PlhConcepto
    {
        public PlhConcepto()
        {
            PlhPlanillaConcepto = new HashSet<PlhPlanillaConcepto>();
        }

        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Abreviado { get; set; }
        public string Denominacion { get; set; }
        public DateTime FechaReg { get; set; }
        public DateTime? FechaMod { get; set; }
        public bool Estado { get; set; }
        public bool IndNombrado { get; set; }

        public virtual ICollection<PlhPlanillaConcepto> PlhPlanillaConcepto { get; set; }
    }
}
