using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class Rol
    {
        public Rol()
        {
            RolPermiso = new HashSet<RolPermiso>();
            RolTurnoAprobador = new HashSet<RolTurnoAprobador>();
            UsuarioRol = new HashSet<UsuarioRol>();
        }

        public int Id { get; set; }
        public string Denominacion { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaReg { get; set; }
        public DateTime? FechaMod { get; set; }
        public bool Estado { get; set; }

        public virtual ICollection<RolPermiso> RolPermiso { get; set; }
        public virtual ICollection<RolTurnoAprobador> RolTurnoAprobador { get; set; }
        public virtual ICollection<UsuarioRol> UsuarioRol { get; set; }
    }
}
