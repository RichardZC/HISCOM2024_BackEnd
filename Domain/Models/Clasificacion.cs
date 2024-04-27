using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class Clasificacion
    {
        public Clasificacion()
        {
            Cargo = new HashSet<Cargo>();
        }

        public int Id { get; set; }
        public string Abreviatura { get; set; }
        public string Denominacion { get; set; }
        public DateTime FechaReg { get; set; }
        public DateTime? FechaMod { get; set; }
        public bool Estado { get; set; }

        public virtual ICollection<Cargo> Cargo { get; set; }
    }
}
