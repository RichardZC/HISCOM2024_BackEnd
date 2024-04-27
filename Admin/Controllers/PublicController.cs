using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Admin.Indexation;
using Admin.Models;
using Domain.Models;
using HashidsNet;
using Lizelaser0310.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nest;
using Path = System.IO.Path;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Hosting;
using AspNetCore.Reporting;
using System.Xml;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;
        private readonly IKeys _keys;
        private readonly IConstants _constants;
        private readonly Dictionary<int, HashSet<string>> _authCache;
        private static readonly HttpClient client = new HttpClient();
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PublicController(HISCOMContext context, ElasticClient elastic, IKeys keys, IConstants constants, 
                                        Dictionary<int, HashSet<string>> authCache, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _elastic = elastic;
            _keys = keys;
            _constants = constants;
            _authCache = authCache;
            _webHostEnvironment = webHostEnvironment;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult<string>> Login(LoginVm input)
        {
            var user = await _context.Usuario
                .Where(x => x.NombreUsuario.Equals(input.Usuario) || x.Correo.Equals(input.Usuario))
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return BadRequest("El usuario no existe, no es un cliente o se encuentra desactivado");
            }
            
            var verifyPassword = AuthUtility.VerifyPassword(input.Clave, user.Contrasena, _keys.EncryptionKey);

            if (!verifyPassword)
            {
                return BadRequest("El usuario y/o contraseña son incorrectos");
            }
            
            // We create the claims (belongings, characteristics) of the user
            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, user.Correo),
                new(ClaimTypes.Name, user.Id.ToString()),
                new(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                // Token lifetime
                Expires = DateTime.UtcNow.AddDays(28),
                // Credentials to generate the token using our secret key and the 256 hash algorithm
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_keys.TokenKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var createdToken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(createdToken);

            await CachePermisos(user.Id);

            return Ok(token);

        }

        [HttpGet("dni/{id}")]
        public async Task<ActionResult> GetPersonReniec(string id)
        {
            try
            {
                if (id.Length != 8)
                    return StatusCode(500, ErrorVm.Create("El dni requiere 8 digitos"));

                var httpClient = new HttpClient();
                var content = await httpClient.GetStringAsync($"http://wsminsa.minsa.gob.pe//WSRENIEC_DNI/SerDNI.asmx/GetReniec?strDNIAuto=40738034&strDNICon=" + id.Trim());
                //var json = JsonConvert.DeserializeObject(content);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(content);
                if (string.IsNullOrEmpty(xmlDoc.ChildNodes[1].ChildNodes[1].InnerText))
                    return Ok();

                var persona = new
                {
                    Dni = xmlDoc.ChildNodes[1].ChildNodes[21].InnerText,                    
                    Paterno = xmlDoc.ChildNodes[1].ChildNodes[1].InnerText,
                    Materno = xmlDoc.ChildNodes[1].ChildNodes[2].InnerText,
                    Nombres = xmlDoc.ChildNodes[1].ChildNodes[3].InnerText,
                    NombreCompleto = xmlDoc.ChildNodes[1].ChildNodes[1].InnerText + " " +
                                        xmlDoc.ChildNodes[1].ChildNodes[2].InnerText + " " +
                                        xmlDoc.ChildNodes[1].ChildNodes[3].InnerText,                    
                    Sexo = xmlDoc.ChildNodes[1].ChildNodes[17].InnerText == "1" ? "M" : "F",
                    FechaNacimiento = xmlDoc.ChildNodes[1].ChildNodes[18].InnerText,
                    FechaEmision = xmlDoc.ChildNodes[1].ChildNodes[19].InnerText,
                    Direccion = xmlDoc.ChildNodes[1].ChildNodes[16].InnerText
                };
                return Ok(persona);
            }
            catch (Exception)
            {
                return StatusCode(500, ErrorVm.Create("El servicio no está disponible"));
            }
        }
        [HttpGet("patient/exams")]
        public async Task<ActionResult> GetExams(string apiKey, string dni)
        {
            if (apiKey == null)
            {
                return Unauthorized();
            }
            
            var dniTrimmed = dni.Trim();

            var dniOk = AuthUtility.VerifyPassword(dniTrimmed, apiKey, _keys.EncryptionKey);

            if (!dniOk)
            {
                return Unauthorized();
            }

            return await PaginationUtility.Paginate(
                query: Request.QueryString.Value,
                dbSet: _context.ExamenClinico,
                middle: (q, _) => q.Where(e => e.DniPaciente == dniTrimmed),
                mutation: (e) => new ExamenClinico()
                {
                    Id = e.Id,
                    DniPaciente = e.DniPaciente,
                    CategoriaId = e.CategoriaId,
                    ExamenPdf = string.Join(",", e.ExamenPdf
                        .Split(",")
                        .Select(x => $"{Request.Scheme}://{Request.Host}/{_constants.ClinicalExamPath}/{x.Trim()}"))
                }
            );
        }

        [HttpPost("patient/login")]
        public async Task<ActionResult> LoginPatient(LoginPatientVm input)
        {
            // Check the patient credentials with a call to RENIEC api
            var checkDni = await Task.Run(() => true);

            if (!checkDni)
            {
                return BadRequest(ErrorVm.Create("Asegúrese de enviar un DNI y/o fecha de emisión correctos"));
            }

            var dniHashed = AuthUtility.HashPassword(input.Dni, _keys.EncryptionKey);
            return Ok(dniHashed);
        }
        
        public async Task CachePermisos(int userId) {
            var permisoUsuario = await (from ur in _context.UsuarioRol
                                        join rp in _context.RolPermiso on ur.RolId equals rp.RolId
                                        join p in _context.Permiso on rp.PermisoId equals p.Id
                                        where ur.UsuarioId.Equals(userId)
                                        select new { p.Accion, p.Ruta })
                                           .Distinct().ToListAsync();
            _authCache.Remove(userId);
            _authCache.Add(userId, new HashSet<string>());
            
            foreach (var item in permisoUsuario) {
                if (item.Ruta != null)
                {
                    _authCache[userId].Add(item.Ruta);
                }

                if (item.Accion != null)
                {
                    foreach (var accion in item.Accion.Split(","))
                    {
                        _authCache[userId].Add(accion.Trim());
                    }
                }
            }
        }

        [HttpGet]
        [Route("search/{hashPdf}")]
        public async Task<ActionResult> Search(string hashPdf)
        {

            if (string.IsNullOrEmpty(hashPdf))
            {
                return BadRequest(ErrorVm.Create("Asegúrese de enviar un código"));
            }

            var baseDirectory = _constants.Storage;// await _context.Parametro.SingleOrDefaultAsync(p => p.Llave == _constants.BaseDirectory);
            var hashIds = new Hashids(_keys.HashIdsKey,6, StaticConstants.Alphabet);

            var hashPdfTrim = hashPdf.Trim();
            var hashPdfPrefix = hashPdfTrim.Substring(0,1);
            var hashPdfCode = hashPdfTrim.Substring(1);
            
            if (hashPdfPrefix!="R" && hashPdfPrefix != "B")
            {
                return BadRequest(ErrorVm.Create("El código ingresado no tiene el formato adecuado"));
            }            
            
            var id = hashIds.Decode(hashPdfCode).First();
            if (hashPdfPrefix == "R")
            {
                var shiftWorkPdf = await _context.RolTurno.FindAsync(id);

                if (shiftWorkPdf == null)
                {
                    return BadRequest(ErrorVm.Create("El código del rol de turnos solicitado es inválido"));
                }

                var payrollYear = shiftWorkPdf.Anio.ToString();

                var pdfPath = Path.Join(baseDirectory.AsSpan(), _constants.ShiftWorkPath.AsSpan(), payrollYear.AsSpan(), $"{hashPdfTrim}.pdf");

                var pdfNotExits = !System.IO.File.Exists(pdfPath);

                if (pdfNotExits)
                {
                    return NotFound();
                }

                return Ok(new
                {
                    Url = $"{Request.Scheme}://{Request.Host}/{_constants.ShiftWorkPath}/{payrollYear}/{hashPdfTrim}.pdf",
                });
            }

            if(hashPdfPrefix == "B")
            {
                var payrollNomPdf = await _context.PlhPlanilla.FindAsync((long)id);

                if (payrollNomPdf == null)
                {
                    return BadRequest(ErrorVm.Create("El código de la planilla solicitada es inválido"));
                }

                var payrollYear = payrollNomPdf.Anio.ToString();

                var pdfPath = Path.Join(baseDirectory.AsSpan(), _constants.PayrollPath.AsSpan(), payrollYear.AsSpan(), $"{hashPdfTrim}.pdf");

                var pdfNotExits = !System.IO.File.Exists(pdfPath);

                if (pdfNotExits)
                {
                    return NotFound();
                }

                return Ok(new
                {
                    Url = $"{Request.Scheme}://{Request.Host}/{_constants.PayrollPath}/{payrollYear}/{hashPdfTrim}.pdf",
                });

            }

            return BadRequest(ErrorVm.Create("El código no está asociado a un archivo válido"));

        }

        [HttpGet()]
        [Route("personal/{id}")]
        public async Task<ActionResult> Personal(string id)
        {            
            var elasticQuery = await _elastic.SearchAsync<PersonalIvm>(s => s.Index(PersonalIvm.indexUid)
                .Query(q => q.Match(m => m.Field(f => f.NumeroDoc).Query(id))));
            var personal = elasticQuery?.Documents?.FirstOrDefault();

            if (personal==null)
            {
                return NotFound("El personal solicitado no existe");
            }
            
            ImageUtility.CreateImageUrl(personal, Request, "Foto", _constants.ImagePath);

            return Ok(personal);
        }

        // GET: api/<UspConsultAppointmentController>
        [HttpGet]
        [Route("consult-appointment")]
        public async Task<ActionResult> GetUpsConsultAppointment(string dni, string token)
        {

            var values = new Dictionary<string, string>
                            {
                                { "secret", _keys.CaptchaKey },
                                { "response", token }
                            };

            var content = new FormUrlEncodedContent(values);

            var responseHttp = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);

            if (!responseHttp.IsSuccessStatusCode)
            {
                return BadRequest(ErrorVm.Create("Error de comunicación con Google Captcha"));
            }
            
            var responseString = await responseHttp.Content.ReadAsStringAsync();

            var response = JObject.Parse(responseString);


            if (!response["success"].Value<bool>())
            {
                return BadRequest(ErrorVm.Create("Error con el código Captcha."));
            }

            var storedProc = "exec his.usp_ConsultarCitaPendiente @DNI";
            var dniParameter = new SqlParameter("@DNI", dni);

            var result = await _context.ConsultarCita.FromSqlRaw(storedProc,dniParameter).ToListAsync();


            return Ok(result);
        }

        [HttpPost()]
        [Route("reporte-cita")]
        public async Task< IActionResult> ReporteCita(int idCita)
        {
            var storedProc = "exec his.usp_rptCita @IdCita";
            var dniParameter = new SqlParameter("@IdCita", idCita);
            var resp = await _context.ConsultarCita.FromSqlRaw(storedProc, dniParameter).ToListAsync();
            if (resp.Count == 0)
                return NotFound();

            string mimtype = "";
            int extension = 1;
            var path = $"{_webHostEnvironment.WebRootPath}\\Reportes\\rptCita.rdlc";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Documento", resp[0].Documento);
            parameters.Add("Paciente", resp[0].Paciente);
            parameters.Add("Fecha", resp[0].FechaCita);
            parameters.Add("Hora", resp[0].HoraCita);
            parameters.Add("Servicio", resp[0].Servicio);
            parameters.Add("Profesional", resp[0].Profesional);
            parameters.Add("Fuente", resp[0].FuenteFinanciamiento);
            parameters.Add("Estado", resp[0].Estado);
            LocalReport localReport = new LocalReport(path);
            
            //localReport.AddDataSource("dsReporte", p);

            var result = localReport.Execute(RenderType.Pdf, extension, parameters, mimtype);
            return File(result.MainStream, "application/pdf","cita.pdf");
        }
    }
}