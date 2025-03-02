using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OLWebApi.Application.Dtos;
using OLWebApi.Infrastructure.Data;
using OLWebApi.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace OLWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // Todos los endpoints requieren token JWT
    public class ComercianteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ComercianteController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Consulta paginada con filtros (GET: /api/comerciante)
        [HttpGet]
        public async Task<IActionResult> GetComerciantes(
            [FromQuery] string nombre = null,
            [FromQuery] DateTime? fechaRegistro = null,
            [FromQuery] string estado = null,
            [FromQuery] int page = 1)
        {
            const int pageSize = 5;

            var query = _context.Comerciante.AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(c => c.NombreRazonSocial.Contains(nombre));

            if (fechaRegistro.HasValue)
                query = query.Where(c => c.FechaRegistro.Date == fechaRegistro.Value.Date);

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(c => c.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase));

            var total = await query.CountAsync();

            var comerciantes = await query
                .OrderBy(c => c.IdComerciante)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Consulta paginada exitosa",
                data = comerciantes,
                total,
                page,
                pageSize
            });
        }

        // 2. Consultar por Id (GET: /api/comerciante/{id})
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetComercianteById(int id)
        {
            var comerciante = await _context.Comerciante.FindAsync(id);
            if (comerciante == null)
                return NotFound(new { success = false, message = "Comerciante no encontrado" });

            return Ok(new { success = true, data = comerciante });
        }

        // 3. Crear Comerciante (POST: /api/comerciante)
        [HttpPost]
        public async Task<IActionResult> CreateComerciante([FromBody] ComercianteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Asignar los valores del DTO a variables locales
            var nombre = dto.NombreRazonSocial;
            var municipio = dto.Municipio;
            var telefono = dto.Telefono;
            var correo = dto.CorreoElectronico;
            var fechaRegistro = dto.FechaRegistro;
            var estado = dto.Estado;
            var fechaActualizacion = DateTime.UtcNow;
            var usuario = User.Identity.Name ?? "Sistema";

            // Configurar el parámetro de salida para capturar el ID generado
            var newIdParam = new SqlParameter
            {
                ParameterName = "@NewId",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output
            };

            // Ejecutar el procedimiento almacenado
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.InsertComerciante @NombreRazonSocial, @Municipio, @Telefono, @CorreoElectronico, @FechaRegistro, @Estado, @FechaActualizacion, @Usuario, @NewId OUTPUT",
                new SqlParameter("@NombreRazonSocial", nombre),
                new SqlParameter("@Municipio", municipio),
                new SqlParameter("@Telefono", telefono ?? (object)DBNull.Value),
                new SqlParameter("@CorreoElectronico", correo ?? (object)DBNull.Value),
                new SqlParameter("@FechaRegistro", fechaRegistro),
                new SqlParameter("@Estado", estado),
                new SqlParameter("@FechaActualizacion", fechaActualizacion),
                new SqlParameter("@Usuario", usuario),
                newIdParam
            );

            // Leer el valor del parámetro de salida (ID generado)
            int newId = (int)newIdParam.Value;
            var comerciante = await _context.Comerciante.FindAsync(newId);

            return CreatedAtAction(nameof(GetComercianteById), new { id = newId }, new { success = true, data = comerciante });
        }


        // 4. Actualizar Comerciante (PUT: /api/comerciante/{id})
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateComerciante(int id, [FromBody] ComercianteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var comerciante = await _context.Comerciante.FindAsync(id);
            if (comerciante == null)
                return NotFound(new { success = false, message = "Comerciante no encontrado" });

            var nombre = dto.NombreRazonSocial;
            var municipio = dto.Municipio;
            var telefono = dto.Telefono;
            var correo = dto.CorreoElectronico;
            var fechaRegistro = dto.FechaRegistro;
            var estado = dto.Estado;
            var fechaActualizacion = DateTime.UtcNow;
            var usuario = User.Identity.Name ?? "Sistema";

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.UpdateComerciante @IdComerciante, @NombreRazonSocial, @Municipio, @Telefono, @CorreoElectronico, @FechaRegistro, @Estado, @FechaActualizacion, @Usuario",
                new SqlParameter("@IdComerciante", id),
                new SqlParameter("@NombreRazonSocial", nombre),
                new SqlParameter("@Municipio", municipio),
                new SqlParameter("@Telefono", telefono ?? (object)DBNull.Value),
                new SqlParameter("@CorreoElectronico", correo ?? (object)DBNull.Value),
                new SqlParameter("@FechaRegistro", fechaRegistro),
                new SqlParameter("@Estado", estado),
                new SqlParameter("@FechaActualizacion", fechaActualizacion),
                new SqlParameter("@Usuario", usuario)
            );

            comerciante = await _context.Comerciante.FindAsync(id);

            return Ok(new { success = true, data = comerciante });
        }

        // 5. Modificar el estado del comerciante mediante PATCH (PATCH: /api/comerciante/{id}/estado)
        [HttpPatch("{id:int}/estado")]
        public async Task<IActionResult> UpdateEstado(int id, [FromBody] string nuevoEstado)
        {
            // Verifica que el comerciante exista
            var comerciante = await _context.Comerciante.FindAsync(id);
            if (comerciante == null)
                return NotFound(new { success = false, message = "Comerciante no encontrado" });

            var fechaActualizacion = DateTime.UtcNow;
            var usuario = User.Identity.Name ?? "Sistema";

            // Ejecuta el procedimiento almacenado para actualizar el estado
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.UpdateEstadoComerciante @IdComerciante, @Estado, @FechaActualizacion, @Usuario",
                new SqlParameter("@IdComerciante", id),
                new SqlParameter("@Estado", nuevoEstado),
                new SqlParameter("@FechaActualizacion", fechaActualizacion),
                new SqlParameter("@Usuario", usuario)
            );

            var updatedComerciante = await _context.Comerciante.FindAsync(id);

            return Ok(new { success = true, data = updatedComerciante });
        }

        // 6. Eliminar Comerciante (DELETE: /api/comerciante/{id})
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteComerciante(int id)
        {
            var comerciante = await _context.Comerciante.FindAsync(id);
            if (comerciante == null)
                return NotFound(new { success = false, message = "Comerciante no encontrado" });

            _context.Comerciante.Remove(comerciante);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Comerciante eliminado exitosamente" });
        }
    }
}
