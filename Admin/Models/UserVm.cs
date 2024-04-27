using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;

namespace Admin.Models
{
    public class UserVm: Usuario
    {
        public static UserVm CreateUserVm(Usuario user)
        {
            if (user==null)
            {
                return null;
            }
            var uservm = new UserVm();
            uservm.Id = user.Id;
            uservm.EmpleadoId = user.EmpleadoId;
            uservm.NombreUsuario = user.NombreUsuario;
            uservm.Correo = user.Correo;
            uservm.Contrasena = user.Contrasena;
            uservm.FechaReg = user.FechaReg;
            uservm.FechaMod = user.FechaMod;
            uservm.Estado = user.Estado;
            uservm.Foto = user.Foto;

            return uservm;
        }
        public List<int> Roles { get; set; }
        public string NuevaFoto { get; set; }
    }
}
