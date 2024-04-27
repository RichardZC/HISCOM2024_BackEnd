using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class RolTurnoDetalle
    {
        public long Id { get; set; }
        public int RolTurnoEstabId { get; set; }
        public int EmpleadoId { get; set; }
        public int TurnoId { get; set; }
        public int Dia { get; set; }

        public virtual Empleado Empleado { get; set; }
        public virtual RolTurnoEstab RolTurnoEstab { get; set; }
        public virtual Turno Turno { get; set; }
    }
}
