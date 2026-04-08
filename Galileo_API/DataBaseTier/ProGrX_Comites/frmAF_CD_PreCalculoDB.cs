using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR; 
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdPreCalculo;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdPreCalculoDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _Security_MainDB;
        

        public FrmAfCdPreCalculoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }

        /// <summary>
        ///  Obtiene la información inicial de la pantalla de pre-cálculo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<CrdPreCalculoPantallaInicialResponse> CrdPreCalculo_PantallaInicial_Obtener(int codEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string query = @"
                        SELECT
                            RTRIM(CodTipoActividad) AS item,
                            RTRIM(NombreTipoActividad) AS descripcion
                        FROM AFI_CD_TIPO_ACTIVIDAD
                        WHERE Activo = 1
                        ORDER BY NombreTipoActividad;";

                var tiposActividad = conn.Query<DropDownListaGenericaModel>(query).ToList();

                return DbHelper.CreateOkResponse(new CrdPreCalculoPantallaInicialResponse
                {
                    FechaRegistro = DateTime.Now.ToString("dd/MM/yyyy"),
                    TiposActividad = tiposActividad
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CrdPreCalculoPantallaInicialResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la información del comité para el pre-cálculo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrdPreCalculoComiteResponse> CrdPreCalculo_Comite_Obtener(int codEmpresa, CrdPreCalculoComiteRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string queryComite = @"
                    SELECT TOP 1
                        RTRIM(C.descripcion) AS ComiteDescripcion
                    FROM afi_cd_comites C
                    LEFT JOIN afi_cd_nombramientos N
                        ON C.cod_comite = N.cod_comite
                    INNER JOIN socios S
                        ON S.cedula = N.cedula
                    WHERE N.cod_comite = @ComiteId
                      AND N.APL_DESEMBOLSOS = 1;";

                const string queryAsociados = @"
                    SELECT COUNT(*) 
                    FROM socios
                    WHERE EstadoActual = 'S'
                      AND cod_departamento IN (
                            SELECT Codigo_UP
                            FROM Afi_CD_Comites_Unidades
                            WHERE cod_comite = @ComiteId
                      );";

                var descripcion = connection.QueryFirstOrDefault<string>(
                    queryComite,
                    new { request.ComiteId });

                var cantidadAsociados = connection.ExecuteScalar<int>(
                    queryAsociados,
                    new { request.ComiteId });

                var response = new CrdPreCalculoComiteResponse
                {
                    ComiteId = request.ComiteId?.Trim() ?? string.Empty,
                    ComiteDescripcion = descripcion?.Trim() ?? string.Empty,
                    CantidadAsociados = cantidadAsociados,
                    AjusteAsociados = request.AjusteAsociados,
                    TotalAsociadosAjustado = cantidadAsociados - request.AjusteAsociados,
                    //TieneMiembroDesembolso = !string.IsNullOrWhiteSpace(descripcion),
                    Mensaje = string.IsNullOrWhiteSpace(descripcion)
                        ? "No se cuenta con miembro asignado al desembolso."
                        : string.Empty
                };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CrdPreCalculoComiteResponse>(
                    "Error al obtener la información del comité.",
                    -1,
                    new CrdPreCalculoComiteResponse());
            }
        }
        /// <summary>
        /// Obtiene la información del grid para el pre-cálculo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrdPreCalculoGridResponse> CrdPreCalculo_Grid_Obtener(int codEmpresa,CrdPreCalculoGridRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string storedProcedure = "spAFI_CD_Actividades_List";

                var parametros = new
                {
                    Tipo = request.CodTipoActividad,
                    TotalAsoc = request.TotalAsociadosAjustado,
                     request.Operacion,
                    Comite = request.ComiteId 
                };

                var actividades = connection.Query<CrdPreCalculoActividadGridItem>(
                    storedProcedure,
                    parametros,
                    commandType: System.Data.CommandType.StoredProcedure).ToList();

                var response = new CrdPreCalculoGridResponse
                {
                    Actividades = actividades,
                    MontoTotalAsignado = actividades
                        .Where(x => x.Asignado)
                        .Sum(x => x.Monto)
                };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CrdPreCalculoGridResponse>(
                    "Error al obtener las actividades.",
                    -1,
                    new CrdPreCalculoGridResponse());
            }
        }

        /// <summary>
        /// consulta listado de comites para busqueda por descripcion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrdPreCalculo_ComiteDesc_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                const string sql = @"
                    select distinct
                       cod_comite as item,
                       U.descripcion as descripcion
                    from afi_cd_comites_unidades A 
                    left join uprogramatica U on A.cod_comite = U.codigo ";
                return connection.Query<DropDownListaGenericaModel>(sql).ToList();
            });
 
        }

        /// <summary>
        /// onsulta listado de comites para busqueda por Id
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrdPreCalculo_ComiteId_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                const string sql = @"
                    select 
                       COD_COMITE as item,
                       DESCRIPCION as descripcion
                    from AFI_CD_COMITES ";
                return connection.Query<DropDownListaGenericaModel>(sql).ToList();
            });

        }
    }
}
