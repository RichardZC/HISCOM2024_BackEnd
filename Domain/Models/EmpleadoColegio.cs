using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class EmpleadoColegio
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public int ColegioId { get; set; }
        public string NroColegiatura { get; set; }

        public virtual ColegioProfesional Colegio { get; set; }
        public virtual Empleado Empleado { get; set; }
    }
}
