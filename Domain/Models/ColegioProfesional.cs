using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class ColegioProfesional
    {
        public ColegioProfesional()
        {
            Empleado = new HashSet<Empleado>();
        }

        public int Id { get; set; }
        public string Denominacion { get; set; }
        public string Decano { get; set; }
        public string Direccion { get; set; }
        public string Telefonos { get; set; }
        public string SitioWeb { get; set; }
        public string Foto { get; set; }
        public DateTime FechaReg { get; set; }
        public DateTime? FechaMod { get; set; }
        public bool Estado { get; set; }

        public virtual ICollection<Empleado> Empleado { get; set; }
    }
}
