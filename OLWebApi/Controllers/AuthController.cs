using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using OLWebApi.Infrastructure.Data; // Asegúrate de usar el namespace correcto donde está AppDbContext
using OLWebApi.Domain.Entities;


namespace OLWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        // Modelo para recibir las credenciales
        public class LoginRequest
        {
            public string CorreoElectronico { get; set; }
            public string Contraseña { get; set; }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Consulta a la base de datos para buscar el usuario
            var usuario = _context.Usuario.FirstOrDefault(u =>
                u.CorreoElectronico == request.CorreoElectronico &&
                u.Contraseña == request.Contraseña);

            if (usuario == null)
            {
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            // Genera el token JWT utilizando el rol y correo del usuario
            var token = GenerateJwtToken(usuario.Rol, usuario.CorreoElectronico);
            return Ok(new { token });
        }
        //public IActionResult Login([FromBody] LoginRequest request)
        //{
        //    // TODO: Aquí debes validar las credenciales contra la base de datos.
        //    // Por ejemplo, verificar si existe un usuario con ese correo y contraseña.
        //    // Este ejemplo es solo demostrativo (NO usar en producción).
        //    if (request.CorreoElectronico == "admin@empresa.com" && request.Contraseña == "adminpass")
        //    {
        //        var token = GenerateJwtToken("Administrador", request.CorreoElectronico);
        //        return Ok(new { token });
        //    }
        //    else if (request.CorreoElectronico == "auxiliar@empresa.com" && request.Contraseña == "auxiliarpass")
        //    {
        //        var token = GenerateJwtToken("Auxiliar de Registro", request.CorreoElectronico);
        //        return Ok(new { token });
        //    }
        //    else
        //    {
        //        return Unauthorized(new { message = "Credenciales inválidas" });
        //    }
        //}

        private string GenerateJwtToken(string role, string correo)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            if (string.IsNullOrEmpty(secretKey))
                throw new ArgumentNullException("SecretKey", "La llave secreta no puede ser nula o vacía.");

            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var securityKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, correo),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
