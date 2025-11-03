using PgxAPI.Models.ERROR;

namespace PgxAPI.Models.ProGrX.Bancos
{
    public class tesReImpresionModels
    {
        public int nSolicitud { get; set; } = 0;
        public string tipo { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string ndocumento { get; set; } = string.Empty;
        public int id_banco { get; set; } = 0;
        public string bancoX { get; set; } = string.Empty;
        public string tipoDocX { get; set; } = string.Empty;
        public string detalle_Anulacion { get; set; } = string.Empty;
        public string estado_Asiento { get; set; } = string.Empty;
        public string comprobante { get; set; } = string.Empty;
        public string verifica { get; set; } = string.Empty;
        public string verificaTag { get; set; } = string.Empty;

        public string usuario { get; set; } = string.Empty;
        public string clave { get; set; } = string.Empty;
        public string usuarioLogin { get; set; } = string.Empty;
    }

    public class tesReImpresionBancoData
    {
        public decimal firmas_desde { get; set; } = 0;
        public decimal firmas_hasta { get; set; } =0;
        public string formato_transferencia { get; set; } = string.Empty;
        public string Lugar_Emision { get; set; } = string.Empty;
    }

    public class tesReImpresionDoc
    {
        public string archivo_especial_ck { get; set; } = string.Empty;
        public string archivo_cheques_firmas { get; set; } = string.Empty;
        public string archivo_cheques_sin_firmas { get; set; } = string.Empty;
    }

    public class ResImpresion
    {
        public ErrorDTO Value { get; set; }
        public int StatusCode { get; set; }
    }

}
