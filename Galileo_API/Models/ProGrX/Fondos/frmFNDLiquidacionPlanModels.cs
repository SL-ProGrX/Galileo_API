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

  

}
