using Microsoft.Data.SqlClient;
using System;

namespace PgxAPI.Models.ProGrX.Fondos
{
    public static class FndLiquidaPlanConst
    {
        public const string vTodos = "TODOS";
    }

    public sealed class FndConsultaPlanDbRow
    {
        public long cod_contrato { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public int? plazo { get; set; }
        public decimal monto { get; set; }
        public decimal aportes { get; set; }
        public decimal rendimiento { get; set; }
        public DateTime? fecha_corte { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public string cuentaahorrox { get; set; } = string.Empty;
        public string bancox { get; set; } = string.Empty;
        public string? bancodesc { get; set; }
        public string estadodesc { get; set; } = string.Empty;
    }

    public sealed class FndConsultaPlanRowDto 
    {
        public bool marcas { get; set; }
        public long cod_contrato { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal aportes { get; set; }
        public decimal rendimiento { get; set; }
        public string bancofinal { get; set; } = string.Empty;
        public string cuentafinal { get; set; } = string.Empty;
        public DateTime? fecha_corte { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public string estadodesc { get; set; } = string.Empty;
    }

    public class FndLiquidacionPlanFiltrosData
    {
        public string? cod_operadora { get; set; } = string.Empty;

        public string? cod_plan { get; set; } = string.Empty;

        public string? desc_plan { get; set; } = string.Empty;

        public string? proceso { get; set; } = string.Empty;

        public string? cuentaFiltro { get; set; } = string.Empty;

        public string? retencion { get; set; } = string.Empty;

        public string? tipoDocumento { get; set; } = string.Empty;

        public int? id_banco { get; set; } = 0;

        public bool chktarjetaactiva_valida { get; set; } = false;

        public string? notas { get; set; } = string.Empty;

        public string? cod_institucion { get; set; } = string.Empty;

        public string? cod_linea { get; set; } = string.Empty;

        public string? desc_linea { get; set; } = string.Empty;

        public bool chkLineas { get; set; } = false;

        public bool usararchivorefer { get; set; } = false;
        public string? archivo { get; set; } = string.Empty;

        public bool chkRndSinAporte { get; set; } = false;

        public bool chkFondosCero { get; set; } = false;

        public bool chkContratosSinMovAportes { get; set; } = false;

        public int? contratosSinMovMeses { get; set; } = 0;

        public bool chkMensualidad { get; set; } = false;

        public decimal? txtMntInicio { get; set; } = 0;

        public decimal? txtMntCorte { get; set; } = 0;

        public bool chkMontos { get; set; } = false;

        public DateTime? dtpInicio { get; set; }

        public DateTime? dtpCorte { get; set; }

        public bool chkFechas { get; set; } = false;

        public string? estado { get; set; } = string.Empty;

        public bool chkEstadoPersonaDiferente { get; set; } = false;

        public string? creditos { get; set; } = string.Empty;

        public int? casos { get; set; } = 0;

        public decimal? aportes { get; set; } = 0;

        public decimal? rendimientos { get; set; } = 0;

        public decimal? total { get; set; } = 0;

        public decimal? multa { get; set; } = 0;

        public string? tipo { get; set; } = string.Empty;

        public DateTime? dtpvence { get; set; }
    }

    public sealed class FndLiquidacionPlanLiquidarItemDto
    {
        public bool marcas { get; set; }
        public long cod_contrato { get; set; }
        public decimal aportes { get; set; }
        public decimal rendimiento { get; set; }
        public string bancofinal { get; set; } = string.Empty;
        public string cuentafinal { get; set; } = string.Empty;
    }

    public class FndLiquidacionPlanLiquidarRequest
    {
        public string cod_operadora { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public string tipoDocumento { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string oficinaTitular { get; set; } = string.Empty;
        public string oficinaUnidad { get; set; } = string.Empty;
        public string oficinaCentroCosto { get; set; } = string.Empty;
        public int enlace { get; set; } = 0;
        public decimal multa { get; set; } = 0;
        public string? notas { get; set; }
        public string? retencionCodigo { get; set; }
        public DateTime? fechaVence { get; set; }
        public List<FndLiquidacionPlanLiquidarItemDto> contratos { get; set; } = new();
        public int codContabilidad { get; set; } = 0;
    }

    public sealed class FndLiquidacionPlanLiquidarResult
    {
        public string documentoReferencia { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public int contratosProcesados { get; set; }
        public decimal totalAportes { get; set; }
        public decimal totalRendimientos { get; set; }
        public decimal totalGeneral { get; set; }
    }

    internal sealed class FndLiquidacionPlanDocumentoRefData
    {
        public DateTime fecha { get; set; }
        public int consecutivo { get; set; }
    }

    public class FndLiquidacionPlanInfoData
    {
        public string descripcion { get; set; } = string.Empty;
        public string cod_moneda { get; set; } = string.Empty;
        public string cuenta_conta { get; set; } = string.Empty;
        public string cuenta_rendimiento { get; set; } = string.Empty;
        public string cuenta_impuestos { get; set; } = string.Empty;
    }

    public class FndLiquidacionPlanOperadoraData
    {
        public string cta_retiros { get; set; } = string.Empty;
        public string cta_ingresos { get; set; } = string.Empty;
    }

    public class FndLiquidacionPlanDocumentoResumenData
    {
        public string cod_operadora { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public string cuenta_conta { get; set; } = string.Empty;
        public string cuenta_rendimiento { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public string isr_cta { get; set; } = string.Empty;
        public long cod_contrato { get; set; }
        public decimal aporte { get; set; }
        public decimal rendimiento { get; set; }
        public decimal multa { get; set; }
        public decimal isr_monto { get; set; }
    }

    public sealed class FndArchivoRefCargaRequest
    {
        public List<FndConsultaPlanRowDto> lineas { get; set; } = new();
    }

    public class ParametroConn
    {
        public SqlConnection conn { get; set; } = new();
        public SqlTransaction? tx { get; set; }
    }

    public class CrearDocumentoGeneralParametros: ParametroConn
    {
        public int codOperador { get; set; } = 0;
        public FndLiquidacionPlanLiquidarRequest? request { get; set; }
        public FndLiquidacionPlanInfoData? plan { get; set; }
        public FndLiquidacionPlanOperadoraData? operadora { get; set; }
        public string? docRef { get; set; }
        public string? tipoDoc { get; set; }
        public string? concepto { get; set; }
    }

    public class InsertarDocumentoMaestroParametros: CrearDocumentoGeneralParametros
    {
        public FndLiquidacionPlanDocumentoResumenData item { get; set; } = new();
    }

    public class EjecutarAsientoParametros: ParametroConn
    {
        public string tipoDocumento { get; set; } = string.Empty;
        public string numDocumento { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public string debeHaber { get; set; } = string.Empty;
        public string codDivisa { get; set; } = string.Empty;
        public int enlace { get; set; } = 0;
        public string codUnidad { get; set; } = string.Empty;
        public string codCentroCosto { get; set; } = string.Empty;
        public string codCuenta { get; set; } = string.Empty;
        public string referencia1 { get; set; } = string.Empty;
        public string referencia2 { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
    }

}
