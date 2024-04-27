using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class Nacionalidad
    {
        public Nacionalidad()
        {
            Empleado = new HashSet<Empleado>();
        }

        public int Id { get; set; }
        public string Pais { get; set; }
        public string Gentilicio { get; set; }
        public string Abreviacion { get; set; }

        public virtual ICollection<Empleado> Empleado { get; set; }
    }
}
