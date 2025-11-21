using Galileo.Models.ERROR;
namespace Galileo.Models.ProGrX.Bancos
{
    public class TesReImpresionModels
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

    public class TesReImpresionBancoData
    {
        public decimal firmas_desde { get; set; } = 0;
        public decimal firmas_hasta { get; set; } =0;
        public string formato_transferencia { get; set; } = string.Empty;
        public string Lugar_Emision { get; set; } = string.Empty;
    }

    public class TesReImpresionDoc
    {
        public string archivo_especial_ck { get; set; } = string.Empty;
        public string archivo_cheques_firmas { get; set; } = string.Empty;
        public string archivo_cheques_sin_firmas { get; set; } = string.Empty;
    }

    public class ResImpresion
    {
        public ErrorDto? Value { get; set; }
        public int StatusCode { get; set; }
    }
}