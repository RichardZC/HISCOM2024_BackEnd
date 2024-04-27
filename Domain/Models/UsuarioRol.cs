using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class UsuarioRol
    {
        public long Id { get; set; }
        public int UsuarioId { get; set; }
        public int RolId { get; set; }

        public virtual Rol Rol { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}
