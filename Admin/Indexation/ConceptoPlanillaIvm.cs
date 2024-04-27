using Domain.Models;

namespace Admin.Indexation
{
    public class ConceptoPlanillaIvm
    {
        public const string indexUid = "concepto-planilla";
        
        public static ConceptoPlanillaIvm GetConceptoPlanillaIvm(PlhConcepto concepto)
        {
            ConceptoPlanillaIvm et = new ConceptoPlanillaIvm();
            et.Id = concepto.Id.ToString();
            et.Codigo = concepto.Codigo;
            et.Abreviado = concepto.Abreviado;
            et.Denominacion = concepto.Denominacion;
            et.Tipo = concepto.IndNombrado ? "NOM" : "CON";
            return et;
        }
        
        public string Id { get; set; }
        public string Codigo { get; set; }
        public string Abreviado { get; set; }
        public string Denominacion { get; set; }
        public string Tipo { get; set; }
    }
}