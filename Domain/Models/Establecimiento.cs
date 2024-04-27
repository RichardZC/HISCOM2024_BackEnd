using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class Establecimiento
    {
        public Establecimiento()
        {
            RolTurnoEstab = new HashSet<RolTurnoEstab>();
        }

        public int Id { get; set; }
        public string Denominacion { get; set; }
        public DateTime FechaReg { get; set; }
        public DateTime? FechaMod { get; set; }
        public bool Estado { get; set; }

        public virtual ICollection<RolTurnoEstab> RolTurnoEstab { get; set; }
    }
}
