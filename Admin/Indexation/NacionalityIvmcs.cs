using Domain.Models;

namespace Admin.Indexation
{
    public class NacionalityIvm
    {
        public const string indexUid = "nacionality";
        public static NacionalityIvm GetNacionalityIvm(Nacionalidad nacionality)
        {
            NacionalityIvm n = new NacionalityIvm();
            n.Id = nacionality.Id.ToString();
            n.Pais = nacionality.Pais;
            n.Gentilicio = nacionality.Gentilicio;
            n.Abreviacion = nacionality.Abreviacion;
            return n;
        }

        public string Id { get; set; }
        public string Pais { get; set; }
        public string Gentilicio { get; set; }
        public string Abreviacion { get; set; }
    }
}