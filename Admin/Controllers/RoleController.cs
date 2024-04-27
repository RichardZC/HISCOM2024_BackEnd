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
    public class RoleController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;
        private readonly Dictionary<int, HashSet<string>> _authCache;

        public RoleController(HISCOMContext context, ElasticClient elastic, Dictionary<int, HashSet<string>> authCache)
        {
            _context = context;
            _elastic = elastic;
            _authCache = authCache;
        }

        // GET: api/role
        [HttpGet]
        public async Task<ActionResult> GetRole()
        {
            return await PaginationUtility.ElasticSearchPaginate
            (
                query: Request.QueryString.Value,
                elastic: _elastic,
                indexUid: RoleIvm.indexUid,
                primaryKey: cp=>cp.Id,
                fields: new List<Expression<Func<RoleIvm,dynamic>>>
                {
                    r=>r.Denominacion,
                }
            );
        }

        // GET: api/role/*
        [HttpGet("{id}")]
        public async Task<ActionResult<Rol>> GetRole(int id)
        {
            var role = await _context.Rol.FindAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            return role;
        }

        // PUT: api/role/*
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRole(int id, RoleVm role)
        {
            if (id != role.Id)
            {
                return BadRequest();
            }
            
            role.FechaMod = DateTime.Now;
            _context.Entry(role).State = EntityState.Modified;

            var entities = await _context.RolPermiso.Where(x => x.RolId == id).ToListAsync();
            _context.RolPermiso.RemoveRange(entities);

            var permisoEntities = role.Permisos.Select(x => new RolPermiso { PermisoId = x, RolId = id });

            _context.RolPermiso.AddRange(permisoEntities);

            await _context.SaveChangesAsync();

            _authCache.Clear();

            var roleEntity = await _context.Rol.Where(r => r.Id == role.Id)
                .SingleOrDefaultAsync();
            
            var roleIvm = RoleIvm.GetRoleIvm(roleEntity);
            await _elastic.UpdateAsync<RoleIvm>(roleIvm.Id, u => 
                u.Index(RoleIvm.indexUid).Doc(roleIvm));

            return NoContent();
        }

        // POST: api/role/*
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Rol>> PostRole(Rol role)
        {
            var dbRole = await (from r in _context.Rol where r.Denominacion.Equals(role.Denominacion)select r).FirstOrDefaultAsync();

            if (dbRole != null) return BadRequest(new {error = "El rol ya existe"});
            
            role.FechaReg = DateTime.Now;
            _context.Rol.Add(role);
            await _context.SaveChangesAsync();

            var roleEntity = await _context.Rol.Where(u => u.Id == role.Id)
                .SingleOrDefaultAsync();
            
            var rolIvm = RoleIvm.GetRoleIvm(roleEntity);
            await _elastic.CreateAsync(rolIvm,b=>b.Index(RoleIvm.indexUid));

            return CreatedAtAction("GetRole", new { id = role.Id }, role);
        }

        // DELETE: api/role/*
        [HttpDelete("{id}")]
        public async Task<ActionResult<Rol>> DeleteRole(int id)
        {
            var role = await _context.Rol.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            _context.Rol.Remove(role);
            await _context.SaveChangesAsync();

            return role;
        }

        private bool RoleExists(int id)
        {
            return _context.Rol.Any(e => e.Id == id);
        }
    }
}
