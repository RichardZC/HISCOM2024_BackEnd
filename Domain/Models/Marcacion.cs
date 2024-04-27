using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class Marcacion
    {
        public long Id { get; set; }
        public int UsuarioId { get; set; }
        public string NumeroDoc { get; set; }
        public DateTime Fecha { get; set; }
    }
}
