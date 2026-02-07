namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{

    public class ContratoBusquedaDto
    {
        public string Contrato { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class ContratoDetalleDto
    {
        public string Cod_Contrato { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Notas { get; set; }
        public short Activo { get; set; }
        public int? Plazo { get; set; }
        public decimal? Tasa_Corriente { get; set; }
        public decimal? Tasa_Mora { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
        public DateTime? Actualiza_Fecha { get; set; }
        public string? Actualiza_Usuario { get; set; }
        public short? Suscripcion_Abierta { get; set; }
        public short? Pagadores_Abierto { get; set; }
    }

    public class ContratoPersonaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Cod_Contrato { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public short Activo { get; set; }
        public int? Plazo { get; set; }
        public decimal? Tasa_Corriente { get; set; }
        public decimal? Tasa_Mora { get; set; }
        public string? Notas { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
        public DateTime? Actualiza_Fecha { get; set; }
        public string? Actualiza_Usuario { get; set; }
        public string? Contrato_Num { get; set; }
        public DateTime? Contrato_Vence { get; set; }
        public string? Contrato_Tipo { get; set; }
    }

    public class ContratoPersonaDeleteParams
    {
        public string Cod_Contrato { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string? Usuario { get; set; } = string.Empty;
    }

    public class ContratoPagadorDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Cod_Contrato { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
    }

    public class ContratoCargoDto
    {
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Contrato { get; set; } = string.Empty;
        public string Cod_Cargo { get; set; } = string.Empty;
        public string? Tipo { get; set; }
        public decimal? Valor { get; set; }
        public string? Frecuencia_Tipo { get; set; }
        public short Frecuencia_Dias { get; set; }
        public short Modifica { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
    }

    public class ContratoCargoDeleteParams
    {
        public string Cod_Contrato { get; set; } = string.Empty;
        public string Cod_Cargo { get; set; } = string.Empty;
        public string? Usuario { get; set; } = string.Empty;
    }

    public class ContratoConceptoDto
    {
        public string Codigo { get; set; } = string.Empty; // cod_concepto
        public string Descripcion { get; set; } = string.Empty;
        public string? Cod_Contrato { get; set; }
        public string? Cod_Concepto { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
    }

    public class ContratoConceptoParams
    {
        public string Cod_Contrato { get; set; } = string.Empty;
        public string Cod_Concepto { get; set; } = string.Empty;
        public string? Usuario { get; set; } = string.Empty;
    }

    public class ContratoSaveParams
    {
        public string Cod_Contrato { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? Notas { get; set; }
        public short? Activo { get; set; }
        public int? Plazo { get; set; }
        public decimal? Tasa_Corriente { get; set; }
        public decimal? Tasa_Mora { get; set; }
        public short? Suscripcion_Abierta { get; set; }
        public short? Pagadores_Abierto { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class ContratoDeleteParams
    {
        public string Cod_Contrato { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

}
