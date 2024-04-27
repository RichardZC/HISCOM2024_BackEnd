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
    [Route("api/professional-college")]
    [ApiController]
    public class ProfessionalCollegeController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;

        public ProfessionalCollegeController(HISCOMContext context, ElasticClient elastic)
        {
            _context = context;
            _elastic = elastic;
        }
        // GET: api/ProfessionalCollege/*
        [HttpGet("{id}")]
        public async Task<ActionResult<ColegioProfesional>> GetProfessionalCollege(int id)
        {
            var professionalCollege = await _context.ColegioProfesional.FindAsync(id);

            if (professionalCollege == null)
            {
                return NotFound();
            }

            return professionalCollege;
        }

        // PUT: api/ProfessionalCollege/*
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProfessionalCollege(int id, ColegioProfesional professionalCollege)
        {
            if (id != professionalCollege.Id)
            {
                return BadRequest();
            }

            professionalCollege.FechaMod = DateTime.Now;
            _context.Entry(professionalCollege).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            var professionalCollegeEntity = await _context.ColegioProfesional.Where(p => p.Id == professionalCollege.Id)
                .SingleOrDefaultAsync();
            
            var professionalCollegeIvm = ProfessionalCollegeIvm.GetProfessionalCollegeIvm(professionalCollegeEntity);
            await _elastic.UpdateAsync<ProfessionalCollegeIvm>(professionalCollegeIvm.Id, u => 
                u.Index(ProfessionalCollegeIvm.indexUid).Doc(professionalCollegeIvm));

            return NoContent();
        }

        // POST: api/ProfessionalCollege
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ColegioProfesional>> PostProfessionalCollege(ColegioProfesional professionalCollege)
        {
            var dbProfessionalCollege = (from p in _context.ColegioProfesional where p.Denominacion.Equals(professionalCollege.Denominacion) select p).SingleOrDefault();

            if (dbProfessionalCollege != null) return BadRequest(new { error = "El colegio profesional ya existe" });
            
            professionalCollege.FechaReg = DateTime.Now;
            professionalCollege.Estado = true;
            _context.ColegioProfesional.Add(professionalCollege);
            await _context.SaveChangesAsync();
            
            var professionalCollegeEntity = await _context.ColegioProfesional.Where(pc => pc.Id == professionalCollege.Id)
                .SingleOrDefaultAsync();
            
            var professionalCollegeIvm = ProfessionalCollegeIvm.GetProfessionalCollegeIvm(professionalCollegeEntity);
            await _elastic.CreateAsync(professionalCollegeIvm,b=>b.Index(ProfessionalCollegeIvm.indexUid));

            return CreatedAtAction("GetProfessionalCollege", new { id = professionalCollege.Id }, professionalCollege);
        }

        // DELETE: api/ProfessionalCollege/*
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfessionalCollege(int id)
        {
            var professionalCollege = await _context.ColegioProfesional.FindAsync(id);
            if (professionalCollege == null)
            {
                return NotFound();
            }

            _context.ColegioProfesional.Remove(professionalCollege);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProfessionalCollegeExists(int id)
        {
            return _context.ColegioProfesional.Any(e => e.Id == id);
        }
    }
}
