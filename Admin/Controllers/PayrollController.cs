using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Admin.DTO;
using Admin.Models;
using Domain.Models;
using Lizelaser0310.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayrollController : ControllerBase
    {
        private readonly HISCOMContext _context;
        private readonly IKeys _keys;
        private readonly IConstants _constants;

        public PayrollController(HISCOMContext context, IKeys keys, IConstants constants)
        {
            _context = context;
            _keys = keys;
            _constants = constants;
        }



        // GET: api/Payroll
        [HttpGet]
        public async Task<ActionResult> GetHistoryPayroll()
        {

            if (User.Identity?.Name == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(User.Identity.Name);

            var user = await _context.Usuario
                .Include(u => u.Empleado)
                .SingleOrDefaultAsync(u => u.Id == userId);

            if (user?.Empleado==null)
            {
                return Unauthorized();
            }

            return await PaginationUtility.Paginate
            (
                query: Request.QueryString.Value,
                dbSet: _context.PlhPlanilla,
                middle: (plhNom, _) => plhNom.Where(p => p.Libele == user.Empleado.NumeroDoc).OrderByDescending(x=>x.Id),
                mutation: (pn) => new PayrollDTO()
                {
                    Id = pn.Id,
                    Type = pn.IndNombrado?$"{TipoPlanillaEnum.Nombrado}":$"{TipoPlanillaEnum.Contratado}",
                    Period = $"{pn.Mes}-{pn.Anio}"
                });
        }
    }
}
