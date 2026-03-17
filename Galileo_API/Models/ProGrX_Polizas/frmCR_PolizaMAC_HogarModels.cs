namespace Galileo_API.Models.ProGrX_Polizas
{
    public abstract class BeneficiariosB1B6MacHogar
    {
        public int? b1_tipo_id { get; set; }
        public string b1_cedula { get; set; } = string.Empty;
        public string b1_nombre { get; set; } = string.Empty;
        public string b1_parentesco { get; set; } = string.Empty;
        public decimal? b1_porcentaje { get; set; }

        public int? b2_tipo_id { get; set; }
        public string b2_cedula { get; set; } = string.Empty;
        public string b2_nombre { get; set; } = string.Empty;
        public string b2_parentesco { get; set; } = string.Empty;
        public decimal? b2_porcentaje { get; set; }

        public int? b3_tipo_id { get; set; }
        public string b3_cedula { get; set; } = string.Empty;
        public string b3_nombre { get; set; } = string.Empty;
        public string b3_parentesco { get; set; } = string.Empty;
        public decimal? b3_porcentaje { get; set; }

        public int? b4_tipo_id { get; set; }
        public string b4_cedula { get; set; } = string.Empty;
        public string b4_nombre { get; set; } = string.Empty;
        public string b4_parentesco { get; set; } = string.Empty;
        public decimal? b4_porcentaje { get; set; }

        public int? b5_tipo_id { get; set; }
        public string b5_cedula { get; set; } = string.Empty;
        public string b5_nombre { get; set; } = string.Empty;
        public string b5_parentesco { get; set; } = string.Empty;
        public decimal? b5_porcentaje { get; set; }

        public int? b6_tipo_id { get; set; }
        public string b6_cedula { get; set; } = string.Empty;
        public string b6_nombre { get; set; } = string.Empty;
        public string b6_parentesco { get; set; } = string.Empty;
        public decimal? b6_porcentaje { get; set; }
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

    public class CrPolizaMacHogarRecepcionRowDto
    {
        public short numero_linea { get; set; }
        public string documento { get; set; } = string.Empty;
        public int fecha_proceso { get; set; }
        public int cod_operadora { get; set; }
        public string cod_plan { get; set; } = string.Empty;
        public int cod_contrato { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal fondos { get; set; }
        public short cod_institucion { get; set; }
        public short existe_persona { get; set; }
        public short existe_contrato { get; set; }
        public short procesado { get; set; }
    }

    public class CrPolizaMacHogarRecepcionValidarRequest
    {
        public string Poliza { get; set; } = string.Empty;
        public DateTime Corte { get; set; } = DateTime.Now;
        public DateTime? Factura { get; set; }
        public List<CrPolizaMacHogarRecepcionRowDto> Filas { get; set; } = new();
    }

    public class CrPolizaMacHogarRecepcionProcesarRequest
    {
        public string Poliza { get; set; } = string.Empty;
        public DateTime Corte { get; set; } = DateTime.Now;
        public DateTime? Factura { get; set; }
        public List<CrPolizaMacHogarRecepcionRowDto> Filas { get; set; } = new();
    }
}
