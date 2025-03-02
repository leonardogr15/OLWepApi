using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OLWebApi.Infrastructure.Data;
using System.Globalization;
using System.Text;

namespace OLWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class ReporteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReporteController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("comerciantes-csv")]
        public async Task<IActionResult> GetReporteComerciantes()
        {
            var reporte = await _context.Comerciante
                .Where(c => c.Estado == "Activo")
                .Select(c => new
                {
                    c.NombreRazonSocial,
                    c.Municipio,
                    c.Telefono,
                    c.CorreoElectronico,
                    c.FechaRegistro,
                    c.Estado,
                    CantidadEstablecimientos = _context.Establecimiento.Where(e => e.IdComerciante == c.IdComerciante).Count(),
                    TotalIngresos = _context.Establecimiento.Where(e => e.IdComerciante == c.IdComerciante).Sum(e => (decimal?)e.Ingresos) ?? 0,
                    CantidadEmpleados = _context.Establecimiento.Where(e => e.IdComerciante == c.IdComerciante).Sum(e => (int?)e.NumeroEmpleados) ?? 0
                })
                .ToListAsync();


            var sb = new StringBuilder();
            sb.AppendLine("NombreRazonSocial|Municipio|Telefono|CorreoElectronico|FechaRegistro|Estado|CantidadEstablecimientos|TotalIngresos|CantidadEmpleados");

            foreach (var item in reporte)
            {
                string fechaFormateada = item.FechaRegistro.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                sb.AppendLine($"{item.NombreRazonSocial}|{item.Municipio}|{item.Telefono}|{item.CorreoElectronico}|{fechaFormateada}|{item.Estado}|{item.CantidadEstablecimientos}|{item.TotalIngresos}|{item.CantidadEmpleados}");
            }

            var fileBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(fileBytes, "text/csv", "ReporteComerciantes.csv");
        }
    }
}

