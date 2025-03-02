using System;
using System.ComponentModel.DataAnnotations;

namespace OLWebApi.Application.Dtos
{
    public class ComercianteDto
    {
        [Required(ErrorMessage = "El nombre o razón social es obligatorio")]
        public string NombreRazonSocial { get; set; }
        
        [Required(ErrorMessage = "El municipio es obligatorio")]
        public string Municipio { get; set; }
        
        public string Telefono { get; set; }
        
        [EmailAddress(ErrorMessage = "El formato del correo es inválido")]
        public string CorreoElectronico { get; set; }
        
        [Required(ErrorMessage = "La fecha de registro es obligatoria")]
        public DateTime FechaRegistro { get; set; }
        
        [Required(ErrorMessage = "El estado es obligatorio")]
        public string Estado { get; set; }
    }
}
