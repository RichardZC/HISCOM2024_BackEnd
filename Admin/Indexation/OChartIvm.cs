using Domain.Models;

namespace Admin.Indexation
{
    public class OChartIvm
    {
        public const string indexUid = "organization-chart";
        
        public static OChartIvm GetOChartIvm(Organigrama oChart)
        {
            OChartIvm o = new OChartIvm();
            o.Id = oChart.Id.ToString();
            o.Nombre = oChart.Denominacion;
            return o;
        }
        
        public string Id { get; set; }
        public string Nombre { get; set; }
    }
}