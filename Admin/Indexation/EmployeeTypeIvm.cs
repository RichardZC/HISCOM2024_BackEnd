using Domain.Models;

namespace Admin.Indexation
{
    public class EmployeeTypeIvm
    {
        public const string indexUid = "employee-type";
        
        public static EmployeeTypeIvm GetEmployeeTypeIvm(TipoEmpleado employeeType)
        {
            EmployeeTypeIvm et = new EmployeeTypeIvm();
            et.Id = employeeType.Id.ToString();
            et.Denominacion = employeeType.Denominacion;
            return et;
        }
        
        public string Id { get; set; }
        public string Denominacion { get; set; }
    }
}