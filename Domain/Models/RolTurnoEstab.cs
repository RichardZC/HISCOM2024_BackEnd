using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class RolTurnoEstab
    {
        public RolTurnoEstab()
        {
            RolTurnoDetalle = new HashSet<RolTurnoDetalle>();
        }

        public int Id { get; set; }
        public int RolTurnoId { get; set; }
        public int EstablecimientoId { get; set; }

        public virtual Establecimiento Establecimiento { get; set; }
        public virtual RolTurno RolTurno { get; set; }
        public virtual ICollection<RolTurnoDetalle> RolTurnoDetalle { get; set; }
    }
}
