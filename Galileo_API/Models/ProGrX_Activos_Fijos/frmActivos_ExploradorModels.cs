namespace Galileo.Models.ProGrX_Activos_Fijos
{
    public class ActivosExploradorFiltrosDto
    {
        public string? nombre { get; set; }
        public string? descripcion { get; set; }
        public string? responsable { get; set; }
        public string? proveedor { get; set; }

        public string? tipoActivo { get; set; }
        public string? departamento { get; set; }
        public string? seccion { get; set; }
        public string? ubicacion { get; set; }

        public string tipoPlaca { get; set; } = "Placa";
        public string? placaDesde { get; set; }
        public string? placaHasta { get; set; }

        public bool infoAdicional { get; set; }

        public DateTime? fechaAdqDesde { get; set; }
        public DateTime? fechaAdqHasta { get; set; }

        public DateTime? fechaInstDesde { get; set; }
        public DateTime? fechaInstHasta { get; set; }

        public string tipoVisualizacion { get; set; } = "L"; // L | A | C
    }

    public class ActivoExploradorDto
    {
        public string num_placa { get; set; }
        public string? placa_alterna { get; set; }
        public string nombre { get; set; }

        public DateTime fecha_adquisicion { get; set; }
        public DateTime? fecha_instalacion { get; set; }

        public string tipo_activo { get; set; }
        public string tipo_activo_desc { get; set; }

        public decimal valor_historico { get; set; }
        public decimal valor_desecho { get; set; }

        public string estado { get; set; }

        // SOLO si infoAdicional = true
        public string? responsable { get; set; }
        public string? departamento { get; set; }
        public string? seccion { get; set; }
        public string? localizacion { get; set; }
        public string? proveedor { get; set; }
    }


}