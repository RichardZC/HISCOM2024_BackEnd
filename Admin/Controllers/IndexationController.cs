using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Admin.Indexation;
using Algolia.Search.Clients;
using Algolia.Search.Models.Common;
using Algolia.Search.Models.Settings;
using Microsoft.Extensions.Configuration;
using Nest;
using Org.BouncyCastle.Crypto.Engines;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IndexationController : ControllerBase
    {
        private readonly string _dbConnection;
        private readonly ElasticClient _elastic;

        public IndexationController(ElasticClient elastic, IConfiguration config)
        {
            _elastic = elastic;
            _dbConnection = config.GetConnectionString("connectionDB");
        }

        private Dictionary<string, Func<Task<BulkResponse>>> GetIndexTasks()
        {
            return new()
            {
                {
                    ParameterIvm.indexUid, () => RefreshIndex(
                        callBack: x => x.Parametro,
                        indexUid: ParameterIvm.indexUid,
                        mutation: ParameterIvm.GetParameterIvm
                    )
                },
                {
                    OChartIvm.indexUid, () => RefreshIndex(
                        callBack: x => x.Organigrama,
                        indexUid: OChartIvm.indexUid,
                        mutation: OChartIvm.GetOChartIvm
                    )
                },
                {
                    PositionIvm.indexUid, () => RefreshIndex(
                        callBack: x => x.Cargo,
                        indexUid: PositionIvm.indexUid,
                        mutation: PositionIvm.GetPositionIvm
                    )
                },
                {
                    EmployeeTypeIvm.indexUid, () => RefreshIndex(
                        callBack: x => x.TipoEmpleado,
                        indexUid: EmployeeTypeIvm.indexUid,
                        mutation: EmployeeTypeIvm.GetEmployeeTypeIvm
                    )
                },
                {
                    ConceptoPlanillaIvm.indexUid,() => RefreshIndex(
                        callBack: x => x.PlhConcepto,
                        indexUid: ConceptoPlanillaIvm.indexUid,
                        mutation: ConceptoPlanillaIvm.GetConceptoPlanillaIvm)
                },
                {
                    WorkingConditionIvm.indexUid, () => RefreshIndex(
                        callBack: x => x.CondicionLaboral,
                        indexUid: WorkingConditionIvm.indexUid,
                        mutation: WorkingConditionIvm.GetWorkingConditionIvm
                    )
                },
                {
                    BankIvm.indexUid, () => RefreshIndex(
                        callBack: x => x.Banco,
                        indexUid: BankIvm.indexUid,
                        mutation: BankIvm.GetBankIvm
                    )
                },
                {
                    NacionalityIvm.indexUid, () => RefreshIndex(
                        callBack: x => x.Nacionalidad,
                        indexUid: NacionalityIvm.indexUid,
                        mutation: NacionalityIvm.GetNacionalityIvm
                    )
                },
                {
                    ProfessionIvm.indexUid, () => RefreshIndex(
                        callBack: x => x.Profesion,
                        indexUid: ProfessionIvm.indexUid,
                        mutation: ProfessionIvm.GetProfessionIvm
                    )
                },
                {
                    ProfessionalCollegeIvm.indexUid, () => RefreshIndex(
                        callBack: x => x.ColegioProfesional,
                        indexUid: ProfessionalCollegeIvm.indexUid,
                        mutation: ProfessionalCollegeIvm.GetProfessionalCollegeIvm
                    )
                },
                {
                    TurnIvm.indexUid, () => RefreshIndex(
                        callBack: x => x.Turno,
                        indexUid: TurnIvm.indexUid,
                        mutation: TurnIvm.GetTurnIvm
                    )
                },
                {
                    EmployeeIvm.indexUid, () => RefreshIndex(
                        callBack: x => x.Empleado
                            .Include(y => y.TipoEmpleado)
                            .Include(y => y.Banco)
                            .Include(y => y.Nacionalidad)
                            .Include(y => y.Cargo)
                            .Include(y => y.Organigrama)
                            .Include(y => y.CondicionLaboral)
                            .Include(y => y.TipoDocumento)
                            .Include(y => y.TipoCuenta),
                        indexUid: EmployeeIvm.indexUid,
                        mutation: EmployeeIvm.GetEmployeeIvm
                    )
                },
                {
                    PersonalIvm.indexUid,
                    () => RefreshIndex(
                      callBack: x => x.Empleado
                          .Include(y => y.TipoEmpleado)                          
                          .Include(y => y.Cargo)
                          .Include(y => y.Organigrama)
                          .Include(y => y.CondicionLaboral)
                          .Include(y => y.Profesion)
                          .Include(y => y.ColegioProfesional)
                          .Include(y => y.TipoEmpleado)
                          .Include(y => y.Usuario),
                      indexUid: PersonalIvm.indexUid,
                      mutation: PersonalIvm.GetPersonalIvm)
                },
                {
                    ShiftWorkIvm.indexUid, () => RefreshIndex(
                        callBack: x => x.RolTurno
                            .Include(rt => rt.Organigrama),
                        indexUid: ShiftWorkIvm.indexUid,
                        mutation: ShiftWorkIvm.GetShiftWorkIvm
                    )
                },
                {
                    UserIvm.indexUid, () => RefreshIndex(
                        callBack: x => x.Usuario.Include(y => y.Empleado),
                        indexUid: UserIvm.indexUid,
                        mutation: UserIvm.GetUserIvm
                    )
                },
                {
                    RoleIvm.indexUid, () => RefreshIndex(
                        callBack: x => x.Rol,
                        indexUid: RoleIvm.indexUid,
                        mutation: RoleIvm.GetRoleIvm
                    )
                },
                {
                    MenuIvm.indexUid, () => RefreshIndex(
                        callBack: x => x.Menu,
                        indexUid: MenuIvm.indexUid,
                        mutation: MenuIvm.GetMenuIvm
                    )
                },
                {
                    PermissionIvm.indexUid, () => RefreshIndex(
                        callBack: x => x.Permiso.Include(y => y.Menu),
                        indexUid: PermissionIvm.indexUid,
                        mutation: PermissionIvm.GetPermissionIvm
                    )
                },
            };
        }
        
        [HttpGet]
        public List<string> GetIndexes()
        {
            var tasks = GetIndexTasks().Keys;
            return new List<string>(tasks);
        }


        [HttpGet("refresh-indexes")]
        public async Task<ActionResult> RefreshIndexes(string indexes)
        {
            if (string.IsNullOrEmpty(indexes))
            {
                return BadRequest("Los índices son obligatorios");
            }

            var tasks = GetIndexTasks();

            var selectedTasks = new List<Task<BulkResponse>>();

            if (indexes == "*")
            {
                selectedTasks.AddRange(tasks.Select(item => item.Value()));
            }
            else
            {
                var entities = indexes.Split(",");
                if (entities.Length == 0)
                {
                    return BadRequest("Envíe al menos una entidad");
                }

                foreach (var index in entities)
                {
                    try
                    {
                        selectedTasks.Add(tasks[index]());
                    }
                    catch (KeyNotFoundException)
                    {
                        return BadRequest($"El índice {index} no fue encontrado");
                    }
                }
            }

            var responses = await Task.WhenAll(selectedTasks);

            return Ok(responses);
        }

        private async Task<BulkResponse> RefreshIndex<T, R>(
            Func<HISCOMContext, IQueryable<T>> callBack,
            string indexUid,
            Func<T, R> mutation
        ) where R : class
        {
            if ((await _elastic.Indices.ExistsAsync(indexUid)).Exists)
            {
                await _elastic.Indices.DeleteAsync(new DeleteIndexRequest(indexUid));
            }

            await _elastic.Indices.CreateAsync(indexUid);


            var optionsBuilder = new DbContextOptionsBuilder<HISCOMContext>();
            optionsBuilder.UseSqlServer(_dbConnection);
            await using var _db = new HISCOMContext(optionsBuilder.Options);

            var entities = await callBack(_db).ToListAsync();
            var values = entities.Select(mutation);

            return await _elastic.BulkAsync(b => b.Index(indexUid).IndexMany(values));

            /*var index = _algolia.InitIndex(indexUid);

            var optionsBuilder = new DbContextOptionsBuilder<HISCOMContext>();
            optionsBuilder.UseSqlServer(_dbConnection);
            await using var _db = new HISCOMContext(optionsBuilder.Options);

            var entities = await callBack(_db).ToListAsync();
            var values = entities.Select(mutation);

            await index.SetSettingsAsync(new IndexSettings()
            {
                CustomRanking = new List<string> { "desc(objectID)" }
            });

            return await index.ReplaceAllObjectsAsync(values);*/
        }
    }
}