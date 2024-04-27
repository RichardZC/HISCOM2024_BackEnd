using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain.Models;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlhConceptoController : ControllerBase
    {
        private readonly HISCOMContext _context;

        public PlhConceptoController(HISCOMContext context)
        {
            _context = context;
        }

        // GET: api/PlhConcepto
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlhConcepto>>> GetPlhConcepto()
        {
            return await _context.PlhConcepto.ToListAsync();
        }

        // GET: api/PlhConcepto/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PlhConcepto>> GetPlhConcepto(int id)
        {
            var plhConcepto = await _context.PlhConcepto.FindAsync(id);

            if (plhConcepto == null)
            {
                return NotFound();
            }

            return plhConcepto;
        }

        // PUT: api/PlhConcepto/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlhConcepto(int id, PlhConcepto plhConcepto)
        {
            if (id != plhConcepto.Id)
            {
                return BadRequest();
            }

            _context.Entry(plhConcepto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlhConceptoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/PlhConcepto
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PlhConcepto>> PostPlhConcepto(PlhConcepto plhConcepto)
        {
            _context.PlhConcepto.Add(plhConcepto);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPlhConcepto", new { id = plhConcepto.Id }, plhConcepto);
        }

        // DELETE: api/PlhConcepto/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlhConcepto(int id)
        {
            var plhConcepto = await _context.PlhConcepto.FindAsync(id);
            if (plhConcepto == null)
            {
                return NotFound();
            }

            _context.PlhConcepto.Remove(plhConcepto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PlhConceptoExists(int id)
        {
            return _context.PlhConcepto.Any(e => e.Id == id);
        }
    }
}
