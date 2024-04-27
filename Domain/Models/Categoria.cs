using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class Categoria
    {
        public Categoria()
        {
            CategoriaEmpleado = new HashSet<CategoriaEmpleado>();
        }

        public int Id { get; set; }
        public int OrganigramaId { get; set; }
        public string Denominacion { get; set; }
        public string Color { get; set; }
        public DateTime FechaReg { get; set; }
        public DateTime? FechaMod { get; set; }
        public bool Estado { get; set; }

        public virtual Organigrama Organigrama { get; set; }
        public virtual ICollection<CategoriaEmpleado> CategoriaEmpleado { get; set; }
    }
}
