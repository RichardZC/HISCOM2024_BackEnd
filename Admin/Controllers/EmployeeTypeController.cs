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
    [Route("api/employee-type")]
    [ApiController]
    public class EmployeeTypeController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;

        public EmployeeTypeController(HISCOMContext context, ElasticClient elastic)
        {
            _context = context;
            _elastic = elastic;
        }

        // GET: api/employeeType/*
        [HttpGet("{id}")]
        public async Task<ActionResult<TipoEmpleado>> GetEmployeeType(int id)
        {
            var employeeType = await _context.TipoEmpleado.FindAsync(id);

            if (employeeType == null)
            {
                return NotFound();
            }

            return employeeType;
        }

        // PUT: api/employeeType/*
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployeeType(int id, TipoEmpleado employeeType)
        {
            if (id != employeeType.Id)
            {
                return BadRequest();
            }
            
            employeeType.FechaMod = DateTime.Now;
            _context.Entry(employeeType).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            var employeeTypeEntity = await _context.TipoEmpleado.Where(te => te.Id == employeeType.Id)
                .SingleOrDefaultAsync();
            
            var employeeTypeIvm = EmployeeTypeIvm.GetEmployeeTypeIvm(employeeTypeEntity);
            await _elastic.UpdateAsync<EmployeeTypeIvm>(employeeTypeIvm.Id, u => 
                u.Index(EmployeeTypeIvm.indexUid).Doc(employeeTypeIvm));

            return NoContent();
        }

        // POST: api/employeeType
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TipoEmpleado>> PostEmployeeType(TipoEmpleado employeeType)
        {

            var dbEmployeeType = (from te in _context.TipoEmpleado where te.Denominacion.Equals(employeeType.Denominacion) select te).SingleOrDefault();

            if (dbEmployeeType != null) return BadRequest(new { error = "EL tipo de empleado ya existe" });
            
            employeeType.FechaReg = DateTime.Now;
            employeeType.Estado = true;
            _context.TipoEmpleado.Add(employeeType);
            await _context.SaveChangesAsync();
            
            var employeeTypeEntity = await _context.TipoEmpleado.Where(te => te.Id == employeeType.Id)
                .SingleOrDefaultAsync();
            
            var employeeTypeIvm = EmployeeTypeIvm.GetEmployeeTypeIvm(employeeTypeEntity);
            await _elastic.CreateAsync(employeeTypeIvm,b=>b.Index(EmployeeTypeIvm.indexUid));

            return CreatedAtAction("GetEmployeeType", new { id = employeeType.Id }, employeeType);
        }

        // DELETE: api/employeeType/*
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployeeType(int id)
        {
            var employeeType = await _context.TipoEmpleado.FindAsync(id);
            if (employeeType == null)
            {
                return NotFound();
            }
            _context.TipoEmpleado.Remove(employeeType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeeTypeExists(int id)
        {
            return _context.TipoEmpleado.Any(e => e.Id == id);
        }
    }
}
