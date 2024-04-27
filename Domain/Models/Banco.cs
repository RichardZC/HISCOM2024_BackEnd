using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class Banco
    {
        public Banco()
        {
            Empleado = new HashSet<Empleado>();
        }

        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Abreviacion { get; set; }

        public virtual ICollection<Empleado> Empleado { get; set; }
    }
}
