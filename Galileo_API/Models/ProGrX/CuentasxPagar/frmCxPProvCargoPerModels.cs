namespace PgxAPI.Models.CxP
{
    public class CargoPerDTOList
    {
        public int Total { get; set; }
        public List<CargoPerDTO> Cargoper { get; set; } = new List<CargoPerDTO>();
    }
    public class CargoPerDTO
    {
        public int Id { get; set; }
        public int Cod_Proveedor { get; set; }
        public string Cod_Cargo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public decimal Saldo { get; set; }
        public decimal Recaudado { get; set; }
        public DateTime Vence { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public decimal Importe_Divisa_Real { get; set; }
        public DateTime Asiento_Fecha { get; set; }
        public string Asiento_Usuario { get; set; } = string.Empty;
        public DateTime Fecha_Cobro_Cargo { get; set; }
        public decimal Tipo_Cambio { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal Anula_Monto { get; set; }
        public string Anula_Usuario { get; set; } = string.Empty;
        public DateTime Anula_Fecha { get; set; }
        public DateTime Anula_Asiento_Fecha { get; set; }
        public string Anula_Asiento_Usuario { get; set; } = string.Empty;
        public DateTime FechaInicioCobro { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public string Cargo_Desc { get; set; } = string.Empty;
        public string Usuario_Sesion { get; set; } = string.Empty;

    }
    public class PagoProvCargosDTOList
    {
        public int Total { get; set; }
        public List<PagoProvCargosDTO> Pagos { get; set; } = new List<PagoProvCargosDTO>();
    }
    public class PagoProvCargosDTO
    {
        public int Idx_Consec { get; set; }
        public int NPago { get; set; }
        public string Cod_Cargo { get; set; } = string.Empty;
        public int Cod_Proveedor { get; set; }
        public string Cod_Factura { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public int Id { get; set; }
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public decimal Tipo_Cambio { get; set; }
        public string Tipo_Cargo { get; set; } = string.Empty;
        public string Tipo_Proceso { get; set; } = string.Empty;
        public DateTime Asiento_Fecha { get; set; }
        public string Asiento_Usuario { get; set; } = string.Empty;
        public DateTime Fecha_Traslada { get; set; }
        public int Tesoreria { get; set; }

    }

    public class Secuencia
    {
        public int Id { get; set; }
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
    }

    public class Cargo
    {
        public string Cod_Cargo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class ProveedorInfo
    {
        public int Cod_Proveedor { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public decimal Tipo_Cambio { get; set; }
        public decimal Saldo { get; set; }
    }

}
