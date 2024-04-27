using Domain.Models;

namespace Admin.Indexation
{
    public class LevelIvm
    {
        public const string indexUid = "level";
        
        public static LevelIvm GetLevelIvm(Nivel level)
        {
            LevelIvm l = new LevelIvm();
            l.Id = level.Id.ToString();
            l.Numero = level.Numero;
            l.Denominacion = level.Denominacion;
            return l;
        }
        
        public string Id { get; set; }
        public int Numero { get; set; }
        public string Denominacion { get; set; }
    }
}