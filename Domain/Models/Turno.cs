using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class Turno
    {
        public Turno()
        {
            RolTurnoDetalle = new HashSet<RolTurnoDetalle>();
        }

        public int Id { get; set; }
        public string Denominacion { get; set; }
        public string Descripcion { get; set; }
        public int Horas { get; set; }
        public DateTime FechaReg { get; set; }
        public DateTime? FechaMod { get; set; }
        public bool Estado { get; set; }
        public TimeSpan? HoraInicio { get; set; }

        public virtual ICollection<RolTurnoDetalle> RolTurnoDetalle { get; set; }
    }
}
