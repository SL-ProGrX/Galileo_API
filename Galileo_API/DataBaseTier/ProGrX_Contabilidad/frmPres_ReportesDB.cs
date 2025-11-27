using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

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
            ErrorDto<List<ModeloGenericList>> resp = new ErrorDto<List<ModeloGenericList>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query1 = $@"select Cc.INICIO_ANIO,Cc.INICIO_MES, Cc.CORTE_ANIO, Cc.CORTE_MES, Pm.Estado
                    from CNTX_CIERRES Cc inner join PRES_MODELOS Pm on Cc.COD_CONTABILIDAD = Pm.COD_CONTABILIDAD and Cc.ID_CIERRE = Pm.ID_CIERRE 
                    where Pm.COD_CONTABILIDAD = {CodContab}
                    and Pm.COD_MODELO = '{CodModelo}'
                    order by Cc.INICIO_ANIO desc";
                    infoPeriodo = connection.Query<PresPeriodoRequest>(query1).FirstOrDefault();

                    if (infoPeriodo == null)
                    {
                        resp.Code = -1;
                        resp.Description = "No se encontró información de periodo.";
                        resp.Result = new List<ModeloGenericList>();
                        return resp;
                    }

                    var query2 = $@"select dbo.fxSys_FechaAnioMesToDatetime(anio,mes) as 'ItmX'
                    From dbo.fxPres_Periodo('{infoPeriodo.Inicio_Anio}','{infoPeriodo.Inicio_Mes}','{infoPeriodo.Corte_Anio}','{infoPeriodo.Corte_Mes}',{CodContab})";
                    resp.Result = connection.Query<ModeloGenericList>(query2).ToList();

                    foreach (var item in resp.Result)
                    {
                        item.IdX = item.ItmX;
                    }

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        public ErrorDto<List<ModeloGenericList>> spPres_Ajustes_Permitidos_Obtener(int CodEmpresa, int codContab, string codModelo, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<ModeloGenericList>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spPres_Modelo_Ajustes_Permitidos {codContab},'{codModelo}', '{Usuario}'";
                    resp.Result = connection.Query<ModeloGenericList>(query).ToList();
                }
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