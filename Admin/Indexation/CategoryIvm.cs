using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;

namespace Admin.Indexation
{
    public class CategoryIvm
    {
        public const string indexUid = "category";
        public static CategoryIvm GetCategoryIvm(Categoria category)
        {
            CategoryIvm c = new CategoryIvm();
            c.Id = category.Id.ToString();
            c.Organigrama = category.Organigrama?.Denominacion;
            c.Denominacion = category.Denominacion;
            c.Color = category.Color;
            return c;
        }

        public string Id { get; set; }
        public string Organigrama { get; set; }
        public string Denominacion { get; set; }
        public string Color { get; set; }
    }
}
