using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;

namespace Admin.DTO
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Denominacion { get; set; }
        public string Color { get; set; }

        public static CategoryDTO Create(Categoria category)
        {
            var u = new CategoryDTO();
            u.Id = category.Id;
            u.Denominacion = category.Denominacion;
            u.Color = category.Color;
            return u;
        }
    }
}
