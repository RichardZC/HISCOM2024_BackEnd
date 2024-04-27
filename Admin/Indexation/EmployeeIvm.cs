using System.Collections.Generic;
using System.Linq;
using Domain.Models;

namespace Admin.Indexation
{
    public class EmployeeIvm
    {
        public const string indexUid = "employee";
        
        public static EmployeeIvm GetEmployeeIvm(Empleado employee)
        {
            EmployeeIvm e = new EmployeeIvm();
            e.Id = employee.Id.ToString();
            e.Organigrama = employee.Organigrama?.Denominacion;
            e.Cargo = employee.Cargo?.Denominacion;
            e.NombreCompleto = $"{employee.ApellidoPaterno} {employee.ApellidoMaterno} {employee.Nombres}";
            e.NumeroDoc = employee.NumeroDoc;

            return e;
        }
        public string Id { get; set; }
        public string TipoEmpleado { get; set; }
        public string Organigrama { get; set; }
        public string Cargo { get; set; }
        public string TipoDocumento { get; set; }
        public string TipoContrato { get; set; }
        public string TipoCuenta { get; set; }
        public string NombreCompleto { get; set; }
        public List<string> Correos { get; set; }
        public string Direccion { get; set; }
        public string NumeroDoc { get; set; }
        public string EstadoCivil { get; set; }
        public string NroCuenta { get; set; }
        public string Cci { get; set; }
        public string NumeroCelular { get; set; }
    }
}