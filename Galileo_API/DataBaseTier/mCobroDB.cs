using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Microsoft.CodeAnalysis;
using Org.BouncyCastle.Utilities;
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
            string sql = "select dbo.fxSIFPrmProcesoAnt(@proceso) as 'Result'";
            var parametros = new { proceso = pProceso };

            return DbHelper.ExecuteSingleQuery<decimal>(_portalDB, CodEmpresa, sql, 0, parametros).Result;
        }

        public decimal fxFechaProcesoSiguiente(int CodEmpresa, decimal pProceso)
        {
            string sql = "select dbo.fxSIFPrmProcesoSig(@proceso) as 'Result'";
            var parametros = new { proceso = pProceso };

            return DbHelper.ExecuteSingleQuery<decimal>(_portalDB, CodEmpresa, sql, 0, parametros).Result;
        }

        public static decimal fxCalcula_Cuota(decimal Monto, int Plazo, object Interes, string? Frecuencia = "M")
        {
            if (Plazo <= 0) return 0m;

            var tasa = Convert.ToDecimal(Interes);
            if (tasa == 0m)
                return Math.Round(Monto / Plazo, 2);

            decimal curInteresMensual;
            switch ((Frecuencia ?? "M").Trim().ToUpperInvariant())
            {
                case "Q":
                    curInteresMensual = tasa / (24m * 100m);
                    break;
                default:
                    curInteresMensual = tasa / (12m * 100m);
                    break;
            }

            decimal curfactor = 1m;
            for (int i = 1; i <= Plazo; i++)
                curfactor *= (1m + curInteresMensual);
            if (curfactor == 1m)
                return Math.Round(Monto / Plazo, 2);

            var curCuota = Monto * curInteresMensual * curfactor / (curfactor - 1m);
            return Math.Round(curCuota, 2);
        }
        public DateTime fxFechaCalculo(int codEmpresa, string? pLinea = "", decimal pPriDeduc = 0, int pDiaPago = 32)
        {
            return new MSeguimientoDB(_config).fxFechaCalculo(codEmpresa, pLinea, pPriDeduc, pDiaPago);
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
            {
                return string.Empty;
            }

            const string sql = @"
                select rtrim(descripcion)
                from catalogo
                where codigo = @codigo;";

            var result = DbHelper.ExecuteSingleQuery<string>(
                _portalDB,
                CodEmpresa,
                sql,
                string.Empty,
                new { codigo = strCodigo });

            if (result.Code != 0)
            {
                return string.Empty;
            }

            return (result.Result ?? string.Empty).Trim();
        }
    }
}
