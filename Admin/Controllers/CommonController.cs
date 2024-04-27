using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Admin.DTO;
using Admin.Models;
using Admin.Templates;
using Algolia.Search.Clients;
using Domain.Models;
using HandlebarsDotNet;
using HashidsNet;
using iText.Html2pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using Lizelaser0310.Utilities;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MoreLinq;
using QRCoder;
using Path = System.IO.Path;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly Dictionary<int, HashSet<string>> _authCache;
        private readonly IKeys _keys;
        private readonly IConstants _constants;

        public CommonController(HISCOMContext db,
            IKeys keys, IConstants constants, Dictionary<int, HashSet<string>> authCache)
        {
            _context = db;
            _keys = keys;
            _constants = constants;
            _authCache = authCache;
        }

        // Authentication methods

        [HttpGet]
        [Route("get-user")]
        public async Task<ActionResult<Usuario>> GetUser()
        {
            var user = await _context.Usuario
                .Include(x => x.Empleado)
                .ThenInclude(e=>e.Organigrama)
                .Where(x => x.Id == int.Parse(User.Identity.Name))
                .SingleOrDefaultAsync();

            if (user == null) return Unauthorized("El usuario no existe");


            ImageUtility.CreateImageUrl(user, Request, "Foto", _constants.ImagePath);

            return Ok(UserDTO.Create(user));
        }

        [HttpGet]
        [Route("manage-profile")]
        public async Task<ActionResult<Usuario>> ManageProfile()
        {
            if (User.Identity?.Name==null)
            {
                return Unauthorized(ErrorVm.Create("El usuario solicitado no existe"));
            }
            var userid = int.Parse(User.Identity.Name);

            var user = await _context.Usuario
                .Include(x => x.Empleado)
                .Where(x => x.Id == userid)
                .SingleOrDefaultAsync();

            if (user == null) return BadRequest("El usuario no existe");


            ImageUtility.CreateImageUrl(user, Request, "Foto", _constants.ImagePath);

            return Ok(ProfileDTO.Create(user));
        }

        [HttpPost]
        [Route("change-password")]
        public async Task<ActionResult<Usuario>> ChangePassword(ProfileDTO credentials)
        {
            if (User.Identity?.Name == null)
            {
                return Unauthorized("El usuario no existe");
            }

            int userid = int.Parse(User.Identity?.Name);
            var user = await (from u in _context.Usuario where u.Id.Equals(userid) select u).SingleOrDefaultAsync();

            if (string.IsNullOrEmpty(credentials.ContrasenaActual) || string.IsNullOrEmpty(credentials.ContrasenaNueva))
            {
                return BadRequest(ErrorVm.Create("Los campos contraseña actual y contraseña nueva son requeridos"));
            }

            if (user == null || !AuthUtility.VerifyPassword(credentials.ContrasenaActual, user.Contrasena,
                _keys.EncryptionKey))
            {
                return BadRequest("Hubo un error al actualizar su contraseña, verifique que la contraseña actual sea la correcta");
            }

            user.Contrasena = AuthUtility.HashPassword(credentials.ContrasenaNueva, _keys.EncryptionKey);
            user.FechaMod = DateTime.Now;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok();

        }

        [HttpGet]
        [Route("reset-password")]
        public async Task<ActionResult> ResetPassword(string email)
        {
            var user = await (from u in _context.Usuario
                       where u.Correo.Equals(email) || u.NombreUsuario.Equals(email)
                       select u)
                    .FirstOrDefaultAsync();
            if (user != null)
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Lizeth La Serna", "lizssdhdd@zohomail.com"));
                message.To.Add(new MailboxAddress(user.NombreUsuario, user.Correo));
                message.Subject = "Solicitud de cambio de contraseña";

                var builder = new BodyBuilder();
                builder.HtmlBody =
                    $"<div>Hola, {user.NombreUsuario}:</div><div>Ingrese a este <a href=\"https://google.com\">enlace</a> para recuperar su contraseña.</div>";


                message.Body = builder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.zoho.com", 465, true);

                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate("lizssdhdd@zohomail.com", "OtLcCtkbOyK8iye");
                    await client.SendAsync(message);
                    client.Disconnect(true);
                }

                return Ok();
            }

            return BadRequest("El usuario no existe");
        }

        // Validation
        // GET: api/common/validate?path=
        [HttpGet("validate/")]
        public ActionResult ValidatePermission(string path)
        {
            //var ruta = path.Split("/");

            var idFormatted = Regex.Replace(path, @"\/\d+", "/*");
            var formatted = Regex.Replace(idFormatted, @"\/$", "");

            if (User.Identity?.Name == null)            
                return Unauthorized();
            //if (ruta?[1] == null)
            //    return BadRequest();
            
            //path = "/" + ruta[1];
            int userId = int.Parse(User.Identity.Name);
            if (!_authCache[userId].Contains(formatted))           
                return Unauthorized();
                                          

            return NoContent();
        }

        [HttpGet("menu/permissions")]
        public async Task<ActionResult> MenuWithPermission()
        {
            var menus = await _context.Menu.Include(m => m.Permiso)
                .Distinct()
                .ToListAsync();

            var result = new List<MenuPVm>();

            foreach (var menu in menus)
            {
                var submenus = new List<SubmenuVm>();
                foreach (var permiso in menu.Permiso)
                {
                    var idx = submenus.FindIndex(s => s.Nombre == permiso.SubMenu);

                    if (idx < 0)
                    {
                        var submenu = new SubmenuVm()
                        {
                            Nombre = permiso.SubMenu,
                            Permisos = new List<Permiso>()
                        };
                        submenu.Permisos.Add(permiso);
                        submenus.Add(submenu);
                    }
                    else
                    {
                        submenus[idx].Permisos.Add(permiso);
                    }
                }

                var menuPermission = new MenuPVm()
                {
                    Nombre = menu.Nombre,
                    Icono = menu.Icono,
                    Submenus = submenus
                };

                result.Add(menuPermission);
            }

            return Ok(result);
        }

        [HttpGet("submenus/")]
        public async Task<ActionResult> GetSubmenu()
        {
            var submenus = await _context.Permiso
                .Select(x => x.SubMenu)
                .Distinct()
                .ToListAsync();

            return Ok(submenus);
        }

        [HttpGet("role/{id}/permissions")]
        public async Task<ActionResult> PermissionByRole(int id)
        {
            var permissions = await (from p in _context.Permiso
                                     join rp in _context.RolPermiso
                                         on p.Id equals rp.PermisoId
                                     where rp.RolId == id
                                     select p).ToListAsync();

            return Ok(permissions);
        }

        


        // Navigation
        // GET: api/common/navigation/
        [HttpGet("navigation/")]
        public async Task<ActionResult<IEnumerable<Permiso>>> GetNavMenu()
        {
            if (User.Identity?.Name == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(User.Identity.Name);
            Usuario user = await _context.Usuario.FindAsync(userId);

            if (user == null)
            {
                return BadRequest("El usuario no existe");
            }

            var menus = await (from m in _context.Menu
                               join p in _context.Permiso
                                   on m.Id equals p.MenuId
                               join rp in _context.RolPermiso
                                   on p.Id equals rp.PermisoId
                               join ur in _context.UsuarioRol
                                   on rp.RolId equals ur.RolId
                               where ur.UsuarioId == userId
                               select m)
                .Distinct()
                .ToListAsync();

            foreach (var menu in menus)
            {
                var permissions = await (from p in _context.Permiso
                                         join rp in _context.RolPermiso
                                             on p.Id equals rp.PermisoId
                                         join ur in _context.UsuarioRol
                                             on rp.RolId equals ur.RolId
                                         where p.MenuId == menu.Id && ur.UsuarioId == userId
                                         where p.Visible && p.Estado
                                         select p).Distinct().ToListAsync();

                var managePermission = permissions
                    .SingleOrDefault(p => p.Accion == nameof(ShiftWorkController.ScheduleShiftWork));

                if (managePermission != null)
                {
                    var parameter = await _context.Parametro
                        .Where(p => p.Llave == _constants.DayLimitKey)
                        .SingleAsync();

                    var success = int.TryParse(parameter.Valor, out int limitDay);

                    if (DateTime.Now.Day > (success ? limitDay : 15))
                    {
                        permissions.Remove(managePermission);
                    }
                }

                menu.Permiso = permissions;
            }

            return Ok(menus);
        }

        private static List<OChartVm> GetChildren(List<Organigrama> hierarchies, int fatherId)
        {
            return hierarchies
                .Where(n => n.PadreId == fatherId)
                .Select(n => new OChartVm(n, GetChildren(hierarchies, n.Id)))
                .ToList();
        }

        // Level Type List
        // GET: api/common/
        [HttpGet("organization-chart/hierarchy")]
        public async Task<ActionResult<Nivel>> GetOChartHierarchy()
        {
            var oChart = await (_context.Organigrama).ToListAsync();
            var result = await (from n in _context.Organigrama where n.PadreId == null select n)
                .Select(n => new OChartVm(n, GetChildren(oChart, n.Id)))
                .ToListAsync();
            return Ok(result);
        }

        // Level Type List
        // GET: api/common/
        [HttpGet("turns")]
        public async Task<ActionResult<Nivel>> GetTurns()
        {
            var turns = await (_context.Turno.Where(t=>t.Estado)).ToListAsync();

            var result = turns.Select(t=> {
                var turnVm = TurnDTO.Create(t);
                return turnVm;
            }).ToList();
            
            return Ok(result);
        }


        // GET: api/marital-status
        [HttpGet("marital-status")]
        public async Task<ActionResult<Paginator<EstadoCivil>>> GetMaritalStatus()
        {
            return await PaginationUtility.Paginate
            (
                query: Request.QueryString.Value,
                dbSet: _context.EstadoCivil
            );
        }


        [HttpGet("shift-work/{id}")]
        public async Task<ActionResult> GetShiftWorkData(int id)
        {
            var shiftWork = await _context.RolTurno
                .Include(rt => rt.Organigrama)
                .Include(rt => rt.RolTurnoEstab)
                .ThenInclude(rte => rte.Establecimiento)
                .Include(rt => rt.RolTurnoEstab)
                .ThenInclude(rte => rte.RolTurnoDetalle)
                .Include(rt => rt.RolTurnoIntento)
                .ThenInclude(rti => rti.SiguienteAprobador)
                .SingleOrDefaultAsync(rt => rt.Id == id);


            if (shiftWork == null)
            {
                return NotFound();
            }


            var turns = (await _context.Turno.ToListAsync()).Select(t =>
            {
                t.RolTurnoDetalle = null;
                return t;
            });

            var categories = await _context.Categoria
                .Where(c => c.OrganigramaId == shiftWork.OrganigramaId)
                .ToListAsync();

            var employees = await _context.Empleado
                .Include(e => e.RolTurnoDetalle)
                .ThenInclude(rtd => rtd.RolTurnoEstab)
                .Include(e => e.CondicionLaboral)
                .Include(e => e.Cargo)
                .Include(e => e.Profesion)
                .Where(e => e.OrganigramaId == shiftWork.OrganigramaId && e.RolTurnoDetalle.Any(rtd => rtd.RolTurnoEstab.RolTurnoId == id))
                .Select(emp => CategoryEmployeeDTO.Create(emp, null, null)).ToListAsync();

            var employeesCategory = await _context.CategoriaEmpleado
                .Include(ec => ec.Empleado)
                .ThenInclude(e => e.CondicionLaboral)
                .Include(ce => ce.Empleado)
                .ThenInclude(e => e.Profesion)
                .Include(ce => ce.Empleado)
                .ThenInclude(e => e.Cargo)
                .Include(e=>e.Empleado)
                .ThenInclude(e => e.RolTurnoDetalle)
                .ThenInclude(rtd => rtd.RolTurnoEstab)
                .Include(ce => ce.Categoria)
                .Where(ec => ec.Empleado.OrganigramaId == shiftWork.OrganigramaId && ec.Empleado.RolTurnoDetalle.Any(rtd => rtd.RolTurnoEstab.RolTurnoId == id))
                .Select(ec => CategoryEmployeeDTO.Create(ec.Empleado, ec.CategoriaId, ec.Categoria.Denominacion))
                .ToListAsync();

            _context.ChangeTracker.Clear();


/*            foreach (var item in shiftWork.RolTurnoEstab)
            {
                item.Establecimiento.RolTurnoEstab = null;
                item.RolTurno = null;

            }*/

            var isApproval = false;

            if (User.Identity?.Name != null)
            {
                var userId = int.Parse(User.Identity.Name);
                var userRoles = await _context.UsuarioRol.Where(ur => ur.UsuarioId == userId).ToListAsync();

                var currentAttemp = shiftWork.RolTurnoIntento.SingleOrDefault(rti => rti.Actual);

                if (currentAttemp?.SiguienteAprobador != null)
                {
                    isApproval = userRoles.Any(ur => ur.RolId == currentAttemp.SiguienteAprobador.AprobadorId);
                }
            }

            return Ok(new
            {
                Organigrama = shiftWork.Organigrama.Denominacion,
                Periodo = new DateTime(shiftWork.Anio, shiftWork.Mes, 1),
                Turnos = turns,
                Dias = DateTime.DaysInMonth(shiftWork.Anio, shiftWork.Mes),
                Empleados = employeesCategory.Union(employees).Distinct(new CompareCategoryEmployeeDTO()).OrderBy(e => e.CategoriaId).ToList(),
                RolTurnoEstabs = shiftWork.RolTurnoEstab.Select(ShiftWorkEstabDTO.Create).ToList(),
                Categorias = categories.Select(CategoryDTO.Create).ToList(),
                Tipo = shiftWork.TipoRolTurno,
                EsSiguienteAprobador = isApproval,
            });
        }

        // List<T> item => List<R>

        // GET: api/doc-type
        [HttpGet("doc-type")]
        public async Task<ActionResult<Paginator<Clasificacion>>> GetDocType()
        {
            var types = await _context.TipoDocumento.ToListAsync();
            return Ok(types);
        }

        // GET: api/account-type
        [HttpGet("account-type")]
        public async Task<ActionResult<Paginator<Clasificacion>>> GetAccountType()
        {
            var types = await _context.TipoCuenta.ToListAsync();
            return Ok(types);
        }

        // GET: api/Notification
        [HttpGet("notification")]
        public async Task<ActionResult<Notificacion>> GetNotification()
        {
            if (User.Identity?.Name == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(User.Identity.Name);
            var user = await _context.Usuario
                .SingleOrDefaultAsync(u => u.Id == userId);
            var notifications = await _context.Notificacion
                .Where(n => n.EmpleadoId == user.EmpleadoId)
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpGet("shift-work/check-print")]
        public async Task<ActionResult> CheckPrintReport(){

            if (User.Identity?.Name==null) {
                return Unauthorized();
            }
            var userId = int.Parse(User.Identity.Name);

            var canPrint = await (from p in _context.Permiso
                                     join rp in _context.RolPermiso
                                         on p.Id equals rp.PermisoId
                                     join ur in _context.UsuarioRol
                                         on rp.RolId equals ur.RolId
                                     where ur.UsuarioId == userId && p.Estado
                                         && p.Accion.Equals(nameof(ShiftWorkController.PrintShiftWorks))
                                     select p).AnyAsync();
            return Ok(canPrint);
        }

        [HttpGet("shift-work/{id}/print")]
        public async Task<ActionResult> PrintShiftWork(int id)
        {
            var shiftWork = await _context.RolTurno
                .Include(rt => rt.Organigrama)
                .ThenInclude(o => o.Padre)
                .Include(rt => rt.RolTurnoEstab)
                .ThenInclude(rte => rte.Establecimiento)
                .SingleOrDefaultAsync(rt => rt.Id == id);
            
            if (shiftWork == null)
            {
                return NotFound();
            }

            if (shiftWork.Estado != $"{EstadoEnum.Aprobado}")
            {
                return BadRequest(ErrorVm.Create("EL rol de turnos aún no está aprobado"));
            }

            var hashIds = new Hashids(_keys.HashIdsKey,6, StaticConstants.Alphabet);
            var fileName = $"R{hashIds.Encode(shiftWork.Id)}";

            var templatePath = Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Templates/shiftWork.hbs"));
            var templateString = await System.IO.File.ReadAllTextAsync(templatePath);
            var template = Handlebars.Compile(templateString);

            var baseDirectory = _constants.Storage;// await _context.Parametro.SingleOrDefaultAsync(p => p.Llave == _constants.BaseDirectory);

            var shiftWorkYear = shiftWork.Anio.ToString();

            var pdfPath = Path.Join(baseDirectory.AsSpan(), _constants.ShiftWorkPath.AsSpan(), shiftWorkYear.AsSpan(),$"{fileName}.pdf");

            var pdfNotExits = !System.IO.File.Exists(pdfPath);

            if (pdfNotExits)
            {

                var categories = await _context.Categoria
                    .Where(c => c.OrganigramaId == shiftWork.OrganigramaId)
                    .ToListAsync();

                var employees = (await _context.Empleado
                    .Include(e => e.Cargo)
                    .Include(e => e.CondicionLaboral)
                    .Include(e => e.RolTurnoDetalle)
                    .ThenInclude(rtd => rtd.RolTurnoEstab)
                    .Include(e=>e.CategoriaEmpleado)
                    .ThenInclude(ce=>ce.Categoria)
                    .Where(e => e.OrganigramaId == shiftWork.OrganigramaId
                                && e.RolTurnoDetalle.Any(rtd => rtd.RolTurnoEstab.RolTurnoId == id))
                    .ToListAsync());

                var culture = new CultureInfo("es-ES");
                var swDate = new DateTime(shiftWork.Anio, shiftWork.Mes, 1);
                var month = swDate.ToString("MMMM", culture);

                var daysMonth = new List<ShiftWorkTemplateDay>();

                while (swDate.Month == shiftWork.Mes)
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

                var establishments = shiftWork.RolTurnoEstab
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

                            var detailsTemplate = details.Select(d=> {
                                var dt = new ShiftWorkTemplateDetails() {
                                    Abbreviation = d,
                                };
                                return dt;
                            }).ToList();

                            var laboralCondition = e.CondicionLaboral.Denominacion;

                            return new ShiftWorkTemplateEmployee()
                            {
                                CategoryId = e.CategoriaEmpleado.Select(ce=>ce.CategoriaId).SingleOrDefault(),
                                FullName = $"{e.ApellidoPaterno} {e.ApellidoMaterno} {e.Nombres}",
                                LaboralCondition = laboralCondition.Length > 4 ? laboralCondition
                                    .Substring(0, 4) : laboralCondition,
                                Details = detailsTemplate,
                                TotalHours = totalHours
                            };

                        }).OrderBy(e=>e.CategoryId).ToList();

                        var categ = employeesData.GroupBy(e => e.CategoryId).Select(ce => new ShiftWorkTemplateCategory()
                        {
                            Denomination = categories.SingleOrDefault(c=>c.Id==ce.Key)?.Denominacion?.ToUpper()??"SIN CATEGORÍA",
                            Employees = ce.ToList()
                        }).ToList();
                        
                        return new ShiftWorkTemplateEstablishment()
                        {
                            Denomination = rte.Establecimiento.Denominacion.ToUpper(),
                            Categories = categ
                        };

                    })
                    .ToList();

                var nextStructureId = shiftWork.Organigrama.Id;
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
                        && rtr.RolTurnoIntento.RolTurnoId == shiftWork.Id)
                    .ToListAsync();

                var signatures = revisions.Select(rtr => {
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

                //var shiftWorkQR = (await _context.Parametro.Where(p => p.Llave == _constants.ShiftWorkQRKey)
                //    .SingleOrDefaultAsync()).Valor;
                //var qrTemplate = Handlebars.Compile(shiftWorkQR);
                //var qrString = qrTemplate(new { BaseUrl = $"{Request.Scheme}://{Request.Host}", FileName = $"{fileName}.pdf" });
                
                string qrString = $"{Request.Scheme}://{Request.Host}/{_constants.ShiftWorkPath}/{shiftWorkYear}/{fileName}.pdf";
                var searchUrl = $"{_constants.HiscomFrontEndUrl}/buscar";

                var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(qrString, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new Base64QRCode(qrCodeData);
                var qrCodeImage = $"data:image/png;base64,{qrCode.GetGraphic(20)}";


                var appr = new List<ShiftWorkTemplateApprovals>();


                var approvals = revisions.OrderByDescending(rtr => rtr.Fecha)
                    .Select(rtr => new ShiftWorkTemplateApprovals
                    {
                        Approver = rtr.RolTurnoAprobador.AprobadorPadre
                            ? $"{culture.TextInfo.ToTitleCase(shiftWork.Organigrama.Padre.Denominacion.ToLower())} - {rtr.Usuario.Empleado.ApellidoPaterno} {rtr.Usuario.Empleado.ApellidoMaterno} {rtr.Usuario.Empleado.Nombres}"
                            : $" {rtr.RolTurnoAprobador.Aprobador.Denominacion} - {rtr.Usuario.Empleado.ApellidoPaterno} {rtr.Usuario.Empleado.ApellidoMaterno} {rtr.Usuario.Empleado.Nombres}",
                        Date = rtr.Fecha.ToString("G", culture)
                    });

                var bossApproval = _context.RolTurnoIntento.Include(rti => rti.RolTurno)
                    .ThenInclude(rt => rt.Jefe)
                    .Where(rti => rti.RolTurnoId == shiftWork.Id && rti.Actual)
                    .Select(rti => new ShiftWorkTemplateApprovals()
                    {
                        Approver = $"{ culture.TextInfo.ToTitleCase(shiftWork.Organigrama.Denominacion.ToLower()) } - {rti.RolTurno.Jefe.ApellidoPaterno} {rti.RolTurno.Jefe.ApellidoMaterno} {rti.RolTurno.Jefe.Nombres}",
                        Date = rti.FechaEnvio.ToString("G", culture)
                    }).SingleOrDefault();

                appr.Add(bossApproval);
                appr.AddRange(approvals);

                var data = new List<ShiftWorkTemplate>()
                {
                    new(){
                        Structure = shiftWork.Organigrama.Denominacion,
                        Month = char.ToUpper(month.First()) + month.Substring(1),
                        Year = shiftWork.Anio,
                        Type = shiftWork.TipoRolTurno == $"{TipoRolTurnoEnum.Regular}"
                            ? "ROL DE TURNOS"
                            : "HORAS COMPLEMENTARIAS",
                        Approvals = appr.OrderBy(approval=>approval.Date).ToList(),
                        Establishments = establishments,
                        Days = daysMonth,
                        Turns = turnMap.Values.ToList(),
                        Signatures = signatures,
                        Levels = levels,
                        HeaderHeight = headerRows * 15,
                        QrCode = qrCodeImage,
                        SearchUrl = searchUrl,
                        FileName = fileName,
                        
                    }
                };
                
                
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
        
        [HttpGet("shift-work/{id}/print-consolidated")]
        public async Task<ActionResult> PrintConsolidatedShiftWork(int id)
        {
            var shiftWork = await _context.RolTurno
                .Include(rt => rt.Organigrama)
                .ThenInclude(o => o.Padre)
                .Include(rt => rt.RolTurnoEstab)
                .ThenInclude(rte => rte.Establecimiento)
                .SingleOrDefaultAsync(rt => rt.Id == id);

            RolTurno shiftWorkComplementary = null;

            if (shiftWork == null)
            {
                return NotFound();
            }

            if (shiftWork.TipoRolTurno != $"{TipoRolTurnoEnum.Regular}")
            {
                return BadRequest(ErrorVm.Create("No se puede imprimir el consolidado a partir de las horas complementarias"));
            }

            if (shiftWork.Estado != $"{EstadoEnum.Aprobado}")
            {
                return BadRequest(ErrorVm.Create("EL rol de turnos aún no está aprobado"));
            }
            
            shiftWorkComplementary = await _context.RolTurno
                .Include(rt => rt.Organigrama)
                .ThenInclude(o => o.Padre)
                .Include(rt => rt.RolTurnoEstab)
                .ThenInclude(rte => rte.Establecimiento)
                .SingleOrDefaultAsync(rt=>rt.OrganigramaId==shiftWork.OrganigramaId 
                                          && rt.TipoRolTurno==$"{TipoRolTurnoEnum.Complementario}" 
                                          && rt.Mes == shiftWork.Mes 
                                          && rt.Anio == shiftWork.Anio);
            
                
            if (shiftWorkComplementary?.Estado != $"{EstadoEnum.Aprobado}")
            {
                return BadRequest(ErrorVm.Create("EL rol de turnos de horas complementarias aún no está aprobado"));
            }
            
            var hashIds = new Hashids(_keys.HashIdsKey,6, StaticConstants.Alphabet);
            var fileName = $"R{hashIds.Encode(shiftWork.Id, shiftWorkComplementary.Id)}";

            var templatePath = Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Templates/shiftWork.hbs"));
            var templateString = await System.IO.File.ReadAllTextAsync(templatePath);
            var template = Handlebars.Compile(templateString);

            var baseDirectory = _constants.Storage; //await _context.Parametro.SingleOrDefaultAsync(p=>p.Llave== _constants.BaseDirectory);
            
            var shiftWorkYear = shiftWork.Anio.ToString();

            var pdfPath = Path.Join(baseDirectory.AsSpan(), _constants.ShiftWorkPath.AsSpan(), shiftWorkYear.AsSpan(),$"{fileName}.pdf");

            var pdfNotExits = !System.IO.File.Exists(pdfPath);

            if (pdfNotExits)
            {

                var categories = await _context.Categoria
                    .Where(c => c.OrganigramaId == shiftWork.OrganigramaId)
                    .ToListAsync();

                var employeesRegular = (await _context.Empleado
                    .Include(e => e.Cargo)
                    .Include(e => e.CondicionLaboral)
                    .Include(e => e.RolTurnoDetalle)
                    .ThenInclude(rtd => rtd.RolTurnoEstab)
                    .Include(e => e.CategoriaEmpleado)
                    .ThenInclude(ce => ce.Categoria)
                    .Where(e => e.OrganigramaId == shiftWork.OrganigramaId
                                && e.RolTurnoDetalle
                                    .Any(rtd => rtd.RolTurnoEstab.RolTurnoId == shiftWork.Id))
                    .ToListAsync());
                
                var employeesComplementary = (await _context.Empleado
                    .Include(e => e.Cargo)
                    .Include(e => e.CondicionLaboral)
                    .Include(e => e.RolTurnoDetalle)
                    .ThenInclude(rtd => rtd.RolTurnoEstab)
                    .Include(e => e.CategoriaEmpleado)
                    .ThenInclude(ce => ce.Categoria)
                    .Where(e => e.OrganigramaId == shiftWork.OrganigramaId
                                && e.RolTurnoDetalle
                                    .Any(rtd => rtd.RolTurnoEstab.RolTurnoId == shiftWorkComplementary.Id))
                    .ToListAsync());

                var employeeHelper = new List<Empleado>();

                //var employeeIdx = employees.FindIndex(e=>e.Id);
                employeeHelper.AddRange(employeesRegular);

                foreach (var e in employeesComplementary.Where(e => employeeHelper.All(eh => eh.Id != e.Id)))
                {
                    employeeHelper.Add(e);
                }

                var culture = new CultureInfo("es-ES");
                var swDate = new DateTime(shiftWork.Anio, shiftWork.Mes, 1);
                var month = swDate.ToString("MMMM", culture);

                var daysMonth = new List<ShiftWorkTemplateDay>();

                while (swDate.Month == shiftWork.Mes)
                {
                    var swtd = new ShiftWorkTemplateDay()
                    {
                        Day = swDate.Day,
                        Abbr = swDate.ToString("ddd", culture).First().ToString().ToUpper()
                    };
                    daysMonth.Add(swtd);
                    swDate = swDate.AddDays(1);
                }

                var establishments = new List<ShiftWorkTemplateEstablishment>();

                var turnMap = new Dictionary<int, ShiftWorkTemplateTurn>();

                var establishmentsRegular = shiftWork.RolTurnoEstab
                    .Select(rte =>
                    {
                        var employeesData = employeeHelper.Select(e =>
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

                            var detailsTemplate = details.Select(d => {
                                var dt = new ShiftWorkTemplateDetails()
                                {
                                    Abbreviation = d,
                                };
                                return dt;
                            }).ToList();

                            return new ShiftWorkTemplateEmployee()
                            {
                                Id = e.Id,
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


                var establishmentsComplementary = shiftWorkComplementary.RolTurnoEstab
                    .Select(rte =>
                    {
                        var employeesData = employeeHelper.Select(e =>
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

                            var detailsTemplate = details.Select((d,i) => {
                                var dt = new ShiftWorkTemplateDetails()
                                {
                                    Abbreviation = d,
                                    BackgroundColor = string.IsNullOrEmpty(d) || (i==details.Count-1) ? "": "#B5EAF0"
                                };
                                return dt;
                            }).ToList();

                            return new ShiftWorkTemplateEmployee()
                            {
                                Id = e.Id,
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


                foreach (var estabr in establishmentsRegular)
                {
                    establishments.Add(estabr);
                }

                foreach (var estabc in establishmentsComplementary)
                {
                    var estab = establishments.Find(est=>est.Id==estabc.Id);

                    if (estab!=null)
                    {
                        foreach (var categoryc in estabc.Categories)
                        {
                            var category = estab.Categories.Find(c=>c.Denomination==categoryc.Denomination);


                            if (category!=null)
                            {
                                foreach (var empc in categoryc.Employees)
                                {
                                    var employee = category.Employees.Find(emp => emp.Id == empc.Id);

                                    if (employee != null)
                                    {
                                        if (employee.TotalHours == 0)
                                        {
                                            employee.TotalHours = empc.TotalHours;
                                        }

                                        for (var i = 0; i < empc.Details.Count; i++)
                                        {

                                            if (i == employee.Details.Count - 1)
                                            {
                                                int sum = 0;
                                                if (int.TryParse(employee.Details[i].Abbreviation, out sum))
                                                {
                                                    employee.Details[i].Abbreviation = (sum + int.Parse(empc.Details[i].Abbreviation)).ToString();
                                                }
                                            }
                                            else
                                            {
                                                employee.Details[i].Abbreviation += empc.Details[i].Abbreviation;
                                                if (!string.IsNullOrEmpty(empc.Details[i].Abbreviation))
                                                {
                                                    employee.Details[i].BackgroundColor = "#B5EAF0";
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        category.Employees.Add(empc);
                                    }
                                }
                            }
                            else
                            {
                                estab.Categories.Add(categoryc);
                            }
                        }
                    }
                    else
                    {
                        establishments.Add(estabc);
                    }
                }

                var nextStructureId = shiftWork.Organigrama.Id;
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

                var revisions = new List<RolTurnoRevision>();

                var revisionsRegular = await _context.RolTurnoRevision
                    .Include(rtr => rtr.Usuario)
                    .ThenInclude(u => u.Empleado)
                    .Include(rtr => rtr.RolTurnoIntento)
                    .Include(rtr => rtr.RolTurnoAprobador)
                    .ThenInclude(rta => rta.Aprobador)
                    .Where(rtr => rtr.RolTurnoIntento.Actual
                        && rtr.RolTurnoIntento.RolTurnoId == shiftWork.Id)
                    .ToListAsync();

                var revisionsComplementary = await _context.RolTurnoRevision
                    .Include(rtr => rtr.Usuario)
                    .ThenInclude(u => u.Empleado)
                    .Include(rtr => rtr.RolTurnoIntento)
                    .Include(rtr => rtr.RolTurnoAprobador)
                    .ThenInclude(rta => rta.Aprobador)
                    .Where(rtr => rtr.RolTurnoIntento.Actual
                        && rtr.RolTurnoIntento.RolTurnoId == shiftWorkComplementary.Id)
                    .ToListAsync();

                var revisionsTemplate = new List<ShiftWorkTemplateApprovals>();

                int idx = 0;

                foreach (var rr in revisionsRegular.OrderByDescending(rr=>rr.Fecha))
                {
                    revisions.Add(rr);
                    revisionsTemplate.Add(new ShiftWorkTemplateApprovals() {
                        Header = idx==0?"Rol de Turnos":null,
                        Approver = rr.RolTurnoAprobador.AprobadorPadre
                                ? $"{culture.TextInfo.ToTitleCase(shiftWork.Organigrama.Padre.Denominacion.ToLower())} - {rr.Usuario.Empleado.ApellidoPaterno} {rr.Usuario.Empleado.ApellidoMaterno} {rr.Usuario.Empleado.Nombres}"
                                : $" {rr.RolTurnoAprobador.Aprobador.Denominacion} - { rr.Usuario.Empleado.ApellidoPaterno } { rr.Usuario.Empleado.ApellidoMaterno } { rr.Usuario.Empleado.Nombres }",
                        Date = rr.Fecha.ToString("G", culture)
                    });
                    idx++;
                }

                idx = 0;

                foreach (var rc in revisionsComplementary.OrderByDescending(rr => rr.Fecha))
                {
                    revisions.Add(rc);

                    revisionsTemplate.Add(new ShiftWorkTemplateApprovals()
                    {
                        Header = idx == 0 ? "Horas Complementarias" : null,
                        Approver = rc.RolTurnoAprobador.AprobadorPadre
                                ? $"{culture.TextInfo.ToTitleCase(shiftWork.Organigrama.Padre.Denominacion.ToLower())} - {rc.Usuario.Empleado.ApellidoPaterno} {rc.Usuario.Empleado.ApellidoMaterno} {rc.Usuario.Empleado.Nombres}"
                                : $" {rc.RolTurnoAprobador.Aprobador.Denominacion} - { rc.Usuario.Empleado.ApellidoPaterno } { rc.Usuario.Empleado.ApellidoMaterno } { rc.Usuario.Empleado.Nombres }",
                        Date = rc.Fecha.ToString("G", culture)
                    });
                    idx++;
                }

/*
                var signatures = revisions.DistinctBy(rtr=>rtr.RolTurnoAprobador.AprobadorId).Select(rtr => {

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

                }).ToList();*/

                foreach (var estab in establishments)
                {
                    estab.Categories = estab.Categories.Where(ec=>ec.Employees.All(e=>e.TotalHours>0)).ToList();
                }

                var shiftWorkQR = (await _context.Parametro.Where(p => p.Llave == _constants.ShiftWorkQRKey)
                    .SingleOrDefaultAsync()).Valor;
                var qrTemplate = Handlebars.Compile(shiftWorkQR);
                var qrString = qrTemplate(new { BaseUrl = $"{Request.Scheme}://{Request.Host}", FileName = $"{fileName}.pdf" });
                var searchUrl = $"{_constants.HiscomFrontEndUrl}/buscar";

                var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(qrString, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new Base64QRCode(qrCodeData);
                var qrCodeImage = $"data:image/png;base64,{qrCode.GetGraphic(20)}";


                var data = new List<ShiftWorkTemplate>()
                {
                    new(){
                        Structure = shiftWork.Organigrama.Denominacion,
                        Month = char.ToUpper(month.First()) + month.Substring(1),
                        Year = shiftWork.Anio,
                        Type = "ROL DE TURNOS Y HORAS COMPLEMENTARIAS",
                        //Approvals = revisionsTemplate.ToList(),
                        Establishments = establishments,
                        Days = daysMonth,
                        Turns = turnMap.Values.ToList(),
                        //Signatures = signatures,
                        Levels = levels,
                        HeaderHeight = headerRows * 15,
                        QrCode = qrCodeImage,
                        FileName = fileName,
                        SearchUrl = searchUrl
                    }
                };
                
                
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


        [HttpGet("payroll/print")]
        public async Task<ActionResult> PrintPayroll(int year, int month)
        {
            if (User.Identity?.Name == null)
            {
                return Unauthorized(ErrorVm.Create("El usuario solicitado no existe"));
            }

            var userid = int.Parse(User.Identity?.Name);

            var employee = await _context.Empleado
                .Include(x => x.Usuario)
                .Include(e => e.Cargo)
                .Include(e => e.CondicionLaboral)
                .Where(x => x.Usuario.Id == userid)
                .SingleOrDefaultAsync();

            if (employee==null)
            {
                return BadRequest("El empleado asignado a ese usuario no existe");
            }

            var templatePath = Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Templates/payroll.hbs"));
            var templateString = await System.IO.File.ReadAllTextAsync(templatePath);
            var template = Handlebars.Compile(templateString);

            var baseDirectory = _constants.Storage;
            var hashIds = new Hashids(_keys.HashIdsKey, 6, StaticConstants.Alphabet);

            var income = new List<PayrollSalary>();
            var expenses = new List<PayrollSalary>();
            var contributions = new List<PayrollSalary>();
            var payrollYear = year.ToString();

            var payrollNom = await _context.PlhPlanilla
                .Include(plhn => plhn.PlhPlanillaConcepto)
                .ThenInclude(pn => pn.PlhConcepto)
                .SingleOrDefaultAsync(rt => rt.Libele == employee.NumeroDoc && rt.Anio == year && rt.Mes == month && rt.IndNombrado);

            if (payrollNom != null)
            {
                var fileName = $"B{hashIds.Encode((int)payrollNom.Id)}";
                var pdfPath = Path.Join(baseDirectory.AsSpan(), _constants.PayrollPath.AsSpan(), payrollYear.AsSpan(), $"{fileName}.pdf");
                var pdfNotExits = !System.IO.File.Exists(pdfPath);

                var culture = new CultureInfo("es-ES");
                var payrollDate = new DateTime(payrollNom.Anio, payrollNom.Mes, 1);

                if (pdfNotExits)
                {
                    string qrString = $"{Request.Scheme}://{Request.Host}/{_constants.PayrollPath}/{payrollYear}/{fileName}.pdf";
                    var searchUrl = $"{_constants.HiscomFrontEndUrl}/buscar";

                    var qrGenerator = new QRCodeGenerator();
                    var qrCodeData = qrGenerator.CreateQrCode(qrString, QRCodeGenerator.ECCLevel.Q);
                    var qrCode = new Base64QRCode(qrCodeData);
                    var qrCodeImage = $"data:image/png;base64,{qrCode.GetGraphic(20)}";
    
                    foreach (var concept in payrollNom.PlhPlanillaConcepto)
                    {
                        if (concept.PlhConcepto.Codigo.Contains("C1"))
                        {
                            if (concept.Saldo != null)
                            {
                                income.Add(new PayrollSalary()
                                {
                                    Key = string.IsNullOrEmpty(concept.PlhConcepto.Denominacion) ? concept.PlhConcepto.Codigo : concept.PlhConcepto.Denominacion,
                                    Value = concept.Saldo
                                });
                            }
                        }
                        else if (concept.PlhConcepto.Codigo.Contains("C2"))
                        {

                            if (concept.Saldo != null)
                            {
                                expenses.Add(new PayrollSalary()
                                {
                                    Key = string.IsNullOrEmpty(concept.PlhConcepto.Denominacion) ? concept.PlhConcepto.Codigo : concept.PlhConcepto.Denominacion,
                                    Value = concept.Saldo
                                });
                            }
                        }
                        else if (concept.PlhConcepto.Codigo.Contains("C3"))
                        {

                            if (concept.Saldo != null)
                            {
                                contributions.Add(new PayrollSalary()
                                {
                                    Key = string.IsNullOrEmpty(concept.PlhConcepto.Denominacion) ? concept.PlhConcepto.Codigo : concept.PlhConcepto.Denominacion,
                                    Value = concept.Saldo
                                });
                            }
                        }
                    }

                    decimal? totalIncome = 0;
                    decimal? totalExpenses = 0;


                    foreach (var inc in income)
                    {
                        totalIncome = totalIncome + inc.Value;
                    }


                    foreach (var inc in expenses)
                    {
                        totalExpenses = totalExpenses + inc.Value;
                    }

                    var data = new PayrollTemplate()
                    {
                        Vacancy = payrollNom.Plaza,
                        Month = char.ToUpper(payrollDate.ToString("MMMM", culture).First()) + payrollDate.ToString("MMMM", culture).Substring(1),
                        Year = payrollNom.Anio,
                        RUCHRA = "20172772278",
                        Occupation = employee.Cargo?.Denominacion ?? "",
                        LaboralCondition = employee.CondicionLaboral?.Denominacion ?? "",
                        DNI = employee.NumeroDoc,
                        FullName = $"{employee.ApellidoPaterno} {employee.ApellidoMaterno} {employee.Nombres}",
                        BirthDate = employee.Nacimiento.ToString("d", culture),
                        AfpCard = payrollNom.Afpcar,
                        AfpDate = payrollNom.Fecafp?.ToString("d", culture),
                        Income = income,
                        Expenses = expenses,
                        Contributions = contributions,
                        TotalIncome = totalIncome,
                        TotalExpenses = totalExpenses,
                        Liquid = totalIncome - totalExpenses,
                        QrCode = qrCodeImage,
                        SearchUrl = searchUrl,
                        FileName = fileName,
                    };


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
                    Url = $"{Request.Scheme}://{Request.Host}/{_constants.PayrollPath}/{payrollYear}/{fileName}.pdf",
                    FirstCreated = pdfNotExits
                });

            }

            var payrollCas = await _context.PlhPlanilla
                .Include(plhc=>plhc.PlhPlanillaConcepto)
                .ThenInclude(pc=>pc.PlhConcepto)
                .SingleOrDefaultAsync(rt => rt.Libele == employee.NumeroDoc && rt.Anio == year && rt.Mes == month && !rt.IndNombrado);

            if (payrollCas != null)
            {

                var fileName = $"B{hashIds.Encode((int)payrollCas.Id)}";


                var pdfPath = Path.Join(baseDirectory.AsSpan(), _constants.PayrollPath.AsSpan(), payrollYear.AsSpan(), $"{fileName}.pdf");

                var pdfNotExits = !System.IO.File.Exists(pdfPath);

                var culture = new CultureInfo("es-ES");
                var payrollDate = new DateTime(payrollCas.Anio, payrollCas.Mes, 1);

                if (pdfNotExits)
                {

                    //var payrollQR = (await _context.Parametro.Where(p => p.Llave == _constants.PayrollQRKey)
                    //    .SingleOrDefaultAsync()).Valor;
                    //var qrTemplate = Handlebars.Compile(payrollQR);
                    //var qrString = qrTemplate(new { BaseUrl = $"{Request.Scheme}://{Request.Host}", FileName = $"{fileName}.pdf" });
                    string qrString = $"{Request.Scheme}://{Request.Host}/{_constants.PayrollPath}/{payrollYear}/{fileName}.pdf";
                    var searchUrl = $"{_constants.HiscomFrontEndUrl}/buscar";

                    var qrGenerator = new QRCodeGenerator();
                    var qrCodeData = qrGenerator.CreateQrCode(qrString, QRCodeGenerator.ECCLevel.Q);
                    var qrCode = new Base64QRCode(qrCodeData);
                    var qrCodeImage = $"data:image/png;base64,{qrCode.GetGraphic(20)}";

                    foreach (var concept in payrollCas.PlhPlanillaConcepto)
                    {
                        if (concept.PlhConcepto.Codigo.Contains("C1"))
                        {
                            if (concept.Saldo != null)
                            {
                                income.Add(new PayrollSalary()
                                {
                                    Key = string.IsNullOrEmpty(concept.PlhConcepto.Denominacion) ? concept.PlhConcepto.Codigo : concept.PlhConcepto.Denominacion,
                                    Value = concept.Saldo
                                });
                            }
                        }
                        else if (concept.PlhConcepto.Codigo.Contains("C2"))
                        {

                            if (concept.Saldo != null)
                            {
                                expenses.Add(new PayrollSalary()
                                {
                                    Key = string.IsNullOrEmpty(concept.PlhConcepto.Denominacion) ? concept.PlhConcepto.Codigo : concept.PlhConcepto.Denominacion,
                                    Value = concept.Saldo
                                });
                            }
                        }
                        else if (concept.PlhConcepto.Codigo.Contains("C3"))
                        {

                            if (concept.Saldo != null)
                            {
                                contributions.Add(new PayrollSalary()
                                {
                                    Key = string.IsNullOrEmpty(concept.PlhConcepto.Denominacion) ? concept.PlhConcepto.Codigo : concept.PlhConcepto.Denominacion,
                                    Value = concept.Saldo
                                });
                            }
                        }
                    }

                    decimal? totalIncome = 0;
                    decimal? totalExpenses = 0;


                    foreach (var inc in income)
                    {
                        totalIncome = totalIncome + inc.Value;
                    }


                    foreach (var inc in expenses)
                    {
                        totalExpenses = totalExpenses + inc.Value;
                    }


                    var data = new PayrollTemplate()
                    {
                        Vacancy = payrollCas.Plaza,
                        Month = char.ToUpper(payrollDate.ToString("MMMM", culture).First()) + payrollDate.ToString("MMMM", culture).Substring(1),
                        Year = payrollCas.Anio,
                        RUCHRA = "20172772278",
                        Occupation = employee.Cargo?.Denominacion ?? "",
                        LaboralCondition = employee.CondicionLaboral?.Denominacion ?? "",
                        DNI = employee.NumeroDoc,
                        FullName = $"{employee.ApellidoPaterno} {employee.ApellidoMaterno} {employee.Nombres}",
                        BirthDate = employee.Nacimiento.ToString("d", culture),
                        AfpCard = payrollCas.Afpcar,
                        AfpDate = payrollCas.Fecafp?.ToString("d", culture),
                        Income = income,
                        Expenses = expenses,
                        Contributions = contributions,
                        TotalIncome = totalIncome,
                        TotalExpenses = totalExpenses,
                        Liquid = totalIncome - totalExpenses,
                        QrCode = qrCodeImage,
                        SearchUrl = searchUrl,
                        FileName = fileName,
                    };


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
                    Url = $"{Request.Scheme}://{Request.Host}/{_constants.PayrollPath}/{payrollYear}/{fileName}.pdf",
                    FirstCreated = pdfNotExits
                });

            }

            return NotFound($"La planilla solicitada para el periodo {month} del {year} no existe");

        }


        [HttpGet("days-in-month")]
        public ActionResult DaysInMonth(bool nextMonth = false)
        {
            var period = nextMonth ? DateTime.Now.AddMonths(1) : DateTime.Now;
            var days = DateTime.DaysInMonth(period.Year, period.Month);
            var culture = new CultureInfo("es-PE");
            return Ok(new
            {
                Ano = period.Year,
                Mes = period.ToString("MMMM", culture),
                MesNumero = period.Month,
                Dias = days
            });
        }

        // GET: api/organization-chart/{id}/has-boss
        [HttpGet("organization-chart/{id}/has-boss")]
        public async Task<ActionResult<bool>> HasBossByOchart(int id)
        {
            var oChart = await _context.Organigrama
                .Include(o => o.Empleado)
                .SingleAsync(o => o.Id == id);
            if (oChart != null)
            {
                return oChart.Empleado.Any(e => e.EsJefe);
            }
            return BadRequest(ErrorVm.Create("La unidad orgánica no existe"));
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword()
        {
            var employees = await _context.Empleado.ToListAsync();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var emp in employees)
                {
                    var user = await _context.Usuario.SingleOrDefaultAsync(u => u.EmpleadoId == emp.Id);
                    user.Contrasena = AuthUtility.HashPassword(emp.NumeroDoc, _keys.EncryptionKey);
                    user.FechaMod = DateTime.Now;
                    _context.Entry(user).State = EntityState.Modified;
                }

                await _context.SaveChangesAsync();


                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return Ok();
        }

        [HttpGet("category/employees")]
        public async Task<ActionResult> CategoryEmployees()
        {
            if (User.Identity?.Name==null)
            {
                return Unauthorized();
            }


            var userid = int.Parse(User.Identity.Name);
            var boss = await _context.Empleado
                .Include(e => e.Usuario)
                .Include(b=>b.Organigrama)
                .Where(e=>e.EsJefe)
                .SingleOrDefaultAsync(e=>e.Usuario.Id==userid);

            if (boss==null)
            {
                return BadRequest("Este usuario no es jefe de la estructura orgánica");
            }

            var categories = await _context.Categoria
                .Where(c=>c.OrganigramaId==boss.OrganigramaId)
                .ToListAsync();

            var employees = await _context.Empleado
                .Include(e=>e.CondicionLaboral)
                .Include(e=>e.Profesion)
                .Include(e=>e.Cargo)
                .Where(e => e.OrganigramaId == boss.OrganigramaId)
                .Select(e=>CategoryEmployeeDTO.Create(e,null,null))
                .ToListAsync();

            var employeesCategory = await _context.CategoriaEmpleado
                .Include(ec=>ec.Empleado)
                .ThenInclude(e=>e.CondicionLaboral)
                .Include(ce=>ce.Empleado)
                .ThenInclude(e=>e.Profesion)
                .Include(ce => ce.Empleado)
                .ThenInclude(e => e.Cargo)
                .Where(ec => ec.Empleado.OrganigramaId== boss.OrganigramaId)
                .Select(ec => CategoryEmployeeDTO.Create(ec.Empleado, ec.CategoriaId,null))
                .ToListAsync();


            return Ok(new
            {
                Empleados = employeesCategory.Union(employees).Distinct(new CompareCategoryEmployeeDTO()).ToList(),
                Organigrama = boss.Organigrama?.Denominacion,
                Categorias = categories?.Select(CategoryDTO.Create)
            });
        }

    }
}