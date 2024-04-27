using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class RolTurnoAprobador
    {
        public RolTurnoAprobador()
        {
            InverseAnterior = new HashSet<RolTurnoAprobador>();
            InverseSiguiente = new HashSet<RolTurnoAprobador>();
            RolTurnoIntento = new HashSet<RolTurnoIntento>();
            RolTurnoRevision = new HashSet<RolTurnoRevision>();
        }

        public int Id { get; set; }
        public int AprobadorId { get; set; }
        public int? AnteriorId { get; set; }
        public int? SiguienteId { get; set; }
        public string TipoRolTurno { get; set; }
        public bool AprobadorPadre { get; set; }

        public virtual RolTurnoAprobador Anterior { get; set; }
        public virtual Rol Aprobador { get; set; }
        public virtual RolTurnoAprobador Siguiente { get; set; }
        public virtual ICollection<RolTurnoAprobador> InverseAnterior { get; set; }
        public virtual ICollection<RolTurnoAprobador> InverseSiguiente { get; set; }
        public virtual ICollection<RolTurnoIntento> RolTurnoIntento { get; set; }
        public virtual ICollection<RolTurnoRevision> RolTurnoRevision { get; set; }
    }
}
