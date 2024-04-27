using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Admin.DTO
{
    public class EstablishmentDTO
    {
        public int Id { get; set; }
        public string Denominacion { get; set; }

        public static EstablishmentDTO Create(Establecimiento estab)
        {
            if (estab == null)
            {
                return null;
            }

            return new EstablishmentDTO()
            {
                Id = estab.Id,
                Denominacion = estab.Denominacion
            };
        }
    }
}
