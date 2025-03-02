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
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // base de datos única para cada prueba
                .Options;
            _context = new AppDbContext(options);

            // Sembrar datos de prueba en la tabla de Comerciantes
            _context.Comerciante.AddRange(
                new Comerciante { IdComerciante = 1, NombreRazonSocial = "Comerciante A", Municipio = "Municipio1", Telefono = "123", CorreoElectronico = "a@example.com", FechaRegistro = DateTime.UtcNow, Estado = "Activo", Usuario = "sa" },
                new Comerciante { IdComerciante = 2, NombreRazonSocial = "Comerciante B", Municipio = "Municipio2", Telefono = "456", CorreoElectronico = "b@example.com", FechaRegistro = DateTime.UtcNow, Estado = "Activo", Usuario = "sa" },
                new Comerciante { IdComerciante = 3, NombreRazonSocial = "Comerciante C", Municipio = "Municipio1", Telefono = "789", CorreoElectronico = "c@example.com", FechaRegistro = DateTime.UtcNow, Estado = "Activo", Usuario = "sa" }
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

            // Act: invocar el método GetMunicipios
            var result = controller.GetMunicipios();

            // Assert: verificar que se retorna un OkObjectResult y que contiene la lista esperada
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Suponiendo que el controlador retorna un objeto anónimo con propiedades "success", "message" y "data"
            // Usamos la serialización para convertirlo a un diccionario, o bien podemos definir una clase de respuesta.
            // Aquí, para simplificar, usaremos System.Text.Json para convertir el objeto.
            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var response = System.Text.Json.JsonSerializer.Deserialize<ResponseModel>(json);

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            // En nuestro seed tenemos 2 municipios distintos: "Municipio1" y "Municipio2"
            Assert.Equal(2, response.Data.Count);
            Assert.Contains("Municipio1", response.Data);
            Assert.Contains("Municipio2", response.Data);
        }

        // Clase auxiliar para mapear la respuesta (ajusta los nombres según tu implementación)
        public class ResponseModel
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public List<string> Data { get; set; }
        }

        // Implementa IDisposable para limpiar la base de datos en memoria después de cada prueba
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _cache.Dispose();
        }
    }
}
