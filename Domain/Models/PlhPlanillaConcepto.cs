using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class PlhPlanillaConcepto
    {
        public long Id { get; set; }
        public long PlhPlanillaId { get; set; }
        public int PlhConceptoId { get; set; }
        public decimal? Saldo { get; set; }

        public virtual PlhConcepto PlhConcepto { get; set; }
        public virtual PlhPlanilla PlhPlanilla { get; set; }
    }
}
