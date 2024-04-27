using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class EmpleadoProfesion
    {
        public long Id { get; set; }
        public int EmpleadoId { get; set; }
        public int ProfesionId { get; set; }

        public virtual Empleado Empleado { get; set; }
        public virtual Profesion Profesion { get; set; }
    }
}
