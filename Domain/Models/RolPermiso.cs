using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class RolPermiso
    {
        public long Id { get; set; }
        public int RolId { get; set; }
        public int PermisoId { get; set; }

        public virtual Permiso Permiso { get; set; }
        public virtual Rol Rol { get; set; }
    }
}
