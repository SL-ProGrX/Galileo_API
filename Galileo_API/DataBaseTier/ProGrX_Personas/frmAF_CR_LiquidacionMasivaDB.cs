using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;
using PgxAPI.Models;

namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_CR_LiquidacionMasivaDB
    {
        private readonly IConfiguration? _config;

        public frmAF_CR_LiquidacionMasivaDB(IConfiguration? config)
        {
            _config = config;
        }

        /// <summary>
        /// Consulta liquidaciones masivas pendientes usando el SP y los parámetros del objeto Filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Filtro"></param>
        /// <returns></returns>
        public ErrorDTO<List<AF_Liquidacion_Masiva>> AF_LiquidacionMasiva_Obtener(int CodEmpresa, AF_LiquidacionMasiva_Filtros Filtro)
        {
            var result = new ErrorDTO<List<AF_Liquidacion_Masiva>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AF_Liquidacion_Masiva>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new
                {
                    Inicio = Filtro.Inicio,
                    Corte = Filtro.Corte,
                    Tipo = Filtro.Tipo,
                    Institucion = Filtro.Institucion,
                    Causa = Filtro.Causa,
                    Cedula = Filtro.Cedula ?? "",
                    Nombre = Filtro.Nombre ?? "",
                    Ejecutivo = Filtro.Ejecutivo ?? "",
                    Usuario = Filtro.Usuario ?? ""
                };

                result.Result = connection.Query<AF_Liquidacion_Masiva>(
                    "spAFI_Renuncia_Liquidacion_Pendiente",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
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
        public ErrorDTO<List<DropDownListaGenericaModel>> AF_LiquidacionMasiva_Obtener_Causas(int CodEmpresa, string tipoApl = null, DateTime? inicio = null, DateTime? corte = null)
        {
            var result = new ErrorDTO<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query;
                object parameters = null;

                // Variante 1: solo causas activas
                if (tipoApl == null && inicio == null && corte == null)
                {
                    query = @"SELECT id_Causa AS item, Descripcion AS descripcion
                              FROM causas_renuncias
                              WHERE ACTIVO = 1";
                }
                // Variante 2: causas activas y filtro por tipoApl
                else if (inicio == null && corte == null)
                {
                    query = @"SELECT id_Causa AS item, Descripcion AS descripcion
                              FROM causas_renuncias
                              WHERE ACTIVO = 1
                                AND Tipo_Apl IN ('A', @TipoApl)";
                    parameters = new { TipoApl = tipoApl };
                }
                // Variante 3: causas activas, filtro por fechas y tipo
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

                result.Result = connection.Query<DropDownListaGenericaModel>(query, parameters).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Consulta las instituciones activas para dropdown.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> AF_LiquidacionMasiva_Obtener_Instituciones(int CodEmpresa)
        {
            var result = new ErrorDTO<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"SELECT cod_Institucion AS item, Descripcion AS descripcion
                                 FROM Instituciones
                                 WHERE ACTIVA = 1";

                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Ejecuta el proceso de liquidación masiva para una renuncia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="RenunciaId"></param>
        /// <param name="Usuario"></param>
        /// <param name="S06"></param>
        /// <returns></returns>
        public ErrorDTO AF_LiquidacionMasiva(int CodEmpresa, int RenunciaId, string Usuario, short S06 = 1)
        {
            var result = new ErrorDTO()
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new
                {
                    RenunciaId,
                    Usuario,
                    S06
                };

                connection.Execute(
                    "spAFI_Renuncia_Liquidacion_Procesa",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }
    }
}
