using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class TipoDocumento
    {
        public TipoDocumento()
        {
            Empleado = new HashSet<Empleado>();
        }

        public int Id { get; set; }
        public string Denominacion { get; set; }
        public string Abreviatura { get; set; }

        public virtual ICollection<Empleado> Empleado { get; set; }
    }
}
