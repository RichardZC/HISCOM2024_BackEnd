using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class RegimenLaboral
    {
        public RegimenLaboral()
        {
            Empleado = new HashSet<Empleado>();
        }

        public string Id { get; set; }
        public string Denominacion { get; set; }
        public DateTime? FechaReg { get; set; }
        public DateTime? FechaMod { get; set; }
        public bool Estado { get; set; }

        public virtual ICollection<Empleado> Empleado { get; set; }
    }
}
