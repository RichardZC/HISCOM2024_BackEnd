using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Admin.DTO
{
    public class EmployeeDTO
    {
        public int Id { get; set; }
        public string NumeroDoc { get; set; }
        public string NombreCompleto { get; set; }
        public LaboralConditionDTO CondicionLaboral { get; set; }
        public string Cargo { get; set; }
        public string Profesion { get; set; }
        public int? CategoriaId { get; set; }
        public string Categoria { get; set; }

        public static EmployeeDTO Create(Empleado emp, int? categoryId=null, string category=null)
        {
            return new EmployeeDTO()
            {
                Id = emp.Id,
                NumeroDoc = emp.NumeroDoc,
                NombreCompleto = emp.ApellidoPaterno + " " + emp.ApellidoMaterno + " " +  emp.Nombres,
                CondicionLaboral = emp.CondicionLaboral != null ? LaboralConditionDTO.Create(emp.CondicionLaboral) : null,
                Cargo = emp.Cargo?.Denominacion,
                Profesion = emp.Profesion?.Denominacion,
                CategoriaId = categoryId,
                Categoria = category?? "Sin Categoría"
            };
        }
    }

    public class CategoryEmployeeDTO
    {
        public int Id { get; set; }
        public string NumeroDoc { get; set; }
        public string NombreCompleto { get; set; }
        public string CondicionLaboral { get; set; }
        public string Cargo { get; set; }
        public string Profesion { get; set; }
        public int? CategoriaId { get; set; }
        public string Categoria { get; set; }

        public static CategoryEmployeeDTO Create(Empleado emp, int? categoriaId, string category = null)
        {
            return new()
            {
                Id = emp.Id,
                NumeroDoc = emp.NumeroDoc,
                NombreCompleto = emp.ApellidoPaterno + " " + emp.ApellidoMaterno + " " + emp.Nombres,
                CondicionLaboral = emp.CondicionLaboral?.Denominacion,
                Cargo = emp.Cargo?.Denominacion,
                Profesion = emp.Profesion?.Denominacion,
                CategoriaId = categoriaId,
                Categoria = category ?? "Sin Categoría"
            };
        }
    }

    class CompareCategoryEmployeeDTO : IEqualityComparer<CategoryEmployeeDTO>
    {
        public bool Equals(CategoryEmployeeDTO x, CategoryEmployeeDTO y)
        {
            return x.Id == y.Id;
        }
        public int GetHashCode(CategoryEmployeeDTO codeh)
        {
            return codeh.Id.GetHashCode();
        }
    }

    class CompareEmployeeDTO : IEqualityComparer<EmployeeDTO>
    {
        public bool Equals(EmployeeDTO x, EmployeeDTO y)
        {
            return x.Id == y.Id;
        }
        public int GetHashCode(EmployeeDTO codeh)
        {
            return codeh.Id.GetHashCode();
        }
    }

    public class LaboralConditionDTO
    {
        public string Denominacion { get; set; }
        public int TotalHoras { get; set; }

        public static LaboralConditionDTO Create(CondicionLaboral cl)
        {
            return new LaboralConditionDTO()
            {
                Denominacion = cl.Denominacion,
                TotalHoras = cl.TotalHoras
            };
        }
    }

    public class ChargeDTO
    {
        public string Denominacion { get; set; }
    }

    public class ProfesionDTO
    {
        public string Denominacion { get; set; }
    }
}
