using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class TipoEmpleado
    {
        public TipoEmpleado()
        {
            Empleado = new HashSet<Empleado>();
        }

        public int Id { get; set; }
        public string Denominacion { get; set; }
        public DateTime FechaReg { get; set; }
        public DateTime? FechaMod { get; set; }
        public bool Estado { get; set; }

        public virtual ICollection<Empleado> Empleado { get; set; }
    }
}
