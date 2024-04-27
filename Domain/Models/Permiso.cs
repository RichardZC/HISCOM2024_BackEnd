using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class Permiso
    {
        public Permiso()
        {
            RolPermiso = new HashSet<RolPermiso>();
        }

        public int Id { get; set; }
        public int? MenuId { get; set; }
        public string Accion { get; set; }
        public string Ruta { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Visible { get; set; }
        public string SubMenu { get; set; }
        public DateTime FechaReg { get; set; }
        public DateTime? FechaMod { get; set; }
        public bool Estado { get; set; }

        public virtual Menu Menu { get; set; }
        public virtual ICollection<RolPermiso> RolPermiso { get; set; }
    }
}
