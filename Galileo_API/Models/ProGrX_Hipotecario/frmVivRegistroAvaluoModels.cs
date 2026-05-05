namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class FrmVivGarantiaAvaluoRegistroRequest
    {
        public long numero_operacion { get; set; } = 0;
        public long id_garantia { get; set; } = 0;
    }

    public class FrmVivGarantiaAvaluoRegistroResponse
    {
        public string CEDULA { get; set; } = string.Empty;
        public string NOMBRE { get; set; } = string.Empty;
        public long ID_SOLICITUD { get; set; } = 0;
        public string expediente { get; set; } = string.Empty;
        public string NumeroFinca { get; set; } = string.Empty;
        public string NumPlanoCatastro { get; set; } = string.Empty;
        public decimal AreaFinca { get; set; } = 0;
        public string DescZona { get; set; } = string.Empty;
        public string NombreProfesional { get; set; } = string.Empty;
        public long IdGarantia { get; set; } = 0;
        public long IdContacto { get; set; } = 0;
        public string Canton { get; set; } = string.Empty;
        public string provincia { get; set; } = string.Empty;
        public string Distrito { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public DateTime? FechaInspeccion { get; set; }
        public decimal ValorTerreno { get; set; } = 0;
        public decimal ValorConstruccion { get; set; } = 0;
        public string ObservacionAvaluo { get; set; } = string.Empty;
        public decimal Viaticos { get; set; } = 0;
        public bool RegistraCalHonorariosDT { get; set; } = false;
        public string Tipo_Poliza { get; set; } = string.Empty;
    }

    public class FrmVivGarantiaAvaluoGuardarRequest
    {
        public long numero_operacion { get; set; } = 0;
        public long id_garantia { get; set; } = 0;
        public long id_contacto { get; set; } = 0;
        public DateTime? fecha_inspeccion { get; set; }
        public decimal valor_terreno { get; set; } = 0;
        public decimal valor_construccion { get; set; } = 0;
        public string observacion_avaluo { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public decimal viaticos { get; set; } = 0;
        public string tipo_poliza { get; set; } = string.Empty;
    }

    public class FrmVivGarantiaAvaluoMontoCambiarRequest
    {
        public long id_garantia { get; set; } = 0;
        public string tipo { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class FrmVivGarantiaAvaluoMontoCambiarResponse
    {
        public int pass { get; set; } = 0;
        public string mensaje { get; set; } = string.Empty;
        public string movimiento { get; set; } = string.Empty;
    }

}
