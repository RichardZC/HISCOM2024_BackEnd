using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class EnlaceReset
    {
        public Guid Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
