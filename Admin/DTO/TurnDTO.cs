using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Admin.DTO
{
    public class TurnDTO
    {
        public int Id { get; set; }
        public string Denominacion { get; set; }
        public string Descripcion { get; set; }
        public TimeSpan? HoraInicio { get; set; }
        public int Horas { get; set; }

        public static TurnDTO Create(Turno turn)
        {
            return new TurnDTO()
            {
                Id = turn.Id,
                Denominacion = turn.Denominacion,
                Descripcion = turn.Descripcion,
                HoraInicio = turn.HoraInicio,
                Horas = turn.Horas
            };
        }
    }
}
