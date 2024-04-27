using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class Notificacion
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public string Mensaje { get; set; }
        public string Ruta { get; set; }
        public DateTime FechaReg { get; set; }
        public bool Estado { get; set; }
        public string Icono { get; set; }

        public virtual Empleado Empleado { get; set; }
    }
}
