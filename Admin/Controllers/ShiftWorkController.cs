using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using Admin.Models;
using Algolia.Search.Clients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Lizelaser0310.Utilities;
using Admin.DTO;
using HashidsNet;
using Admin.Templates;
using System.Globalization;
using System.IO;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Hosting;
using iText.Html2pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using QRCoder;
using Path = System.IO.Path;

namespace Admin.Controllers
{
    [Route("api/shift-work")]
    [ApiController]
    public class ShiftWorkController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly IKeys _keys;
        private readonly IConstants _constants;

        public ShiftWorkController(HISCOMContext context, IKeys keys, IConstants constants)
        {
            _context = context;
            _keys = keys;
            _constants = constants;
        }



        // GET: api/ShiftWork
        [HttpGet("history")]
        public async Task<ActionResult> GetHistoryShiftWork()
        {
            if (User.Identity?.Name == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(User.Identity.Name);

            var user = await _context.Usuario
                .Include(u => u.Empleado)
                .SingleAsync(u => u.Id == userId);

            var userRoles = await _context.UsuarioRol
                .Include(ur => ur.Rol)
                .Where(ur => ur.UsuarioId == userId)
                .Select(u => u.Rol.Id)
                .ToListAsync();

            if (user?.Empleado == null  || userRoles.Count == 0) 
            {
                return Unauthorized();
            }

            return await PaginationUtility.Paginate
            (
                query: Request.QueryString.Value,
                dbSet: _context.RolTurno,
                searchProps: (rt, s) => rt.Where(x =>
                    x.Organigrama.Denominacion.ToUpper().Contains(s.ToUpper())
                    || x.Anio.ToString().Contains(s)
                    || x.Jefe.ApellidoPaterno.Contains(s)
                    || x.Jefe.ApellidoMaterno.Contains(s)
                    || x.Jefe.Nombres.Contains(s)
                ),
                middle: (rtdb, query) =>
                {
                    var swType = query.GetParam("type", $"{TipoRolTurnoEnum.Regular}");                        

                    return rtdb.Include(rt=>rt.Organigrama)
                        .Include(rt=>rt.Jefe)
                        .Where(rt => rt.TipoRolTurno == swType && 
                            rt.Estado == $"{EstadoEnum.Aprobado}"
                        )
                        //.Distinct()
                        .OrderByDescending(rt=>rt.FechaMod??rt.FechaReg);
                }
            );
        }

        // GET: api/ShiftWork
        [HttpGet("review")]
        public async Task<ActionResult> GetReviewShiftWork()
        {
            if (User.Identity?.Name == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(User.Identity.Name);

            var user = await _context.Usuario
                .Include(u => u.Empleado)
                .SingleOrDefaultAsync(u => u.Id == userId);


            var userRolesIds = await _context.UsuarioRol
                .Include(ur => ur.Rol)
                .Where(ur => ur.UsuarioId == userId)
                .Select(u => u.Rol.Id)
                .ToListAsync();


            if (user?.Empleado == null || userRolesIds.Count == 0)
            {
                return Unauthorized();
            }

            var isFatherApproval = await _context.RolTurnoAprobador
                .Where(rta=>userRolesIds.Contains(rta.AprobadorId) && rta.AprobadorPadre)
                .AnyAsync();
            var shiftWorkPeriod = DateTime.Now.AddMonths(1);


            var result = await PaginationUtility.Paginate
            (
                query: Request.QueryString.Value,
                dbSet: _context.RolTurno,
                searchProps: (rt, s) => rt.Where(x =>
                    x.Organigrama.Denominacion.ToUpper().Contains(s.ToUpper())
                    || x.Anio.ToString().Contains(s)
                    || x.Jefe.ApellidoPaterno.Contains(s)
                    || x.Jefe.ApellidoMaterno.Contains(s)
                    || x.Jefe.Nombres.Contains(s)
                ),
                mutation: ShiftWorkDTO.Create,
                middle: (rtdb, query) =>
                {
                    var swType = query.GetParam("type", $"{TipoRolTurnoEnum.Regular}");

                    return rtdb
                        .Include(rt => rt.Organigrama)
                        .Include(rt => rt.Jefe)
                        .Include(rt => rt.RolTurnoIntento)
                        .ThenInclude(rti => rti.SiguienteAprobador)
                        .Include(rt => rt.RolTurnoIntento)
                        .ThenInclude(rt => rt.RolTurnoRevision)
                        .Where(rt => rt.TipoRolTurno == swType && rt.Mes == shiftWorkPeriod.Month && (isFatherApproval? rt.Organigrama.PadreId==user.Empleado.OrganigramaId:true) && (
                            rt.Estado == $"{EstadoEnum.Aprobado}" || (
                                rt.RolTurnoIntento.Any(rti =>
                                    (rti.Actual && (
                                        (rti.SiguienteAprobador != null
                                            && userRolesIds.Contains(rti.SiguienteAprobador.AprobadorId)
                                            && (rt.Organigrama.PadreId == user.Empleado.OrganigramaId 
                                                || !rti.SiguienteAprobador.AprobadorPadre))
                                        || rti.RolTurnoRevision.Any(rtr => rtr.UsuarioId == userId)))
                                )
                                || (rt.RolTurnoIntento.Count(rti => rti.FechaCierre != null) > 0 
                                    && rt.RolTurnoIntento.OrderByDescending(rti => rti.FechaCierre)
                                        .First().RolTurnoRevision.Any(rtr => rtr.UsuarioId == userId 
                                            && rtr.Estado == $"{EstadoEnum.Observado}"))
                            )
                        ))
                        //.Distinct()
                        .OrderByDescending(rt => rt.FechaMod ?? rt.FechaReg);
                }
            );

            return result;
        }


        [HttpGet]
        public async Task<ActionResult> GetBossShiftWork()
        {
            if (User.Identity?.Name == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(User.Identity.Name);

            var canSchedule = await (from p in _context.Permiso
                                     join rp in _context.RolPermiso
                                         on p.Id equals rp.PermisoId
                                     join ur in _context.UsuarioRol
                                         on rp.RolId equals ur.RolId
                                     where ur.UsuarioId == userId && p.Estado 
                                         && p.Accion.Equals(nameof(ScheduleShiftWork))
                                     select p).AnyAsync();

            var canScheduleRegular = false;
            var canScheduleComplementary = false;

            if (canSchedule)
            {
                var user = await _context.Usuario
                    .Include(u => u.Empleado)
                    .ThenInclude(e => e.Cargo)
                    .SingleOrDefaultAsync(u => u.Id == userId);

                if (user.Empleado.EsJefe)
                {
                    var shiftWorkPeriod = DateTime.Now.AddMonths(1);
                    // Get the master day limit to schedule a shift work
                    var parameter = await _context.Parametro
                        .Where(p => p.Llave == _constants.DayLimitKey)
                        .SingleAsync();

                    var swsRegular = await _context.RolTurno
                        .Where(rt => rt.OrganigramaId == user.Empleado.OrganigramaId
                                && rt.Mes == shiftWorkPeriod.Month
                                && rt.Anio == shiftWorkPeriod.Year
                                && rt.TipoRolTurno == $"{TipoRolTurnoEnum.Regular}"
                                && rt.Estado != $"{EstadoEnum.Eliminado}")
                        .ToListAsync();

                    var swsComplementary = await _context.RolTurno
                        .Where(rt => rt.OrganigramaId == user.Empleado.OrganigramaId
                                && rt.Mes == shiftWorkPeriod.Month
                                && rt.Anio == shiftWorkPeriod.Year
                                && rt.TipoRolTurno == $"{TipoRolTurnoEnum.Complementario}"
                                && rt.Estado != $"{EstadoEnum.Eliminado}")
                        .ToListAsync();



                    var success = int.TryParse(parameter.Valor, out int limitDay);
                    // A shift work can only be scheduled before the master day limit
                    canScheduleRegular = swsRegular.Count == 0 && DateTime.Now.Day <= (success ? limitDay : 15);
                    canScheduleComplementary = swsRegular.Any(rt=>rt.Estado == $"{EstadoEnum.Aprobado}") && swsComplementary.Count == 0;
                }

            }


            return await PaginationUtility.Paginate
            (
                query: Request.QueryString.Value,
                dbSet: _context.RolTurno,
                searchProps: (rt, s) => rt.Where(x =>
                    x.Organigrama.Denominacion.ToUpper().Contains(s.ToUpper())
                    || x.Anio.ToString().Contains(s)
                    || x.Jefe.ApellidoPaterno.Contains(s)
                    || x.Jefe.ApellidoMaterno.Contains(s)
                    || x.Jefe.Nombres.Contains(s)
                ),
                middle: (rtdb, query) =>
                {
                    var swType = query.GetParam("type", $"{TipoRolTurnoEnum.Regular}");

                    return rtdb.Include(rt => rt.Organigrama)
                         .Include(rt => rt.Jefe)

                         .Include(rt => rt.RolTurnoIntento)
                         .Where(rt=> rt.TipoRolTurno == swType && rt.JefeId == userId)
                         .OrderByDescending(rt => rt.FechaMod ?? rt.FechaReg);
                },
                metadata: new Dictionary<string, dynamic>()
                {
                    {"canSchedule", canSchedule},
                    {"canScheduleRegular", canScheduleRegular},
                    {"canScheduleComplementary",canScheduleComplementary}
                }
            );
        }

        // GET: api/ShiftWork/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RolTurno>> GetShiftWork(int id)
        {
            var shiftWork = await _context.RolTurno.FindAsync(id);

            if (shiftWork == null)
            {
                return NotFound();
            }

            return shiftWork;
        }

        [HttpGet("manage")]
        public async Task<ActionResult> ManageShiftWork(int?id,string type="Regular")
        {
            if (User.Identity.Name==null)
            {
                return Unauthorized();
            }
            var boss = await _context.Usuario
                .Include(u => u.Empleado)
                .SingleOrDefaultAsync(x => x.Id == int.Parse(User.Identity.Name));


            if (!boss.Empleado.EsJefe)
            {
                return Unauthorized(ErrorVm.Create("Este usuario no puede realizar rol de turnos"));
            }

            
            _context.ChangeTracker.Clear();

            var categories = await _context.Categoria
                .Where(c => c.OrganigramaId == boss.Empleado.OrganigramaId)
                .ToListAsync();

            var employees = await _context.Empleado
                .Include(e => e.CondicionLaboral)
                .Include(e => e.Cargo)
                .Include(e => e.Profesion)
                .Where(e => e.OrganigramaId == boss.Empleado.OrganigramaId)
                .Select(e => EmployeeDTO.Create(e,null,null))
                .ToListAsync();

            var employeesCategory = await _context.CategoriaEmpleado
                .Include(ec => ec.Empleado)
                .ThenInclude(e => e.CondicionLaboral)
                .Include(ce => ce.Empleado)
                .ThenInclude(e => e.Profesion)
                .Include(ce => ce.Empleado)
                .ThenInclude(e => e.Cargo)
                .Include(ce=>ce.Categoria)
                .Where(ec => ec.Empleado.OrganigramaId == boss.Empleado.OrganigramaId)
                .Select(ec => EmployeeDTO.Create(ec.Empleado, ec.CategoriaId,ec.Categoria.Denominacion))
                .ToListAsync();

            _context.ChangeTracker.Clear();

            var structure = await _context.Organigrama.FindAsync(boss.Empleado.OrganigramaId);
            var establishments = await _context.Establecimiento.ToListAsync();
            var turns = await _context.Turno.ToListAsync();
            
            _context.ChangeTracker.Clear();

            List<RolTurno> currentShiftWorks;

            RolTurno currentShiftWork = null;
            //ShiftWorkDTO rolturnoActual = null;
            List<RolTurnoEstab> approvalShiftWorkEstabs = null;
            List<RolTurnoDetalle> details = null;
            List<RolTurnoDetalle> disabledDetails = null;

            Enum.TryParse(type, out TipoRolTurnoEnum typeShiftWork);

            var shiftWorkPeriod = DateTime.Now;
            if (typeShiftWork==TipoRolTurnoEnum.Regular)
            {
                // Get the master day limit to schedule a shift work
                var parameter = await _context.Parametro
                    .Where(p => p.Llave == _constants.DayLimitKey)
                    .SingleAsync();

                var success = int.TryParse(parameter.Valor, out var limitDay);

                // A shift work can only be scheduled before the master day limit
                if (shiftWorkPeriod.Day > (success ? limitDay : 15))
                {
                    return Unauthorized(ErrorVm.Create("Fecha límite de edición para este rol de turno expirada"));
                }
                
                shiftWorkPeriod = shiftWorkPeriod.AddMonths(1);
                
                currentShiftWorks = await _context.RolTurno
                    .Include(rt => rt.RolTurnoEstab)
                    .ThenInclude(rte => rte.RolTurnoDetalle)
                    .Where(rt => rt.OrganigramaId == boss.Empleado.OrganigramaId 
                                 && rt.Mes == shiftWorkPeriod.Month 
                                 && rt.Anio == shiftWorkPeriod.Year 
                                 && rt.TipoRolTurno==$"{typeShiftWork}"
                                 && (rt.Estado==$"{EstadoEnum.Creado}" || rt.Estado==$"{EstadoEnum.Observado}"))
                    .ToListAsync();
            }

            else if (typeShiftWork==TipoRolTurnoEnum.Complementario)
            {
                shiftWorkPeriod = shiftWorkPeriod.AddMonths(1);
                
                if (id!=null)
                {
                    currentShiftWorks = await _context.RolTurno
                        .Include(rt => rt.RolTurnoEstab)
                        .ThenInclude(rte => rte.RolTurnoDetalle)
                        .Where(rt => rt.Id == id && rt.OrganigramaId==boss.Empleado.OrganigramaId 
                                                 && (rt.Estado==$"{EstadoEnum.Creado}" || rt.Estado==$"{EstadoEnum.Observado}"))
                        .ToListAsync();
                }
                else
                {
                    currentShiftWorks = await _context.RolTurno
                        .Include(rt => rt.RolTurnoEstab)
                        .ThenInclude(rte => rte.RolTurnoDetalle)
                        .Where(rt => rt.OrganigramaId == boss.Empleado.OrganigramaId
                                     && rt.TipoRolTurno==$"{typeShiftWork}"
                                     && (rt.Estado==$"{EstadoEnum.Creado}" || rt.Estado==$"{EstadoEnum.Observado}"))
                        .ToListAsync();                
                }

                var regularShiftWork = await _context.RolTurno
                    .Include(rt => rt.RolTurnoEstab)
                    .ThenInclude(rte => rte.RolTurnoDetalle)
                    .SingleOrDefaultAsync(rt => rt.OrganigramaId == boss.Empleado.OrganigramaId
                                 && rt.Mes == shiftWorkPeriod.Month
                                 && rt.Anio == shiftWorkPeriod.Year
                                 && rt.TipoRolTurno == $"{TipoRolTurnoEnum.Regular}"
                                 && (rt.Estado == $"{EstadoEnum.Aprobado}"));
                
                if (regularShiftWork!=null)
                {
                    approvalShiftWorkEstabs = regularShiftWork.RolTurnoEstab.ToList();
                    disabledDetails = regularShiftWork.RolTurnoEstab.SelectMany(rte=>rte.RolTurnoDetalle).ToList();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            
            if (currentShiftWorks.Count>0)
            {
                if (currentShiftWorks.Count>1)
                {
                    return Conflict(ErrorVm.Create("Existe mas de un rol de turnos asignados a su usuario. Contacte con soporte técnico"));
                }

                if (currentShiftWorks.Count==1)
                {
                    currentShiftWork = currentShiftWorks.First();
                    //rolturnoActual = ShiftWorkDTO.Create(currentShiftWorks.First());

                    if (currentShiftWork.Estado==$"{EstadoEnum.Enviado}")
                    {
                        return BadRequest(ErrorVm.Create("No puede administrar un rol de turnos ya enviado"));
                    }

                    details = currentShiftWork.RolTurnoEstab
                        .SelectMany(rte => rte.RolTurnoDetalle).ToList();
                    shiftWorkPeriod = new DateTime(currentShiftWork.Anio, currentShiftWork.Mes,1);
                }
            }
            var data = new
            {
                Empleados = employeesCategory.Union(employees).Distinct(new CompareEmployeeDTO()).OrderBy(e => e.CategoriaId).ToList(),
                Establecimientos = establishments.Select(EstablishmentDTO.Create),
                Turnos = turns.Select(TurnDTO.Create),
                Dias = DateTime.DaysInMonth(shiftWorkPeriod.Year, shiftWorkPeriod.Month),
                Periodo = shiftWorkPeriod,
                PeriodoActual = DateTime.Now,
                Organigrama = structure.Denominacion,
                RolTurno = ShiftWorkDTO.Create(currentShiftWork),
                RolTurnoEstabs = ((currentShiftWork?.RolTurnoEstab.Select(ShiftWorkEstabDTO.Create)) ?? new List<ShiftWorkEstabDTO>())
                    .Concat((approvalShiftWorkEstabs?.Select(ShiftWorkEstabDTO.Create)) ?? new List<ShiftWorkEstabDTO>()),
                Detalles = details?.Select(ShiftWorkDetailDTO.Create) ?? new List<ShiftWorkDetailDTO>(),
                DetallesDeshabilitados = disabledDetails?.Select(ShiftWorkDetailDTO.Create) ?? new List<ShiftWorkDetailDTO>(),
                Categorias = categories.Select(CategoryDTO.Create).ToList()
            };
            return Ok(data);
        }

        private string Message(TipoRolTurnoEnum type)
        {
            return type == TipoRolTurnoEnum.Complementario ? "horas complementarias" : "rol de turnos";
        }
        private string Message(string type)
        {
            return type == $"{TipoRolTurnoEnum.Complementario}" ? "horas complementarias" : "rol de turnos";
        }

        [HttpPost]
        public async Task<ActionResult> ScheduleShiftWork(ShiftWorkVm shiftWorkFront)
        {
            if (User.Identity.Name==null)
            {
                return Unauthorized();
            }
            // Get the user/employee who made the request
            var boss = await _context.Usuario
                .Include(u => u.Empleado)
                .SingleOrDefaultAsync(x => x.Id == int.Parse(User.Identity.Name));

            // Get the position of the employee who made the request
            var position = await _context.Cargo
                .SingleOrDefaultAsync(c => c.Id == boss.Empleado.CargoId);

            // Free the internal cache of Entity Framework
            _context.ChangeTracker.Clear();

            if (!boss.Empleado.Estado)
            {
                return Unauthorized(ErrorVm.Create("El usuario no está activo en el sistema"));
            }

            // Only bosses can schedule shift works
            if (!boss.Empleado.EsJefe)
            {
                return Unauthorized(ErrorVm.Create("Este usuario no puede realizar rol de turnos"));
            }

            if (shiftWorkFront.Estado != EstadoEnum.Creado && shiftWorkFront.Estado != EstadoEnum.Enviado)
            {
                return BadRequest(
                    ErrorVm.Create("El rol de turnos solo se puede enviar a revisión o guardar borrador"));
            }

            // Get the current datetime
            var shiftWorkPeriod = DateTime.Now.AddMonths(1);

            // A helper data structure to storage every employee id of the requested organic structure to his assigned
            // shift work details
            var employeeIdSwd = new Dictionary<int, List<RolTurnoDetalle>>();

            if (shiftWorkFront.EstabDetalles.Count == 0)
            {
                return BadRequest(
                    ErrorVm.Create($"Asegúrese de programar {Message(shiftWorkFront.TipoRolTurno)} para al menos un establecimiento"));
            }

            // Mapping the structure employee ids to the input shift work establishment shift work details
            foreach (var swe in shiftWorkFront.EstabDetalles)
            {
                // Fails if a shift work is sent without shift work details
                if (swe.Detalles.Count == 0)
                {
                    return BadRequest(ErrorVm.Create(
                        "Cada rol de turnos por establecimiento debe tener al menos un empleado programado"));
                }

                foreach (var swd in swe.Detalles)
                {
                    if (employeeIdSwd.ContainsKey(swd.EmpleadoId))
                    {
                        employeeIdSwd[swd.EmpleadoId].Add(swd);
                    }
                    else
                    {
                        employeeIdSwd.Add(swd.EmpleadoId, new List<RolTurnoDetalle> {swd});
                    }
                }
            }

            // Get all employee of the boss organic structure
            var employeesDb = await _context.Empleado
                .Include(e => e.Cargo)
                .Where(e => e.OrganigramaId == boss.Empleado.OrganigramaId && e.Estado)
                .ToListAsync();

            // Get all employee ids of the frontend payload
            var employeesIdFront = employeeIdSwd.Keys;
            // Get all current employees of the front end payload
            var employeesFront = await _context.Empleado
                .Include(e => e.Cargo)
                .Where(e => employeesIdFront.Contains(e.Id))
                .ToListAsync();

            // Check the all employees are in the same organic structure
            foreach (var emp in employeesFront)
            {
                if (emp.OrganigramaId != boss.Empleado.OrganigramaId)
                {
                    return BadRequest(ErrorVm.Create(
                        $"El empleado {emp.ApellidoPaterno} {emp.ApellidoMaterno} {emp.Nombres} no pertenece a la estructura orgánica "));
                }
            }


            var shiftWorkDb = await _context.RolTurno
                                    .Include(rt => rt.RolTurnoEstab)
                                    .ThenInclude(rte => rte.RolTurnoDetalle)
                                    .SingleOrDefaultAsync(rt =>
                                        rt.OrganigramaId == boss.Empleado.OrganigramaId
                                        && rt.Mes == shiftWorkPeriod.Month
                                        && rt.Anio == shiftWorkPeriod.Year
                                        && rt.TipoRolTurno == $"{shiftWorkFront.TipoRolTurno}"
                                        && rt.Estado == $"{EstadoEnum.Enviado}");

            if (shiftWorkDb != null)
            {
                return BadRequest(ErrorVm.Create($"Ya existe {Message(shiftWorkDb.TipoRolTurno)} en estado enviado en su estructura orgánica"));
            }

            /*
             * REGULAR SHIFT WORK
             */
            if (shiftWorkFront.TipoRolTurno == TipoRolTurnoEnum.Regular)
            {
                // Get the master day limit to schedule a shift work
                var parameter = await _context.Parametro
                    .Where(p => p.Llave == _constants.DayLimitKey)
                    .SingleAsync();

                var success = int.TryParse(parameter.Valor, out int limitDay);

                // A shift work can only be scheduled before the master day limit
                if (shiftWorkPeriod.Day > (success ? limitDay : 15))
                {
                    return Unauthorized(ErrorVm.Create("Fecha límite de edición para este rol de turnos y/o horas complementarias"));
                }

                if (shiftWorkFront.Estado == EstadoEnum.Enviado)
                {
/*                  var employeesDbIds = employeesDb.Select(e => e.Id);
                    var employeesIdUnion = employeesDbIds.Union(employeesIdFront).ToList();
                    if (employeesIdUnion.Count != employeesDb.Count || employeesIdUnion.Count != employeesIdFront.Count)
                    {
                        return BadRequest(
                            ErrorVm.Create("Asegúrese de programar rol de turnos para todos los empleados a su cargo"));
                    }
                    */
                    var turnIdTotalHours = new Dictionary<int, int>(
                        await _context.Turno.Select(t => new KeyValuePair<int, int>(t.Id, t.Horas))
                            .ToListAsync());

                    foreach (var eid in employeesIdFront)
                    {
                        var hoursAccumulated = employeeIdSwd[eid]
                            .Aggregate(0, (acc, rtd) => acc + turnIdTotalHours[rtd.TurnoId]);

                        var employee = await _context.Empleado
                            .Include(e => e.CondicionLaboral)
                            .SingleOrDefaultAsync(e => e.Id == eid);

                        if (hoursAccumulated < employee.CondicionLaboral.TotalHoras)
                        {
                            return BadRequest(ErrorVm.Create(
                                $"El empleado {employee.Nombres} {employee.ApellidoPaterno} {employee.ApellidoMaterno} " +
                                $"debe cumplir con las horas de su contrato"));
                        }
                    }
                }

                await SaveShiftWork(shiftWorkFront, TipoRolTurnoEnum.Regular, boss.Empleado, shiftWorkPeriod, boss.Empleado.Id,
                    shiftWorkFront.Estado);
            }
            else if (shiftWorkFront.TipoRolTurno == TipoRolTurnoEnum.Complementario)
            {

                var swsRegular = await _context.RolTurno
                     .AnyAsync(rt => rt.OrganigramaId == boss.Empleado.OrganigramaId
                             && rt.Mes == shiftWorkPeriod.Month
                             && rt.Anio == shiftWorkPeriod.Year
                             && rt.TipoRolTurno == $"{TipoRolTurnoEnum.Regular}"
                             && rt.Estado == $"{EstadoEnum.Aprobado}");

                if (!swsRegular)
                {
                    return BadRequest("No puede programar horas complementarias antes de programar el rol de turnos regular");
                }

                await SaveShiftWork(shiftWorkFront, TipoRolTurnoEnum.Complementario, boss.Empleado, shiftWorkPeriod, boss.Empleado.Id,
                    shiftWorkFront.Estado);
            }

            return NoContent();
        }

        private async Task SaveShiftWork(ShiftWorkVm shiftWorkFront, TipoRolTurnoEnum tipoRolTurno,
            Empleado boss, DateTime shiftWorkPeriod, int bossId, EstadoEnum estado)
        {
            var shiftWorkDb = await _context.RolTurno
                .Include(rt => rt.RolTurnoEstab)
                .ThenInclude(rte => rte.RolTurnoDetalle)
                .SingleOrDefaultAsync(rt =>
                    rt.OrganigramaId == boss.OrganigramaId
                    && rt.Mes == shiftWorkPeriod.Month
                    && rt.Anio == shiftWorkPeriod.Year
                    && rt.TipoRolTurno == tipoRolTurno.ToString()
                    && (rt.Estado == $"{EstadoEnum.Observado}" 
                        || rt.Estado == $"{EstadoEnum.Creado}"));
            
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int? attemptSwId = null;

                if (shiftWorkDb != null)
                {
                    var swEstabsDb = shiftWorkDb.RolTurnoEstab.ToList();

                    foreach (var sweFront in shiftWorkFront.EstabDetalles)
                    {
                        var shiftWorkEstabDb =
                            swEstabsDb.Find(swe => swe.EstablecimientoId == sweFront.EstablecimientoId);
                        if (shiftWorkEstabDb != null)
                        {
                            foreach (var swdfront in sweFront.Detalles)
                            {
                                swdfront.RolTurnoEstabId = shiftWorkEstabDb.Id;
                                swdfront.RolTurnoEstab = null;
                            }

                            _context.RolTurnoDetalle.RemoveRange(shiftWorkEstabDb.RolTurnoDetalle);
                            _context.RolTurnoDetalle.AddRange(sweFront.Detalles);
                            await _context.SaveChangesAsync();

                            swEstabsDb.Remove(shiftWorkEstabDb);
                        }
                        else
                        {
                            var swEstab = new RolTurnoEstab()
                            {
                                EstablecimientoId = sweFront.EstablecimientoId,
                                RolTurnoId = shiftWorkDb.Id
                            };
                            _context.RolTurnoEstab.Add(swEstab);
                            await _context.SaveChangesAsync();

                            foreach (var swdFront in sweFront.Detalles)
                            {
                                swdFront.RolTurnoEstabId = swEstab.Id;
                            }

                            _context.RolTurnoDetalle.AddRange(sweFront.Detalles);
                            await _context.SaveChangesAsync();
                        }
                    }


                    if (swEstabsDb.Count > 0)
                    {
                        foreach (var rte in swEstabsDb)
                        {
                            _context.RolTurnoDetalle.RemoveRange(rte.RolTurnoDetalle);
                            await _context.SaveChangesAsync();
                            _context.RolTurnoEstab.Remove(rte);
                            await _context.SaveChangesAsync();
                        }
                    }

                    shiftWorkDb.Estado = estado.ToString();
                    _context.Entry(shiftWorkDb).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    if (estado == EstadoEnum.Enviado)
                    {
                        attemptSwId = shiftWorkDb.Id;
                    }
                }
                else
                {
                    var shiftWork = new RolTurno()
                    {
                        OrganigramaId = boss.OrganigramaId,
                        TipoRolTurno = shiftWorkFront.TipoRolTurno.ToString(),
                        Mes = shiftWorkPeriod.Month,
                        Anio = shiftWorkPeriod.Year,
                        JefeId = bossId,
                        FechaReg = DateTime.Now,
                        Estado = estado.ToString(),
                    };
                    _context.RolTurno.Add(shiftWork);
                    await _context.SaveChangesAsync();
                    
                    foreach (var sweFront in shiftWorkFront.EstabDetalles)
                    {
                        _context.ChangeTracker.Clear();
                        var shiftWorkEstab = new RolTurnoEstab()
                        {
                            RolTurnoId = shiftWork.Id,
                            EstablecimientoId = sweFront.EstablecimientoId
                        };
                        _context.RolTurnoEstab.Add(shiftWorkEstab);
                        await _context.SaveChangesAsync();

                        foreach (var swdFront in sweFront.Detalles)
                        {
                            swdFront.RolTurnoEstabId = shiftWorkEstab.Id;
                            swdFront.RolTurnoEstab = null;
                            _context.RolTurnoDetalle.Add(swdFront);
                        }

                        await _context.SaveChangesAsync();
                    }

                    if (estado == EstadoEnum.Enviado)
                    {
                        attemptSwId = shiftWork.Id;
                    }
                }

                if (attemptSwId != null)
                {
                    var firstApproval = await _context.RolTurnoAprobador
                        .SingleOrDefaultAsync(rta => rta.TipoRolTurno == $"{tipoRolTurno}" 
                            && rta.AnteriorId == null);

                    var shiftWorkAttempts = new RolTurnoIntento()
                    {
                        RolTurnoId = (int) attemptSwId,
                        FechaEnvio = DateTime.Now,
                        Actual = true,
                        SiguienteAprobadorId = firstApproval.Id
                    };

                    _context.RolTurnoIntento.Add(shiftWorkAttempts);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpGet("{ids}/print")]
        public async Task<ActionResult> PrintShiftWorks(string ids, string type = "Regular")
        {
            List<RolTurno> shiftWorks;
            List<int> swIds;

            if (ids == "*" && type == $"{TipoRolTurnoEnum.Regular}")
            {
                var currentPeriod = DateTime.Now.AddMonths(1);
                shiftWorks = await _context.RolTurno
                    .Include(rt => rt.Organigrama)
                    .ThenInclude(o => o.Padre)
                    .Include(rt => rt.RolTurnoEstab)
                    .ThenInclude(rte => rte.Establecimiento)
                    .Where(rt => rt.Estado == $"{EstadoEnum.Aprobado}" && rt.TipoRolTurno == $"{TipoRolTurnoEnum.Regular}" && rt.Mes == currentPeriod.Month && rt.Anio == currentPeriod.Year).ToListAsync();

                swIds = shiftWorks.Select(rt => rt.Id).ToList();
            }
            else if (ids == "*" && type == $"{TipoRolTurnoEnum.Complementario}")
            {
                var currentPeriod = DateTime.Now.AddMonths(1);
                shiftWorks = await _context.RolTurno
                    .Include(rt => rt.Organigrama)
                    .ThenInclude(o => o.Padre)
                    .Include(rt => rt.RolTurnoEstab)
                    .ThenInclude(rte => rte.Establecimiento)
                    .Where(rt => rt.Estado == $"{EstadoEnum.Aprobado}" && rt.TipoRolTurno == $"{TipoRolTurnoEnum.Complementario}" && rt.Mes == currentPeriod.Month && rt.Anio == currentPeriod.Year).ToListAsync();

                swIds = shiftWorks.Select(rt => rt.Id).ToList();
            }
            else
            {
                swIds = ids.Split(',').Select(swid => int.Parse(swid)).ToList();

                shiftWorks = await _context.RolTurno
                    .Include(rt => rt.Organigrama)
                    .ThenInclude(o => o.Padre)
                    .Include(rt => rt.RolTurnoEstab)
                    .ThenInclude(rte => rte.Establecimiento)
                    .Where(rt => swIds.Contains(rt.Id)).ToListAsync();

                if (shiftWorks.Any(rt => rt.Estado != $"{EstadoEnum.Aprobado}"))
                {
                    if (shiftWorks.Count == 1)
                    {
                        return BadRequest(ErrorVm.Create("EL rol de turnos aún no está aprobado"));
                    }

                    return BadRequest(ErrorVm.Create("Todos los roles de turnos seleccionados deben haber sido aprobados"));
                }
            }

            if (shiftWorks.Count == 0)
            {
                return NotFound();
            }

            var hashIds = new Hashids(_keys.HashIdsKey,6, StaticConstants.Alphabet);
            var fileName = $"R{hashIds.Encode(swIds)}";

            var templatePath = Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Templates/shiftWork.hbs"));
            var templateString = await System.IO.File.ReadAllTextAsync(templatePath);
            var template = Handlebars.Compile(templateString);

            var baseDirectory = _constants.Storage;// await _context.Parametro.SingleOrDefaultAsync(p => p.Llave == _constants.BaseDirectory);

            var shiftWorkYear = shiftWorks.First().Anio.ToString();

            var pdfPath = Path.Join(baseDirectory.AsSpan(), _constants.ShiftWorkPath.AsSpan(), shiftWorkYear.AsSpan(),$"{fileName}.pdf");

            var pdfNotExits = !System.IO.File.Exists(pdfPath);

            if (pdfNotExits)
            {
                var data = new List<ShiftWorkTemplate>();

                var shiftWorkQR = (await _context.Parametro.Where(p => p.Llave == _constants.ShiftWorkQRKey)
                    .SingleOrDefaultAsync()).Valor;
                var qrTemplate = Handlebars.Compile(shiftWorkQR);
                var qrString = qrTemplate(new { BaseUrl = $"{Request.Scheme}://{Request.Host}", FileName = $"{fileName}.pdf" });
                var searchUrl = $"{_constants.HiscomFrontEndUrl}/buscar";

                var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(qrString, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new Base64QRCode(qrCodeData);
                var qrCodeImage = $"data:image/png;base64,{qrCode.GetGraphic(20)}";

                foreach (var rt in shiftWorks)
                {

                    var categories = await _context.Categoria
                        .Where(c => c.OrganigramaId == rt.OrganigramaId)
                        .ToListAsync();

                    var employees = (await _context.Empleado
                        .Include(e => e.Cargo)
                        .Include(e => e.CondicionLaboral)
                        .Include(e => e.RolTurnoDetalle)
                        .ThenInclude(rtd => rtd.RolTurnoEstab)
                        .Include(e => e.CategoriaEmpleado)
                        .ThenInclude(ce => ce.Categoria)
                        .Where(e => e.OrganigramaId == rt.OrganigramaId
                                    && e.RolTurnoDetalle.Any(rtd => rtd.RolTurnoEstab.RolTurnoId == rt.Id))
                        .ToListAsync());

                    var culture = new CultureInfo("es-ES");
                    var swDate = new DateTime(rt.Anio, rt.Mes, 1);
                    var month = swDate.ToString("MMMM", culture);

                    var daysMonth = new List<ShiftWorkTemplateDay>();

                    while (swDate.Month == rt.Mes)
                    {
                        var swtd = new ShiftWorkTemplateDay()
                        {
                            Day = swDate.Day,
                            Abbr = swDate.ToString("ddd", culture).First().ToString().ToUpper()
                        };
                        daysMonth.Add(swtd);
                        swDate = swDate.AddDays(1);
                    }

                    var turnMap = new Dictionary<int, ShiftWorkTemplateTurn>();

                    var establishments = rt.RolTurnoEstab
                        .Select(rte =>
                        {
                            var employeesData = employees.Select(e =>
                            {

                                var totalHours = 0;
                                var turnDetails = _context.RolTurnoDetalle
                                    .Include(rtd => rtd.Turno)
                                    .Where(rtd => rtd.RolTurnoEstabId == rte.Id && rtd.EmpleadoId == e.Id)
                                    .ToList()
                                    .Select(rtd =>
                                    {
                                        totalHours += rtd.Turno.Horas;
                                        if (!turnMap.ContainsKey(rtd.TurnoId))
                                        {
                                            turnMap.Add(rtd.TurnoId, new ShiftWorkTemplateTurn()
                                            {
                                                Denomination = rtd.Turno.Denominacion,
                                                Description = rtd.Turno.Descripcion
                                            });
                                        }
                                        return new KeyValuePair<int, string>(rtd.Dia, rtd.Turno.Denominacion);
                                    });

                                var detailsMap = new Dictionary<int, string>(turnDetails);

                                var details = daysMonth.Select(swtd =>
                                    detailsMap.ContainsKey(swtd.Day) ? detailsMap[swtd.Day] : ""
                                ).ToList();

                                details.Add(totalHours.ToString());
                                var laboralCondition = e.CondicionLaboral.Denominacion;

                                var detailsTemplate = details.Select(d =>
                                {
                                    var dt = new ShiftWorkTemplateDetails()
                                    {
                                        Abbreviation = d,
                                    };
                                    return dt;
                                }).ToList();

                                return new ShiftWorkTemplateEmployee()
                                {
                                    CategoryId = e.CategoriaEmpleado.Select(ce => ce.CategoriaId).SingleOrDefault(),
                                    FullName = $"{e.ApellidoPaterno} {e.ApellidoMaterno} {e.Nombres}",
                                    LaboralCondition = laboralCondition.Length > 4 ? laboralCondition
                                        .Substring(0, 4) : laboralCondition,
                                    Details = detailsTemplate,
                                    TotalHours = totalHours
                                };

                            }).ToList();

                            var categ = employeesData.GroupBy(e => e.CategoryId).Select(ce => new ShiftWorkTemplateCategory()
                            {
                                Denomination = categories.SingleOrDefault(c => c.Id == ce.Key)?.Denominacion?.ToUpper() ?? "SIN CATEGORÍA",
                                Employees = ce.ToList()
                            }).ToList();

                            return new ShiftWorkTemplateEstablishment()
                            {
                                Denomination = rte.Establecimiento.Denominacion.ToUpper(),
                                Categories = categ
                            };
                        })
                        .ToList();

                    var nextStructureId = rt.Organigrama.Id;
                    var levels = new List<ShiftWorkTemplateLevel>();
                    while (nextStructureId > 0)
                    {
                        var st = _context.Organigrama
                            .Include(o => o.Nivel)
                            .SingleOrDefault(o => o.Id == nextStructureId);
                        if (st == null || st.Nivel.Numero == 0)
                        {
                            break;
                        }
                        var swtl = new ShiftWorkTemplateLevel();
                        swtl.Level = st.Nivel.Denominacion;
                        swtl.Denomination = st.Denominacion;
                        levels.Add(swtl);
                        nextStructureId = st.PadreId ?? 0;
                    }

                    levels.Reverse();
                    var headerRows = levels.Count >= 2 ? levels.Count : 2;


                    var revisions = await _context.RolTurnoRevision
                        .Include(rtr => rtr.Usuario)
                        .ThenInclude(u => u.Empleado)
                        .Include(rtr => rtr.RolTurnoIntento)
                        .Include(rtr => rtr.RolTurnoAprobador)
                        .ThenInclude(rta => rta.Aprobador)
                        .Where(rtr => rtr.RolTurnoIntento.Actual
                            && rtr.RolTurnoIntento.RolTurnoId == rt.Id)
                        .ToListAsync();

                    var signatures = revisions.Select(rtr =>
                    {
                        var approval = _context.Usuario.Include(e => e.Empleado)
                        .ThenInclude(e => e.Organigrama)
                        .ThenInclude(o => o.Nivel)
                        .SingleOrDefault(u => u.Id == rtr.UsuarioId);

                        var denomination = rtr.RolTurnoAprobador.AprobadorPadre
                        ? approval.Empleado.Organigrama.Nivel.Denominacion
                        : approval.Empleado.Organigrama.Denominacion;

                        return new ShiftWorkTemplateSignature()
                        {
                            Denomination = denomination,
                            Dashes = new string('-', denomination.Length > 20 ? 20 : denomination.Length)
                        };
                    }).ToList();


                    data.Add(new ShiftWorkTemplate()
                    {
                        Structure = rt.Organigrama.Denominacion,
                        Month = char.ToUpper(month.First()) + month.Substring(1),
                        Year = rt.Anio,
                        Type = rt.TipoRolTurno == $"{TipoRolTurnoEnum.Regular}"
                            ? "ROL DE TURNOS Y HORARIOS"
                            : "HORAS COMPLEMENTARIAS",
                        Approvals = revisions.OrderByDescending(rtr => rtr.Fecha)
                            .Select(rtr => new ShiftWorkTemplateApprovals
                            {
                                Approver = rtr.RolTurnoAprobador.AprobadorPadre
                                    ? $"{culture.TextInfo.ToTitleCase(rt.Organigrama.Padre.Denominacion.ToLower())} - {rtr.Usuario.Empleado.ApellidoPaterno} {rtr.Usuario.Empleado.ApellidoMaterno} {rtr.Usuario.Empleado.Nombres}"
                                    : $" {rtr.RolTurnoAprobador.Aprobador.Denominacion} - { rtr.Usuario.Empleado.ApellidoPaterno } { rtr.Usuario.Empleado.ApellidoMaterno } { rtr.Usuario.Empleado.Nombres }",
                                Date = rtr.Fecha.ToString("G", culture)
                            })
                            .ToList(),
                        Establishments = establishments,
                        Days = daysMonth,
                        Turns = turnMap.Values.ToList(),
                        Signatures = signatures,
                        Levels = levels,
                        HeaderHeight = headerRows * 15,
                        QrCode = qrCodeImage,
                        SearchUrl = searchUrl,
                        FileName = fileName
                    });
                }
                var htmlString = template(data);

                new FileInfo(pdfPath).Directory?.Create();
                await using FileStream pdfDest = System.IO.File.Open(pdfPath, FileMode.OpenOrCreate);
                var pdf = new PdfDocument(new PdfWriter(pdfDest));
                var pageSize = PageSize.A4.Rotate();
                pdf.SetDefaultPageSize(pageSize);

                var props = new ConverterProperties();
                //var device = new MediaDeviceDescription(MediaType.PRINT);
                //props.SetMediaDeviceDescription(device);
                HtmlConverter.ConvertToPdf(htmlString, pdf, props);
            }

            return Ok(new
            {
                Url = $"{Request.Scheme}://{Request.Host}/{_constants.ShiftWorkPath}/{shiftWorkYear}/{fileName}.pdf",
                FirstCreated = pdfNotExits
            });
        }

        [HttpPut("review")]
        public async Task<IActionResult> CheckShiftWork(PutShiftWorkVm shiftWorkVm)
        {
            if (User.Identity?.Name==null)
            {
                return Forbid();
            }

            var userId = int.Parse(User.Identity.Name);
            var approval = await _context.Usuario
                .Include(u => u.UsuarioRol)
                .Include(u=>u.Empleado)
                .SingleOrDefaultAsync(u => u.Id == userId);

            if (approval == null)
            {
                return Forbid();
            }

            if (shiftWorkVm.Estado!=EstadoEnum.Aprobado && shiftWorkVm.Estado!=EstadoEnum.Observado)
            {
                return BadRequest(ErrorVm.Create("Solo se puede aprobar u observar el rol de turnos o rol de turnos y/o horas complementarias"));
            }
            
            var shiftworkdb = await _context.RolTurno
                .Include(rt=>rt.Organigrama)
                .Include(rt => rt.Jefe)
                .ThenInclude(j=>j.Usuario)
                .SingleOrDefaultAsync(rt => rt.Id == shiftWorkVm.Id);

            if (shiftworkdb == null)
            {
                return NotFound();
            }

            if (shiftworkdb.Estado!=$"{EstadoEnum.Enviado}")
            {
                return BadRequest(ErrorVm.Create("El rol de turnos y/o horas complementarias debe haber sido enviado por el jefe a cargo"));
            }

            var currentAttempt = await _context.RolTurnoIntento
                .SingleOrDefaultAsync(rti => rti.RolTurnoId == shiftworkdb.Id && rti.Actual);

            if (currentAttempt==null)
            {
                return BadRequest(ErrorVm.Create("Este rol de turnos y/o horas complementarias aún no puede pasar a revisión"));
            }

            var reviews = await _context.RolTurnoRevision
                .Include(rtr => rtr.RolTurnoIntento)
                .Include(rtr=> rtr.RolTurnoAprobador)
                .Where(rtr => rtr.RolTurnoIntento.Id == currentAttempt.Id)
                .OrderBy(rtr => rtr.Fecha)
                .ToListAsync();

            RolTurnoAprobador currentApproval;

            if (reviews.Count > 0)
            {
                var lastReview = reviews.Last();

                if (lastReview.RolTurnoAprobador.SiguienteId == null)
                {
                    return BadRequest(ErrorVm.Create("Este rol de turnos y/o horas complementarias ya no permite más revisiones"));
                }

                currentApproval = await _context.RolTurnoAprobador
                    .Include(rta => rta.Aprobador)
                    .Include(rta => rta.Anterior)
                    .Include(rta => rta.Siguiente)
                    .SingleOrDefaultAsync(rta => rta.Id == lastReview.RolTurnoAprobador.SiguienteId);
            } 
            else
            {
                currentApproval = await _context.RolTurnoAprobador
                    .Include(rta => rta.Anterior)
                    .Include(rta => rta.Siguiente)
                    .Include(rta => rta.Aprobador)
                    .SingleOrDefaultAsync(rta => rta.TipoRolTurno == shiftworkdb.TipoRolTurno 
                        && rta.AnteriorId == null);

                var childrenStructures = await _context.Organigrama
                        .Where(o => o.PadreId == approval.Empleado.OrganigramaId)
                        .ToListAsync();

                var swOChartIsChild = childrenStructures.Any(o => o.Id == shiftworkdb.OrganigramaId);

                if (!swOChartIsChild)
                {
                    while (currentApproval.AprobadorPadre) {
                        currentApproval = await _context.RolTurnoAprobador
                            .Include(rta => rta.Anterior)
                            .Include(rta => rta.Siguiente)
                            .Include(rta => rta.Aprobador)
                            .SingleOrDefaultAsync(rta => rta.Id == currentApproval.SiguienteId);

                        if (currentApproval == null) {
                            return BadRequest(ErrorVm.Create("Este rol de turnos y/o horas complementarias ya no permite más revisiones"));
                        }
                    }
                }
            }

            if (!approval.UsuarioRol.Any(ur => ur.RolId == currentApproval.AprobadorId))
            {
                return BadRequest(ErrorVm.Create("Este usuario no tiene permiso de aprobar o rechazar el rol de turnos y/o horas complementarias"));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (currentApproval.Siguiente == null)
                {
                    shiftworkdb.Estado = $"{shiftWorkVm.Estado}";

                    if (shiftWorkVm.Estado == EstadoEnum.Aprobado)
                    {
                        currentAttempt.SiguienteAprobadorId = null;
                        currentAttempt.FechaCierre = DateTime.Now;
                    } 
                    else
                    {
                        var first = await _context.RolTurnoAprobador
                            .SingleOrDefaultAsync(rta => rta.TipoRolTurno == shiftworkdb.TipoRolTurno 
                                && rta.AnteriorId == null);
                        currentAttempt.SiguienteAprobadorId = first.Id;
                    }                 
                } 
                else
                {
                    currentAttempt.SiguienteAprobadorId = currentApproval.Siguiente.Id;
                }

                if (shiftWorkVm.Estado == EstadoEnum.Observado)
                {
                    shiftworkdb.Estado = $"{EstadoEnum.Observado}";
                    shiftworkdb.Observacion = shiftWorkVm.Observacion;
                    currentAttempt.Actual = false;
                    currentAttempt.FechaCierre = DateTime.Now;
                } 
                else 
                {
                    shiftworkdb.Observacion = null;
                }

                shiftworkdb.FechaMod = DateTime.Now;
                _context.Entry(shiftworkdb).State = EntityState.Modified;
                _context.Entry(currentAttempt).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                var swRevision = new RolTurnoRevision

                {
                    RolTurnoIntentoId = currentAttempt.Id,
                    RolTurnoAprobadorId = currentApproval.Id,
                    UsuarioId = userId,
                    Fecha = DateTime.Now,
                    Estado = $"{shiftWorkVm.Estado}",
                    Observacion = shiftWorkVm.Observacion
                };

                _context.RolTurnoRevision.Add(swRevision);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }


/*            if (shiftworkdb.Estado == $"{EstadoEnum.Observado}")
            {

                if (shiftworkdb.Jefe != null)
                {
                    var culture = new System.Globalization.CultureInfo("es-ES");
                    var month = new DateTime(shiftworkdb.Ano, shiftworkdb.Mes, 1).ToString("MMMM", culture);
                    await EmailUtility.SendEmail(_emailCredentials, $"HRA-{currentApproval.Aprobador.Denominacion}",
                        $"Observación en la programación de {Message(shiftworkdb.TipoRolTurno)}", new Dictionary<string, string>()
                        {
                            {
                                $"{shiftworkdb.Jefe.ApellidoPaterno} {shiftworkdb.Jefe.ApellidoMaterno} {shiftworkdb.Jefe.Nombres}",
                                shiftworkdb.Jefe.Usuario.Correo
                            }
                        },
                        $"<div>Hola, {shiftworkdb.Jefe.Usuario.NombreUsuario},</div><div>Su programación de rol de turnos para {month} del {shiftworkdb.Ano} tiene la siguiente observación:</div>" +
                        $"<div>{shiftWorkVm.Observacion}</div>");
                }
            }*/

            return NoContent();
        }

        // DELETE: api/ShiftWork/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShiftWork(int id)
        {
            var shiftWork = await _context.RolTurno.FindAsync(id);
            if (shiftWork == null)
            {
                return NotFound();
            }

            _context.RolTurno.Remove(shiftWork);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RolTurnoExists(int id)
        {
            return _context.RolTurno.Any(e => e.Id == id);
        }
    }
}