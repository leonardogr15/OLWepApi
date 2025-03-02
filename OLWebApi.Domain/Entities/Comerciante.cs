using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLWebApi.Domain.Entities
{
    public class Comerciante
    {
        [Key]
        public int IdComerciante { get; set; }

        public string NombreRazonSocial { get; set; }
        public string Municipio { get; set; }
        public string? Telefono { get; set; }
        public string? CorreoElectronico { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public string Usuario { get; set; }

        public ICollection<Establecimiento> Establecimientos { get; set; }
    }
}
