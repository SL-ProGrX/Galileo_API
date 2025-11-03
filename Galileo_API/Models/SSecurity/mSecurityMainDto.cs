namespace PgxAPI.Models
{
    public class ParametrosAccesoDTO
    {
        public long EmpresaId { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public int Modulo { get; set; }
        public string FormName { get; set; } = string.Empty;
        public string Boton { get; set; } = string.Empty;
    }

    public class BitacoraInsertarDTO
    {
        public long EmpresaId { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public int Modulo { get; set; }
        public string Movimiento { get; set; } = string.Empty;
        public string DetalleMovimiento { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public string AppNombre { get; set; } = string.Empty;


        public string AppEquipo { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        public string AppIP { get; set; } = string.Empty;
    }

  

    public class FiltroLazy
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
    }

    public class SEGLogInsertarDTO
    {
        public string AppName { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string PTransac { get; set; } = string.Empty;
        public string PNotas { get; set; } = string.Empty;
        public string PUserMov { get; set; } = string.Empty;
        public string AppMaquina { get; set; } = string.Empty;
    }

    public class DerechoMDIObtenerDTO
    {
        public int Cliente { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public int Modulo { get; set; }
        public string FormX { get; set; } = string.Empty;
        public string Opcion { get; set; } = string.Empty;
    }


}
