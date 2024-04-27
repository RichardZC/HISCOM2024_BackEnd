using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class Nivel
    {
        public Nivel()
        {
            Organigrama = new HashSet<Organigrama>();
        }

        public int Id { get; set; }
        public byte Numero { get; set; }
        public string Denominacion { get; set; }
        public DateTime FechaReg { get; set; }
        public bool? FechaMod { get; set; }
        public bool Estado { get; set; }

        public virtual ICollection<Organigrama> Organigrama { get; set; }
    }
}
