using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Admin.Indexation;
using Admin.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Domain.Models;
using EFCore.BulkExtensions;
using Lizelaser0310.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using Newtonsoft.Json;


namespace Admin.Controllers
{
    public class UploadForm
    {
        public IFormFile Document { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
    }

    public class ExamForm
    {
        public List<IFormFile> Exams { get; set; }
        public string Category { get; set; }
        public string Dni { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;
        private readonly IKeys _keys;
        private readonly IConstants _constants;

        public UploadController(HISCOMContext context, ElasticClient elastic, IKeys keys, IConstants constants)
        {
            _context = context;
            _elastic = elastic;
            _keys = keys;
            _constants = constants;
        }

        [HttpPost("upload-exam")]
        public async Task<ActionResult> UploadExam([FromForm] ExamForm exam)
        {
            if (exam.Exams.Count == 0)
            {
                return BadRequest(ErrorVm.Create("Asegúrese de enviar al menos un archivo"));
            }

            var year = DateTime.Now.Year.ToString();

            var pdfPath = Path.Join(_constants.Storage, _constants.ClinicalExamPath, year);

            new DirectoryInfo(pdfPath).Create();

            var fileNames = new List<string>();

            foreach (var pdf in exam.Exams)
            {
                if (!pdf.FileName.EndsWith(".pdf"))
                {
                    continue;
                }

                var pdfName = $"{Guid.NewGuid()}.pdf";
                var filePath = Path.Combine(pdfPath, pdfName);
                await using Stream fileStream = new FileStream(filePath, FileMode.Create);
                await pdf.CopyToAsync(fileStream);
                fileNames.Add($"{year}/{pdfName}");
            }

            if (fileNames.Count == 0)
            {
                return BadRequest(ErrorVm.Create("Ningún archivo enviado tiene formato pdf"));
            }

            var clinicalExam = new ExamenClinico
            {
                DniPaciente = exam.Dni,
                CategoriaId = exam.Category,
                ExamenPdf = string.Join(",", fileNames)
            };

            _context.ExamenClinico.Add(clinicalExam);
            await _context.SaveChangesAsync();

            var examIvm = ClinicalExamIvm.GetClinicalExamIvm(clinicalExam);
            await _elastic.CreateAsync(examIvm, b => b.Index(ClinicalExamIvm.indexUid));

            return Ok();
        }

        [HttpPost("upload-document")]
        public async Task<IActionResult> UploadDocument([FromForm] UploadForm form)
        {
            if (form.Document == null || form.Document.Length == 0)
                return BadRequest();

            var format = Path.GetExtension(form.Document.FileName).ToLowerInvariant();
            if (format != ".xls" && format != ".xlsx")
                return BadRequest(ErrorVm.Create($"El formato {format} no es soportado"));

            var dniSet = new HashSet<string>();

            // Open file as read-only.
            using (var doc = SpreadsheetDocument.Open(form.Document.OpenReadStream(), false))
            {
                var wbPart = doc.WorkbookPart;
                if (wbPart == null)
                    return BadRequest();

                //Read the first Sheets 
                var sheet = wbPart.Workbook.Sheets?.GetFirstChild<Sheet>();
                if (sheet?.Id?.Value == null)
                    return BadRequest();

                var worksheet = (wbPart.GetPartById(sheet.Id.Value) as WorksheetPart)?.Worksheet;
                if (worksheet == null)
                    return BadRequest();

                var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                if (stringTable == null)
                    return BadRequest();

                var rows = worksheet.Descendants<Row>();
                var sheetText = sheet.Name?.InnerText?.Trim();
                if (sheetText == null)
                    return BadRequest();
                
                if (sheetText.Contains("DATOSPLH"))
                {
                    //form.Year != null && form.Month != null
                    var plhRgx = new Regex(@"^DATOSPLH(CAS|NOM)(_)\d{6}$");
                    if (!plhRgx.IsMatch(sheetText))
                        return BadRequest(ErrorVm.Create("El Nombre de la hoja excel no tiene formato DATOSPLHNOM_YYYYMM / DATOSPLHCAS_YYYYMM"));

                    var periodo = sheetText.Split("_")[1];
                    int anio = int.Parse(periodo.Substring(0, 4));
                    int mes = int.Parse(periodo.Substring(4, 2));
                    var isPlhNom = sheetText.Contains("DATOSPLHNOM");

                    var firstRow = rows.First();
                    var plhConcepts = new List<PlhConcepto>();
                    var plhConceptsDb = await _context.PlhConcepto.Where(x => x.IndNombrado == isPlhNom).ToListAsync();
                    var headerRgx = new Regex(@"^C\d{4,}$");

                    foreach (var cell in firstRow.Descendants<Cell>())
                    {
                        var value = cell.GetCellValue(stringTable)?.Trim();
                        if (string.IsNullOrEmpty(value))
                            break;
                        if (!headerRgx.IsMatch(value))
                            continue;

                        if (plhConceptsDb.All(p => p.Codigo != value))
                        {
                            var concept = new PlhConcepto();
                            concept.Codigo = value;
                            concept.FechaReg = DateTime.Now;
                            concept.Estado = true;
                            concept.IndNombrado = isPlhNom;
                            plhConcepts.Add(concept);
                        }
                    }

                    if (plhConcepts.Count > 0)
                    {
                        await _context.BulkInsertAsync(plhConcepts);
                    }

                    var conceptPairs = await _context.PlhConcepto.Where(x => x.IndNombrado == isPlhNom)
                        .Select(p => new KeyValuePair<string, int>(p.Codigo, p.Id))
                        .ToListAsync();
                    var conceptDict = new Dictionary<string, int>(conceptPairs);
                    var columnRgx = new Regex(@"\d");
                    var plhList = new List<PlhPlanilla>();
                    var employeeList = new List<Empleado>();
                    

                    var periodPlh = await _context.PlhPlanilla
                        .AnyAsync(plh => plh.Anio == anio && plh.Mes == mes && plh.IndNombrado == isPlhNom);

                    if (periodPlh)
                    {
                        return Conflict(ErrorVm.Create(
                            $"La planilla para el año {anio} y el mes {mes} ya se encuentra registrada"));
                    }

                    var userRole = new Lazy<Rol>(() => _context.Rol.SingleOrDefault(r => r.Denominacion == "Usuario"));

                    await using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        foreach (var row in rows.Skip(1))
                        {
                            var nameCell = row.GetCellValue(stringTable, "I");
                            var codmet = row.GetCellValue(stringTable, "G");

                            if (string.IsNullOrEmpty(nameCell) || nameCell.Contains("*") || codmet.Equals("99999"))
                                continue;

                            var dni = row.GetCellValue(stringTable, "N");
                            if (dni.Length > 8)
                                dni = dni.Substring(dni.Length - 8, 8);

                            if (string.IsNullOrEmpty(dni))
                                break;
                            if (dniSet.Contains(dni))
                                continue;

                            decimal? suma = 0;
                            foreach (var cell in row.Descendants<Cell>())
                            {
                                var columnName = cell.CellReference?.Value;
                                if (string.IsNullOrEmpty(columnName))
                                    break;

                                var column = columnRgx.Replace(columnName, "");
                                var headerValue = firstRow.GetCellValue(stringTable, column);

                                if (string.IsNullOrEmpty(headerValue)) break;
                                if (!headerValue.Contains("C10")) continue;
                                //if (!headerRgx.IsMatch(headerValue)) continue;

                                suma = suma + cell.GetCellDecimal(stringTable);
                            }
                            if (suma == 0)
                                continue;

                            var plh = new PlhPlanilla();
                            plh.Mes = mes;
                            plh.Anio = anio;
                            plh.Plaza = row.GetCellValue(stringTable, "H");
                            plh.Libele = dni;
                            plh.FechaNac = row.GetCellDate(stringTable, "O");
                            plh.CodCar = row.GetCellValue(stringTable, "J");
                            plh.Regim = row.GetCellValue(stringTable, "Q");
                            plh.Ipsscar = row.GetCellValue(stringTable, "T");
                            plh.Afpcar = row.GetCellValue(stringTable, "U");
                            plh.Fecafp = row.GetCellDate(stringTable, "V");
                            plh.Codsiaf = row.GetCellValue(stringTable, "W");
                            plh.Ctaban = row.GetCellValue(stringTable, "X");
                            plh.Condic = int.Parse(row.GetCellValue(stringTable, "Z"));
                            plh.Fecalt = row.GetCellDate(stringTable, "P");
                            plh.Pat = row.GetCellValue(stringTable, isPlhNom ? "AF" : "AG");
                            plh.Mat = row.GetCellValue(stringTable, isPlhNom ? "AG" : "AH");
                            plh.Nom = row.GetCellValue(stringTable, isPlhNom ? "AH" : "AI");
                            plh.Sexo = row.GetCellValue(stringTable, "Y");
                            plh.IndNombrado = isPlhNom;

                            dniSet.Add(dni);

                            var employeeDb = await _context.Empleado.IgnoreQueryFilters()
                                .Include(e => e.Usuario)
                                .SingleOrDefaultAsync(e => e.NumeroDoc == dni);

                            if (employeeDb == null)
                            {
                                var firstname = plh.Nom;
                                var lastName = plh.Pat;
                                var email =
                                    $"{firstname.Split(" ").First().ToUpper()}.{lastName.ToUpper()}@HRAYACUCHO.GOB.PE";
                                var employee = new Empleado
                                {
                                    TipoDocumentoId = 1,
                                    NumeroDoc = dni,
                                    ApellidoPaterno = lastName,
                                    ApellidoMaterno = plh.Mat,
                                    Nombres = firstname,
                                    NumeroCuenta = plh.Ctaban,
                                    Nacimiento = plh.FechaNac.Value,
                                    Sexo = plh.Sexo,
                                    TipoEmpleadoId = plh.Condic,
                                    FechaIngreso = plh.Fecalt,
                                    EsJefe = false,
                                    FechaReg = DateTime.Now,
                                    Estado = true,
                                    IndPlanillaNom = isPlhNom,
                                    IndPlanillaCas = !isPlhNom,
                                //Roles = new List<int>(),
                                Usuario = new Usuario()
                                    {
                                        Correo = email,
                                        NombreUsuario = email.Split("@")[0],
                                        Contrasena = AuthUtility.HashPassword(dni, _keys.EncryptionKey),
                                        FechaReg = DateTime.Now,
                                        Estado = true,
                                        UsuarioRol = new List<UsuarioRol>()
                                        {
                                            new UsuarioRol(){RolId = userRole.Value.Id }
                                        }
                                    }
                                };                                
                                employeeList.Add(employee);
                            }
                            else
                            {
                                employeeDb.FechaMod = DateTime.Now;
                                employeeDb.Estado = true;
                                employeeDb.TipoEmpleadoId = plh.Condic;
                                employeeDb.FechaIngreso = plh.Fecalt;
                                employeeDb.IndPlanillaNom = isPlhNom;
                                employeeDb.IndPlanillaCas = !isPlhNom;
                                _context.Entry(employeeDb).State = EntityState.Modified;

                                if (employeeDb.Usuario != null)
                                {
                                    employeeDb.Usuario.FechaMod = DateTime.Now;
                                    employeeDb.Usuario.Estado = true;
                                    _context.Entry(employeeDb.Usuario).State = EntityState.Modified;
                                }
                            }

                            var plhConceptList = new List<PlhPlanillaConcepto>();
                            foreach (var cell in row.Descendants<Cell>())
                            {
                                var columnName = cell.CellReference?.Value;

                                if (string.IsNullOrEmpty(columnName))
                                    break;

                                var column = columnRgx.Replace(columnName, "");
                                var headerValue = firstRow.GetCellValue(stringTable, column);

                                if (string.IsNullOrEmpty(headerValue))
                                    break;
                                if (!headerRgx.IsMatch(headerValue))
                                    continue;

                                var plhConcept = new PlhPlanillaConcepto
                                {
                                    PlhPlanillaId = plh.Id,
                                    PlhConceptoId = conceptDict[headerValue],
                                    Saldo = cell.GetCellDecimal(stringTable)
                                };
                                if (plhConcept.Saldo > 0)
                                    plhConceptList.Add(plhConcept);
                            }

                            plh.PlhPlanillaConcepto = plhConceptList;
                            plhList.Add(plh);
                        }

                        var lstEmpleado = await _context.Empleado.Where(x => x.IndPlanillaNom == isPlhNom && x.IndPlanillaCas == !isPlhNom).ToListAsync();
                        foreach (var emp in lstEmpleado)
                        {
                            if (!dniSet.Contains(emp.NumeroDoc))
                            {
                                emp.FechaMod = DateTime.Now;
                                emp.Estado = false;
                                _context.Entry(emp).State = EntityState.Modified;

                                var user = await _context.Usuario.SingleOrDefaultAsync(u => u.EmpleadoId == emp.Id);
                                if (user != null)
                                {
                                    user.FechaMod = DateTime.Now;
                                    user.Estado = false;
                                    _context.Entry(user).State = EntityState.Modified;
                                }
                            }
                        }

                        if (employeeList.Count > 0)
                            await _context.BulkInsertAsync(employeeList, b => b.IncludeGraph = true);

                        if (plhList.Count > 0)
                            await _context.BulkInsertAsync(plhList, b => b.IncludeGraph = true);

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
                else
                {
                    if (!(sheet?.Name?.InnerText ?? "").Contains("reporte_padron_nominal"))
                    {
                        return BadRequest(ErrorVm.Create("El archivo no tiene formato INFORHUS"));
                    }

                    var ochart = await _context.Organigrama.ToListAsync();
                    var civilStates = await _context.EstadoCivil.ToListAsync();
                    var professions = await _context.Profesion.ToListAsync();
                    var positions = await _context.Cargo.ToListAsync();
                    var laboralRegimes = await _context.RegimenLaboral.ToListAsync();
                    var workingConditions = await _context.CondicionLaboral.ToListAsync();
                    var banks = await _context.Banco.ToListAsync();
                    var accountTypes = await _context.TipoCuenta.ToListAsync();


                    foreach (var row in rows.Skip(4))
                    {
                        var dniValue = row.GetCellValue(stringTable, "S");

                        if (string.IsNullOrEmpty(dniValue))
                        {
                            break;
                        }

                        if (dniSet.Contains(dniValue))
                        {
                            continue;
                        }

                        var employee = await _context.Empleado.IgnoreQueryFilters()
                            .SingleOrDefaultAsync(e => e.NumeroDoc == dniValue);

                        if (employee != null)
                        {
                            dniSet.Add(dniValue);

                            var structure = row.GetCellValue(stringTable, "CU");

                            if (!string.IsNullOrEmpty(structure))
                            {
                                employee.OrganigramaId = GetStructure(structure,
                                    ochart);
                            }

                            employee.ApellidoPaterno = row.GetCellValue(stringTable, "U");
                            employee.ApellidoMaterno = row.GetCellValue(stringTable, "V");
                            employee.Nombres = row.GetCellValue(stringTable, "T");
                            employee.Nacimiento = row.GetCellDate(stringTable, "X") ?? employee.Nacimiento;
                            employee.Sexo = row.GetCellValue(stringTable, "Y") == "Femenino" ? "F" : "M";
                            employee.EstadoCivilId = GetCivilState(row.GetCellValue(stringTable, "Z"), civilStates);
                            employee.Direccion = row.GetCellValue(stringTable, "AA");
                            employee.Correos = row.GetCellValue(stringTable, "AC");
                            employee.Telefonos = row.GetCellValue(stringTable, "AD");
                            employee.ProfesionId = GetProfession(row.GetCellValue(stringTable, "AG"), professions);
                            employee.NumeroColegiatura = row.GetCellValue(stringTable, "AI");
                            employee.CargoId = GetPosition(row.GetCellValue(stringTable, "AT"), positions);
                            employee.RegimenLaboralId =
                                GetLaborRegime(row.GetCellValue(stringTable, "AU"), laboralRegimes);
                            employee.CondicionLaboralId =
                                GetWorkingCondition(row.GetCellValue(stringTable, "AV"), workingConditions);
                            employee.FechaNombramiento = row.GetCellDate(stringTable, "BK");
                            employee.FechaIngreso = row.GetCellDate(stringTable, "BS");
                            employee.BancoId = GetBank(row.GetCellValue(stringTable, "CC"), banks);
                            employee.TipoCuentaId = GetAccountType(row.GetCellValue(stringTable, "CD"), accountTypes);
                            employee.NumeroCuenta = row.GetCellValue(stringTable, "CE");
                            employee.CuentaInterbancaria = row.GetCellValue(stringTable, "CF");
                            employee.FechaMod = DateTime.Now;
                            employee.IndInforhus = true;


                            _context.Entry(employee).State = EntityState.Modified;
                        }
                    }

                    await _context.SaveChangesAsync();
                }
            }

            return Ok();
        }

        private string GetWorkingCondition(string workingCondition, List<CondicionLaboral> workingConditions)
        {
            return workingConditions.SingleOrDefault(p => p.Denominacion == workingCondition)?.Id;
        }

        private string GetLaborRegime(string laboralRegime, List<RegimenLaboral> laboralRegimes)
        {
            return laboralRegimes.SingleOrDefault(p => p.Denominacion == laboralRegime)?.Id;
        }

        private string GetPosition(string position, List<Cargo> positions)
        {
            return positions.SingleOrDefault(p => p.Denominacion == position)?.Id;
        }

        private string GetProfession(string profession, List<Profesion> professions)
        {
            return professions.SingleOrDefault(p => p.Denominacion == profession)?.Id;
        }

        private int? GetStructure(string structure, List<Organigrama> ochart)
        {
            return ochart.SingleOrDefault(o => o.Denominacion == structure)?.Id;
        }

        private int? GetAccountType(string accountType, List<TipoCuenta> accountTypes)
        {
            return accountTypes.SingleOrDefault(at => at.Denominacion == accountType)?.Id;
        }

        private int? GetBank(string bank, List<Banco> banks)
        {
            return banks.SingleOrDefault(b => b.Nombre == bank)?.Id;
        }

        private int? GetCivilState(string civilState, List<EstadoCivil> civilStates)
        {
            return civilStates.SingleOrDefault(cs => cs.Denominacion == civilState)?.Id;
        }
    }

    public static class OpenXMLHelpers
    {
        public static CultureInfo _culture = new CultureInfo("es-PE");
        public static Regex _columnRegex = new Regex(@"^[A-Za-z]+\d+$");

        public static Cell GetCell(this Row row, string columnName)
        {
            var column = columnName.Trim();

            return row.Elements<Cell>().SingleOrDefault(c =>
                c.CellReference != null &&
                string.Compare(c.CellReference.Value, _columnRegex.IsMatch(column) ? column : $"{column}{row.RowIndex}",
                    StringComparison.OrdinalIgnoreCase) == 0);
        }

        public static string GetCellValue(this Cell cell, SharedStringTablePart stringTable)
        {
            var value = cell?.InnerText;

            if (value == null)
            {
                return null;
            }

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                var intValue = int.Parse(value);
                return stringTable.SharedStringTable.ElementAt(intValue).InnerText;
            }

            return value;
        }

        public static string GetCellValue(this Row row, SharedStringTablePart stringTable, string columnName)
        {
            var cell = GetCell(row, columnName);
            return cell.GetCellValue(stringTable);
        }

        public static decimal? GetCellDecimal(this Cell cell, SharedStringTablePart stringTable)
        {
            var value = cell.GetCellValue(stringTable);

            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return decimal.Parse(value, _culture);
        }

        public static decimal? GetCellDecimal(this Row row, SharedStringTablePart stringTable, string columnName)
        {
            var value = row.GetCellValue(stringTable, columnName);

            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return decimal.Parse(value, _culture);
        }

        public static DateTime? GetCellDate(this Row row, SharedStringTablePart stringTable, string columnName)
        {
            var value = row.GetCellValue(stringTable, columnName);

            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            try
            {
                return DateTime.Parse(value, _culture);
            }
            catch
            {
                var intValue = int.Parse(value);
                return DateTime.FromOADate(intValue);
            }
        }
    }
}