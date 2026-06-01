namespace Galileo.Models.AH
{
    public class FrmAhExcedentesTiposSalidasDto
    {
        public string cod_salida { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activa { get; set; } = false;
        public bool opcion_sistema { get; set; } = false;
        public int destino_operadora { get; set; } = 0;
        public string destino_plan { get; set; } = string.Empty;
        public int destino_banco { get; set; } = 0;
        public string tipo_aplicacion { get; set; } = string.Empty;
        public bool permite_reclasificar { get; set; } = false;
        public bool requiere_porcentaje { get; set; } = false;
        public string tipo_aplicacion_desc { get; set; } = string.Empty;
        public string plan_desc { get; set; } = string.Empty;
        public string banco_desc { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesTiposSalidasGuardarRequest: FrmAhExcedentesTiposSalidasDto
    {
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesTiposSalidasGuardarResponse
    {
        public string cod_salida { get; set; } = string.Empty;
        public string accion { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesTiposSalidasPlanDto
    {
        public string cod_plan { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int cod_operadora { get; set; } = 0;
    }

    public class FrmAhExcedentesTiposSalidasBancoDto
    {
        public int id_banco { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public string cta { get; set; } = string.Empty;
    }
}