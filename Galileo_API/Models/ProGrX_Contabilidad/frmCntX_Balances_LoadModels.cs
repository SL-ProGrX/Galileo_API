using Galileo.Models;

namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXBalancesLoadPantallaDto
    {
        public int contabilidad { get; set; } = 0;
        public int anio { get; set; } = 0;
        public int mes { get; set; } = 0;
        public string periodo_desc { get; set; } = string.Empty;
        public int consolida_ind { get; set; } = 0;
        public int consolida_conta { get; set; } = 0;
        public string consolida_unidad { get; set; } = string.Empty;
        public List<DropDownListaGenericaModel?> unidades { get; set; } = new();
    }

    public class CntXBalancesLoadHistoricoListarRequestDto
    {
        public int contabilidad { get; set; } = 0;
        public string unidad { get; set; } = string.Empty;
        public int anio { get; set; } = 0;
        public int mes { get; set; } = 0;
    }

    public class CntXBalancesLoadArchivoFilaDto
    {
        public int linea { get; set; } = 0;
        public string cuenta { get; set; } = string.Empty;
        public string consolidadora { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal saldo_inicial { get; set; } = 0;
        public decimal debitos { get; set; } = 0;
        public decimal creditos { get; set; } = 0;
        public decimal saldo_final { get; set; } = 0;
        public decimal tc { get; set; } = 0;
        public string cta_excluye { get; set; } = string.Empty;
    }

    public class CntXBalancesLoadArchivoCargarRequestDto
    {
        public int contabilidad { get; set; } = 0;
        public string unidad { get; set; } = string.Empty;
        public int anio { get; set; } = 0;
        public int mes { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public List<CntXBalancesLoadArchivoFilaDto> lineas { get; set; } = new();
    }

    public class CntXBalancesLoadProcesoRequestDto
    {
        public int contabilidad { get; set; } = 0;
        public string unidad { get; set; } = string.Empty;
        public int anio { get; set; } = 0;
        public int mes { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
    }

    public class CntXBalancesLoadImportaContaBaseRequestDto
    {
        public int contabilidad { get; set; } = 0;
        public int anio { get; set; } = 0;
        public int mes { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
    }

    public class CntXBalancesLoadResultadoDto
    {
        public string cuenta { get; set; } = string.Empty;
        public string consolidadora { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal saldo_inicial { get; set; } = 0;
        public decimal debitos { get; set; } = 0;
        public decimal creditos { get; set; } = 0;
        public decimal saldo_final { get; set; } = 0;
        public string validacion { get; set; } = string.Empty;
        public string divisa { get; set; } = string.Empty;
        public decimal tc { get; set; } = 0;
        public string cta_excluye { get; set; } = string.Empty;
    }

    public class CntXBalancesLoadHistoricoDetalleDto
    {
        public int historico_id { get; set; } = 0;
        public string unidad { get; set; } = string.Empty;
        public string periodo { get; set; } = string.Empty;
        public string archivo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha_registro { get; set; }
        public int total_registros { get; set; } = 0;
        public string estado { get; set; } = string.Empty;
    }

    public class CntXBalancesLoadProcesoResultDto
    {
        public int pass { get; set; } = 0;
        public string mensaje { get; set; } = string.Empty;
    }

    public class CntXBalancesLoadValidaDto
    {
        public int casos_erroneos { get; set; } = 0;
    }

    public class CntXBalancesLoadContabilidadInfoDto
    {
        public int consolida_ind { get; set; } = 0;
        public int consolida_conta { get; set; } = 0;
        public string consolida_unidad { get; set; } = string.Empty;
    }

    public class CntXBalancesLoadPeriodoDto
    {
        public string periodo_desc { get; set; } = string.Empty;
    }
}