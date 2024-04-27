using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitasController : ControllerBase
    {
        private readonly HISCOMContext _context;

        public CitasController(HISCOMContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("consultar")]
        public async Task<ActionResult> Consultar(string id)
        {            
                var storedProc = "exec his.usp_ConsultarCita @DNI";
                var dniParameter = new SqlParameter("@DNI", id);

                var result = await _context.ConsultarCita.FromSqlRaw(storedProc, dniParameter).ToListAsync();
                return Ok(result);            
        }
    }
}
