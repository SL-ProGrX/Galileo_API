namespace Galileo.Models.INV
{
    public class PermisosBodegasDto
    {
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string estado { get; set; } = string.Empty;
        public DateTime? ult_modificacion { get; set; }
        public bool modifica { get; set; } = false;
        public bool autoriza { get; set; } = false;
        public bool procesa { get; set; } = false;
    }

    public class InvBodegasPermisoActualizarRequest
    {
        public string cod_bodega { get; set; } = string.Empty;
        public string tipo_transaccion { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string permiso { get; set; } = string.Empty;
        public bool valor { get; set; } = false;
    }

    public class BodegasDto
    {
        public string cod_bodega { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public string cod_cta_gastostf { get; set; } = string.Empty;
        public string cod_cta_ingresostf { get; set; } = string.Empty;
        public int permite_entradas { get; set; } = 0;
        public int permite_salidas { get; set; } = 0;
        public int utiliza_permisos { get; set; } = 0;
        public string estado { get; set; } = string.Empty;
    }
}