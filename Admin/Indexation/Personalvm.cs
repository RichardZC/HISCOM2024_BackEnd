using System.Collections.Generic;
using System.Linq;
using Domain.Models;

namespace Admin.Indexation
{
    public class PersonalIvm
    {
        public const string indexUid = "personal";
        
        public static PersonalIvm GetPersonalIvm(Empleado employee)
        {
            PersonalIvm e = new PersonalIvm();
            e.Id = employee.Id.ToString();
            e.NumeroDoc = employee.NumeroDoc;
            e.NombreCompleto = $"{employee.ApellidoPaterno} {employee.ApellidoMaterno} {employee.Nombres}";
            e.Correo = employee.Usuario.Correo;
            e.Organigrama = employee.Organigrama?.Denominacion;
            e.Cargo = employee.Cargo?.Denominacion;
            e.TipoEmpleado = employee.TipoEmpleado?.Denominacion;
            e.CondicionLaboral = employee.CondicionLaboral == null ? "" : employee.CondicionLaboral.Denominacion;
            e.Profesion = employee.Profesion?.Denominacion;
            e.ColegioProfesional = employee.ColegioProfesional == null ? "" : employee.ColegioProfesional.Denominacion;
            e.ColegioProfesionalNro = employee.NumeroColegiatura;
            e.FechaIngreso = employee.FechaIngreso.HasValue ? "" : employee.FechaIngreso?.ToShortDateString();
            e.Foto = employee.Usuario.Foto;
            return e;
        }
        public string Id { get; set; }
        public string NumeroDoc { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string Organigrama { get; set; }
        public string Cargo { get; set; }       
        public string TipoEmpleado { get; set; }                
        public string CondicionLaboral { get; set; } 
        public string Profesion { get; set; }
        public string ColegioProfesional { get; set; }
        public string ColegioProfesionalNro { get; set; }
        public string FechaIngreso { get; set; }
        public string Foto { get; set; }
    }
}