using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.Models;

namespace Galileo.DataBaseTier.ProGrX_Personas
{
    public class FrmAFCrLiquidacionMasivaDB
    {
        private readonly IConfiguration _config;

        public FrmAFCrLiquidacionMasivaDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Consulta liquidaciones masivas pendientes usando el SP y los parámetros del objeto Filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<AfLiquidacionMasiva>> AF_LiquidacionMasiva_Obtener(int CodEmpresa, AfLiquidacionMasivaFiltros Filtro)
        {
            if (Filtro is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de liquidación masiva son requeridos.", -2, new List<AfLiquidacionMasiva>());
            }

            return EjecutarStoredProcedureList<AfLiquidacionMasiva>(
                CodEmpresa,
                "spAFI_Renuncia_Liquidacion_Pendiente",
                new
                {
                    Inicio = Filtro.Inicio,
                    Corte = Filtro.Corte,
                    Tipo = Filtro.Tipo,
                    Institucion = Filtro.Institucion,
                    Causa = Filtro.Causa,
                    Cedula = Filtro.Cedula ?? string.Empty,
                    Nombre = Filtro.Nombre ?? string.Empty,
                    Ejecutivo = Filtro.Ejecutivo ?? string.Empty,
                    Usuario = Filtro.Usuario ?? string.Empty
                });
        }

        /// <summary>
        /// Consulta las causas de renuncia para dropdown, con variantes según los parámetros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoApl"></param>
        /// <param name="inicio"></param>
        /// <param name="corte"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_LiquidacionMasiva_Obtener_Causas(int CodEmpresa, string? tipoApl = null, DateTime? inicio = null, DateTime? corte = null)
        {
            string query;
            object? parameters = null;

            if (tipoApl == null && inicio == null && corte == null)
            {
                query = @"SELECT id_Causa AS item, Descripcion AS descripcion
                          FROM causas_renuncias
                          WHERE ACTIVO = 1";
            }
            else if (inicio == null && corte == null)
            {
                query = @"SELECT id_Causa AS item, Descripcion AS descripcion
                          FROM causas_renuncias
                          WHERE ACTIVO = 1
                            AND Tipo_Apl IN ('A', @TipoApl)";
                parameters = new { TipoApl = tipoApl };
            }
            else
            {
                query = @"SELECT id_Causa AS item, Descripcion AS descripcion
                          FROM causas_renuncias
                          WHERE ACTIVO = 1
                            AND id_Causa IN (
                                SELECT ID_CAUSA
                                FROM AFI_CR_RENUNCIAS
                                WHERE registro_Fecha BETWEEN @Inicio AND @Corte
                                  AND Tipo IN ('A', @Tipo)
                                  AND Estado = 'P'
                                  AND LIQ IS NULL
                                GROUP BY ID_CAUSA
                            )";
                parameters = new
                {
                    Inicio = inicio?.Date.ToString("yyyy-MM-dd") + " 00:00:00",
                    Corte = corte?.Date.ToString("yyyy-MM-dd") + " 23:59:59",
                    Tipo = tipoApl ?? "P"
                };
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                query,
                parameters);
        }

        /// <summary>
        /// Consulta las instituciones activas para dropdown.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_LiquidacionMasiva_Obtener_Instituciones(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT cod_Institucion AS item, Descripcion AS descripcion
                  FROM Instituciones
                  WHERE ACTIVA = 1");
        }

        /// <summary>
        /// Ejecuta el proceso de liquidación masiva para una renuncia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="RenunciaId"></param>
        /// <param name="Usuario"></param>
        /// <param name="S06"></param>
        /// <returns></returns>
        public ErrorDto AF_LiquidacionMasiva(int CodEmpresa, int RenunciaId, string Usuario, short S06 = 1)
        {
            return EjecutarStoredProcedure(
                CodEmpresa,
                "spAFI_Renuncia_Liquidacion_Procesa",
                new
                {
                    RenunciaId,
                    Usuario,
                    S06
                },
                "Error al ejecutar liquidación masiva.");
        }

        private ErrorDto<List<T>> EjecutarStoredProcedureList<T>(int codEmpresa, string storedProcedure, object parameters)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.Query<T>(storedProcedure, parameters, commandType: System.Data.CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<T>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al ejecutar procedimiento almacenado.", result.Code.GetValueOrDefault(-1), new List<T>());
        }

        private ErrorDto EjecutarStoredProcedure(int codEmpresa, string storedProcedure, object parameters, string errorMessage)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                connection.Execute(storedProcedure, parameters, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? errorMessage, result.Code.GetValueOrDefault(-1));
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}
