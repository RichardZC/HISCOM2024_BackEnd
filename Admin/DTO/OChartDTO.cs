using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;

namespace Admin.DTO
{
    public class OChartDTO
    {
        public int Id { get; set; }
        public string Denominacion { get; set; }

        public static OChartDTO Create(Organigrama ochart)
        {
            if (ochart == null)
            {
                return null;
            }

            OChartDTO o = new OChartDTO();
            o.Id = ochart.Id;
            o.Denominacion = ochart.Denominacion;
            return o;
        }
    }
}
