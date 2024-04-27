using System.Collections.Generic;
using Domain.Models;

namespace Admin.Models
{
    public class OChartVm: Organigrama
    {
        public OChartVm()
        {
            
        }
        public OChartVm(Organigrama ochart, List<OChartVm> hijos = null)
        {
            Id = ochart.Id;
            PadreId = ochart.PadreId;
            Denominacion = ochart.Denominacion;
            Hijos = hijos ?? new List<OChartVm>();
        }

        public static OChartVm CreateOChartVm(Organigrama oChart)
        {
            if (oChart==null)
            {
                return null;
            }
            var oChartVm = new OChartVm();
            oChartVm.Id = oChart.Id;
            oChartVm.Denominacion = oChart.Denominacion;
            oChartVm.NivelId = oChart.Nivel.Id;
            oChartVm.PadreId = oChart.PadreId;
            oChartVm.FechaReg = oChart.FechaReg;
            oChartVm.FechaMod = oChart.FechaMod;
            oChartVm.Estado = oChart.Estado;
            return oChartVm;
        }
        public List<OChartVm> Hijos { get; set; }
        public List<int> Cargos { get; set; }

    }
}