using Galileo.Models.TES;

namespace Galileo.Models.ProGrX.Bancos
{
   
    // =========================================================
    // DTO específico de reclasificación
    // (solo deja aquí lo que realmente es “extra”)
    // =========================================================
    public class TesReclasificacionDto : TesTransaccionDto
    {
        public string? bancoDesc { get; set; }
        public string? bancoCta { get; set; }
        public string? tipoDesc { get; set; }
    }

    // =========================
    // Reclasificación (modelos)
    // =========================

    public class TesReclasificaBaseModel
    {
        public int nsolicitud { get; set; } = 0;
        public string? tipo { get; set; }
        public string? usuario { get; set; }
        public string? nota { get; set; }
    }

    public class TesReclasificaBancoModel : TesReclasificaBaseModel
    {
        public string? bancoDestino { get; set; }
    }

    public class TesReclasificaDocBaseModel : TesReclasificaBaseModel
    {
        public string? ndocumento { get; set; }
        public int id_banco { get; set; } = 0;
    }

    public class TesReclasificaDocumentoModel : TesReclasificaDocBaseModel
    {
        public bool DocBase { get; set; } = false;
    }

    public class TesReclasificaSolicitudModel : TesReclasificaDocBaseModel
    {
        public int tipoId { get; set; } = 0;
        public bool permiteReqId { get; set; } = false;
    }

    // =========================
    // Solicitudes
    // =========================

    public class TesSolicitudesData
    {
        public int nsolicitud { get; set; } = 0;
        public string? tipo { get; set; }
        public string? codigo { get; set; }
        public string? beneficiario { get; set; }
        public float monto { get; set; } = 0;
        public string? estado { get; set; }
        public string? cod_unidad { get; set; }
    }
}
