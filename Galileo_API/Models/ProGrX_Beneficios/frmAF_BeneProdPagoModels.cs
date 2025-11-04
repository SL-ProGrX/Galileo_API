namespace PgxAPI.Models.AF
{
    public class AfiBeneProdAsgDataList
    {
        public int Total { get; set; }
        public List<AfiBeneProdAsgData> Beneficios { get; set; } = new List<AfiBeneProdAsgData>();
    }
    
    public class AfiBeneProdAsgData
    {
        public bool estado { get; set; }
        public int Consec { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public float Monto { get; set; }
        public string autoriza_user { get; set; } = string.Empty;
        public string Cod_Producto { get; set; } = string.Empty;
        public float Costo_Unidad { get; set; }
        public string ProductoDesc { get; set; } = string.Empty;
        public int id_pago { get; set; }
        public string expediente { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; } 
        public long id_beneficio { get; set; }
        public long linea { get; set; }
        public bool tarjeta { get; set; } = false;
        public string? noTarjeta { get; set; }
    }

    public class AfiBeneProdData
    {
        public string cod_Beneficio { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
}