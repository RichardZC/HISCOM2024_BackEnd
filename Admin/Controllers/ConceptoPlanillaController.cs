using Admin.Indexation;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Admin.Controllers
{
    [Route("api/concepto-planilla")]
    [ApiController]
    public class ConceptoPlanillaController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly ElasticClient _elastic;

        public ConceptoPlanillaController(HISCOMContext context, ElasticClient elastic)
        {
            _context = context;
            _elastic = elastic;
        }

        // GET: api/concepto-planilla/*
        [HttpGet("{id}")]
        public async Task<ActionResult<PlhConcepto>> GetConceptoPlanilla(int id)
        {
            var obj = await _context.PlhConcepto.FindAsync(id);
            if (obj == null)            
                return NotFound();
            return obj;
        }

        // PUT: api/concepto-planilla/*
        [HttpPut("{id}")]
        public async Task<IActionResult> PutConceptoPlanilla(int id, PlhConcepto conceptoPlanilla)
        {
            if (id != conceptoPlanilla.Id)            
                return BadRequest();
            
            var conceptoPlanillaEntity = await _context.PlhConcepto.Where(te => te.Id == conceptoPlanilla.Id).SingleOrDefaultAsync();
            conceptoPlanillaEntity.FechaMod = DateTime.Now;
            conceptoPlanillaEntity.Abreviado = conceptoPlanilla.Abreviado;
            conceptoPlanillaEntity.Denominacion = conceptoPlanilla.Denominacion;
            await _context.SaveChangesAsync();                     

            var conceptoPlanillaIvm = ConceptoPlanillaIvm.GetConceptoPlanillaIvm(conceptoPlanillaEntity);
            await _elastic.UpdateAsync<ConceptoPlanillaIvm>(conceptoPlanillaIvm.Id, u =>
                u.Index(ConceptoPlanillaIvm.indexUid).Doc(conceptoPlanillaIvm));

            return NoContent();
        }
    }
}
