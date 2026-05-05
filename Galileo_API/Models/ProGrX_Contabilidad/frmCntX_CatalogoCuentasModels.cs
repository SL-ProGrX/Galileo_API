using Galileo.Models;

namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXCatalogoCuentasFiltroRequest
    {
        public int CodContabilidad { get; set; }
        public int PeriodoAnio { get; set; }
        public int PeriodoMes { get; set; }
        public string Cuenta { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string CodDivisa { get; set; } = "TODOS";
        public int Nivel { get; set; } = 8;
        public bool MostrarBalance { get; set; }
    }

    public class CntXCatalogoCuentaDto
    {
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Cod_Cuenta_Mask { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public string Tipo_Cuenta { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public bool Acepta_Movimientos { get; set; }
        public bool Presupuesto { get; set; }
        public bool Bloqueada { get; set; }
        public bool Cuenta_Auxiliar { get; set; }
        public decimal Saldo_Inicial { get; set; }
        public decimal Total_Debitos { get; set; }
        public decimal Total_Creditos { get; set; }
        public bool IsNew { get; set; }
    }

    public class CntXCatalogoCuentaEstadoRequest
    {
        public int CodContabilidad { get; set; }
        public string Cuenta { get; set; } = string.Empty;
        public string Campo { get; set; } = string.Empty;
        public bool Valor { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class CntXCatalogoCuentaGuardarRequest
    {
        public int CodContabilidad { get; set; }
        public string Cuenta { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string CodDivisa { get; set; } = string.Empty;
        public string TipoCuenta { get; set; } = string.Empty;
        public bool AceptaMovimientos { get; set; }
        public bool Presupuesto { get; set; }
        public bool Bloqueada { get; set; }
        public bool CuentaAuxiliar { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public bool IsNew { get; set; }
    }

    public class CntXCatalogoCuentaGuardarResponse
    {
        public string Cod_Cuenta_Mask { get; set; } = string.Empty;
        public string Movimiento { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
    }

    public class CntXCatalogoCuentaDetalleDto
    {
        public string Cod_Cuenta_Mask { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Descripcion_Alterna { get; set; } = string.Empty;
        public bool Exclusiva_Indica { get; set; }
        public string Exclusiva_Unidad { get; set; } = string.Empty;
        public string Exclusiva_Unidad_Desc { get; set; } = string.Empty;
        public string Exclusiva_Centro_Costo { get; set; } = string.Empty;
        public string Exclusiva_Centro_Costo_Desc { get; set; } = string.Empty;
        public bool Prorratea_Indica { get; set; }
        public decimal Prorratea_Total { get; set; }
        public string Prorratea_Unidad { get; set; } = string.Empty;
        public string Prorratea_Unidad_Desc { get; set; } = string.Empty;
        public string Prorratea_Centro_Costo { get; set; } = string.Empty;
        public string Prorratea_Centro_Costo_Desc { get; set; } = string.Empty;
        public bool Dc_Especial_Indica { get; set; }
        public string Dc_Cta_Ingreso_Mask { get; set; } = string.Empty;
        public string Dc_Cta_Ingreso_Desc { get; set; } = string.Empty;
        public string Dc_Cta_Gasto_Mask { get; set; } = string.Empty;
        public string Dc_Cta_Gasto_Desc { get; set; } = string.Empty;
        public string Dc_Unidad { get; set; } = string.Empty;
        public string Dc_Unidad_Desc { get; set; } = string.Empty;
        public string Dc_Centro_Costo { get; set; } = string.Empty;
        public string Dc_Centro_Costo_Desc { get; set; } = string.Empty;
    }

    public class CntXCatalogoCuentaDetalleResponse
    {
        public CntXCatalogoCuentaDetalleDto Detalle { get; set; } = new();
        public List<CntXCuentaTraduccionDto> Traducciones { get; set; } = [];
        public List<CntXCuentaProrrataDto> Prorrateos { get; set; } = [];
    }

    public class CntXCuentaTraduccionDto
    {
        public string Cod_Idioma { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CntXCuentaProrrataDto
    {
        public string Cod_Unidad { get; set; } = string.Empty;
        public string Unidad_Desc { get; set; } = string.Empty;
        public string Cod_Centro_Costo { get; set; } = string.Empty;
        public string Centro_Desc { get; set; } = string.Empty;
        public decimal Porcentaje { get; set; }
    }

    public class CntXCatalogoCuentaDetalleGuardarRequest
    {
        public int CodContabilidad { get; set; }
        public string Cuenta { get; set; } = string.Empty;
        public string DescripcionAlterna { get; set; } = string.Empty;
        public bool ExclusivaIndica { get; set; }
        public string ExclusivaUnidad { get; set; } = string.Empty;
        public string ExclusivaCentro { get; set; } = string.Empty;
        public bool ProrrateaIndica { get; set; }
        public string ProrrateaUnidad { get; set; } = string.Empty;
        public string ProrrateaCentro { get; set; } = string.Empty;
        public decimal ProrrateaTotal { get; set; }
        public bool DcIndica { get; set; }
        public string DcUnidad { get; set; } = string.Empty;
        public string DcCentro { get; set; } = string.Empty;
        public string DcCuentaIngreso { get; set; } = string.Empty;
        public string DcCuentaGasto { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class CntXCatalogoMapeoRequest
    {
        public int CodContabilidad { get; set; }
        public string CuentaActual { get; set; } = string.Empty;
        public string CuentaNueva { get; set; } = string.Empty;
        public bool CambiarTransacciones { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class CntXCatalogoBajaNivelRequest
    {
        public int CodContabilidad { get; set; }
        public string Cuenta { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class CntXCatalogoBajaNivelDto
    {
        public string Cuenta { get; set; } = string.Empty;
    }

    public class CntXCatalogoTraduccionGuardarRequest
    {
        public int CodContabilidad { get; set; }
        public string Cuenta { get; set; } = string.Empty;
        public string CodIdioma { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class CntXCatalogoProrrataGuardarRequest
    {
        public int CodContabilidad { get; set; }
        public string Cuenta { get; set; } = string.Empty;
        public string CodUnidad { get; set; } = string.Empty;
        public string CodCentroCosto { get; set; } = string.Empty;
        public decimal Porcentaje { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }
}
