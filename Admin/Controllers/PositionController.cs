using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    [Route("api/[controller]")]
    [ApiController]
    public class PositionController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;

        public PositionController(HISCOMContext context, ElasticClient elastic)
        {
            _context = context;
            _elastic = elastic;
        }
        
        [HttpGet]
        public async Task<ActionResult> GetPosition()
        {
            return await PaginationUtility.ElasticSearchPaginate
            (
                query: Request.QueryString.Value,
                indexUid: PositionIvm.indexUid,
                elastic: _elastic,
                primaryKey: p=>p.Id,
                fields: new List<Expression<Func<PositionIvm,dynamic>>>
                {
                    e=>e.Denominacion
                }
            );
        }


        // GET: api/position/*
        [HttpGet("{id}")]
        public async Task<ActionResult<Cargo>> GetPosition(string id)
        {
            var position = await _context.Cargo.FindAsync(id);

            if (position == null)
            {
                return NotFound();
            }

            return position;
        }

        // PUT: api/position/*
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPosition(string id, Cargo position)
        {
            if (id != position.Id)
            {
                return BadRequest();
            }
            /*var error = await ValidatePayload(position);
            if (error != null)
            {
                return BadRequest(error);
            }*/
            
            position.FechaMod = DateTime.Now;
            _context.Entry(position).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            var positionEntity = await _context.Cargo.Where(p => p.Id == position.Id)
                .SingleOrDefaultAsync();
            
            var positionIvm = PositionIvm.GetPositionIvm(positionEntity);
            await _elastic.UpdateAsync<PositionIvm>(positionIvm.Id, u => 
                u.Index(PositionIvm.indexUid).Doc(positionIvm));

            return NoContent();
        }

        // POST: api/position
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Cargo>> PostPosition(Cargo position)
        {
            /*var error = await ValidatePayload(position);
            if (error != null)
            {
                return BadRequest(error);
            }*/

            position.FechaReg = DateTime.Now;
            position.Estado = true;
            _context.Cargo.Add(position);
            await _context.SaveChangesAsync();
            
            var positionEntity = await _context.Cargo.Where(p => p.Id == position.Id)
                .SingleOrDefaultAsync();
            
            var positionIvm = PositionIvm.GetPositionIvm(positionEntity);
            await _elastic.CreateAsync(positionIvm,b=>b.Index(PositionIvm.indexUid));

            return CreatedAtAction("GetPosition", new { id = position.Id }, position);
        }

        /*public ErrorVm ValidatePayload(Cargo position)
        {
            ErrorVm result = new ErrorVm();


            if (position.Total < 1)
            {
                result.AddMessage(
                    ()=>position.Total,
                    "El total de puestos del cargo debe ser un número entero positivo");
            }
            
            /*if (position.EsEncargado)
            {
                if (structure!=null && structure.Cargo.Any(c=>c.EsEncargado))
                {
                    result.AddMessage(
                        ()=>position.EsEncargado,
                        "Ya existe un cargo jefe asignado a la unidad orgánica");
                }
                if (position.Total>1)
                {
                    result.AddMessage(
                        ()=>position.Total,
                        "El jefe de la unidad orgánica no debe tener mas de un puesto");
                }
            }

            return result.IsEmpty()?null:result;
        }*/

        // DELETE: api/position/*
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePosition(int id)
        {
            var position = await _context.Cargo.FindAsync(id);
            if (position == null)
            {
                return NotFound();
            }
            _context.Cargo.Remove(position);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PositionExists(string id)
        {
            return _context.Cargo.Any(e => e.Id == id);
        }
    }
}
