namespace Galileo.Models.PRES
{
    public class CuentasCatalogoData
    {
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Asignada { get; set; }
    }

    public class PresModeloCuentasImportData
    {
        public int Cod_Contabilidad { get; set; }
        public string? Cod_Modelo { get; set; }
        public string? Cod_Cuenta { get; set; }
        public string? Cod_Unidad { get; set; }
        public string? Cod_Centro_Costo { get; set; }
        public string Corte { get; set; } = string.Empty;
        public int Anio { get; set; }
        public int Mes { get; set; }
        public float Monto { get; set; }
        public string? Usuario { get; set; }
        public int Inicializa { get; set; }
        public string? Descripcion { get; set; }
        public string? Presupuesto { get; set; }
        public string? Detalle { get; set; }
    }

    public class PresModeloCuentasHorizontal
    {
        public string Cuenta { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;
        public string Centro { get; set; } = string.Empty;
        public float Enero { get; set; }
        public float Febrero { get; set; }
        public float Marzo { get; set; }
        public float Abril { get; set; }
        public float Mayo { get; set; }
        public float Junio { get; set; }
        public float Julio { get; set; }
        public float Agosto { get; set; }
        public float Septiembre { get; set; }
        public float Octubre { get; set; }
        public float Noviembre { get; set; }
        public float Diciembre { get; set; }
    }

    public class CntXPeriodoFiscalMeses
    {
        public int Cod_Contabilidad { get; set; }
        public int Id_Cierre { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public DateTime Corte { get; set; }
    }
}