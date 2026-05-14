using System.Text.Json.Serialization;

namespace Galileo.Models.ProGrX.Fondos
{
    public class FndContratoDto
    {
        public int cod_contrato { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class FiltrosBuscarExclusionDto
    {
        public string? cod_operadora { get; set; }
        public string? cod_plan { get; set; } = string.Empty;
        public string? cod_plan_desc { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public int? contrato { get; set; }
        public string? cedula { get; set; } = string.Empty;
        public string? nombre { get; set; } = string.Empty;
        public int? cod_contrato { get; set; } 
        public bool todas_fechas { get; set; } = false;
    }

    public class FndExclusionMultaDto
    {
        public int idregistro { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int cod_contrato { get; set; }
        public string cod_plan { get; set; } = string.Empty;
        public string plan_desc { get; set; } = string.Empty;
        public string excluye_desc { get; set; } = string.Empty;
        public DateTime? fecha_registro { get; set; }
        public string usuario_registro { get; set; } = string.Empty;
        public DateTime? fecha_actualiza { get; set; }
        public string usuario_actualiza { get; set; } = string.Empty;
    }

    public class RegistrarExclusionDto
    {
        public string? cod_operadora { get; set; }
        public string? cod_plan { get; set; } = string.Empty;
        public int? cod_contrato { get; set; }
        public string? cedula { get; set; } = string.Empty;
        public bool excluye { get; set; }
        public string? usuario { get; set; } = string.Empty;
    }
}