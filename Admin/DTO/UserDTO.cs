using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Admin.Models;
using Domain.Models;

namespace Admin.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public EmployeeUserDTO Empleado { get; set; }
        public string NombreUsuario { get; set; }
        public string Correo { get; set; }
        public string Contrasena { get; set; }
        public string Foto { get; set; }
        public DateTime FechaReg { get; set; }
        public DateTime? FechaMod { get; set; }
        public bool Estado { get; set; }

        public static UserDTO Create(Usuario user)
        {
            UserDTO u = new UserDTO();
            u.Id = user.Id;
            u.EmpleadoId = user.EmpleadoId;
            u.Empleado = EmployeeUserDTO.Create(user.Empleado);
            u.NombreUsuario = user.NombreUsuario;
            u.Correo = user.Correo;
            u.Contrasena = user.Contrasena;
            u.Foto = user.Foto;
            u.FechaReg = user.FechaReg;
            u.FechaMod = user.FechaMod;
            u.Estado = user.Estado;

            return u;
        }
    }

    public class EmployeeUserDTO
    {
        public int Id { get; set; }
        public int? OrganigramaId { get; set; }
        public OChartDTO Organigrama { get; set; }
        public string NumeroDoc { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string Nombres { get; set; }
        public string Direccion { get; set; }
        public string Correos { get; set; }
        public bool EsJefe { get; set; }
        public bool Estado { get; set; }

        public static EmployeeUserDTO Create(Empleado employee)
        {
            EmployeeUserDTO e = new EmployeeUserDTO();
            e.Id = employee.Id;
            e.OrganigramaId = employee.OrganigramaId;
            e.Organigrama = OChartDTO.Create(employee.Organigrama);
            e.NumeroDoc = employee.NumeroDoc;
            e.ApellidoPaterno = employee.ApellidoPaterno;
            e.ApellidoMaterno = employee.ApellidoMaterno;
            e.Nombres = employee.Nombres;
            e.Direccion = employee.Direccion;
            e.Correos = employee.Correos;
            e.EsJefe = employee.EsJefe;
            e.Estado = employee.Estado;

            return e;
        }
    }
}
