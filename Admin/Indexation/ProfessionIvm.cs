using Domain.Models;

namespace Admin.Indexation
{
    public class ProfessionIvm
    {
        public const string indexUid = "profession";
        
        public static ProfessionIvm GetProfessionIvm(Profesion profession)
        {
            ProfessionIvm p = new ProfessionIvm();
            p.Id = profession.Id;
            p.Abreviacion = profession.Abreviacion;
            p.Denominacion = profession.Denominacion;
            return p;
        }
        
        public string Id { get; set; }
        public string Abreviacion { get; set; }
        public string Denominacion { get; set; }
        
    }
}