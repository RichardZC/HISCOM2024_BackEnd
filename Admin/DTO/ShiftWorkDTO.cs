using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Admin.DTO
{
    public class ShiftWorkDTO
    {
        public int Id { get; set; }
        public string EstructuraOrganica { get; set; }
        public int Mes { get; set; }
        public int Ano { get; set; }
        public string Responsable { get; set; }
        public DateTime FechaReg { get; set; }
        public DateTime? FechaMod { get; set; }
        public string Estado { get; set; }
        public string Observacion { get; set; }

        public static ShiftWorkDTO Create(RolTurno rt)
        {
            if (rt==null)
            {
                return null;
            }

            return new ShiftWorkDTO() {
                Id = rt.Id,
                EstructuraOrganica = rt.Organigrama?.Denominacion,
                Mes = rt.Mes,
                Ano = rt.Anio,
                Responsable = rt.Jefe!=null?rt.Jefe.ApellidoPaterno + " " + rt.Jefe.ApellidoMaterno + " " + rt.Jefe.Nombres:null,
                FechaReg = rt.FechaReg,
                FechaMod = rt.FechaMod,
                Estado = rt.Estado,
                Observacion = rt.Observacion
            };
        }
    }

    public class ShiftWorkEstabDTO
    {
        public int Id { get; set; }
        public int RolTurnoId { get; set; }
        public int EstablecimientoId { get; set; }
        public EstablishmentDTO Establecimiento { get; set; }
        public List<ShiftWorkDetailDTO> RolTurnoDetalle { get; set; }


        public static ShiftWorkEstabDTO Create(RolTurnoEstab rte)
        {
            return new ShiftWorkEstabDTO()
            {
                Id = rte.Id,
                RolTurnoId = rte.RolTurnoId,
                EstablecimientoId = rte.EstablecimientoId,
                Establecimiento = EstablishmentDTO.Create(rte.Establecimiento),
                RolTurnoDetalle = rte.RolTurnoDetalle?.Select(ShiftWorkDetailDTO.Create).ToList()
            };
        }
    }

    public class ShiftWorkDetailDTO
    {
        public int RolTurnoEstabId { get; set; }
        public int EmpleadoId { get; set; }
        public int TurnoId { get; set; }
        public int Dia { get; set; }

        public static ShiftWorkDetailDTO Create(RolTurnoDetalle rtd)
        {
            return new ShiftWorkDetailDTO()
            {
                RolTurnoEstabId = rtd.RolTurnoEstabId,
                EmpleadoId = rtd.EmpleadoId,
                TurnoId = rtd.TurnoId,
                Dia = rtd.Dia
            };
        }
    }
}
