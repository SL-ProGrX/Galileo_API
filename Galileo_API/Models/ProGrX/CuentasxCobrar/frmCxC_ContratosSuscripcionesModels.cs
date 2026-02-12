using System;

namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    // Base para contratos
    public class ContratoBase
    {
        public string Cod_Contrato { get; set; }
        public string Cedula { get; set; }
    }

    // DTO principal de contrato
    public class CxcPersonaContratoDto : ContratoBase
    {
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
        public string Nombre { get; set; }
    }

    public class CxcPersonaContratoSaveParams : ContratoBase
    {
        public string? Notas { get; set; }
        public short? Activo { get; set; }
        public int? Plazo { get; set; }
        public decimal? Tasa_Corriente { get; set; }
        public decimal? Tasa_Mora { get; set; }
        public string Usuario { get; set; }
        public string? Contrato_Num { get; set; }
        public string? Contrato_Tipo { get; set; }
        public DateTime? Contrato_Vence { get; set; }
    }

    public class CxcPersonaContratoDeleteParams : ContratoBase { }

    // Pagador
    public class CxcPersonaContratoPagadorDto : ContratoBase
    {
        public string? Cedula_Pagador { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
        public string Nombre { get; set; }
    }

    public class CxcPersonaContratoPagadorSaveParams : ContratoBase
    {
        public string Cedula_Pagador { get; set; }
        public string Registro_Usuario { get; set; }
    }

    public class CxcPersonaContratoPagadorDeleteParams : ContratoBase
    {
        public string Cedula_Pagador { get; set; }
    }

    // Suscripción
    public class CxcPersonaContratoSuscripcionDto : ContratoBase
    {
        public string Cod_Cargo { get; set; }
        public short? Frecuencia_Dias { get; set; }
        public DateTime? Pago_Ultimo { get; set; }
        public DateTime? Pago_Proximo { get; set; }
        public decimal? Recaudado { get; set; }
        public string? Frecuencia_Tipo { get; set; }
        public string? Tipo { get; set; }
        public decimal? Valor { get; set; }
        public short? Modifica { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
        public string Descripcion { get; set; }
    }

    public class CxcPersonaContratoSuscripcionSaveParams : ContratoBase
    {
        public string Cod_Cargo { get; set; }
        public string? Tipo { get; set; }
        public decimal? Valor { get; set; }
        public string? Frecuencia_Tipo { get; set; }
        public short Frecuencia_Dias { get; set; }
        public decimal? Recaudado { get; set; }
        public DateTime? Pago_Ultimo { get; set; }
        public DateTime? Pago_Proximo { get; set; }
        public short? Modifica { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; }
    }

    public class CxcPersonaContratoSuscripcionDeleteParams : ContratoBase
    {
        public string Cod_Cargo { get; set; }
    }
}
