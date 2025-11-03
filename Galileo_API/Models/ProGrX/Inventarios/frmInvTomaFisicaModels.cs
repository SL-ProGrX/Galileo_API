namespace PgxAPI.Models.INV
{
    public class Toma_FisicaDTO
    {
        public int consecutivo { get; set; }
        public string Cod_Bodega { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public DateTime? Fecha_Crea { get; set; }
        public string User_Crea { get; set; } = string.Empty;
        public DateTime? Fecha_Inicio { get; set; }
        public DateTime? Fecha_Corte { get; set; }
        public DateTime? Fecha_Aplica { get; set; }
        public string User_Aplica { get; set; } = string.Empty;
        public string Tipo_Asiento { get; set; } = string.Empty;
        public string Num_Asiento { get; set; } = string.Empty;
        public DateTime? Fecha_Asiento { get; set; }
        public string Causa_Entrada { get; set; } = string.Empty;
        public string Causa_Salida { get; set; } = string.Empty;
        public int Cod_Proveedor_Entrada { get; set; }
        public int Cod_Entradag { get; set; }
        public int Cod_Salidag { get; set; }
    }

    public class Toma_FisicaDetalleDTO
    {
        public int consecutivo { get; set; }
        public string Cod_Bodega { get; set; } = string.Empty;

        public string bodega { get; set; } = string.Empty;
        public string Cod_Producto { get; set; } = string.Empty;
        public int Existencia_Logica { get; set; }
        public int Existencia_Fisica { get; set; }
        public string Ubicacion { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public string tipo { get; set; } = string.Empty;
    }

}
