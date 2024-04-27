using Domain.Models;

namespace Admin.Indexation
{
    public class PermissionIvm
    {
        public const string indexUid = "permission";
        public static PermissionIvm GetPermissionIvm(Permiso permission)
        {
            PermissionIvm p = new PermissionIvm();
            p.Id = permission.Id.ToString();
            p.Menu = permission.Menu.Nombre;
            p.Accion = permission.Accion;
            p.Ruta = permission.Ruta;
            p.Nombre = permission.Nombre;
            p.Visible = permission.Visible;
            return p;
        }
        public string Id { get; set; }
        public string Menu { get; set; }
        public string Accion { get; set; }
        public string Ruta { get; set; }
        public string Nombre { get; set; }
        public bool Visible { get; set; }
    }
}