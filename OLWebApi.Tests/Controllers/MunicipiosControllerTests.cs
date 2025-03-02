using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OLWebApi.Domain.Entities;
using OLWebApi.Infrastructure.Data;
using OLWebApi.Controllers;
using Xunit;

namespace OLWebApi.Tests.Controllers
{
    public class MunicipiosControllerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public MunicipiosControllerTests()
        {
            // Configura el DbContext para usar una base de datos en memoria
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);

            // Sembrar datos de prueba en la tabla de Comerciantes
            _context.Comerciante.AddRange(
                new Comerciante
                {
                    IdComerciante = 10,
                    NombreRazonSocial = "La Esquina Comercial",
                    Municipio = "Jamundí",
                    Telefono = "3101234567",
                    CorreoElectronico = "contacto@laesquina.com",
                    FechaRegistro = DateTime.UtcNow,
                    Estado = "Activo",
                    Usuario = "sa"
                },
                new Comerciante
                {
                    IdComerciante = 12,
                    NombreRazonSocial = "Supermercado Central",
                    Municipio = "Yumbo",
                    Telefono = "3123456789",
                    CorreoElectronico = "info@supercentral.com",
                    FechaRegistro = DateTime.UtcNow,
                    Estado = "Activo",
                    Usuario = "sa"
                },
                new Comerciante
                {
                    IdComerciante = 13,
                    NombreRazonSocial = "Panadería El Buen Pan",
                    Municipio = "Palmira",
                    Telefono = "3156789012",
                    CorreoElectronico = "ventas@elbuenpan.com",
                    FechaRegistro = DateTime.UtcNow,
                    Estado = "Activo",
                    Usuario = "sa"
                }
            );

            _context.SaveChanges();

            // Configura un MemoryCache real para las pruebas
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        [Fact]
        public void GetMunicipios_ReturnsDistinctMunicipios()
        {
            // Arrange: crear instancia del controlador inyectando el contexto y el cache
            var controller = new MunicipiosController(_context, _cache);
            var result = controller.GetMunicipios();
            var okResult = Assert.IsType<OkObjectResult>(result);

            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var response = System.Text.Json.JsonSerializer.Deserialize<ResponseModel>(json);

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(3, response.Data.Count);
            Assert.Contains("Jamundí", response.Data);
            Assert.Contains("Yumbo", response.Data);
            Assert.Contains("Palmira", response.Data);
        }

        public class ResponseModel
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public List<string> Data { get; set; }
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _cache.Dispose();
        }
    }
}
