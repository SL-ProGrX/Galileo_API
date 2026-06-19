using Galileo.Models;

namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrCarteraSensibilizacionPantallaData
    {
        public List<DropDownListaGenericaModel> destinos { get; set; } = new();
        public List<DropDownListaGenericaModel> recursos { get; set; } = new();
        public List<DropDownListaGenericaModel> instituciones { get; set; } = new();
        public DateTime? fecha_inicio_default { get; set; }
        public DateTime? fecha_corte_default { get; set; }
        public bool todas_lineas_default { get; set; } = false;
        public bool todas_fechas_default { get; set; } = false;
        public bool filtros_add_default { get; set; } = false;
        public bool tbp_pts_add_default { get; set; } = false;
        public decimal pts_add_default { get; set; } = 0;
    }

    public class CrCarteraSensibilizacionRequest
    {
        public string codigo { get; set; } = string.Empty;
        public string destino { get; set; } = string.Empty;
        public string recurso { get; set; } = string.Empty;
        public int cod_institucion { get; set; } = 0;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public bool todas_lineas { get; set; } = false;
        public bool todas_fechas { get; set; } = false;
        public bool aplicar_tbp_pts_add { get; set; } = false;
        public decimal? tasa { get; set; }
        public decimal? pts_add { get; set; }
        public bool usar_plazos { get; set; } = false;
        public int? plazo_inicio { get; set; }
        public int? plazo_corte { get; set; }
        public bool usar_tasas { get; set; } = false;
        public decimal? tasa_inicio { get; set; }
        public decimal? tasa_corte { get; set; }
    }

    public class CrCarteraSensibilizacionGridItem
    {
        public long operacion { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal montoapr { get; set; } = 0;
        public decimal saldo { get; set; } = 0;
        public int plazo { get; set; } = 0;
        public decimal interesv { get; set; } = 0;
        public decimal cuota { get; set; } = 0;
        public DateTime? fechaforp { get; set; }
        public decimal tasa_original { get; set; } = 0;
        public decimal tbp_puntos_add { get; set; } = 0;
        public decimal tasa_piso { get; set; } = 0;
        public int plazo_faltante { get; set; } = 0;
        public decimal cuota_01 { get; set; } = 0;
        public decimal cuota_02 { get; set; } = 0;
        public decimal cuota_03 { get; set; } = 0;
        public decimal cuota_04 { get; set; } = 0;
        public decimal tasa_01 { get; set; } = 0;
        public decimal tasa_02 { get; set; } = 0;
        public decimal tasa_03 { get; set; } = 0;
        public decimal tasa_04 { get; set; } = 0;
    }

    public class CrCarteraSensibilizacionResultadoData
    {
        public List<CrCarteraSensibilizacionGridItem> detalle { get; set; } = new();
        public int casos { get; set; } = 0;
        public decimal cuotas_actuales { get; set; } = 0;
        public decimal cuotas_nuevas { get; set; } = 0;
    }

    public class CrCarteraSensibilizacionGenerarData
    {
        public int registros_generados { get; set; } = 0;
    }

    internal sealed class CrCarteraSensibilizacionOperacionBase
    {
        public long id_solicitud { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal montoapr { get; set; } = 0;
        public decimal saldo { get; set; } = 0;
        public decimal cuota { get; set; } = 0;
        public int plazo { get; set; } = 0;
        public decimal interesv { get; set; } = 0;
        public decimal prideduc { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public DateTime? fechaforp { get; set; }
        public decimal tasaoriginal { get; set; } = 0;
        public decimal liq_valor { get; set; } = 0;
        public int plazofaltante { get; set; } = 0;
        public decimal liqtasa { get; set; } = 0;
        public decimal tbp_puntosadd { get; set; } = 0;
        public decimal tasa_piso { get; set; } = 0;
    }

    public class CrCarteraSensibilizacionLiquidezItem
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal cuota_01 { get; set; } = 0;
        public decimal cuota_02 { get; set; } = 0;
        public decimal cuota_03 { get; set; } = 0;
        public decimal cuota_04 { get; set; } = 0;
        public decimal devengado_mes { get; set; } = 0;
        public decimal liquidez_simple { get; set; } = 0;
        public decimal liquidez_confianza { get; set; } = 0;
        public decimal total_carga_ccss { get; set; } = 0;
        public decimal deducciones { get; set; } = 0;
        public decimal saldo_fijo { get; set; } = 0;
        public decimal cuota_fija { get; set; } = 0;
    }
}