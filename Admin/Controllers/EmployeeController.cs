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
using Microsoft.AspNetCore.Hosting;
using NaCl;
using Nest;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;
        private readonly IKeys _keys;
        private readonly IConstants _constants;
        private readonly Dictionary<int, HashSet<string>> _authCache;

        public EmployeeController(HISCOMContext context, ElasticClient elastic, IKeys keys, IConstants constants, Dictionary<int, HashSet<string>> authCache)
        {
            _context = context;
            _elastic = elastic;
            _keys = keys;
            _constants = constants;
            _authCache = authCache;
        }

        // GET: api/employee
        [HttpGet]
        public async Task<ActionResult> GetEmployee()
        {
            
            return await PaginationUtility.ElasticSearchPaginate
            (
                query: Request.QueryString.Value,
                indexUid: EmployeeIvm.indexUid,
                elastic: _elastic,
                primaryKey: p=>p.Id,
                fields: new List<Expression<Func<EmployeeIvm,dynamic>>>
                {
                    e=>e.NumeroDoc,
                    e=>e.NombreCompleto,
                    e=>e.Organigrama,
                    e=>e.Cargo
                }
            );
        }

        // GET: api/employee/*
        [HttpGet("{id}")]
        public async Task<ActionResult<Empleado>> GetEmployee(int id)
        {
            var employee = await _context.Empleado
                .Include(e=>e.Usuario)
                .Include(e=>e.Profesion)
                .Include(e=>e.Cargo)
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(e=>e.Id==id);

            if (employee?.Usuario == null)
            {
                return BadRequest(ErrorVm.Create("El empleado o su usuario asignado no existe"));
            }
            var employeeVm = EmployeeVm.CreateEmployeeVm(employee);

            
            employeeVm.Roles = await _context.UsuarioRol
                .Where(ur => ur.UsuarioId == employee.Usuario.Id)
                .Select(ur=>ur.RolId).ToListAsync();

            employeeVm.CorreoUsuario = employee.Usuario.Correo;
            
            ImageUtility.CreateImageUrl(employee.Usuario, Request, "Foto",_constants.ImagePath);

            employeeVm.Foto = employee.Usuario.Foto;

            return employeeVm;
        }

        // PUT: api/employee/*
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, EmployeeVm employeeVm)
        {
            if (id != employeeVm.Id)
            {
                return BadRequest(ErrorVm.Create("El id del empleado no coincide con el objeto enviado"));
            }

            var existEmployee = await _context.Empleado.AnyAsync(e => e.Id == id);

            if (!existEmployee )
            {
                return BadRequest(ErrorVm.Create("El empleado no existe"));
            }

            //_context.Entry(employee).State = EntityState.Detached;

            //_context.ChangeTracker.Clear();


            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var canSchedule = await _context.RolPermiso.Include(rp => rp.Rol)
                    .AnyAsync(rp => employeeVm.Roles.Contains(rp.RolId) 
                                    && (rp.Permiso.Accion.Equals(nameof(ShiftWorkController.ScheduleShiftWork)) || rp.Permiso.Accion.Equals(nameof(ShiftWorkController.CheckShiftWork))));

                var isMultipleApproval = await _context.RolTurnoAprobador.Include(rta=>rta.Aprobador)
                    .AnyAsync(rta=>employeeVm.Roles.Contains(rta.AprobadorId) && !rta.AprobadorPadre);

                //if (!employee.EsJefe && !isMultipleApproval && canSchedule)
                //{
                //    var oChart = await _context.Organigrama.Include(o => o.Empleado).SingleOrDefaultAsync(o => o.Id == employeeVm.OrganigramaId);

                //    if (oChart != null && oChart.Empleado.Any(e => e.EsJefe))
                //    {
                //        return BadRequest(ErrorVm.Create("Ya existe un cargo jefe asignado a la unidad orgánica"));
                //    }
                //}

                var baseDirectory = _constants.Storage;// await _context.Parametro.SingleOrDefaultAsync(p => p.Llave == _constants.BaseDirectory);

                employeeVm.FechaMod = DateTime.Now;
                employeeVm.EsJefe = canSchedule;
                _context.Update(employeeVm);
                //_context.Entry(employeeVm).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var user = await _context.Usuario
                    .Where(u => u.EmpleadoId == employeeVm.Id)
                    .IgnoreQueryFilters()
                    .SingleOrDefaultAsync();

                if (user == null)
                {
                    return BadRequest(ErrorVm.Create("El usuario asignado a este empleado no existe"));
                }
                
                user.Correo = employeeVm.CorreoUsuario;
                user.NombreUsuario = employeeVm.CorreoUsuario.Split("@")[0];

                if (employeeVm.Foto != null)
                {
                    user.Foto = ImageUtility.SaveImage(baseDirectory, employeeVm.Foto, employeeVm.NumeroDoc, _constants.ImagePath);
                }

                user.FechaMod = DateTime.Now;
                user.Estado = employeeVm.Estado;
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                _context.ChangeTracker.Clear();

                var roles = await _context.UsuarioRol                                   
                    .Where(ur => ur.UsuarioId == user.Id).ToListAsync();                              
                _context.UsuarioRol.RemoveRange(roles);

                var userRoles = (employeeVm.Roles?? new List<int>()).Select(r => new UsuarioRol()
                {
                    UsuarioId = user.Id,
                    RolId = r
                });
                _context.UsuarioRol.AddRange(userRoles);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                _authCache.Remove(user.Id);

            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                throw ;
            }

            var employeeEntity = await _context.Empleado.Where(e => e.Id == employeeVm.Id)
                .Include(e => e.TipoEmpleado)
                .Include(e=>e.Cargo)
                .Include(e=>e.Organigrama)
                .Include(e=>e.CondicionLaboral)
                .Include(e=>e.TipoDocumento)
                .Include(e=>e.TipoCuenta)
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync();
            
            var employeeIvm = EmployeeIvm.GetEmployeeIvm(employeeEntity);
            await _elastic.UpdateAsync<EmployeeIvm>(employeeIvm.Id, u => 
                u.Index(EmployeeIvm.indexUid).Doc(employeeIvm));


            return NoContent();
        }

        // POST: api/employee
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Empleado>> PostEmployee(EmployeeVm employee)
        {
            var dbEmployee = await _context.Empleado.Include(e=>e.Organigrama)
                .ThenInclude(e=>e.Empleado)
                .Where(e=>e.NumeroDoc==employee.NumeroDoc)
                .SingleOrDefaultAsync();
            
            if (dbEmployee != null)
            {
                return BadRequest(ErrorVm.Create("El número de documento asociado al empleado ya se encuentra registrado"));
            }
            
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var canSchedule = await _context.RolPermiso.Include(rp => rp.Rol)
                    .AnyAsync(rp => employee.Roles.Contains(rp.RolId) && rp.Permiso.Accion.Equals(nameof(ShiftWorkController.ScheduleShiftWork)));

                if (canSchedule)
                {
                    var oChart = await _context.Organigrama.Include(o => o.Empleado).SingleOrDefaultAsync(o => o.Id == employee.OrganigramaId);

                    if (oChart != null && oChart.Empleado.Any(e => e.EsJefe))
                    {
                        return BadRequest(ErrorVm.Create("Ya existe un cargo jefe asignado a la unidad orgánica"));
                    }
                }
                
                employee.FechaReg = DateTime.Now;
                employee.Estado = true;
                employee.EsJefe = canSchedule;
                _context.Empleado.Add(employee);
                await _context.SaveChangesAsync();
                
                var user = new Usuario()
                {
                    EmpleadoId = employee.Id,
                    Correo = employee.CorreoUsuario,
                    NombreUsuario = employee.CorreoUsuario.Split("@")[0],
                    Contrasena = AuthUtility.HashPassword(employee.NumeroDoc, _keys.EncryptionKey),
                    Foto = ImageUtility.SaveImage(_constants.Storage, employee.Foto, employee.NumeroDoc, _constants.ImagePath),
                    FechaReg = DateTime.Now,
                    Estado = true
                };
                
                _context.Usuario.Add(user);
                await _context.SaveChangesAsync();
                
                var roles = employee.Roles.Select(r => new UsuarioRol()
                {
                    UsuarioId = user.Id,
                    RolId = r
                });

                _context.UsuarioRol.AddRange(roles);
                await _context.SaveChangesAsync();


                var boss = await _context.Empleado
                    .Include(e=>e.Cargo)
                    .SingleOrDefaultAsync(e => e.OrganigramaId == employee.OrganigramaId && e.EsJefe && e.Id!=employee.Id);
                
                if (boss!=null)
                {
                    var notification = new Notificacion()
                    {
                        EmpleadoId = boss.Id,
                        Ruta = "/shift-work/schedule",
                        Mensaje = $"Se ha agregado el empleado {employee.ApellidoPaterno} {employee.ApellidoMaterno} {employee.Nombres} a su cargo",
                        Icono = "mdi-bell",
                        FechaReg = DateTime.Now,
                        Estado = false
                    };
                    _context.Notificacion.Add(notification);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.CommitAsync();
                throw;
            }

            _context.ChangeTracker.Clear();

            var employeeEntity = await _context.Empleado.Where(e => e.Id == employee.Id)
                    .Include(y => y.TipoEmpleado)
                    .Include(y=>y.Banco)
                    .Include(y=>y.Nacionalidad)
                    .Include(y=>y.Cargo)                  
                    .Include(y=>y.Organigrama)
                    .Include(y=>y.CondicionLaboral)
                    .Include(y=>y.TipoDocumento)
                    .Include(y=>y.TipoCuenta) 
                .SingleOrDefaultAsync();
            
            var employeeIvm = EmployeeIvm.GetEmployeeIvm(employeeEntity);
            await _elastic.CreateAsync(employeeIvm,b=>b.Index(EmployeeIvm.indexUid));

            return CreatedAtAction("GetEmployee", new { id = employee.Id }, employee);

        }
        
        public async Task<ErrorVm> ValidatePayload(EmployeeVm employee)
        {
            ErrorVm result = new ErrorVm();


            var oChart = await _context.Organigrama
                .Include(o => o.Empleado).SingleOrDefaultAsync(o => o.Id == employee.OrganigramaId);

            if (oChart != null && oChart.Empleado.Any(c => c.EsJefe))
            {
                result.AddError("Ya existe un cargo jefe asignado a la unidad orgánica");
            }

            return result.IsEmpty()?null:result;
        }  

        // DELETE: api/employee/*
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Empleado.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            _context.Empleado.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeeExists(int id)
        {
            return _context.Empleado.Any(e => e.Id == id);
        }
    }
}
