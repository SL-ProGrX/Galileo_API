using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using System.Globalization;

namespace Galileo_API.DataBaseTier
{
    public class MCobroDb
    {
        private readonly PortalDB _portalDB;
        private readonly IConfiguration _config;

        public MCobroDb(IConfiguration config)
        {
            _config = config;
            _portalDB = new PortalDB(config);
        }

        public decimal fxFechaProcesoAnterior(int CodEmpresa, decimal pProceso)
        {
            string sql = "select dbo.fxSIFPrmProcesoAnt(@proceso) as Result";

            var parametros = new
            {
                proceso = pProceso
            };

            return DbHelper.ExecuteSingleQuery<decimal>(
                _portalDB,
                CodEmpresa,
                sql,
                0,
                parametros
            ).Result;
        }

        public decimal fxFechaProcesoSiguiente(int CodEmpresa, decimal pProceso)
        {
            string sql = "select dbo.fxSIFPrmProcesoSig(@proceso) as Result";

            var parametros = new
            {
                proceso = pProceso
            };

            return DbHelper.ExecuteSingleQuery<decimal>(
                _portalDB,
                CodEmpresa,
                sql,
                0,
                parametros
            ).Result;
        }

        public static decimal fxCalcula_Cuota(decimal Monto, int Plazo, object Interes, string? Frecuencia = "M")
        {
            if (Plazo <= 0)
                return 0m;

            var tasa = Convert.ToDecimal(Interes);

            if (tasa == 0m)
                return Math.Round(Monto / Plazo, 2);

            decimal interesPeriodo;

            switch ((Frecuencia ?? "M").Trim().ToUpperInvariant())
            {
                case "Q":
                    interesPeriodo = tasa / (24m * 100m);
                    break;

                default:
                    interesPeriodo = tasa / (12m * 100m);
                    break;
            }

            decimal factor = 1m;

            for (int i = 1; i <= Plazo; i++)
                factor *= (1m + interesPeriodo);

            if (factor == 1m)
                return Math.Round(Monto / Plazo, 2);

            var cuota = Monto * interesPeriodo * factor / (factor - 1m);

            return Math.Round(cuota, 2);
        }

        public DateTime fxFechaCalculo(int codEmpresa, string? pLinea = "", decimal pPriDeduc = 0, int pDiaPago = 32)
        {
            return new MSeguimientoDB(_config)
                .fxFechaCalculo(codEmpresa, pLinea, pPriDeduc, pDiaPago);
        }

        public static string fxDescribeEstado(string? strEstado)
        {
            var e = (strEstado ?? string.Empty).Trim().ToUpperInvariant();

            return e switch
            {
                "A" => "Activo",
                "N" => "Anulado",
                "C" => "Cancelado",
                _ => ""
            };
        }

        public string fxDescribeCodigo(int CodEmpresa, string strCodigo)
        {
            strCodigo = (strCodigo ?? string.Empty).Trim();

            if (strCodigo.Length == 0)
                return string.Empty;

            const string sql = @"
                select rtrim(descripcion)
                from catalogo
                where codigo = @codigo";

            var result = DbHelper.ExecuteSingleQuery<string>(
                _portalDB,
                CodEmpresa,
                sql,
                string.Empty,
                new { codigo = strCodigo }
            );

            if (result.Code != 0)
                return string.Empty;

            return (result.Result ?? string.Empty).Trim();
        }

        public string fxCBRParametro(int CodEmpresa, string pParametro)
        {
            pParametro = (pParametro ?? string.Empty).Trim();

            if (pParametro.Length == 0)
                return string.Empty;

            const string sql = @"
                select rtrim(valor)
                from cbr_parametros
                where cod_parametro = @Parametro";

            var result = DbHelper.ExecuteSingleQuery<string>(
                _portalDB,
                CodEmpresa,
                sql,
                string.Empty,
                new { Parametro = pParametro }
            );

            if (result.Code != 0)
                return string.Empty;

            return (result.Result ?? string.Empty).Trim();
        }

        public int fxCBRPlazoRestante(int CodEmpresa, long pOperacion)
        {
            const string sql = @"
                select
                    isnull(plazo,0) as Plazo,
                    isnull(prideduc,0) as PriDeduc
                from reg_creditos
                where id_solicitud = @Operacion";

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            conn.Open();

            var row = conn.QueryFirstOrDefault<CbrPlazoRestanteRow>(
                sql,
                new { Operacion = pOperacion }
            );

            if (row == null)
                return 1;

            decimal primerDeduccionActual =
                new MSeguimientoDB(_config).fxPrimerDeduccion(CodEmpresa);

            string priDeducTexto =
                row.PriDeduc.ToString("0", CultureInfo.InvariantCulture);

            if (priDeducTexto.Length < 6)
                return 1;

            int anio = int.Parse(priDeducTexto.Substring(0, 4), CultureInfo.InvariantCulture);
            int mes = int.Parse(priDeducTexto.Substring(4, 2), CultureInfo.InvariantCulture);

            int contador = 0;

            long resultado =
                Convert.ToInt64($"{anio}{mes:00}", CultureInfo.InvariantCulture);

            while (primerDeduccionActual >= resultado)
            {
                if (mes == 12)
                {
                    mes = 1;
                    anio++;
                }
                else
                {
                    mes++;
                }

                contador++;

                resultado =
                    Convert.ToInt64($"{anio}{mes:00}", CultureInfo.InvariantCulture);
            }

            contador = row.Plazo - contador;
            contador = contador + 1;

            if (contador <= 0)
                contador = 1;

            return contador;
        }

        private sealed class CbrPlazoRestanteRow
        {
            public int Plazo { get; set; } = 0;
            public decimal PriDeduc { get; set; } = 0m;
        }
    }
}