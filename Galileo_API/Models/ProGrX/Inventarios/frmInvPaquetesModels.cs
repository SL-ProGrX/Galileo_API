namespace Galileo.Models.INV
{
    public class PaqueteDto
    {
        public int? Cod_Paquete { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha_Crea { get; set; }
        public string User_Crea { get; set; } = string.Empty;
        public string? User_Modifica { get; set; } = string.Empty;
        public DateTime Fecha_Inicio { get; set; }
        public string? Notas { get; set; }
        public DateTime Fecha_Modifica { get; set; }
        public DateTime Fecha_Corte { get; set; }
        public DateTime Frecuencia_Horai { get; set; }
        public DateTime Frecuencia_Horac { get; set; }
        public bool? Frecuencia_Lunes { get; set; }
        public bool? Frecuencia_Martes { get; set; }
        public bool? Frecuencia_Miercoles { get; set; }
        public bool? Frecuencia_Jueves { get; set; }
        public bool? Frecuencia_Viernes { get; set; }
        public bool? Frecuencia_Sabado { get; set; }
        public bool? Frecuencia_Domingo { get; set; }

    }

    public class PaqueteDataLista
    {
        public int Total { get; set; }
        public List<PaqueteDto> Lista { get; set; } = new List<PaqueteDto>();
    }

    public class PaqueteDetalleDto
    {
        public int Linea { get; set; }
        public string Cod_Producto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Cod_Paquete { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Porc_Utilidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Imp_Ventas { get; set; }
        public decimal Imp_Consumo { get; set; }
        public decimal Total { get; set; }

        public string unidad { get; set; } = string.Empty;
    }
}