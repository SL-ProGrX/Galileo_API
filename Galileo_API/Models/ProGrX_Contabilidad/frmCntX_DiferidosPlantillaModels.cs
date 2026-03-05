namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXDiferidosData
    {
        public int cod_diferido { get; set; } = 0;
        public string tipo_asiento { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
        public DateTime fecha_crea { get; set; }
        public string user_crea { get; set; } = string.Empty;
        public int consecutivo { get; set; } = 0;
    }

    public class CntXDiferidosDetalleData
    {
        public string cod_cuenta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal porc_debito { get; set; } = 0;
        public decimal porc_credito { get; set; } = 0;
        public int linea { get; set; } = 0;
        public string cod_unidad { get; set; } = string.Empty;
        public string unides { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string ccdes { get; set; } = string.Empty;
    }

    public class CntXDiferidosPlantillaRequest
    {
        public int cod_contabilidad { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public bool asiento_plantilla { get; set; } = false;
        public bool edita { get; set; } = false;
        public CntXDiferidosData data { get; set; } = new CntXDiferidosData();
        public List<CntXDiferidosDetalleData> detalles { get; set; } = new List<CntXDiferidosDetalleData>();
    }
}
