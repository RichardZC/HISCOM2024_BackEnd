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
    public class TurnController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;

        public TurnController(HISCOMContext context, ElasticClient elastic)
        {
            _context = context;
            _elastic = elastic;
        }

        // GET: api/Turn/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Turno>> GetTurn(int id)
        {
            var turn = await _context.Turno.FindAsync(id);

            if (turn == null)
            {
                return NotFound();
            }

            return turn;
        }

        // PUT: api/Turn/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTurn(int id, Turno turn)
        {
            if (id != turn.Id)
            {
                return BadRequest();
            }

            turn.FechaMod = DateTime.Now;
            _context.Entry(turn).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            var turnEntity = await _context.Turno.Where(t => t.Id == turn.Id)
                .SingleOrDefaultAsync();
            
            var turnIvm = TurnIvm.GetTurnIvm(turnEntity);
            await _elastic.UpdateAsync<TurnIvm>(turnIvm.Id, u => 
                u.Index(TurnIvm.indexUid).Doc(turnIvm));

            return NoContent();
        }

        // POST: api/Turn
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Turno>> PostTurn(Turno turn)
        {
            var dbTurn = (from t in _context.Turno where t.Denominacion.Equals(turn.Denominacion) || t.Descripcion.Equals(turn.Descripcion) select t).SingleOrDefault();

            if (dbTurn != null) return BadRequest(new { error = "EL turno ya existe" });
            
            turn.FechaReg = DateTime.Now;
            turn.Estado = true;
            _context.Turno.Add(turn);
            await _context.SaveChangesAsync();

            var turnEntity = await _context.Turno.Where(t => t.Id == turn.Id)
                .SingleOrDefaultAsync();
            
            var turnIvm = TurnIvm.GetTurnIvm(turnEntity);
            await _elastic.CreateAsync(turnIvm,b=>b.Index(TurnIvm.indexUid));
            
            return CreatedAtAction("GetTurn", new { id = turn.Id }, turn);
        }

        // DELETE: api/Turn/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTurn(int id)
        {
            var turn = await _context.Turno.FindAsync(id);
            if (turn == null)
            {
                return NotFound();
            }

            _context.Turno.Remove(turn);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TurnExists(int id)
        {
            return _context.Turno.Any(e => e.Id == id);
        }
    }
}
