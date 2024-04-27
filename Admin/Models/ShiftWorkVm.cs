using System.Collections.Generic;
using Domain.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Admin.Models
{
    public enum TipoRolTurnoEnum
    {
        Regular, 
        Extraordinario,
        Complementario
    }
    
    public enum EstadoEnum
    {
        Enviado, 
        Observado,
        Aprobado,
        Eliminado,
        Creado
    }

    public enum TipoImpresionEnum
    {
        Normal,
        Consolidado
    }
    public class ShiftWorkVm
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public TipoRolTurnoEnum TipoRolTurno { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public EstadoEnum Estado { get; set; }
        public bool MesSiguiente { get; set; }
        public List<ShiftWorkEstabVm> EstabDetalles { get; set; }
        
    }

    public class ShiftWorkEstabVm
    {
        public int EstablecimientoId { get; set; }
        public List<RolTurnoDetalle> Detalles { get; set; }
    }

    public class PutShiftWorkVm
    {
        public int Id { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public EstadoEnum Estado { get; set; }
        public string Observacion { get; set; }
    }
}