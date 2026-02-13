using System;

namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxcCargoDto
    {
        public string Cod_Cargo { get; set; }
        public string Descripcion { get; set; }
    }

    public class CxcContratoCargoDto
    {
        public string Cod_Contrato { get; set; }
        public string Cod_Cargo { get; set; }
        public string Descripcion { get; set; }
        public string? Tipo { get; set; }
        public decimal? Valor { get; set; }
        public string? Frecuencia_Tipo { get; set; }
        public short Frecuencia_Dias { get; set; }
        public short Modifica { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
    }
    public class CxcContratoCargoSaveParams
    {
        public string Cod_Contrato { get; set; }
        public string Cod_Cargo { get; set; }
        public string? Tipo { get; set; }
        public decimal? Valor { get; set; }
        public string? Frecuencia_Tipo { get; set; }
        public short? Frecuencia_Dias { get; set; }
        public short? Modifica { get; set; }
        public string Registro_Usuario { get; set; }
    }

    public class CxcContratoCargoDeleteParams
    {
        public string Cod_Contrato { get; set; }
        public string Cod_Cargo { get; set; }
    }
}
