using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class RolTurno
    {
        public RolTurno()
        {
            RolTurnoEstab = new HashSet<RolTurnoEstab>();
            RolTurnoIntento = new HashSet<RolTurnoIntento>();
        }

        public int Id { get; set; }
        public int? OrganigramaId { get; set; }
        public int JefeId { get; set; }
        public string TipoRolTurno { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string Estado { get; set; }
        public string Observacion { get; set; }
        public DateTime FechaReg { get; set; }
        public DateTime? FechaMod { get; set; }

        public virtual Empleado Jefe { get; set; }
        public virtual Organigrama Organigrama { get; set; }
        public virtual ICollection<RolTurnoEstab> RolTurnoEstab { get; set; }
        public virtual ICollection<RolTurnoIntento> RolTurnoIntento { get; set; }
    }
}
