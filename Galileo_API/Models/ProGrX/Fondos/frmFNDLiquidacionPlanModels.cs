using System;

namespace PgxAPI.Models.ProGrX.Fondos
{
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
        public string fechafinal { get; set; } = string.Empty;
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

    //public class FndPlanDetalleData
    //{
    //    public string Descripcion { get; set; } = string.Empty;
    //    public DateTime Vence { get; set; }
    //}

    //public class FndLiquidacionPlanData
    //{
    //    public string cod_transaccion { get; set; } = string.Empty;
    //    public string tipo_documento { get; set; } = string.Empty;
    //    public DateTime registro_fecha { get; set; }           // si lo seteás desde C#, si no lo usás va en SQL
    //    public string registro_usuario { get; set; } = string.Empty;

    //    public string cliente_identificacion { get; set; } = string.Empty;
    //    public string cliente_nombre { get; set; } = string.Empty;

    //    public string cod_concepto { get; set; } = string.Empty;
    //    public decimal monto { get; set; }
    //    public string estado { get; set; } = "P";

    //    public string referencia_01 { get; set; } = string.Empty;
    //    public string referencia_02 { get; set; } = string.Empty;

    //    public string cod_oficina { get; set; } = string.Empty;

    //    public string linea1 { get; set; } = string.Empty;
    //    public string linea2 { get; set; } = string.Empty;
    //    public string linea3 { get; set; } = string.Empty;
    //    public string linea4 { get; set; } = string.Empty;
    //    public string linea5 { get; set; } = string.Empty;
    //    public string linea6 { get; set; } = string.Empty;
    //    public string linea7 { get; set; } = string.Empty;

    //    public string detalle { get; set; } = string.Empty;
    //    public string documento { get; set; } = string.Empty;

    //    public decimal pAportes { get; set; } = 0;
    //    public decimal pRendimiento { get; set; } = 0;

    //    public string notas { get; set; } = string.Empty;
    //}

    //public class FndConsultaPlanRow
    //{
    //    public long cod_contrato { get; set; }
    //    public string cedula { get; set; } = string.Empty;
    //    public string nombre { get; set; } = string.Empty;

    //    public string estado { get; set; } = string.Empty;
    //    public int plazo { get; set; }
    //    public decimal monto { get; set; }
    //    public decimal aportes { get; set; }
    //    public decimal rendimiento { get; set; }

    //    public DateTime? fecha_corte { get; set; }
    //    public DateTime fecha_inicio { get; set; }

    //    public string cuentaahorrox { get; set; } = string.Empty;
    //    public int bancox { get; set; }
    //    public string? bancodesc { get; set; }

    //    public string estadodesc { get; set; } = string.Empty;
    //}

    //public class FndConsultaPlanFiltro
    //{
    //    public string plan { get; set; } = string.Empty;                 // vPlan

    //    public int cod_operadora { get; set; }                           // cboOperadora.ItemData(...)
    //    public string tipo_documento { get; set; } = string.Empty;       // cboTipoDocumento.Text
    //    public string proceso { get; set; } = string.Empty;              // cboProceso.Text

    //    public int banco_id { get; set; }                                // cboBanco.ItemData(...)
    //    public string banco_texto { get; set; } = "TODOS";               // cboBanco.Text
    //    public string cuenta_filtro { get; set; } = "TODOS";             // cboCuentaFiltro.Text
    //    public string grupo_bancario { get; set; } = "";                 // mGrupoBancario

    //    public int? institucion_id { get; set; }                         // null si "TODOS"
    //    public bool filtrar_linea { get; set; }                          // chkLineas.Value = vbUnchecked
    //    public string linea_codigo { get; set; } = "";                   // txtLinea.Text

    //    public bool filtrar_fechas { get; set; }                         // chkFechas.Value = vbUnchecked
    //    public DateTime fecha_inicio_desde { get; set; }                 // dtpInicio.Value
    //    public DateTime fecha_inicio_hasta { get; set; }                 // dtpCorte.Value

    //    public string estado_persona { get; set; } = "TODOS";            // cboEstado.Text
    //    public string estado_persona_id { get; set; } = "";              // cboEstado.ItemData(...)
    //    public bool estado_persona_diferente { get; set; }               // chkEstadoPersonaDiferente

    //    public bool fondos_cero { get; set; }                            // chkFondosCero
    //    public bool filtrar_montos { get; set; }                         // chkMontos.Value = vbUnchecked
    //    public decimal monto_desde { get; set; }
    //    public decimal monto_hasta { get; set; }

    //    public bool contratos_sin_mov_aportes { get; set; }              // chkContratosSinMovAportes
    //    public int contratos_sin_mov_meses { get; set; }                 // txtContratosSinMovMeses.Text

    //    public bool rnd_sin_aporte { get; set; }                         // chkRndSinAporte
    //    public bool mensualidad_cero { get; set; }                       // chkMensualidad

    //    public string creditos_filtro { get; set; } = "TODOS";           // cboCreditos.Text

    //    public bool usar_archivo_ref { get; set; }                       // txtArchivo.Text <> ""
    //    public string? usuario_archivo { get; set; }                     // glogon.Usuario si aplica
    //}

    //public class FndLiquidacionPlanResumenData
    //{
    //    public int cod_operadora { get; set; }
    //    public string cod_plan { get; set; } = string.Empty;

    //    public string cuenta_conta { get; set; } = string.Empty;
    //    public string cuenta_rendimiento { get; set; } = string.Empty;

    //    public string cod_cuenta { get; set; } = string.Empty;

    //    public string isr_cta { get; set; } = string.Empty;   // alias "ISR_Cta"
    //    public long cod_contrato { get; set; }               // alias "Cod_Contrato"

    //    public decimal aporte { get; set; }                 // alias "Aporte"
    //    public decimal rendimiento { get; set; }            // alias "Rendimiento"
    //    public decimal multa { get; set; }                  // alias "Multa"

    //    public decimal isr_monto { get; set; }              // alias "ISR_Monto"
    //}

    //public class FndProcesoLiquidacionData
    //{
    //    public long contrato { get; set; }                 // vContrato (col 2)
    //    public decimal aportes_liq { get; set; }           // col 5
    //    public decimal rendi_liq { get; set; }             // col 6
    //    public string banco { get; set; } = "0";           // col 7 (string en VB6)
    //    public string cta_ahorros { get; set; } = "";      // col 8
    //}

}
