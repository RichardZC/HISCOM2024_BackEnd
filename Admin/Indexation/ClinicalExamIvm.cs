using System.Collections.Generic;
using System.Linq;
using Domain.Models;

namespace Admin.Indexation
{
    public class ClinicalExamIvm
    {
        public const string indexUid = "clinical-exam";
        
        public static ClinicalExamIvm GetClinicalExamIvm(ExamenClinico exam)
        {
            return new()
            {
                Id = exam.Id.ToString(),
                Dni = exam.DniPaciente,
                Categoria = exam.CategoriaId,
                Examenes = exam.ExamenPdf
                    .Split(",")
                    .Select(x => x.Trim())
                    .ToList()
            };
        }
        
        public string Id { get; set; }
        public string Dni { get; set; }
        public string Categoria { get; set; }
        public List<string> Examenes { get; set; }
    }
}