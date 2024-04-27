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
    [Route("api/working-condition")]
    [ApiController]
    public class ContractTypeController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;

        public ContractTypeController(HISCOMContext context, ElasticClient elastic)
        {
            _context = context;
            _elastic = elastic;
        }

        // GET: api/working-condition/*
        [HttpGet("{id}")]
        public async Task<ActionResult<CondicionLaboral>> GetWorkingCondition(string id)
        {
            var contractType = await _context.CondicionLaboral.FindAsync(id);

            if (contractType == null)
            {
                return NotFound();
            }

            return contractType;
        }

        // PUT: api/working-condition/*
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutContractType(string id, CondicionLaboral workingCondition)
        {
            if (id != workingCondition.Id)
            {
                return BadRequest();
            }
            
            workingCondition.FechaMod = DateTime.Now;
            _context.Entry(workingCondition).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            var contractTypeEntity = await _context.CondicionLaboral.Where(ct => ct.Id == workingCondition.Id)
                .SingleOrDefaultAsync();
            
            var contractTypeIvm = WorkingConditionIvm.GetWorkingConditionIvm(contractTypeEntity);
            await _elastic.UpdateAsync<WorkingConditionIvm>(contractTypeIvm.Id, u => 
                u.Index(WorkingConditionIvm.indexUid).Doc(contractTypeIvm));

            return NoContent();
        }

        // POST: api/working-condition
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CondicionLaboral>> PostContractType(CondicionLaboral workingCondition)
        {
            var dbWorkingCondition = (from ct in _context.CondicionLaboral where ct.Denominacion.Equals(workingCondition.Denominacion) select ct).SingleOrDefault();

            if (dbWorkingCondition != null) return BadRequest(new { error = "El tipo de contrato ya existe" });
            
            workingCondition.FechaReg = DateTime.Now;
            workingCondition.Estado = true;
            _context.CondicionLaboral.Add(workingCondition);
            await _context.SaveChangesAsync();
            
            var contractTypeEntity = await _context.CondicionLaboral.Where(u => u.Id == workingCondition.Id)
                .SingleOrDefaultAsync();
            
            var contractTypeIvm = WorkingConditionIvm.GetWorkingConditionIvm(contractTypeEntity);
            await _elastic.CreateAsync(contractTypeIvm,b=>b.Index(WorkingConditionIvm.indexUid));

            return CreatedAtAction("GetWorkingCondition", new { id = workingCondition.Id }, workingCondition);
        }

        // DELETE: api/working-condition/*
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContractType(int id)
        {
            var contractType = await _context.CondicionLaboral.FindAsync(id);
            if (contractType == null)
            {
                return NotFound();
            }

            _context.CondicionLaboral.Remove(contractType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ContractTypeExists(string id)
        {
            return _context.CondicionLaboral.Any(e => e.Id == id);
        }
    }
}
