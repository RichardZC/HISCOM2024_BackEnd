using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Admin.Models
{
    public class PermissionVm
    {
        public PermissionVm(Permiso permission)
        {
            Id = permission.Id;
            MenuId = permission.MenuId;
            Nombre = permission.Nombre;
            Accion = permission.Accion;
            Ruta = permission.Ruta;
            Descripcion = permission.Descripcion;
            Visible = permission.Visible;
            MenuNombre = permission.Menu.Nombre;
            Hijos = new List<PermissionVm>();
        }

        public int Id { get; set; }
        public int? MenuId { get; set; }
        public string MenuNombre { get; set; }
        public string Accion { get; set; }
        public string Ruta { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Visible { get; set; }

        public List<PermissionVm> Hijos { get; set; }

    }
}
