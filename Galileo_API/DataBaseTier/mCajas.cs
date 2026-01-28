using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.CxP;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using System.IO.Pipelines;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class MCajas
    {
        private readonly PortalDB _portalDB;

        public MCajas(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }
        public static string FxStringCifrado(string input)
        {
            var asciiReversed = string.Concat(
                input
                    .Select(c => ((int)c).ToString())
                    .Reverse()
            );

            var resultBuilder = new System.Text.StringBuilder();
            int sec = 0;

            for (int i = 0; i < asciiReversed.Length; i += 3)
            {
                int len = Math.Min(3, asciiReversed.Length - i);
                int block = int.Parse(asciiReversed.Substring(i, len));

                block += sec switch
                {
                    0 => 1,
                    1 => -5,
                    2 => 7,
                    3 => -13,
                    4 => -2,
                    5 => 3,
                    _ => 0
                };

                sec = (sec + 1) % 6;

                resultBuilder.Append(block);
            }

            return FxDepuraCadena(resultBuilder.ToString());
        }

        private static string FxDepuraCadena(string cadena)
        {
            var finalBuilder = new System.Text.StringBuilder();

            for (int i = 0; i < cadena.Length - 1; i++)
            {
                if (int.TryParse(cadena.Substring(i, 2), out int n) &&
                    n > 31 && n != 39 && n != 34)
                {
                    finalBuilder.Insert(0, (char)n);
                }
            }
            return finalBuilder.ToString();
        }

        public decimal fxCajasTipoCambio(int codEmpresa, int gEnlace, string pDivisa, string? pTipo = "C")
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                string sql = @"select dbo.fxCajas_TipoCambio(@gEnlace, @pDivisa,dbo.MyGetdate() , @pTipo) as 'TipoCambio'";
                var tipoCambio = conn.QueryFirstOrDefault<decimal?>(sql, new { gEnlace, pDivisa, pTipo });
                return tipoCambio ?? 1m;
            }).Result;
        }

        public bool fxCajasAbonosCbrJud(int CodEmpresa, string pCaja, string pUsuario)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select dbo.fxCajas_AbonoCbrJudAutorizada(@caja, @usuario)";

                var valor = conn.QueryFirstOrDefault<int?>(
                    sql,
                    new
                    {
                        caja = pCaja,
                        usuario = pUsuario
                    }
                );

                return valor.HasValue && valor.Value == 1;
            }).Result;
        }

        public string fxCajasAperturaEstado(int CodEmpresa, string caja, int apertura)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @" select Estado from cajas_aperturas_main 
                    where cod_caja = @caja and cod_apertura = @apertura";

                var estado = conn.QueryFirstOrDefault<string>(
                    sql,
                    new
                    {
                        caja,
                        apertura
                    }
                );

                return string.IsNullOrWhiteSpace(estado) ? "C" : estado.Trim();
            }).Result ?? "";
        }

    }
}