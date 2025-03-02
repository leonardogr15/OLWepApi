using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OLWebApi.Infrastructure.Data;  // Asegúrate que AppDbContext esté en este namespace
using System;
using System.Collections.Generic;
using System.Linq;

namespace OLWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MunicipiosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "MunicipiosCache";

        public MunicipiosController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult GetMunicipios()
        {
            // Intenta obtener los datos desde el cache
            if (!_cache.TryGetValue(CacheKey, out List<string> municipios))
            {
                // Consulta a la base de datos: selecciona los municipios únicos de la entidad Comerciantes
                municipios = _context.Comerciante
                                     .Select(c => c.Municipio)
                                     .Distinct()
                                     .ToList();

                // Configura las opciones del cache (por ejemplo, expiración de 10 minutos)
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                // Guarda en cache
                _cache.Set(CacheKey, municipios, cacheEntryOptions);
            }

            // Retorna la respuesta estandarizada
            return Ok(new
            {
                Success = true,
                Message = "Lista de municipios obtenida exitosamente",
                Data = municipios
            });
        }
    }
}
