namespace Galileo.Models.INV
{
    public class TranRequisicionData
    {
        public int cod_requisicion { get; set; } = 0;
        public string cod_entsal { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string genera_user { get; set; } = string.Empty;
        public DateTime? genera_fecha { get; set; }
        public string autoriza_user { get; set; } = string.Empty;
        public DateTime? autoriza_fecha { get; set; }
        public string documento { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public bool plantilla { get; set; } = false;
        public string causa { get; set; } = string.Empty;
        public decimal total { get; set; } = 0m;
        public string cod_unidad { get; set; } = string.Empty;
        public string procesa_user { get; set; } = string.Empty;
        public DateTime? procesa_fecha { get; set; }
        public string recibe_user { get; set; } = string.Empty;
        public string responsable_activo { get; set; } = string.Empty;
    }

    public class InvReqProduc
    {
        public int linea { get; set; } = 0;
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal cantidad { get; set; } = 0m;
        public decimal costo { get; set; } = 0m;
        public decimal costo_registrado { get; set; } = 0m;
        public decimal total { get; set; } = 0m;
        public decimal despacho { get; set; } = 0m;
        public string cod_bodega { get; set; } = string.Empty;
        public string bodega { get; set; } = string.Empty;
        public string numero_placa { get; set; } = string.Empty;
        public int? id_control { get; set; }
        public decimal solicitado { get; set; } = 0m;
    }

    public class InvRequsUsuarioRecibe
    {
        public string usuario { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string identificacion { get; set; } = string.Empty;
    }

    public class InvRequesicionesActivosLista
    {
        public int total { get; set; } = 0;
        public List<InvRequesicionesActivosData> lista { get; set; } = [];
    }

    public class InvRequesicionesActivosData
    {
        public int id_control { get; set; } = 0;
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_bodega { get; set; } = string.Empty;
        public decimal cantidad { get; set; } = 0m;
        public decimal costo { get; set; } = 0m;
        public decimal costo_unitario { get; set; } = 0m;
        public decimal total { get; set; } = 0m;
        public string cabys { get; set; } = string.Empty;
        public string cod_barras { get; set; } = string.Empty;
        public string numero_placa { get; set; } = string.Empty;
    }

    public class InvReqFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
        public string? cod_unidad { get; set; }
        public string? documento { get; set; }
        public string? usuario { get; set; }
    }
}