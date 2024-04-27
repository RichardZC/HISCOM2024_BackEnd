using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class CategoriaEmpleado
    {
        public long Id { get; set; }
        public int CategoriaId { get; set; }
        public int EmpleadoId { get; set; }

        public virtual Categoria Categoria { get; set; }
        public virtual Empleado Empleado { get; set; }
    }
}
