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
    public class PermissionController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;
        private readonly Dictionary<int, HashSet<string>> _authCache;

        public PermissionController(HISCOMContext context, ElasticClient elastic, Dictionary<int, HashSet<string>> authCache)
        {
            _context = context;
            _elastic = elastic;
            _authCache = authCache;
        }

        // GET: api/permission
        [HttpGet]
        public async Task<ActionResult<Paginator<PermissionIvm>>> GetPermission()
        {
            return await PaginationUtility.ElasticSearchPaginate
            (
                query: Request.QueryString.Value,
                indexUid: PermissionIvm.indexUid,
                elastic: _elastic,
                primaryKey: p=>p.Id,
                fields: new List<Expression<Func<PermissionIvm,dynamic>>>
                {
                    p=>p.Accion,
                    p=>p.Nombre,
                }
            );
        }

        // GET: api/permission/*
        [HttpGet("{id}")]
        public async Task<ActionResult<Permiso>> GetPermission(int id)
        {
            var permission = await _context.Permiso.FindAsync(id);

            if (permission == null)
            {
                return NotFound();
            }

            return permission;
        }


        // PUT: api/permission/*
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPermission(int id, Permiso permission)
        {
            if (id != permission.Id)
            {
                return BadRequest();
            }
            
            permission.FechaMod = DateTime.Now;
            _context.Entry(permission).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _authCache.Clear();
            
            var permissionEntity = await _context.Permiso.Where(p => p.Id == permission.Id)
                .Include(p => p.Menu)
                .SingleOrDefaultAsync();
            
            var permissionIvm = PermissionIvm.GetPermissionIvm(permissionEntity);
            await _elastic.UpdateAsync<PermissionIvm>(permissionIvm.Id, u => 
                u.Index(PermissionIvm.indexUid).Doc(permissionIvm));

            return NoContent();
        }

        // POST: api/permission
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Permiso>> PostPermission(Permiso permission)
        {
            var dbPermission = (from p in _context.Permiso where p.Accion.Equals(permission.Accion) && p.Ruta.Equals(permission.Ruta) select p)
                .SingleOrDefault();

            if (dbPermission != null) return BadRequest(new { error = "El permiso ya existe en el sistema" });
            
            permission.FechaReg = DateTime.Now;
            permission.Estado = true;
            _context.Permiso.Add(permission);
            await _context.SaveChangesAsync();
            
            var permissionEntity = await _context.Permiso.Where(p => p.Id == permission.Id)
                .Include(p => p.Menu)
                .SingleOrDefaultAsync();
                
            var permissionIvm = PermissionIvm.GetPermissionIvm(permissionEntity);
            await _elastic.CreateAsync(permissionIvm,b=>b.Index(PermissionIvm.indexUid));
                
            return CreatedAtAction("GetPermission", new { id = permission.Id }, permission);
        }

        // DELETE: api/permission/*
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var permission = await _context.Permiso.FindAsync(id);
            if (permission == null)
            {
                return NotFound();
            }

            _context.Permiso.Remove(permission);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PermissionExists(int id)
        {
            return _context.Permiso.Any(e => e.Id == id);
        }
    }
}
