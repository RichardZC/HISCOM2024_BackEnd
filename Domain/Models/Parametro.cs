using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class Parametro
    {
        public int Id { get; set; }
        public string Llave { get; set; }
        public string Valor { get; set; }
        public DateTime FechaReg { get; set; }
        public DateTime? FechaMod { get; set; }
        public bool Estado { get; set; }
    }
}
