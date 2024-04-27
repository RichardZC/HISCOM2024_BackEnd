using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class Organigrama
    {
        public Organigrama()
        {
            Categoria = new HashSet<Categoria>();
            Empleado = new HashSet<Empleado>();
            InversePadre = new HashSet<Organigrama>();
            RolTurno = new HashSet<RolTurno>();
        }

        public int Id { get; set; }
        public int? NivelId { get; set; }
        public int? PadreId { get; set; }
        public string Denominacion { get; set; }
        public DateTime FechaReg { get; set; }
        public DateTime? FechaMod { get; set; }
        public bool Estado { get; set; }

        public virtual Nivel Nivel { get; set; }
        public virtual Organigrama Padre { get; set; }
        public virtual ICollection<Categoria> Categoria { get; set; }
        public virtual ICollection<Empleado> Empleado { get; set; }
        public virtual ICollection<Organigrama> InversePadre { get; set; }
        public virtual ICollection<RolTurno> RolTurno { get; set; }
    }
}
