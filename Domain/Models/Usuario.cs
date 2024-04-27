using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class Usuario
    {
        public Usuario()
        {
            Auditoria = new HashSet<Auditoria>();
            RolTurnoRevision = new HashSet<RolTurnoRevision>();
            UsuarioRol = new HashSet<UsuarioRol>();
        }

        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public string NombreUsuario { get; set; }
        public string Correo { get; set; }
        public string Contrasena { get; set; }
        public string Foto { get; set; }
        public DateTime FechaReg { get; set; }
        public DateTime? FechaMod { get; set; }
        public bool Estado { get; set; }

        public virtual Empleado Empleado { get; set; }
        public virtual ICollection<Auditoria> Auditoria { get; set; }
        public virtual ICollection<RolTurnoRevision> RolTurnoRevision { get; set; }
        public virtual ICollection<UsuarioRol> UsuarioRol { get; set; }
    }
}
