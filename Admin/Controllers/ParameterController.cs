using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    [Route("api/[controller]")]
    [ApiController]
    public class ParameterController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;

        public ParameterController(HISCOMContext context, ElasticClient elastic)
        {
            _context = context;
            _elastic = elastic;
        }

        // GET: api/Parameter
        [HttpGet]
        public async Task<ActionResult> GetParameter()
        {
            return await PaginationUtility.ElasticSearchPaginate
            (
                query: Request.QueryString.Value,
                indexUid: ParameterIvm.indexUid,
                elastic: _elastic,
                primaryKey: p=>p.Id,
                fields: new List<Expression<Func<ParameterIvm,dynamic>>>
                {
                    p=>p.Llave,
                }
            );
        }

        // GET: api/Parameter/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Parametro>> GetParameter(int id)
        {
            var parameter = await _context.Parametro.FindAsync(id);

            if (parameter == null)
            {
                return NotFound();
            }

            return parameter;
        }

        // PUT: api/Parameter/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutParameter(int id, Parametro parameter)
        {
            if (id != parameter.Id)
            {
                return BadRequest();
            }

            parameter.FechaMod = DateTime.Now;
            _context.Entry(parameter).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            var parameterEntity = await _context.Parametro
                .Where(t => t.Id == parameter.Id)
                .SingleOrDefaultAsync();
            
            var parameterIvm = ParameterIvm.GetParameterIvm(parameterEntity);
            await _elastic.UpdateAsync<ParameterIvm>(parameterIvm.Id, u => 
                u.Index(ParameterIvm.indexUid).Doc(parameterIvm));

            return NoContent();
        }

        // POST: api/Parameter
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Parametro>> PostParameter(Parametro parameter)
        {
            var dbParameter = await _context.Parametro
                .SingleOrDefaultAsync(p=>p.Llave.Equals(parameter.Llave));

            if (dbParameter != null) return BadRequest(new { error = "EL parámetro ya existe" });
            
            parameter.FechaReg = DateTime.Now;
            parameter.Estado = true;
            _context.Parametro.Add(parameter);
            await _context.SaveChangesAsync();

            var parameterEntity = await _context.Parametro.Where(p => p.Id == parameter.Id)
                .SingleOrDefaultAsync();
            
            var parameterIvm = ParameterIvm.GetParameterIvm(parameterEntity);
            await _elastic.CreateAsync(parameterIvm,b=>b.Index(ParameterIvm.indexUid));
            
            return CreatedAtAction("GetParameter", new { id = parameter.Id }, parameter);
        }

        // DELETE: api/Parameter/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParameter(int id)
        {
            var parameter= await _context.Parametro.FindAsync(id);
            if (parameter == null)
            {
                return NotFound();
            }

            _context.Parametro.Remove(parameter);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ParameterExists(int id)
        {
            return _context.Parametro.Any(e => e.Id == id);
        }
    }
}
