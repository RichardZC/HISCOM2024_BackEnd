using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class Cargo
    {
        public Cargo()
        {
            Empleado = new HashSet<Empleado>();
        }

        public string Id { get; set; }
        public int? ClasificacionId { get; set; }
        public string Denominacion { get; set; }
        public DateTime FechaReg { get; set; }
        public DateTime? FechaMod { get; set; }
        public bool Estado { get; set; }

        public virtual Clasificacion Clasificacion { get; set; }
        public virtual ICollection<Empleado> Empleado { get; set; }
    }
}
