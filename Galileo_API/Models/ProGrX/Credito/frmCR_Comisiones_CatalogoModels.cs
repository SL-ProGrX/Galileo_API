namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrComisionesCatalogoData
    {
        public string cod_comision { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public string base_calculo { get; set; } = string.Empty;
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string cuenta_desc { get; set; } = string.Empty;
        public bool activa { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
    }

    public class CrComisionesCatalogoGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public CrComisionesCatalogoData comision { get; set; } = new();
    }

    public class CrComisionesCatalogoEliminarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_comision { get; set; } = string.Empty;
    }

    public class CrComisionesCatalogoPorcentajeData
    {
        public long linea_id { get; set; }
        public decimal inicio { get; set; }
        public decimal corte { get; set; }
        public decimal venta { get; set; }
        public decimal formalizacion { get; set; }
    }

    public class CrComisionesCatalogoPorcentajesRequest
    {
        public string cod_comision { get; set; } = string.Empty;
    }

    public class CrComisionesCatalogoPorcentajeGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_comision { get; set; } = string.Empty;
        public CrComisionesCatalogoPorcentajeData porcentaje { get; set; } = new();
    }

    public class CrComisionesCatalogoPorcentajeEliminarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_comision { get; set; } = string.Empty;
        public long linea_id { get; set; }
    }

    public class CrComisionesCatalogoLineaData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public class CrComisionesCatalogoLineasRequest
    {
        public string cod_comision { get; set; } = string.Empty;
    }

    public class CrComisionesCatalogoLineaAsignarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_comision { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public class CrComisionesCatalogoCuentaLookupData
    {
        public string cod_cuenta { get; set; } = string.Empty;
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
}
