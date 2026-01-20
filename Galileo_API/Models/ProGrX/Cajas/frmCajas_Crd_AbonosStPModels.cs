using Galileo.Models.CxP;
using System;

namespace Galileo_API.Models.ProGrX.Cajas
{
    #region Simulación (migración de lógica VB6 a backend)

    public sealed class ProyeccionCuotaDto
    {
        public decimal Interes { get; set; }
        public decimal Amortiza { get; set; }
        public long FechaProceso { get; set; }
        public decimal Saldo { get; set; }
        public decimal Cuota { get; set; }
    }

    public sealed class SimularCuotasRequest
    {
        public long OperacionId { get; set; }
        public int CantidadCuotas { get; set; }
        public long FecUltMov { get; set; }          // yyyymm
        public long PriDeduc { get; set; }           // yyyymm
        public int Plazo { get; set; }
        public decimal Interes { get; set; }         // tasa anual (ej: 12.5)
        public decimal SaldoMes { get; set; }
        public decimal Cuota { get; set; }           // cuota base
        public decimal AmortizaActual { get; set; }  // amortiza actual (estado)
        public string BaseCalculo { get; set; } = "01";
        public bool EsRetencion { get; set; }
    }

    public sealed class SimularCuotasResponse
    {
        public List<ProyeccionCuotaDto> Proyeccion { get; set; } = new();
        public decimal TotalInteres { get; set; }
        public decimal TotalAmortiza { get; set; }
        public long FecUltMovR { get; set; }
        public decimal CuotaR { get; set; }
        public decimal SaldoR { get; set; }
        public int CuotasMaximas { get; set; }
    }

    public sealed class RecalculaCuotaRequest
    {
        public decimal SaldoR { get; set; }
        public int Plazo { get; set; }
        public long PriDeduc { get; set; }
        public long FecUltMovR { get; set; }
        public decimal Interes { get; set; }
    }

    public sealed class RecalculaCuotaResponse
    {
        public decimal CuotaR { get; set; }
    }

    public sealed class MoraConsultaResponse
    {
        public List<CajasCrdAbonoMorosidadData> Items { get; set; } = new();
        public int Cuotas { get; set; }
        public decimal Amortiza { get; set; }
        public decimal Interes { get; set; }
        public decimal Cargos { get; set; }
        public decimal Total { get; set; }
        public bool PermiteExtraordinario { get; set; }
    }


    public sealed class CajasCrdAbonosStPDData
    {
        // reg_creditos
        public long id_solicitud { get; set; }

        public decimal saldo { get; set; }

        public decimal saldo_mes { get; set; }

        public string proceso { get; set; } = string.Empty;

        public string divisa { get; set; } = "COL";

        public decimal? interesv { get; set; }

        // Use @ to escape the reserved keyword 'int'
        public int @int { get; set; }   

        public int plazo { get; set; }

        public decimal interesc { get; set; }

        public decimal amortiza { get; set; }

        public decimal? fecult { get; set; }  // yyyymm (VB6 lo trata como Long)

        public long prideduc { get; set; }

        public int? opex { get; set; }

        public decimal cuota { get; set; }

        public string codigo { get; set; } = string.Empty;

        public string cedula { get; set; } = string.Empty;

        public int? cuotas_planilla { get; set; }

        public int? cuotas_directas { get; set; }

        public decimal montoApr { get; set; }

        public DateTime? fechaforp { get; set; }

        public string base_calculo { get; set; } = string.Empty;

        // socios
        public string nombre { get; set; } = string.Empty;

        // catalogo
        public string descripcion { get; set; } = string.Empty;

        public string retencion { get; set; } = "N"; // 'S' / 'N'

        public string poliza { get; set; } = "N";    // 'S' / 'N'

        public decimal porc_cargo_cancelacion { get; set; }

        // función
        public int caja_valida_concepto { get; set; }

        public decimal glngFechaCR { get; set; }
    }

    public sealed class CajasCrdAbonosStpVariables
    {
        public string? detalle { get; set; }
        public string? vTipoDoc { get; set; }
        public long? vNumDoc { get; set; }
        public string? vConcepto { get; set; }
        public string? vCuenta { get; set; }
        public int? id_solicutud { get; set; }

        public DateTime? _fechaAbono { get; set; }
        public DateTime? _fechaMinima { get; set; }

        public bool? chkRecalculaCuota { get; set; }

        public decimal? vAmortizacion { get; set; }
        public decimal? vMoraAmortiza { get; set; }
        public decimal? vInteres { get; set; }
        public decimal? vMoraInteres { get; set; }

        public decimal? vCargo { get; set; }
        public decimal? vMoraCargo { get; set; }

        public decimal? vAnticipo { get; set; }
        public string? vAnticipoTag { get; set; } = string.Empty;
        public decimal? vCompromiso { get; set; }

