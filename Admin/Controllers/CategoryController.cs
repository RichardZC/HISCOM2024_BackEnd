using System;
using System.Collections.Generic;
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
using Nest;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;
        public CategoryController(HISCOMContext context, ElasticClient elastic)
        {
            _context = context;
            _elastic = elastic;
        }

        // GET: api/Categoria
        [HttpGet]
        public async Task<ActionResult<Paginator<CategoryIvm>>> GetCategory()
        {
            if (User.Identity?.Name==null)
            {
                return Unauthorized(ErrorVm.Create("El usuario no existe"));
            }

            var userid = int.Parse(User.Identity.Name);

            var boss = await _context.Empleado
                .Include(e => e.Usuario)
                .SingleOrDefaultAsync(e=>e.Usuario.Id==userid);

            if (boss==null)
            {
                return BadRequest(ErrorVm.Create("El empleado no existe"));
            }

            _context.ChangeTracker.Clear();

            return await PaginationUtility.Paginate
            (
                query: Request.QueryString.Value,
                dbSet: _context.Categoria,
                middle: (q, qp) => q.Where(c => c.OrganigramaId == boss.OrganigramaId)
            );
        }


        // GET: api/Categoria/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Categoria>> GetCategory(int id)
        {
            var categoria = await _context.Categoria.FindAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            return categoria;
        }

        // PUT: api/Category/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Categoria category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            category.FechaMod = DateTime.Now;
            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();

/*            var categoryEntity = await _context.Categoria.Where(p => p.Id == category.Id)
                .Include(c => c.Organigrama)
                .SingleOrDefaultAsync();

            var categoryIvm = CategoryIvm.GetCategoryIvm(categoryEntity);
            await _elastic.UpdateAsync<CategoryIvm>(categoryIvm.Id, u =>
                u.Index(CategoryIvm.indexUid).Doc(categoryIvm));*/

            return NoContent();
        }

        // PUT: api/Category/assign
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("assign")]
        public async Task<IActionResult> AssignCategory(List<CategoriaEmpleado> empcategories)
        {
            if (User.Identity?.Name==null)
            {
                return Unauthorized(ErrorVm.Create("El usuario no existe"));
            }

            var userid = int.Parse(User.Identity.Name);

            var boss = await _context.Empleado
                .Include(e=>e.Usuario)
                .Where(e=>e.Usuario.Id==userid).SingleOrDefaultAsync();

            if (boss == null)
            {
                return BadRequest("No existe un empleado asignado a este usuario");
            }

            var empCategoriesDb = await _context.CategoriaEmpleado
                .Include(ce=>ce.Categoria)
                .Where(ce=>ce.Categoria.OrganigramaId==boss.OrganigramaId)
                .ToListAsync();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.CategoriaEmpleado.RemoveRange(empCategoriesDb);
                _context.CategoriaEmpleado.AddRange(empcategories);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return NoContent();
        }

        // POST: api/Category
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Categoria>> PostCategory(Categoria category)
        {
            var dbCategory = await _context.Categoria
                .Where(c=>c.OrganigramaId == category.OrganigramaId && c.Denominacion == category.Denominacion)
                .SingleOrDefaultAsync();

            if (dbCategory != null) return BadRequest(new { error = "La categoría ya existe en el sistema" });

            category.FechaReg = DateTime.Now;
            category.Estado = true;
            _context.Categoria.Add(category);
            await _context.SaveChangesAsync();

/*            var categoryEntity = await _context.Categoria.Where(c => c.Id == category.Id)
                .Include(c => c.Organigrama)
                .SingleOrDefaultAsync();

            var categoryIvm = CategoryIvm.GetCategoryIvm(categoryEntity);
            await _elastic.CreateAsync(categoryIvm, b => b.Index(CategoryIvm.indexUid));*/

            return CreatedAtAction("GetCategory", new { id = category.Id }, category);
        }

        // DELETE: api/Category/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.Categoria.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }

            _context.Categoria.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return _context.Categoria.Any(e => e.Id == id);
        }
    }
}
