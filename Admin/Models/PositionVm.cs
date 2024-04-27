using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Models
{
    public class PositionVm: Cargo
    {
        public static PositionVm Create(Cargo position)
        {
            if (position==null)
            {
                return null;
            }
            var p = new PositionVm();
            p.Id = position.Id;
            p.ClasificacionId = position.ClasificacionId;
            p.Denominacion = position.Denominacion;
            p.FechaReg = position.FechaReg;
            p.FechaMod = position.FechaMod;
            p.Estado = position.Estado;

            return p;
        }
        public int FreePosition { get; set; }
    }
}