        public decimal? vPoliza { get; set; }
        public DateTime FechaCancelacion { get; set; }
        public bool FechaCancelacionEnable { get; set; }
        public decimal? vCompromisoPoliza { get; set; }
        public decimal? vDiferencia { get; set; }

        public decimal? totalCajas { get; set; }

        public string? notas { get; set; } = string.Empty;

        public int? iDias { get; set; }

        public decimal? vTempCuota { get; set; }

        public string? vBaseCalculo { get; set; } = string.Empty;
        public decimal? vPrideduc { get; set; }
        public decimal? vOperacion { get; set; }
        public decimal? vPlazo { get; set; }

        public decimal? vSaldoMes { get; set; }

        public decimal? vCuotasDeducidas { get; set; }
        public decimal? vCuotasDirectas { get; set; }

        public bool? vRetencion { get; set; }

        public decimal? vTempAmort { get; set; }
        public decimal? vTempCargo { get; set; }
        public decimal? vTempIntCor { get; set; }

        public string? pCharRelleno { get; set; } = string.Empty;

        public int? codCaja { get; set; }
        public int? codApertura { get; set; }
        public string? usuarioRegistro { get; set; }              // glogon.Usuario / ModuloCajas.mUsuario
        public string? oficinaTitular { get; set; }                // GLOBALES.gOficinaTitular
        public int? enlace { get; set; }                          // GLOBALES.gEnlace
        public decimal? tipoCambio { get; set; }                // pTipoCambio
        public string? tiquete { get; set; }                      // ModuloCajas.mTiquete
        public string? unidadCaja { get; set; }
    }

    public class CajasCrdAbonoRequest
    {
        public long id_solicitud { get; set; }
        public int totalCajs { get; set; }
        public string tipoDoc { get; set; }
        public string numDoc { get; set; }
        public int concepto { get; set; }
        public string mUsuario { get; set; }
        public string mCaja { get; set; }
        public int mApertura { get; set; }
        public int chkRecalculaCuota { get; set; }
        public int datosAnticipo { get; set; }
        public string tipo { get; set; }
        public int datosInteres { get; set; }
        public DateTime FechaCancelacion { get; set; }

        public string vNotas { get; set; } = string.Empty;

        public string? vCuenta { get; set; }

        public long? lblFecUltMovR {  get; set; }
    }

    public class CajasCrdAbonoMorosidadData
    {
        public int id_moro { get; set; }                 // rs!id_moro
        public int id_solicitud { get; set; }            // rs!Id_Solicitud
        public string fechap { get; set; }               // rs!fechap (formato "####-##")
        public decimal intc { get; set; }                // rs!IntC
        public decimal intm { get; set; }                // rs!IntM
        public decimal amortiza { get; set; }            // rs!Amortiza
        public decimal cargo { get; set; }               // rs!Cargo
        public decimal total { get; set; }               // rs!IntC + rs!IntM + rs!Amortiza + rs!Cargo
        public bool checked_ { get; set; } = true;       // itmX.Checked = True
    }

    public class CajasCrdAbonoCargaOperacionData
    {
        public int id_solicitud { get; set; }
        public decimal saldo { get; set; }
        public decimal saldo_mes { get; set; }
        public decimal interesv { get; set; }
        public decimal @int { get; set; }             // “int” es palabra reservada en C#, se escapa con @
        public int plazo { get; set; }
        public decimal interesc { get; set; }
        public decimal amortiza { get; set; }
        public DateTime fecult { get; set; }
        public string opex { get; set; } = string.Empty;
        public decimal cuota { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public int cuotas_planilla { get; set; }
        public int cuotas_directas { get; set; }
        public string retencion { get; set; } = string.Empty;
        public string poliza { get; set; } = string.Empty;
    }

    public class CajasCrdAbonoAfectacionData
    {
        public decimal IntCor { get; set; } = 0;
        public decimal IntMor { get; set; } = 0;
        public decimal Principal { get; set; } = 0;
        public decimal Cargos { get; set; } = 0;
        public decimal Polizas { get; set; } = 0;
    }

    public sealed class CajasCrdAbonooperacionCtas
    {
        public int id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public decimal saldo { get; set; }
        public string proceso { get; set; } = string.Empty;          // 'N' / 'S'
        public string retencion { get; set; } = string.Empty;        // 'N' / 'S'
        public string poliza { get; set; } = string.Empty;           // 'N' / 'S'
        public string cta_amortiza { get; set; } = string.Empty;
        public string cta_int_c { get; set; } = string.Empty;
        public string cta_int_m { get; set; } = string.Empty;
        public string? cta_cargos { get; set; }
        public string cod_divisa { get; set; } = string.Empty;
        public string cod_oficina_r { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string? cod_centro_costo { get; set; }
        public decimal tipo_cambio { get; set; }
        public string? cta_iva { get; set; }
    }
}
#endregion