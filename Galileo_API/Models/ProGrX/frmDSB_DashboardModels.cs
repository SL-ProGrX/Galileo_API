namespace Galileo.Models.ProGrX
{
    public class DashboardCategoriaData
    {
        public string? Cod_Categoria { get; set; }
        public string? Descripcion { get; set; }
    }

    public class DashboardClientesData
    {
        public DashboardClientesResumenData? Resumen { get; set; }
        public List<DashboardClientesPuntoData> Edades { get; set; } = [];
        public List<DashboardClientesPuntoData> Generaciones { get; set; } = [];
        public List<DashboardClientesPuntoData> Causas { get; set; } = [];
        public List<DashboardTopOpcionData> OpcionesTop { get; set; } = [];
    }

    public class DashboardClientesResumenData
    {
        public DateTime Corte { get; set; }
        public int Total { get; set; }
        public int Nuevos { get; set; }
        public int Reingresos { get; set; }
        public int Salidas { get; set; }
        public int ExAsociados { get; set; }
        public double? INuevos { get; set; }
        public double? IReingresos { get; set; }
        public double? IExAsociados { get; set; }
    }

    public class DashboardCreditosData
    {
        public DashboardCreditosResumenData? Resumen { get; set; }
        public List<DashboardClientesPuntoData> Garantias { get; set; } = [];
        public List<DashboardClientesPuntoData> Morosidad { get; set; } = [];
        public List<DashboardClientesPuntoData> TppGarantia { get; set; } = [];
        public List<DashboardTopOpcionData> OpcionesTop { get; set; } = [];
    }

    public class DashboardCreditosResumenData
    {
        public DateTime Corte { get; set; }
        public double? Tpp { get; set; }
        public double? Pipp { get; set; }
        public double? IMora_Activa { get; set; }
        public double? ICbrJud { get; set; }
        public double? IRefinancia { get; set; }
        public double? ICancela { get; set; }
        public double? CCPr { get; set; }
        public double? CLPr { get; set; }
        public double? TSaldo { get; set; }
        public double? Colocacion { get; set; }
        public double? NOperaciones { get; set; }
        public double? Refinancia { get; set; }
        public double? TCbrJud { get; set; }
    }

    public class DashboardAhorrosData
    {
        public DashboardAhorrosResumenData? Resumen { get; set; }
        public List<DashboardClientesPuntoData> Planes { get; set; } = [];
        public List<DashboardClientesPuntoData> Grupos { get; set; } = [];
        public List<DashboardClientesPuntoData> Patrimonio { get; set; } = [];
        public List<DashboardTopOpcionData> OpcionesTop { get; set; } = [];
    }

    public class DashboardAhorrosResumenData
    {
        public DateTime Corte { get; set; }
        public double? Casos { get; set; }
        public double? Total { get; set; }
        public double? Aportes { get; set; }
        public double? Rendimiento { get; set; }
        public double? Aportaciones { get; set; }
        public double? Retiros { get; set; }
    }

    public class DashboardModuloData
    {
        public DashboardModuloResumenData? Resumen { get; set; }
        public List<DashboardModuloPuntoData> Puntos { get; set; } = [];
        public List<DashboardTopOpcionData> OpcionesTop { get; set; } = [];
    }

    public class DashboardModuloResumenData
    {
        public DateTime Corte { get; set; }
        public Dictionary<string, double?> Valores { get; set; } = [];
    }

    public class DashboardModuloPuntoData
    {
        public string? CodigoGrafico { get; set; }
        public DateTime? Corte { get; set; }
        public string? Descripcion { get; set; }
        public string? Serie { get; set; }
        public double? Value { get; set; }
    }

    public class DashboardClientesPuntoData
    {
        public DateTime? Corte { get; set; }
        public string? Descripcion { get; set; }
        public string? Serie { get; set; }
        public double? Value { get; set; }
    }

    public class DashboardTopOpcionData
    {
        public string? Cod_Kpi { get; set; }
        public string? Descripcion { get; set; }
    }

    public class DashboardTopFilaData
    {
        public string? Descripcion { get; set; }
        public double? Dato { get; set; }
    }
}
