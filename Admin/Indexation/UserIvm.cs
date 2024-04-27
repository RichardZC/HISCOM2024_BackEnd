using System;
using System.Globalization;
using Domain.Models;

namespace Admin.Indexation
{
    public class UserIvm
    {
        public const string indexUid = "user";
        public static UserIvm GetUserIvm(Usuario user)
        {
            UserIvm u = new UserIvm();
            u.Id = user.Id.ToString();
            u.EmpleadoId = user.EmpleadoId;
            u.NombreCompleto = $"{user.Empleado.ApellidoPaterno} {user.Empleado.ApellidoMaterno} {user.Empleado.Nombres}";
            u.NombreUsuario = user.NombreUsuario;
            u.Correo = user.Correo;
            u.FechaReg = user.FechaReg;
            return u;
        }
        public string Id { get; set; }
        public int EmpleadoId { get; set; }
        public string NombreCompleto { get; set; }
        public string NombreUsuario { get; set; }
        public string Correo { get; set; }
        public DateTime FechaReg { get; set; }
    }
}