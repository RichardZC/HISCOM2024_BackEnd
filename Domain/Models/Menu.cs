using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class Menu
    {
        public Menu()
        {
            Permiso = new HashSet<Permiso>();
        }

        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Icono { get; set; }
        public DateTime FechaReg { get; set; }
        public DateTime? FechaMod { get; set; }
        public bool Estado { get; set; }

        public virtual ICollection<Permiso> Permiso { get; set; }
    }
}
