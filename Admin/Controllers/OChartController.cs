using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Admin.Indexation;
using Admin.Models;
using Algolia.Search.Clients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Lizelaser0310.Utilities;
using Nest;

namespace Admin.Controllers
{
    [Route("api/organization-chart")]
    [ApiController]
    public class OChartController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;

        public OChartController(HISCOMContext context, ElasticClient elastic)
        {
            _context = context;
            _elastic = elastic;
        }

        // GET: api/oChart/*
        [HttpGet("{id}")]
        public async Task<ActionResult<Organigrama>> GetOChart(int id)
        {
            var oChart = await _context.Organigrama
                .Include(o=>o.Nivel)
                .SingleOrDefaultAsync(o=>o.Id==id);
            var oChartVm = OChartVm.CreateOChartVm(oChart);

            return oChartVm;
        }

        // PUT: api/oChart/*
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOChart(int id, OChartVm oChart)
        {
            if (id != oChart.Id)
            {
                return BadRequest();
            }
            
            oChart.FechaMod = DateTime.Now;
            _context.Entry(oChart).State = EntityState.Modified;
            
            await _context.SaveChangesAsync();
            
            var oChartEntity = await _context.Organigrama.Where(l => l.Id == oChart.Id)
                .SingleOrDefaultAsync();
            
            var oChartIvm = OChartIvm.GetOChartIvm(oChartEntity);
            await _elastic.UpdateAsync<OChartIvm>(oChartIvm.Id, u => 
                u.Index(OChartIvm.indexUid).Doc(oChartIvm));

            return NoContent();
        }

        // POST: api/oChart
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Organigrama>> PostOChart(OChartVm oChart)
        {
            var dbOChart = (from o in _context.Organigrama where o.Denominacion.Equals(oChart.Denominacion) select o).SingleOrDefault();

            if (dbOChart != null) return BadRequest(new { error = "Esta estructura orgánica ya existe" });
            
            oChart.FechaReg = DateTime.Now;
            oChart.Estado = true;
            _context.Organigrama.Add(oChart);
            await _context.SaveChangesAsync();
            
            
            var oChartEntity = await _context.Organigrama.Where(o => o.Id == oChart.Id)
                .SingleOrDefaultAsync();
            
            var oChartIvm = OChartIvm.GetOChartIvm(oChartEntity);
            await _elastic.CreateAsync(oChartIvm,b=>b.Index(OChartIvm.indexUid));

            return CreatedAtAction("GetOChart", new { id = oChart.Id }, oChart);
        }

        // DELETE: api/oChart/*
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOChart(int id)
        {
            var oChart = await _context.Organigrama.FindAsync(id);
            if (oChart == null)
            {
                return NotFound();
            }
            _context.Organigrama.Remove(oChart);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OChartExists(int id)
        {
            return _context.Organigrama.Any(e => e.Id == id);
        }
    }
}
