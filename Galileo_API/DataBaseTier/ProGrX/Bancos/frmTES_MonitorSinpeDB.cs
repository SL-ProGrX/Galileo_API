using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.Models.ProGrX.Bancos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesMonitorSinpeDB
    {
        private readonly PortalDB _portalDB;

        public FrmTesMonitorSinpeDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Método para consultar el total de sobres de consulta
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        public ErrorDto<decimal> fxFnd_SobresConsultaTotal(int CodEmpresa, string? cedula, string? plan)
        {
            const string sql = "SELECT dbo.fxFnd_SobresConsultaTotalSobres(@cedula, @codPlan) AS MontoSobres";
            var p = new DynamicParameters();
            p.Add("@cedula", cedula, DbType.String);   // o sin DbType si no querés
            p.Add("@codPlan", plan, DbType.String);

            return DbHelper.ExecuteSingleQuery<decimal>(_portalDB, CodEmpresa, sql, 0, p);
        }

        /// <summary>
        /// Método para consultar el saldo total de los contratos SINPE
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<decimal> Tes_MonitorSinpeContrato_Consultar(int CodEmpresa)
        {
            const string sql = @"SELECT
	                                COALESCE(SUM(COALESCE(f.APORTES, 0) + COALESCE(f.RENDIMIENTO, 0)), 0) AS SALDO
                                FROM FND_CONTRATOS f
                                WHERE COD_PLAN = 'SINPE'";

            return DbHelper.ExecuteSingleQuery<decimal>(_portalDB, CodEmpresa, sql, 0);
        }

        /// <summary>
        /// Método para consultar los débitos y créditos SINPE por concepto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// <returns></returns>
        public ErrorDto<List<TesMonitorSinpeDebCrdModels>> Tes_MonitorSinpeDebCred_Consultar(int CodEmpresa, DateTime fechaInicio, DateTime fechaFin)
        {
            const string sql = @"SELECT
                                    ROW_NUMBER() OVER (ORDER BY cp.DESCRIPCION) AS Consecutivo,
			                        SUM(CASE
				                        WHEN d.MONTO <= 0 THEN d.MONTO
			                        END) AS Debito
		                           ,SUM(CASE
				                        WHEN d.MONTO >= 0 THEN d.MONTO
			                        END) AS Credito
		                           ,cp.DESCRIPCION
		                        FROM FND_CONTRATOS_DETALLE d
		                        INNER JOIN FND_CONTRATOS c
			                        ON c.COD_OPERADORA = d.COD_OPERADORA
				                        AND c.COD_PLAN = d.COD_PLAN
				                        AND c.COD_CONTRATO = d.COD_CONTRATO
		                        INNER JOIN SIF_CONCEPTOS cp
			                        ON cp.COD_CONCEPTO = d.COD_CONCEPTO
		                        WHERE d.COD_PLAN = 'SINPE'
		                        AND (CONVERT(DATE, d.FECHA) BETWEEN @fechaInicio AND @fechaFin)
		                        GROUP BY cp.DESCRIPCION;";

            var p = new DynamicParameters();
            p.Add("@fechaInicio", fechaInicio, DbType.DateTime);   // o sin DbType si no querés
            p.Add("@fechaFin", fechaFin, DbType.DateTime);

            return DbHelper.ExecuteListQuery<TesMonitorSinpeDebCrdModels>(_portalDB, CodEmpresa, sql, p); 
        }

        /// <summary>
        /// Método para consultar los débitos y créditos SINPE en tránsito
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// <returns></returns>
        public ErrorDto<List<TesMonitorSinpeDebCrdModels>> Tes_MonitorSinpeTransitos_Consultar(int CodEmpresa, DateTime fechaInicio, DateTime fechaFin)
        {
            const string sql = @"SELECT
                                    ROW_NUMBER() OVER (ORDER BY CEDULA) AS Consecutivo,
			                        SUM(CASE
				                        WHEN MONTO <= 0 THEN MONTO
			                        END) AS Debito
		                           ,SUM(CASE
				                        WHEN MONTO >= 0 THEN MONTO
			                        END) AS Credito
		                        FROM SINPE_MOV_TRANSITO
		                        WHERE (CONVERT(DATE, REGISTRO_FECHA) BETWEEN @fechaInicio AND @fechaFin)
		                        AND estado = 4
		                        GROUP BY CEDULA;";

            var p = new DynamicParameters();
            p.Add("@fechaInicio", fechaInicio, DbType.DateTime);   // o sin DbType si no querés
            p.Add("@fechaFin", fechaFin, DbType.DateTime);

            return DbHelper.ExecuteListQuery<TesMonitorSinpeDebCrdModels>(_portalDB, CodEmpresa, sql, p);
        }

    }
}
