using System.Collections.Generic;
using Domain.Models;

namespace Admin.Models
{
    public class SubmenuVm
    {
        public string Nombre { get; set; }
        public List<Permiso> Permisos { get; set; }
    }
    public class MenuPVm
    {
        public string Nombre { get; set; }
        public string Icono { get; set; }
        public List<SubmenuVm> Submenus { get; set; }
    }
}