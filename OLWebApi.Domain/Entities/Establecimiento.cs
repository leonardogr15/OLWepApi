using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OLWebApi.Domain.Entities
{
    public class Establecimiento
    {
        [Key]
        public int IdEstablecimiento { get; set; }

        [Required]
        public string NombreEstablecimiento { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Ingresos { get; set; }

        [Required]
        public int NumeroEmpleados { get; set; }

        [ForeignKey("Comerciante")]
        public int IdComerciante { get; set; }
        public Comerciante Comerciante { get; set; }
    }
}
