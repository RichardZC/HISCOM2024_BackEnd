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
    public class MenuController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;
        public MenuController(HISCOMContext context, ElasticClient elastic)
        {
            _context = context;
            _elastic = elastic;
        }

        // GET: api/menu
        [HttpGet]
        public async Task<ActionResult> GetMenu()
        {
            return await PaginationUtility.ElasticSearchPaginate
            (
                query: Request.QueryString.Value,
                indexUid: MenuIvm.indexUid,
                elastic: _elastic,
                primaryKey: p=>p.Id,
                fields: new List<Expression<Func<MenuIvm,dynamic>>>
                {
                    m=>m.Nombre,
                }
            );
        }

        // GET: api/menu/*
        [HttpGet("{id}")]
        public async Task<ActionResult<Menu>> GetMenu(int id)
        {
            var menu = await _context.Menu.FindAsync(id);

            if (menu == null)
            {
                return NotFound();
            }

            return menu;
        }

        // PUT: api/menu/*
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMenu(int id, Menu menu)
        {
            if (id != menu.Id)
            {
                return BadRequest();
            }
            
            menu.FechaMod = DateTime.Now;
            _context.Entry(menu).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            
            var menuEntity = await _context.Menu.Where(m => m.Id == menu.Id)
                .SingleOrDefaultAsync();

            var menuIvm = MenuIvm.GetMenuIvm(menuEntity);
            await _elastic.UpdateAsync<MenuIvm>(menuIvm.Id, u => 
                u.Index(MenuIvm.indexUid).Doc(menuIvm));

            return NoContent();
        }

        // POST: api/menu
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Menu>> PostMenu(Menu menu)
        {
            var dbmenu = (from m in _context.Menu where m.Nombre.Equals(menu.Nombre) && m.Icono.Equals(menu.Icono) select m).SingleOrDefault();

            if (dbmenu != null) return BadRequest(new {error = "El menu ya existe"});
            
            menu.FechaReg = DateTime.Now;
            menu.Estado = true;
            _context.Menu.Add(menu);
            await _context.SaveChangesAsync();
            
            var menuEntity = await _context.Menu.Where(m => m.Id == menu.Id)
                .SingleOrDefaultAsync();

            var menuIvm = MenuIvm.GetMenuIvm(menuEntity);
            await _elastic.CreateAsync(menuIvm,b=>b.Index(MenuIvm.indexUid));

            return CreatedAtAction("GetMenu", new { id = menu.Id }, menu);

        }

        // DELETE: api/menu/*
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenu(int id)
        {
            var menu = await _context.Menu.FindAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            _context.Menu.Remove(menu);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MenuExists(int id)
        {
            return _context.Menu.Any(e => e.Id == id);
        }
    }
}
