using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Admin.Indexation;
using Algolia.Search.Clients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Lizelaser0310.Utilities;
using Nest;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LevelController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;

        public LevelController(HISCOMContext context, ElasticClient elastic)
        {
            _context = context;
            _elastic = elastic;
        }

        // GET: api/Level/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Nivel>> GetLevel(int id)
        {
            var level = await _context.Nivel.FindAsync(id);

            if (level == null)
            {
                return NotFound();
            }

            return level;
        }

        // PUT: api/Level/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLevel(int id, Nivel level)
        {
            if (id != level.Id)
            {
                return BadRequest();
            }

            _context.Entry(level).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            
            var levelEntity = await _context.Nivel.Where(n => n.Id == level.Id)
                .SingleOrDefaultAsync();
            
            var levelIvm = LevelIvm.GetLevelIvm(levelEntity);
            await _elastic.UpdateAsync<LevelIvm>(levelIvm.Id, u => 
                u.Index(LevelIvm.indexUid).Doc(levelIvm));

            return NoContent();
        }

        // POST: api/Level
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Nivel>> PostLevel(Nivel level)
        {
            _context.Nivel.Add(level);
            await _context.SaveChangesAsync();
            
            var levelEntity = await _context.Nivel.Where(n => n.Id == level.Id)
                .SingleOrDefaultAsync();
            
            var levelIvm = LevelIvm.GetLevelIvm(levelEntity);
            await _elastic.CreateAsync(levelIvm,b=>b.Index(LevelIvm.indexUid));

            return CreatedAtAction("GetLevel", new { id = level.Id }, level);
        }

        // DELETE: api/Level/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLevel(int id)
        {
            var level = await _context.Nivel.FindAsync(id);
            if (level == null)
            {
                return NotFound();
            }

            _context.Nivel.Remove(level);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LevelExists(int id)
        {
            return _context.Nivel.Any(e => e.Id == id);
        }
    }
}
