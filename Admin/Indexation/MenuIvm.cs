using Domain.Models;

namespace Admin.Indexation
{
    public class MenuIvm
    {
        public const string indexUid = "menu";
        public static MenuIvm GetMenuIvm(Menu menu)
        {
            MenuIvm m = new MenuIvm();
            m.Id = menu.Id.ToString();
            m.Nombre = menu.Nombre;
            m.Icono = menu.Icono;
            return m;
        }
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Icono { get; set; }
    }
}