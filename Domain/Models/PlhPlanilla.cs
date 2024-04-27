using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class PlhPlanilla
    {
        public PlhPlanilla()
        {
            PlhPlanillaConcepto = new HashSet<PlhPlanillaConcepto>();
        }

        public long Id { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public string Plaza { get; set; }
        public string Pat { get; set; }
        public string Mat { get; set; }
        public string Nom { get; set; }
        public string Libele { get; set; }
        public string Sexo { get; set; }
        public DateTime? FechaNac { get; set; }
        public string CodCar { get; set; }
        public string Regim { get; set; }
        public string Ipsscar { get; set; }
        public string Afpcar { get; set; }
        public DateTime? Fecafp { get; set; }
        public string Codsiaf { get; set; }
        public string Ctaban { get; set; }
        public int? Condic { get; set; }
        public DateTime? Fecalt { get; set; }
        public bool IndNombrado { get; set; }

        public virtual ICollection<PlhPlanillaConcepto> PlhPlanillaConcepto { get; set; }
    }
}
