using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain.Models;
using System;
using Admin.Indexation;
using Admin.Models;
using Algolia.Search.Clients;
using Lizelaser0310.Utilities;
using Microsoft.AspNetCore.Hosting;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IKeys _keys;

        public UserController(HISCOMContext context, IWebHostEnvironment env, IKeys keys)
        {
            _context = context;
            _env = env;
            _keys = keys;
        }

        // GET: api/user
        /*[HttpGet]
        public async Task<ActionResult<Paginator<UserIvm>>> GetUser()
        {
            return await _context.Usuario.ToListAsync();
        }*/

        // GET: api/user/*
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUser(int id)
        {

            var user = await _context.Usuario.FindAsync(id);
            var userVm = UserVm.CreateUserVm(user);

            userVm.Roles = await (from u in _context.Usuario
                                  join ur in _context.UsuarioRol
                                  on u.Id equals ur.UsuarioId
                                  join r in _context.Rol
                                  on ur.RolId equals r.Id
                                  where ur.UsuarioId == id
                                  select r.Id).ToListAsync();

            if (user == null)
            {
                return NotFound();
            }
            ImageUtility.CreateImageUrl(userVm, Request, "Foto");

            return userVm;
        }

        // PUT: api/user/*
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserVm user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            if (user.NuevaFoto != null)
            {
                user.Foto = ImageUtility.SaveImage(_env.ContentRootPath, user.NuevaFoto,"");
            }
            else
            {
                var leftPath = ImageUtility.GetPath(Request);
                if (user.Foto != null && user.Foto.StartsWith(leftPath))
                {
                    user.Foto = user.Foto.Replace(leftPath, "");
                }
            }

            user.FechaMod = DateTime.Now;
            _context.Entry(user).State = EntityState.Modified;

            if (user.Roles != null && user.Roles.Count > 0)
            {
                var entities = await _context.UsuarioRol.Where(x => x.UsuarioId == id).ToListAsync();
                _context.UsuarioRol.RemoveRange(entities);

                var rolEntities = user.Roles.Select(x => new UsuarioRol { RolId = x, UsuarioId = id });

                _context.UsuarioRol.AddRange(rolEntities);
            }

            await _context.SaveChangesAsync();
            

            return NoContent();
        }

        // POST: api/user
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUser(UserVm user)
        {
            var dbUser = (from u in _context.Usuario where u.NombreUsuario.Equals(user.NombreUsuario) && u.Correo.Equals(user.Correo) select u).SingleOrDefault();
            
            if (dbUser != null) return BadRequest(new { error = "El usuario ya existe" });

            user.Foto = ImageUtility.SaveImage(_env.ContentRootPath, user.Foto,"");
            user.Contrasena = AuthUtility.HashPassword("12345678", _keys.EncryptionKey);
            user.FechaReg = DateTime.Now;
            _context.Usuario.Add(user);
            await _context.SaveChangesAsync();

            int usuarioId = user.Id;
            List<UsuarioRol> roles = new List<UsuarioRol>();

            foreach (var item in user.Roles)
            {
                UsuarioRol ur = new UsuarioRol();
                ur.UsuarioId = usuarioId;
                ur.RolId = item;
                roles.Add(ur);
            }
            _context.UsuarioRol.AddRange(roles);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // DELETE: api/user/*
        [HttpDelete("{id}")]
        public async Task<ActionResult<Usuario>> DeleteUser(int id)
        {
            var user = await _context.Usuario.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Usuario.Remove(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private bool UserExists(int id)
        {
            return _context.Usuario.Any(e => e.Id == id);
        }
    }
}
