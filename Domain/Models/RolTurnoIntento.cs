using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class RolTurnoIntento
    {
        public RolTurnoIntento()
        {
            RolTurnoRevision = new HashSet<RolTurnoRevision>();
        }

        public int Id { get; set; }
        public int RolTurnoId { get; set; }
        public DateTime FechaEnvio { get; set; }
        public DateTime? FechaCierre { get; set; }
        public bool Actual { get; set; }
        public int? SiguienteAprobadorId { get; set; }

        public virtual RolTurno RolTurno { get; set; }
        public virtual RolTurnoAprobador SiguienteAprobador { get; set; }
        public virtual ICollection<RolTurnoRevision> RolTurnoRevision { get; set; }
    }
}
