namespace Galileo.Models.ProGrX_Activos_Fijos
{
    public class ActivosExploradorFiltrosDto
    {
        public string? nombre { get; set; }
        public string? descripcion { get; set; }
        public string? responsable { get; set; }
        public string? identificacion { get; set; }
        public string? responsable_codigo { get; set; }
        public bool? infoAdicional { get; set; }
        public string? proveedor { get; set; }
        public string? modelo { get; set; }
        public string? marca { get; set; }
        public string? proveedor_codigo { get; set; }
        public string? serie { get; set; }
        public int? lineas { get; set; } = 1000;

        public string? tipo_activo { get; set; }
        public string? departamento { get; set; }
        public string? seccion { get; set; }
        public string? ubicacion { get; set; }
        public string? localiza { get; set; }

        public string tipoPlaca { get; set; } = "Placa";
        public string? placa_tipo { get; set; }
        public string? placaDesde { get; set; }
        public string? placaHasta { get; set; }
        public string? placa_inicio { get; set; }
        public string? placa_fin { get; set; }
        public string? estado { get; set; }
        public DateTime? fecha_periodo { get; set; }

        public DateTime? fechaAdqDesde { get; set; }
        public DateTime? fechaAdqHasta { get; set; }

        public DateTime? fechaInstDesde { get; set; }
        public DateTime? fechaInstHasta { get; set; }

        public DateTime? fecha_adq_desde { get; set; }
        public DateTime? fecha_adq_hasta { get; set; }
        public bool fecha_adq_activa { get; set; }
        public DateTime? fecha_inst_desde { get; set; }
        public DateTime? fecha_inst_hasta { get; set; }
        public bool fecha_inst_activa { get; set; }

        public string? tipoVisualizacion { get; set; } = "L"; // L | A | C
    }

    public class ActivoExploradorDto
    {
        public string? num_placa { get; set; }
        public string? placa_alterna { get; set; }
        public string? nombre { get; set; }

        public DateTime? fecha_adquisicion { get; set; }
        public DateTime? fecha_instalacion { get; set; }

        public string? tipo_activo { get; set; }
        public string? tipo_activo_desc { get; set; }

        public decimal? valor_historico { get; set; }
        public decimal? valor_desecho { get; set; }

        public string? estado { get; set; }

        public string? responsable { get; set; }
        public string? identificacion { get; set; }
        public string? departamento { get; set; }
        public string? seccion { get; set; }
        public string? localizacion { get; set; }
        public string? proveedor { get; set; }
        public string? modelo { get; set; }
        public string? marca { get; set; }
        public string? num_serie { get; set; }
        public string? otras_senas { get; set; }

        public string? vida_util { get; set; }
        public decimal? depreciacion_anterior { get; set; }
        public decimal? depreciacion_mes { get; set; }
        public decimal? depreciacion_acumulada { get; set; }
        public decimal? valor_libros { get; set; }
        public DateTime? corte { get; set; }


    }

    public class PeriodoExploradorDto
    {
        public int? anio { get; set; }
        public int? mes { get; set; }
        public DateTime? fecha_periodo { get; set; }
        public string? periodo { get; set; } = string.Empty;
    }


    public class ActivosExploradorAsientoDto
    {
        public string? num_asiento { get; set; } = string.Empty;
        public string? tipo_asiento { get; set; } = string.Empty;
        public DateTime? fecha_asiento { get; set; }
        public string? descripcion { get; set; } = string.Empty;
        public decimal? debe { get; set; }
        public decimal? haber { get; set; }
        public string? aplicado { get; set; } = string.Empty;
        public string? notas { get; set; } = string.Empty;
    }

    public class ActivosExploradorAsientoDetalleDto
    {
        public string? cuenta { get; set; } = string.Empty;
        public string? descripcion { get; set; } = string.Empty;
        public decimal? debito { get; set; }
        public decimal? credito { get; set; }
        public string? detalle { get; set; } = string.Empty;
        public string? referencia { get; set; } = string.Empty;
        public string? num_documento { get; set; } = string.Empty;
    }

    public class ActivosExploradorModificacionDto
    {
        public int? id_addret { get; set; }
        public string? nombre { get; set; } = string.Empty;
        public string? num_placa { get; set; } = string.Empty;
        public string?    tipo { get; set; } = string.Empty;
        public string? justificacion { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public decimal? monto { get; set; }
        public string? descripcion { get; set; } = string.Empty;
    }





}
