using Domain.Models;

namespace Admin.Indexation
{
    public class RoleIvm
    {
        public const string indexUid = "role";
        
        public static RoleIvm GetRoleIvm(Rol role)
        {
            RoleIvm r = new RoleIvm();
            r.Id = role.Id.ToString();
            r.Denominacion = role.Denominacion +  " - " + role.Descripcion;
            return r;
        }
        
        public string Id { get; set; }
        public string Denominacion { get; set; }
        public string Descripcion { get; set; }
        
    }
}