using System;
using System.Linq;
using Domain.Models;

namespace Admin.Indexation
{
    public class ShiftWorkIvm
    {
        public const string indexUid = "shift-work";
        
        public static ShiftWorkIvm GetShiftWorkIvm(RolTurno shiftWork)
        {
            ShiftWorkIvm sw = new ShiftWorkIvm();
            var culture = new System.Globalization.CultureInfo("es-ES");
            var month = new DateTime(shiftWork.Anio, shiftWork.Mes, 1).ToString("MMMM", culture);
            sw.Id = shiftWork.Id.ToString();
            sw.Organigrama = shiftWork.Organigrama?.Denominacion;
            sw.Mes = char.ToUpper(month.First())+month.Substring(1);
            sw.Ano = shiftWork.Anio;
            sw.Estado = shiftWork.Estado;
            return sw;
        }
        
        public string Id { get; set; }
        public string Organigrama { get; set; }
        public string Mes { get; set; }
        public int Ano { get; set; }
        public string Estado { get; set; }
        public string Observacion { get; set; }
    }
}