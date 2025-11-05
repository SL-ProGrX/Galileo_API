using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_PlanillaEnviaDB
    {
        private readonly IConfiguration _config;

        public frmAF_PlanillaEnviaDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene instuciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> AF_Instituciones_Obtener(int CodEmpresa)
        {
            var response = new ErrorDTO<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                const string query = @"
                            SELECT 
                                cod_institucion AS item,
                                RTRIM(descripcion) AS descripcion
                            FROM instituciones
                            WHERE activa = 1
                            ORDER BY descripcion;";

                response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        /// Obtiene Periodos Proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> AF_PeriodosProceso_Obtener(int CodEmpresa)
        {
            var response = new ErrorDTO<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                const string query = @";WITH Periodos AS (
                        SELECT dbo.fxSIFPrmProcesoAnt(
                                   dbo.fxSIFPrmProcesoAnt(YEAR(dbo.MyGetdate()) * 100 + MONTH(dbo.MyGetdate()))
                               ) AS item,
                               0 AS Orden
                        UNION ALL
                        SELECT dbo.fxSIFPrmProcesoSig(item), Orden + 1
                        FROM Periodos
                        WHERE Orden < 6
                    )
                    SELECT item
                    FROM Periodos
                    ORDER BY item;";

                response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtener archivo planilla
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codinstitucion"></param>
        /// <param name="fechaproceso"></param>
        /// <returns></returns>
        public ErrorDTO<List<AF_ArchivoResultadoDTO>> AF_Archivo_Obtener(int CodEmpresa, string codinstitucion, string fechaproceso)
        {
            var response = new ErrorDTO<List<AF_ArchivoResultadoDTO>> { Code = 0, Result = new() };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                const string query = @"EXEC spPrm_Formato_PG_Soc @CodInstitucion, @FechaProceso";

                response.Result = connection.Query<AF_ArchivoResultadoDTO>(
                    query,
                    new { codinstitucion, fechaproceso }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }





    }

}
