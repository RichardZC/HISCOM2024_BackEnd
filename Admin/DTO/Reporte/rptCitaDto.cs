using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Admin.DTO.Reporte
{
    public class rptCitaDto
    {
        public int IdCita { get; set; }
        public string Documento { get; set; }
        public string Paciente { get; set; }
        public string FechaCita { get; set; }
        public string HoraCita { get; set; }
        public string Servicio { get; set; }
        public string Profesional { get; set; }
        public string FuenteFinanciamiento { get; set; }
        public string Estado { get; set; }
    }
}
