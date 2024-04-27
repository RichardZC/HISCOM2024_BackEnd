using System;
using System.Linq;
using System.Threading.Tasks;
using Admin.Indexation;
using Algolia.Search.Clients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Nest;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfessionController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;
        public ProfessionController(HISCOMContext context, ElasticClient elastic)
        {
            _context = context;
            _elastic = elastic;
        }
        // GET: api/profession/*
        [HttpGet("{id}")]
        public async Task<ActionResult<Profesion>> GetProfession(string id)
        {
            var profession = await _context.Profesion.FindAsync(id);

            if (profession == null)
            {
                return NotFound();
            }

            return profession;
        }

        // PUT: api/profession/*
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProfession(string id, Profesion profession)
        {
            if (id != profession.Id)
            {
                return BadRequest();
            }
            
            profession.FechaMod = DateTime.Now;
            _context.Entry(profession).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            var professionEntity = await _context.Profesion.Where(p => p.Id == profession.Id)
                .SingleOrDefaultAsync();
            
            var professionIvm = ProfessionIvm.GetProfessionIvm(professionEntity);
            await _elastic.UpdateAsync<ProfessionIvm>(professionIvm.Id, u => 
                u.Index(ProfessionIvm.indexUid).Doc(professionIvm));

            return NoContent();
        }

        // POST: api/profession
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Profesion>> PostProfession(Profesion profession)
        {
            var dbProfession = (from p in _context.Profesion where p.Denominacion.Equals(profession.Denominacion) select p).SingleOrDefault();

            if (dbProfession != null) return BadRequest(new { error = "La profesión ya existe" });
            
            profession.FechaReg = DateTime.Now;
            profession.Estado = true;
            _context.Profesion.Add(profession);
            await _context.SaveChangesAsync();
            
            var professionEntity = await _context.Profesion.Where(p => p.Id == profession.Id)
                .SingleOrDefaultAsync();
            
            var professionIvm = ProfessionIvm.GetProfessionIvm(professionEntity);
            await _elastic.CreateAsync(professionIvm,b=>b.Index(ProfessionIvm.indexUid));

            return CreatedAtAction("GetProfession", new { id = profession.Id }, profession);
        }

        // DELETE: api/profession/*
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfession(int id)
        {
            var profession = await _context.Profesion.FindAsync(id);
            if (profession == null)
            {
                return NotFound();
            }
            _context.Profesion.Remove(profession);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProfessionExists(string id)
        {
            return _context.Profesion.Any(e => e.Id == id);
        }
    }
}
