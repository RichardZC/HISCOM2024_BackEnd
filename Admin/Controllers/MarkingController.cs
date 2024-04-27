using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Admin.Indexation;
using Admin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Lizelaser0310.Utilities;
using MoreLinq;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarkingController : ControllerBase
    {
        private readonly HISCOMContext _context;

        public MarkingController(HISCOMContext context)
        {
            _context = context;
        }

        // GET: api/Marking
        [HttpGet("periods")]
        public async Task<ActionResult> GetMarkingPeriods(int page=1, int itemsPerPage=5)
        {
            if (page < 1 || itemsPerPage < 1)
            {
                return BadRequest();
            }

            if (User.Identity?.Name == null)
            {
                return Unauthorized(ErrorVm.Create("El usuario solicitado no existe"));
            }

            var userid = int.Parse(User.Identity.Name);

            var user = await _context.Usuario.Include(u => u.Empleado)
                .SingleOrDefaultAsync(u => u.Id == userid);


            var periods = _context.Marcacion
                .Where(m => m.NumeroDoc == user.Empleado.NumeroDoc)
                .OrderByDescending(p=>p.Fecha)
                .Select(m => new MarkingPeriod() { Year = m.Fecha.Year, Month = m.Fecha.Month })
                .Distinct();

            var totalItems = await periods.CountAsync();


            return Ok(new Paginator<MarkingPeriod>
            {
                Items = await periods
                    .Skip((page - 1) * itemsPerPage)
                    .Take(itemsPerPage)
                    .ToListAsync(),
                CurrentPage = page,
                ItemsPerPage = itemsPerPage,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage)
            });
        }

        // GET: api/Marking
        [HttpGet("days")]
        public async Task<ActionResult> GetMarkingDays(int year, int month)
        {
            if (User.Identity?.Name == null)
            {
                return Unauthorized(ErrorVm.Create("El usuario solicitado no existe"));
            }
            var userid = int.Parse(User.Identity.Name);

            var user = await _context.Usuario.Include(u => u.Empleado)
                .SingleOrDefaultAsync(u=>u.Id==userid);

            return await PaginationUtility.Paginate(
                query: Request.QueryString.Value,
                dbSet: _context.Marcacion,
                middle: (m,_) => m.Where(m => m.NumeroDoc == user.Empleado.NumeroDoc && m.Fecha.Year == year && m.Fecha.Month == month)
                );


        }

        // GET: api/Marking/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Marcacion>> GetMarking(long id)
        {
            var marcacion = await _context.Marcacion.FindAsync(id);

            if (marcacion == null)
            {
                return NotFound();
            }

            return marcacion;
        }

    }
}
