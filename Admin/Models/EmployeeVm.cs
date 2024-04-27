using System.Collections.Generic;
using Domain.Models;

namespace Admin.Models
{
    public class EmployeeVm: Empleado
    {
        public static EmployeeVm CreateEmployeeVm(Empleado employee)
        {
            if (employee==null)
            {
                return null;
            }
            var e = new EmployeeVm();
            e.Id = employee.Id;
            e.OrganigramaId = employee.OrganigramaId;
            e.CargoId = employee.CargoId;
            e.TipoEmpleadoId = employee.TipoEmpleadoId;
            e.CondicionLaboralId = employee.CondicionLaboralId;
            e.ProfesionId = employee.ProfesionId;
            e.ColegioProfesionalId = employee.ColegioProfesionalId;
            e.NacionalidadId = employee.NacionalidadId;
            e.BancoId = employee.BancoId;
            e.TipoDocumentoId = employee.TipoDocumentoId;
            e.TipoCuentaId = employee.TipoCuentaId;
            e.EstadoCivilId = employee.EstadoCivilId;
            e.ApellidoPaterno = employee.ApellidoPaterno;
            e.ApellidoMaterno = employee.ApellidoMaterno;
            e.Nombres = employee.Nombres;
            e.ProfesionDenominacion = employee.Profesion?.Denominacion;
            e.Nacimiento = employee.Nacimiento;
            e.Sexo = employee.Sexo;
            e.FechaIngreso = employee.FechaIngreso;
            e.NumeroColegiatura = employee.NumeroColegiatura;
            e.Correos = employee.Correos;
            e.NumeroDoc = employee.NumeroDoc;
            e.Direccion = employee.Direccion;
            e.NumeroCuenta = employee.NumeroCuenta;
            e.CuentaInterbancaria = employee.CuentaInterbancaria;
            e.Telefonos = employee.Telefonos;
            e.FechaNombramiento = employee.FechaNombramiento;
            e.FechaReg = employee.FechaReg;
            e.FechaMod = employee.FechaMod;
            e.Estado = employee.Estado;

            return e;
        }
        
        public string ProfesionDenominacion { get; set; }
        public string CorreoUsuario { get; set; }
        public string Foto { get; set; }
        public List<int> Roles { get; set; }
    }
}