namespace PgxAPI.Models.INV
{
    public class TranRequisicionData
    {
        public int Cod_Requisicion { get; set; }
        public string Cod_Entsal { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Genera_User { get; set; } = string.Empty;
        public DateTime Genera_Fecha { get; set; }
        public string Autoriza_User { get; set; } = string.Empty;
        public DateTime Autoriza_Fecha { get; set; }
        public string Documento { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public bool Plantilla { get; set; } = false;
        public string Causa { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;

        public string procesa_user { get; set; } = string.Empty;
        public string procesa_fecha { get; set; } = string.Empty;
        public string recibe_user { get; set; } = string.Empty;
        public string responsable_activo { get; set; } = string.Empty;
    }

    public class InvReqProduc
    {
        public int Linea { get; set; }
        public string Cod_Producto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public float Cantidad { get; set; }
        public float Costo { get; set; }
        public float Total { get; set; }
        public float Despacho { get; set; }
        public string Cod_Bodega { get; set; } = string.Empty;
        public string Bodega { get; set; } = string.Empty;
        public string numero_placa { get; set; } = string.Empty;
        public int? id_control { get; set; }

        public float solicitado { get; set; }
    }

    public class InvRequsUsuarioRecibe
    {
        public string? usuario { get; set; }
        public string? nombre { get; set; }
        public string? identificacion { get; set; }
    }

    public class  InvRequesicionesActivosLista
    {
        public int total { get; set; }
        public List<InvRequesicionesActivosData> lista { get; set; } = new List<InvRequesicionesActivosData>();

    }

    public class InvRequesicionesActivosData
    {
        public int id_control { get; set; }
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_bodega { get; set; } = string.Empty;
        public int cantidad { get; set; }
        public float costo { get; set; }
        public float total { get; set; }
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