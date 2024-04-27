using System.Collections.Generic;
using Domain.Models;

namespace Admin.Models
{
    public class RoleVm: Rol
    {
        public List<int> Permisos { get; set; }
    }
}