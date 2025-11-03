using System.Drawing;

namespace PgxAPI.Models.PRES
{
    public class ModeloGenericList
    {
        public string IdX { get; set; } = string.Empty;
        public string ItmX { get; set; } = string.Empty;
        public string Inicio_Anio { get; set; } = string.Empty;
    }

    public class CntxCierres
    {
        public int Inicio_Anio { get; set; }
        public int Inicio_Mes { get; set; }
        public int Corte_Anio { get; set; }
        public int Corte_Mes { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class CntxCuentasData
    {
        public string Cod_Cuenta_Mask { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CuentasLista
    {
        public int total { get; set; }
        public List<CntxCuentasData> lista { get; set; } = new List<CntxCuentasData>();
    }

    public class PresCuenta
    {
        public string Cod_Modelo { get; set; } = string.Empty;
        public int? Cod_Contabilidad { get; set; } = null;
        public string? Cod_Unidad { get; set; } = string.Empty;
        public string? Cod_Centro_Costo { get; set; } = string.Empty;
        public string? Cod_Cuenta { get; set; } = string.Empty;
        public string? Vista { get; set; } = string.Empty;
        public DateTime? Periodo { get; set; }
    }

    public class VistaPresCuentaData
    {
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
        public string Cod_Unidad { get; set; } = string.Empty;
        public string Cod_Centro_Costo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public float Real_Mes { get; set; }
        public float Mensual { get; set; }
        public float Diferencia_Mes { get; set; }
        public float Real_Acumulado { get; set; }
        public float Acumulado { get; set; }
        public float Diferencia_Acumulada { get; set; }
        public float Pres_Total { get; set; }
        public float Diferencia_Total { get; set; }
        public float Ejecutado_Mes { get; set; }
        public float Ejecutado_Acumulado { get; set; }
        public float Ejecutado_Total { get; set; }
        public int Acepta_Movimientos { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public DateTime Periodo { get; set; }
        public float Pre_Mensual_Inicial { get; set; }
        public float Ajuste_Positivo { get; set; }
        public float Ajuste_Negativo { get; set; }
    }

}
