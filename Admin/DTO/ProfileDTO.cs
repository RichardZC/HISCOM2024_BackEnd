using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Admin.DTO
{
    public class ProfileDTO
    {
        public string Foto { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string ContrasenaActual { get; set; }
        public string ContrasenaNueva { get; set; }

        public static ProfileDTO Create(Usuario user)
        {
            var u = new ProfileDTO();
            u.Foto = user.Foto;
            u.NombreCompleto = user.Empleado.ApellidoPaterno + " " + user.Empleado.ApellidoMaterno + " " + user.Empleado.Nombres;
            u.Correo = user.Correo;
            return u;
        }
    }
}
