using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmPresReportesDb
    {
        private readonly IConfiguration _config;

        public FrmPresReportesDb(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<ModeloGenericList>> fxPres_Periodo_Obtener(int CodEmpresa, int CodContab, string CodModelo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            PresPeriodoRequest? infoPeriodo = null;
            var resp = new ErrorDto<List<ModeloGenericList>>
            {
                Result = new List<ModeloGenericList>()
            };

            const string sqlPeriodo = @"
                SELECT 
                    Cc.INICIO_ANIO,
                    Cc.INICIO_MES, 
                    Cc.CORTE_ANIO, 
                    Cc.CORTE_MES, 
                    Pm.Estado
                FROM CNTX_CIERRES Cc 
                INNER JOIN PRES_MODELOS Pm 
                    ON Cc.COD_CONTABILIDAD = Pm.COD_CONTABILIDAD 
                    AND Cc.ID_CIERRE = Pm.ID_CIERRE 
                WHERE Pm.COD_CONTABILIDAD = @CodContab
                  AND Pm.COD_MODELO       = @CodModelo
                ORDER BY Cc.INICIO_ANIO DESC;";

            const string sqlFechas = @"
                SELECT dbo.fxSys_FechaAnioMesToDatetime(anio, mes) AS ItmX
                FROM dbo.fxPres_Periodo(@InicioAnio, @InicioMes, @CorteAnio, @CorteMes, @CodContab);";

            try
            {
                using var connection = new SqlConnection(stringConn);

                // 1. Obtener info de periodo
                infoPeriodo = connection.Query<PresPeriodoRequest>(
                    sqlPeriodo,
                    new
                    {
                        CodContab,
                        CodModelo
                    }).FirstOrDefault();

                if (infoPeriodo == null)
                {
                    resp.Code = -1;
                    resp.Description = "No se encontró información de periodo.";
                    resp.Result = new List<ModeloGenericList>();
                    return resp;
                }

                // 2. Obtener lista de fechas usando fxPres_Periodo con parámetros
                resp.Result = connection.Query<ModeloGenericList>(
                    sqlFechas,
                    new
                    {
                        InicioAnio = infoPeriodo.Inicio_Anio,
                        InicioMes = infoPeriodo.Inicio_Mes,
                        CorteAnio = infoPeriodo.Corte_Anio,
                        CorteMes = infoPeriodo.Corte_Mes,
                        CodContab
                    }).ToList();

                // 3. Copiar ItmX a IdX
                foreach (var item in resp.Result)
                {
                    item.IdX = item.ItmX;
                }

                resp.Code = 0;
                resp.Description = "OK";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<List<ModeloGenericList>> spPres_Ajustes_Permitidos_Obtener(
            int CodEmpresa,
            int codContab,
            string codModelo,
            string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<ModeloGenericList>>();

            const string proc = "[spPres_Modelo_Ajustes_Permitidos]";

            try
            {
                using var connection = new SqlConnection(stringConn);

                resp.Result = connection.Query<ModeloGenericList>(
                    proc,
                    new
                    {
                        CodContab = codContab,
                        CodModelo = codModelo,
                        Usuario
                    },
                    commandType: CommandType.StoredProcedure
                ).ToList();

                resp.Code = 0;
                resp.Description = "OK";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }
    }
}