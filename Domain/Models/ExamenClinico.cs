using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class ExamenClinico
    {
        public int Id { get; set; }
        public string DniPaciente { get; set; }
        public string CategoriaId { get; set; }
        public string ExamenPdf { get; set; }
    }
}
