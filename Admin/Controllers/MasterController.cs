using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Admin.Indexation;
using Algolia.Search.Clients;
using Domain.Models;
using Lizelaser0310.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Nest;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;

        public MasterController(HISCOMContext context, ElasticClient elastic)
        {
            _context = context;
            _elastic = elastic;
        }
        
        // Employee Type List
        // GET: api/employeeType
        [HttpGet("employee-type")]
        public async Task<ActionResult<Paginator<EmployeeTypeIvm>>> GetEmployeeType()
        {
            return await PaginationUtility.ElasticSearchPaginate
            (
                query: Request.QueryString.Value,
                indexUid: EmployeeTypeIvm.indexUid,
                elastic: _elastic,
                primaryKey: p=>p.Id,
                fields: new List<Expression<Func<EmployeeTypeIvm,dynamic>>>
                {
                    et=>et.Denominacion,
                }
            );
        }
        [HttpGet("concepto-planilla")]
        public async Task<ActionResult<Paginator<ConceptoPlanillaIvm>>> GetConceptoPlanilla()
        {
            var qry = Request.QueryString.Value + "&sort=codigo:desc";
            return await PaginationUtility.ElasticSearchPaginate
            (
                query: qry,
                indexUid: ConceptoPlanillaIvm.indexUid,
                elastic: _elastic,
                primaryKey: p => p.Id,
                fields: new List<Expression<Func<ConceptoPlanillaIvm, dynamic>>>
                {
                    et=>et.Codigo,
                    et=>et.Abreviado,
                    et=>et.Denominacion,
                }
            );
        }

        [HttpGet("bank")]
        public async Task<ActionResult<Paginator<BankIvm>>> GetBank()
        {
            return await PaginationUtility.ElasticSearchPaginate
            (
                query: Request.QueryString.Value,
                indexUid: BankIvm.indexUid,
                elastic: _elastic,
                primaryKey: p=>p.Id,
                fields: new List<Expression<Func<BankIvm,dynamic>>>
                {
                    b=>b.Nombre,
                    b=>b.Abreviacion
                }
            );
        }

        [HttpGet("nationality")]
        public async Task<ActionResult<Paginator<NacionalityIvm>>> GetNationality()
        {
            return await PaginationUtility.ElasticSearchPaginate
            (
                query: Request.QueryString.Value,
                indexUid: NacionalityIvm.indexUid,
                elastic: _elastic,
                primaryKey: p=>p.Id,
                fields: new List<Expression<Func<NacionalityIvm,dynamic>>>
                {
                    n=>n.Pais,
                    n=>n.Gentilicio
                }
            );
        }
        // GET: api/contractType
        [HttpGet("working-condition")]
        public async Task<ActionResult<Paginator<WorkingConditionIvm>>> GetWorkingCondition()
        {
            return await PaginationUtility.ElasticSearchPaginate
            (
                query: Request.QueryString.Value,
                indexUid: WorkingConditionIvm.indexUid,
                elastic: _elastic,
                primaryKey: p=>p.Id,
                fields: new List<Expression<Func<WorkingConditionIvm,dynamic>>>
                {
                    wc=>wc.Denominacion,
                }
            );
        }

        // GET: api/clasifications
        [HttpGet("level")]
        public async Task<ActionResult<Paginator<Clasificacion>>> GetLevel()
        {
            return await PaginationUtility.Paginate
            (
                query: Request.QueryString.Value,
                dbSet: _context.Nivel
            );
        }

        // GET: api/oChart
        [HttpGet("organization-chart")]
        public async Task<ActionResult<Paginator<OChartIvm>>> GetOChart()
        {
            return await PaginationUtility.ElasticSearchPaginate
            (
                query: Request.QueryString.Value,
                indexUid: OChartIvm.indexUid,
                elastic: _elastic,
                primaryKey: p=>p.Id,
                fields: new List<Expression<Func<OChartIvm,dynamic>>>
                {
                    o=>o.Nombre,
                }
            );
        }
        
        // GET: api/clasifications
        [HttpGet("clasification")]
        public async Task<ActionResult<Paginator<Clasificacion>>> GetClasification()
        {
            return await PaginationUtility.Paginate
            (
                query: Request.QueryString.Value,
                dbSet: _context.Clasificacion
            );
        }
        
        
        // GET: api/position
        [HttpGet("position")]
        public async Task<ActionResult> GetPosition()
        {
            return await PaginationUtility.ElasticSearchPaginate
            (
                query: Request.QueryString.Value,
                indexUid: PositionIvm.indexUid,
                elastic: _elastic,
                primaryKey: p=>p.Id,
                fields: new List<Expression<Func<PositionIvm,dynamic>>>
                {
                    p=>p.Denominacion,
                }
            );
        }

        // GET: api/profession
        [HttpGet("profession")]
        public async Task<ActionResult> GetProfession()
        {
            return await PaginationUtility.ElasticSearchPaginate
            (
                query: Request.QueryString.Value,
                indexUid: ProfessionIvm.indexUid,
                elastic: _elastic,
                primaryKey: p=>p.Id,
                fields: new List<Expression<Func<ProfessionIvm,dynamic>>>
                {
                    p=>p.Denominacion,
                }
            );
        }
        
        [HttpGet("professional-college")]
        public async Task<ActionResult<Paginator<ColegioProfesional>>> GetProfessionalCollege()
        {
            return await PaginationUtility.ElasticSearchPaginate
            (
                query: Request.QueryString.Value,
                indexUid: ProfessionalCollegeIvm.indexUid,
                elastic: _elastic,
                primaryKey: cp=>cp.Id,
                fields: new List<Expression<Func<ProfessionalCollegeIvm,dynamic>>>
                {
                    pc=>pc.Denominacion,
                    pc=>pc.Decano,
                }
            );
        }
        
        // GET: api/turn
        [HttpGet("turn")]
        public async Task<ActionResult<Paginator<TurnIvm>>> GetTurn()
        {
            return await PaginationUtility.ElasticSearchPaginate
            (
                query: Request.QueryString.Value,
                indexUid: TurnIvm.indexUid,
                elastic: _elastic,
                primaryKey: cp=>cp.Id,
                fields: new List<Expression<Func<TurnIvm,dynamic>>>
                {
                    t=>t.Denominacion,
                    t=>t.Descripcion,
                }
            );
        }
    }
}