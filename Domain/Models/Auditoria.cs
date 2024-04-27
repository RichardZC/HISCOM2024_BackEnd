using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class Auditoria
    {
        public long Id { get; set; }
        public int UsuarioId { get; set; }
        public string Controlador { get; set; }
        public string Accion { get; set; }
        public DateTime Fecha { get; set; }
        public int Duracion { get; set; }
        public string DireccionIp { get; set; }
        public string Navegador { get; set; }

        public virtual Usuario Usuario { get; set; }
    }
}
