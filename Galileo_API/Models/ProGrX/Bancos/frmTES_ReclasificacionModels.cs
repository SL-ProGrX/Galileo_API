using PgxAPI.Models.TES;
using System;

namespace PgxAPI.Models.ProGrX.Bancos
{
    public class Tes_ReclasificacionDTO
    {
        public int nsolicitud { get; set; }
        public int? id_banco { get; set; }
        public string tipo { get; set; }
        public string codigo { get; set; }
        public string beneficiario { get; set; }
        public decimal? monto { get; set; }
        public DateTime? fecha_solicitud { get; set; }
        public string estado { get; set; }
        public DateTime? fecha_emision { get; set; }
        public DateTime? fecha_anula { get; set; }
        public string estadoi { get; set; }
        public string modulo { get; set; }
        public string cta_ahorros { get; set; }
        public string ndocumento { get; set; }
        public string detalle1 { get; set; }
        public string detalle2 { get; set; }
        public string detalle3 { get; set; }
        public string detalle4 { get; set; }
        public string detalle5 { get; set; }
        public string referencia { get; set; }
        public string submodulo { get; set; }
        public string genera { get; set; }
        public string actualiza { get; set; }
        public string ubicacion_actual { get; set; }
        public DateTime? fecha_traslado { get; set; }
        public string ubicacion_anterior { get; set; }
        public string entregado { get; set; }
        public string autoriza { get; set; }
        public DateTime? fecha_asiento { get; set; }
        public DateTime? fecha_asiento2 { get; set; }
        public string estado_asiento { get; set; }
        public DateTime? fecha_autorizacion { get; set; }
        public string user_autoriza { get; set; }
        public string op { get; set; }
        public string detalle_anulacion { get; set; }
        public string user_asiento_emision { get; set; }
        public string user_asiento_anula { get; set; }
        public string cod_concepto { get; set; }
        public string cod_unidad { get; set; }
        public string user_genera { get; set; }
        public string user_solicita { get; set; }
        public string user_anula { get; set; }
        public string user_entrega { get; set; }
        public DateTime? fecha_entrega { get; set; }
        public string documento_ref { get; set; }
        public string documento_base { get; set; }
        public string detalle { get; set; }
        public string user_hold { get; set; }
        public DateTime? fecha_hold { get; set; }
        public DateTime? firmas_autoriza_fecha { get; set; }
        public string firmas_autoriza_usuario { get; set; }
        public decimal? tipo_cambio { get; set; }
        public string cod_divisa { get; set; }
        public int? tipo_beneficiario { get; set; }
        public string cod_app { get; set; }
        public string ref_01 { get; set; }
        public string ref_02 { get; set; }
        public string ref_03 { get; set; }
        public string id_token { get; set; }
        public string remesa_tipo { get; set; }
        public string remesa_id { get; set; }
        public string asiento_numero { get; set; }
        public string asiento_numero_anu { get; set; }
        public string concilia_id { get; set; }
        public string concilia_tipo { get; set; }
        public DateTime? concilia_fecha { get; set; }
        public string concilia_usuario { get; set; }
        public string cod_plan { get; set; }
        public string modo_protegido { get; set; }
        public string bancoDesc { get; set; }
        public string bancoCta { get; set; }
        public string tipoDesc { get; set; }
    }

    public class Tes_ReclasificaBancoModel
    {
        public int nsolicitud { get; set; }
        public string bancoDestino { get; set; }
        public string tipo { get; set; }
        public string usuario { get; set; }
        public string nota { get; set; }
    }

    public class Tes_ReclasificaDocumentoModel
    {
        public int nsolicitud { get; set; }
        public string ndocumento { get; set; }
        public int id_banco { get; set; }
        public string tipo { get; set; }
        public string usuario { get; set; }
        public string nota { get; set; }
    }

    public class Tes_ReclasificaSolicitudModel
    {
        public int nsolicitud { get; set; }
        public string? ndocumento { get; set; }
        public int id_banco { get; set; }
        public string tipo { get; set; }
        public string usuario { get; set; }
        public string nota { get; set; }
        public int tipoId { get; set; }
        public bool permiteReqId { get; set; }
    }

    public class Tes_SolicitudesData
    {
        public int nsolicitud { get; set; }
        public string tipo { get; set; }
        public string codigo { get; set; }
        public string beneficiario { get; set; }
        public float monto { get; set; }
        public string estado { get; set; }
        public string cod_unidad { get; set; }
    }

}
