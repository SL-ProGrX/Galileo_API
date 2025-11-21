namespace Galileo.Models.Security
{
    public class UsuarioEmpresa
    {
        public string Usuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class UsuarioPermisosCopiar
    {
        public int Cliente { get; set; }
        public string UsBase { get; set; } = string.Empty;
        public string UsDestino { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public bool RS_Roles { get; set; }
        public bool RS_Estaciones { get; set; }
        public bool RS_Horarios { get; set; }
        public bool RS_Inicializa { get; set; }
        public bool RO_Deducciones { get; set; }
        public bool RO_Contabilidad { get; set; }
        public bool RO_Creditos { get; set; }
        public bool RO_Resolucion_Crd { get; set; }
        public bool RO_Cobros { get; set; }
        public bool RO_Cajas { get; set; }
        public bool RO_Bancos { get; set; }
        public bool RO_Presupuesto { get; set; }
        public bool RO_Inventarios { get; set; }
        public bool RO_Compras { get; set; }
        public bool RO_Inicializa { get; set; }
    }
}
