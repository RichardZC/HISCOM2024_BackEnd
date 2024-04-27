using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class RolTurnoRevision
    {
        public int Id { get; set; }
        public int RolTurnoIntentoId { get; set; }
        public int RolTurnoAprobadorId { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public string Observacion { get; set; }
        public int UsuarioId { get; set; }

        public virtual RolTurnoAprobador RolTurnoAprobador { get; set; }
        public virtual RolTurnoIntento RolTurnoIntento { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}
