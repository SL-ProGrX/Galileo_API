
using System.ComponentModel.DataAnnotations;

namespace Galileo.Models.ProGrX.Cobros
{
    public class CoControlSegHistGestionDto
    {
        public int cod_seg { get; set; }
        public DateTime? fecha { get; set; }
        public int? tiempo_resolucion { get; set; }
        public DateTime? vence { get; set; }
        public string? cod_gestion { get; set; }
        public string? gestion { get; set; }
        public string? notas { get; set; }
        public string? usuario { get; set; }
        public decimal? monto { get; set; }
        public string? cod_arreglo { get; set; }
        public string? arreglo { get; set; }
        public DateTime? arreglo_vence { get; set; }
        public string? cod_causa { get; set; }
        public string? causa { get; set; }
    }
    public class CoControlSegHistOficialDto
    {
        public DateTime? fecha_asignacion { get; set; }
        public string? usuario { get; set; }
        public int? mantener { get; set; }
        public int? rebajo_doble { get; set; }
        public int? aplica_mora { get; set; }
    }

    public class CoControlSegHistOficialActualizarDto
    {
        public string cedula { get; set; } = string.Empty;
        [Required]
        public DateTime? fecha_asignacion { get; set; }
        public string usuario_asignado { get; set; } = string.Empty;
        [Required]
        public int? mantener { get; set; }
        [Required]
        public int? rebajo_doble { get; set; }
        [Required]
        public int? aplica_mora { get; set; }

        public string usuario { get; set; } = string.Empty;
    }

    public class CoControlSegGestionInfoDto
    {
        public string cod_gestion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public int modifica_usuario { get; set; }
        public decimal modifica_desviacion { get; set; }
        public int acceso { get; set; }
    }

    public class CoControlSegVenceRangoDto
    {
        public DateTime fecha_min { get; set; }
        public DateTime fecha_max { get; set; }
        public int dias_max { get; set; }
    }

    public class CoControlSegRegistrarDto
    {
        public string cedula { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;

        public string cod_gestion { get; set; } = string.Empty;
        [Required]
        public DateTime? vence { get; set; }
        public string notas { get; set; } = string.Empty;

        public string oficina { get; set; } = string.Empty;
        [Required]
        public decimal? monto { get; set; }
        public int operacion { get; set; } = 0;

        public string cod_causa { get; set; } = string.Empty;
        public string cod_arreglo { get; set; } = string.Empty;
    }
    public class CoControlSegFiadorDto
    {
        public string? estado_mora { get; set; }
        public int id_solicitud { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public string? estado_persona { get; set; }
        public string? institucion { get; set; }
    }
    public class CoControlSegComisionDto
    {
        public string? cod_remesa { get; set; }
        public string? usuario { get; set; }
        public decimal? monto { get; set; }
        public string? tesoreria_numero { get; set; }
        public DateTime? tesoreria_fecha { get; set; }
    }
    public class CoControlSegEstadoDto
    {
        public string cedula { get; set; } = "";
        public int operaciones_activas { get; set; }
        public int operaciones_mora_activa { get; set; }
        public string texto { get; set; } = "";
        public string estado_tag { get; set; } = "N";
    }
    public class CoControlSegRetencionDto
    {
        public int id_solicitud { get; set; }
        public string? codigo { get; set; }  
        public string? descripcion { get; set; }     
        public decimal? abonos { get; set; }     
        public string? gestion { get; set; }         
        public decimal? monto { get; set; }          
    }
    public class CoControlSegHistDetalleDto
    {
        public int operacion { get; set; }
        public string? codigo { get; set; }    
        public int? cuotas { get; set; }      
        public decimal? mora { get; set; }    
        public decimal? saldo { get; set; } 
        public decimal? abono { get; set; }     
        public string? estado_actual { get; set; }  
    }

}
