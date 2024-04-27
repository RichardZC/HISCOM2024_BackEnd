using System;
using System.Linq;
using System.Threading.Tasks;
using Admin.Indexation;
using Algolia.Search.Clients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Lizelaser0310.Utilities;
using Nest;

namespace Admin.Controllers
{
    [Route("api/nacionality")]
    [ApiController]
    public class NacionalityController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;

        public NacionalityController(HISCOMContext context, ElasticClient elastic)
        {
            _context = context;
            _elastic = elastic;
        }

        // GET: api/nacionality/*
        [HttpGet("{id}")]
        public async Task<ActionResult<Nacionalidad>> GetNacionality(int id)
        {
            var nacionality = await _context.Nacionalidad.FindAsync(id);

            if (nacionality == null)
            {
                return NotFound();
            }

            return nacionality;
        }

        // PUT: api/nacionality/*
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNacionality(int id, Nacionalidad nacionality)
        {
            if (id != nacionality.Id)
            {
                return BadRequest();
            }

            //nacionality.FechaMod = DateTime.Now;
            _context.Entry(nacionality).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var nationalidadEntity = await _context.Nacionalidad.Where(te => te.Id == nacionality.Id)
                .SingleOrDefaultAsync();

            var nationalityIvm = NacionalityIvm.GetNacionalityIvm(nationalidadEntity);
            await _elastic.UpdateAsync<NacionalityIvm>(nationalityIvm.Id, u => 
                u.Index(NacionalityIvm.indexUid).Doc(nationalityIvm));

            return NoContent();
        }

        // POST: api/nacionality
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Nacionalidad>> PostNacionality(Nacionalidad nacionality)
        {

            var dbnacionalidad = (from te in _context.Nacionalidad where te.Pais.Equals(nacionality.Pais) select te).SingleOrDefault();

            if (dbnacionalidad != null) return BadRequest(new { error = "EL tipo de empleado ya existe" });

            // nacionality.FechaReg = DateTime.Now;
            //nacionality.Estado = true;
            _context.Nacionalidad.Add(nacionality);
            await _context.SaveChangesAsync();

            var nacionalidadEntity = await _context.Nacionalidad.Where(te => te.Id == nacionality.Id)
                .SingleOrDefaultAsync();

            var nacionalityIvm = NacionalityIvm.GetNacionalityIvm(nacionalidadEntity);
            await _elastic.CreateAsync(nacionalityIvm,b=>b.Index(NacionalityIvm.indexUid));

            return CreatedAtAction("GetNacionality", new { id = nacionality.Id }, nacionality);
        }

        // DELETE: api/nacionality/*
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNacionality(int id)
        {
            var nacionalidad = await _context.Nacionalidad.FindAsync(id);
            if (nacionalidad == null)
            {
                return NotFound();
            }
            _context.Nacionalidad.Remove(nacionalidad);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool nacionalidadExist(int id)
        {
            return _context.Nacionalidad.Any(e => e.Id == id);
        }
    }
}
