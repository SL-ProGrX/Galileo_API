namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    /// <summary>
    /// Causa registrada para el expediente en el tab Causas de frmPreaEstudiov2.
    /// </summary>
    public class FrmPreaEstudiov2CausaDto
    {
        /// <summary>Código de causa registrado en CRD_PREA_GESTION.COD_CAUSAS.</summary>
        public string cod_causas { get; set; } = string.Empty;

        /// <summary>Descripción de la causa desde OPERACION_CAUSAS.DESCRIPCION.</summary>
        public string descripcion { get; set; } = string.Empty;

        /// <summary>Fecha de registro de la causa.</summary>
        public DateTime? registro_fecha { get; set; }

        /// <summary>Usuario que registró la causa.</summary>
        public string registro_usuario { get; set; } = string.Empty;

        /// <summary>Tipo de causa registrado en CRD_PREA_GESTION.TIPO.</summary>
        public string tipo { get; set; } = string.Empty;
    }
}
