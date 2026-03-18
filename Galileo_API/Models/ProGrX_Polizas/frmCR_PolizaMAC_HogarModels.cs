namespace Galileo_API.Models.ProGrX_Polizas
{
  
    public abstract class CrPolizaMacHogarRecepcionRequestBase
    {
        public string Poliza { get; set; } = string.Empty;
        public DateTime Corte { get; set; } = DateTime.Now;
        public DateTime? Factura { get; set; }
        public List<CrPolizaMacHogarRecepcionRowDto> Filas { get; set; } = new();
    }

    public class CrPolizaMacHogarEnvioConsultaRequest
    {
        public string Poliza { get; set; } = string.Empty;
        public DateTime Corte { get; set; } = DateTime.Now;
        public short Beneficiarios { get; set; } = 1;
        public string TipoMovimiento { get; set; } = "T";
    }

    public class CrPolizaMacHogarEnvioRow : BeneficiariosB1B6MacHogar
    {
        public DateTime? corte { get; set; }
        public int? tipoid { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string apellido_1 { get; set; } = string.Empty;
        public string apellido_2 { get; set; } = string.Empty;
        public string nombre_1 { get; set; } = string.Empty;
        public string nombre_2 { get; set; } = string.Empty;
        public string genero { get; set; } = string.Empty;
        public DateTime? fecha_nacimiento { get; set; }
        public string email { get; set; } = string.Empty;
        public string nacionalidad { get; set; } = string.Empty;
        public string provincia { get; set; } = string.Empty;
        public string canton { get; set; } = string.Empty;
        public string distrito { get; set; } = string.Empty;
        public string tipo_telefono { get; set; } = string.Empty;
        public string telefono { get; set; } = string.Empty;
        public decimal? monto_asegurado_01 { get; set; }
        public decimal? monto_asegurado_02 { get; set; }
        public decimal? prima_recaudada { get; set; }
        public string numero_poliza { get; set; } = string.Empty;
        public string numero_referencia { get; set; } = string.Empty;
        public decimal? recargo { get; set; }
        public string movimiento { get; set; } = string.Empty;
        public string nacionalidad_desc { get; set; } = string.Empty;
        public string nacionalidad_cod_alter { get; set; } = string.Empty;
        public string moneda { get; set; } = string.Empty;
        public string nombre_completo { get; set; } = string.Empty;
        public int? edad { get; set; }
        public DateTime? fecha_emision { get; set; }
        public string provincia_desc { get; set; } = string.Empty;
        public string canton_desc { get; set; } = string.Empty;
        public string distrito_desc { get; set; } = string.Empty;
        public DateTime? poliza_emite { get; set; }
        public string cod_poliza { get; set; } = string.Empty;
        public int? id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public long? credito_operacion { get; set; }
        public string credito_codigo { get; set; } = string.Empty;
        public DateTime? credito_fecha { get; set; }
        public decimal? credito_monto { get; set; }
        public decimal? credito_saldo { get; set; }
        public string credito_estado { get; set; } = string.Empty;
        public int? vinculadas { get; set; }
        public string? dir_completa { get; set; }
    }

    public class CrPolizaMacHogarBeneficiariosRowDto : BeneficiariosB1B6MacHogar
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CrPolizaMacHogarRecepcionValidarRequest : CrPolizaMacHogarRecepcionRequestBase
    {
    }

    public class CrPolizaMacHogarRecepcionProcesarRequest : CrPolizaMacHogarRecepcionRequestBase
    {
    }

    public abstract class BeneficiariosB1B6MacHogar : PolizaBeneficiariosB1B6Base
    {
    }

    public class CrPolizaMacHogarRecepcionRowDto : PolizaRecepcionRowBase
    {
    }

}
