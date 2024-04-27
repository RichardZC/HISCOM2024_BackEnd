using Domain.Models;

namespace Admin.Indexation
{
    public class ProfessionalCollegeIvm
    {
        public const string indexUid = "professional-college";
        
        public static ProfessionalCollegeIvm GetProfessionalCollegeIvm(ColegioProfesional professionalCollege)
        {
            ProfessionalCollegeIvm pc = new ProfessionalCollegeIvm();
            pc.Id = professionalCollege.Id.ToString();
            pc.Denominacion = professionalCollege.Denominacion;
            pc.Decano = professionalCollege.Decano;
            pc.Direccion = professionalCollege.Direccion;
            pc.Telefonos = professionalCollege.Telefonos;
            pc.SitioWeb = professionalCollege.SitioWeb;
            return pc;
        }
        
        public string Id { get; set; }
        public string Denominacion { get; set; }
        public string Decano { get; set; }
        public string Direccion { get; set; }
        public string Telefonos { get; set; }
        public string SitioWeb { get; set; }

    }
}