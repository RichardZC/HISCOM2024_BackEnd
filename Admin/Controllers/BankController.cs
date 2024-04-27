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
    [Route("api/bank")]
    [ApiController]
    public class BankController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;

        public BankController(HISCOMContext context, ElasticClient elastic)
        {
            _context = context;
            _elastic = elastic;
        }

        // GET: api/banco/*
        [HttpGet("{id}")]
        public async Task<ActionResult<Banco>> GetBank(int id)
        {
            var bank = await _context.Banco.FindAsync(id);

            if (bank == null)
            {
                return NotFound();
            }

            return bank;
        }

        // PUT: api/banco/*
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBank(int id, Banco bank)
        {
            if (id != bank.Id)
            {
                return BadRequest();
            }
            
            //banco.FechaMod = DateTime.Now;
            _context.Entry(bank).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            var bancoEntity = await _context.Banco.Where(te => te.Id == bank.Id)
                .SingleOrDefaultAsync();
            
            var bancoIvm = BankIvm.GetBankIvm(bancoEntity);
            await _elastic.UpdateAsync<BankIvm>(bancoIvm.Id, u => 
                u.Index(BankIvm.indexUid).Doc(bancoIvm));
            
            return NoContent();
        }

        // POST: api/banco
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Banco>> PostBank(Banco bank)
        {

            var dbbanco = (from te in _context.Banco where te.Nombre.Equals(bank.Nombre) select te).SingleOrDefault();

            if (dbbanco != null) return BadRequest(new { error = "EL tipo de empleado ya existe" });
            
           // banco.FechaReg = DateTime.Now;
           //banco.Estado = true;
            _context.Banco.Add(bank);
            await _context.SaveChangesAsync();
            
            var bancoEntity = await _context.Banco.Where(te => te.Id == bank.Id)
                .SingleOrDefaultAsync();
            
            var bancoIvm = BankIvm.GetBankIvm(bancoEntity);
            await _elastic.CreateAsync(bancoIvm,b=>b.Index(BankIvm.indexUid));

            return CreatedAtAction("GetBank", new { id = bank.Id }, bank);
        }

        // DELETE: api/banco/*
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBank(int id)
        {
            var banco = await _context.Banco.FindAsync(id);
            if (banco == null)
            {
                return NotFound();
            }
            _context.Banco.Remove(banco);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool bancoExists(int id)
        {
            return _context.Banco.Any(e => e.Id == id);
        }
    }
}
