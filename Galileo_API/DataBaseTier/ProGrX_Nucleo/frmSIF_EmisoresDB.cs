using Dapper;
using Microsoft.Data.SqlClient;
using System.Text;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSifEmisoresDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 10; // Modulo de Tesorer�a
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmSifEmisoresDB(IConfiguration config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene una lista de emisores con paginaci�n y filtros (lazy loading).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SifEmisoresLista> SIF_EmisoresLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SifEmisoresLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new SifEmisoresLista()
                {
                    total = 0,
                    lista = new List<SifEmisoresData>()
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var p = new DynamicParameters();

                // Filtro
                bool hasFilter = TryAddEmisoresFiltro(filtros, p);

                // Total
                const string countNoFilter = "SELECT COUNT(cod_emisor) FROM sif_emisores;";
                const string countWithFilter = @"SELECT COUNT(cod_emisor)
                                                FROM sif_emisores
                                                WHERE (cod_emisor LIKE @Q OR descripcion LIKE @Q);";

                result.Result.total = connection.ExecuteScalar<int>(hasFilter ? countWithFilter : countNoFilter, p);

                // Sorting (sin SQL dinámico)
                string sortField = NormalizeEmisoresSortField(filtros?.sortField);
                int sortDir = (filtros?.sortOrder ?? 1) == 0 ? 0 : 1; // 1=ASC, 0=DESC
                p.Add("@SORTFIELD", sortField);
                p.Add("@SORTDIR", sortDir);

                // Paginación
                int offset = Math.Max(0, filtros?.pagina ?? 0);
                int fetch = Math.Max(1, filtros?.paginacion ?? 30);
                p.Add("@OFFSET", offset);
                p.Add("@FETCH", fetch);

                var sb = new StringBuilder();
                sb.Append(@"SELECT cod_emisor, descripcion, activo
FROM sif_emisores");

                if (hasFilter)
                {
                    sb.Append(" WHERE (cod_emisor LIKE @Q OR descripcion LIKE @Q) ");
                }

                sb.Append(@"
 ORDER BY
    CASE WHEN @SORTFIELD = 'COD_EMISOR'   AND @SORTDIR = 1 THEN cod_emisor END ASC,
    CASE WHEN @SORTFIELD = 'COD_EMISOR'   AND @SORTDIR = 0 THEN cod_emisor END DESC,
    CASE WHEN @SORTFIELD = 'DESCRIPCION'  AND @SORTDIR = 1 THEN descripcion END ASC,
    CASE WHEN @SORTFIELD = 'DESCRIPCION'  AND @SORTDIR = 0 THEN descripcion END DESC,
    CASE WHEN @SORTFIELD = 'ACTIVO'       AND @SORTDIR = 1 THEN activo END ASC,
    CASE WHEN @SORTFIELD = 'ACTIVO'       AND @SORTDIR = 0 THEN activo END DESC,
    cod_emisor ASC");

                sb.Append(" OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY ");

                result.Result.lista = connection.Query<SifEmisoresData>(sb.ToString(), p).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<SifEmisoresData>();
            }
            return result;
        }

        /// <summary>
        /// Obtiene una lista de emisores con filtros aplicados (sin paginaci�n).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SifEmisoresData>> SIF_Emisores_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SifEmisoresData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifEmisoresData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var p = new DynamicParameters();
                bool hasFilter = TryAddEmisoresFiltro(filtros, p);

                var sb = new StringBuilder();
                sb.Append(@"SELECT cod_emisor, descripcion, activo
FROM sif_emisores");

                if (hasFilter)
                {
                    sb.Append(" WHERE (cod_emisor LIKE @Q OR descripcion LIKE @Q) ");
                }

                sb.Append(" ORDER BY cod_emisor");

                result.Result = connection.Query<SifEmisoresData>(sb.ToString(), p).ToList();
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
        /// Inserta o actualiza un emisor.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="emisor"></param>
        /// <returns></returns>
        public ErrorDto SIF_Emisores_Guardar(int CodEmpresa, string usuario, SifEmisoresData emisor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);

                // Validar si existe el emisor
                var queryExiste = @"SELECT COUNT(*) FROM sif_emisores WHERE UPPER(cod_emisor) = @cod_emisor";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { cod_emisor = emisor.cod_emisor != null ? emisor.cod_emisor.ToUpper() : string.Empty });

                if (existe == 0)
                {
                    result = SIF_Emisores_Insertar(connection, CodEmpresa, usuario, emisor);
                }
                else
                {
                    result = SIF_Emisores_Actualizar(connection, CodEmpresa, usuario, emisor);
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Inserta un nuevo emisor y registra en bit�cora.
        /// </summary>
        private ErrorDto SIF_Emisores_Insertar(SqlConnection connection, int CodEmpresa, string usuario, SifEmisoresData emisor)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Emisor registrado correctamente."
            };
            try
            {
                var queryInsert = @"INSERT INTO sif_emisores (cod_emisor, descripcion, activo, registro_usuario, registro_fecha)
                                    VALUES (@cod_emisor, @descripcion, @activo, @registro_usuario, GETDATE())";
                connection.Execute(queryInsert, new
                {
                    cod_emisor = emisor.cod_emisor != null ? emisor.cod_emisor.ToUpper() : string.Empty,
                    emisor.descripcion,
                    emisor.activo,
                    registro_usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Mantenimiento Emisores: {emisor.cod_emisor} - {emisor.descripcion}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Actualiza un emisor existente y registra en bit�cora.
        /// </summary>
        private ErrorDto SIF_Emisores_Actualizar(SqlConnection connection, int CodEmpresa, string usuario, SifEmisoresData emisor)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Emisor actualizado correctamente."
            };
            try
            {
                var queryUpdate = @"UPDATE sif_emisores
                                    SET descripcion = @descripcion,
                                        activo = @activo
                                    WHERE UPPER(cod_emisor) = @cod_emisor";
                connection.Execute(queryUpdate, new
                {
                    cod_emisor = emisor.cod_emisor != null ? emisor.cod_emisor.ToUpper() : string.Empty,
                    emisor.descripcion,
                    emisor.activo
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Mantenimiento Emisores: {emisor.cod_emisor} - {emisor.descripcion}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Valida si un c�digo o descripci�n de emisor ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="emisor"></param>
        /// <returns></returns>
        public ErrorDto SIF_Emisores_Valida(int CodEmpresa, SifEmisoresData emisor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);

                const string query = @"SELECT COUNT(*) FROM sif_emisores
WHERE UPPER(cod_emisor) = @cod_emisor
   OR UPPER(descripcion) = @descripcion";

                int existe = connection.QueryFirstOrDefault<int>(query, new
                {
                    cod_emisor = (emisor.cod_emisor ?? string.Empty).ToUpper(),
                    descripcion = (emisor.descripcion ?? string.Empty).ToUpper()
                });

                if (existe > 0)
                {
                    result.Code = -1;
                    result.Description = "Ya existe un emisor con ese código o descripción.";
                }
                else
                {
                    result.Code = 0;
                    result.Description = "El código y la descripción de emisor son válidos.";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Elimina un emisor por su c�digo y registra en bit�cora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_emisor"></param>
        /// <returns></returns>
        public ErrorDto SIF_Emisores_Eliminar(int CodEmpresa, string usuario, string cod_emisor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);

                // Verifica que exista el emisor antes de eliminar
                var queryExiste = @"SELECT COUNT(*) FROM sif_emisores WHERE UPPER(cod_emisor) = @cod_emisor";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { cod_emisor = cod_emisor.ToUpper() });

                if (existe == 0)
                {
                    result.Code = -2;
                    result.Description = $"El emisor con el c�digo {cod_emisor} no existe.";
                    return result;
                }

                var queryDelete = @"DELETE FROM sif_emisores WHERE UPPER(cod_emisor) = @cod_emisor";
                connection.Execute(queryDelete, new { cod_emisor = cod_emisor.ToUpper() });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Mantenimiento Emisores: {cod_emisor}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Obtiene la lista de tarjetas y su asignaci�n para un emisor.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_emisor"></param>
        /// <returns></returns>
        public ErrorDto<List<SifTarjetasAsignadasData>> SIF_EmisoresTarjetas_Obtener(int CodEmpresa, string cod_emisor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SifTarjetasAsignadasData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifTarjetasAsignadasData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"SELECT E.cod_Tarjeta AS Codigo, E.descripcion, X.cod_Tarjeta AS Asignado
                      FROM sif_Tarjetas E
                      LEFT JOIN sif_emisores_tarjetas X ON E.cod_Tarjeta = X.cod_Tarjeta
                        AND X.cod_Emisor = @cod_emisor
                      ORDER BY X.cod_Tarjeta DESC, E.cod_Tarjeta";
                result.Result = connection.Query<SifTarjetasAsignadasData>(query, new { cod_emisor }).ToList();
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
        /// Asigna una tarjeta a un emisor.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="asignacion"></param>
        /// <returns></returns>
        public ErrorDto SIF_EmisorTarjeta_Asignar(int CodEmpresa, string usuario, SifEmisorTarjetaData asignacion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"INSERT INTO sif_emisores_tarjetas (cod_emisor, cod_tarjeta, registro_usuario, registro_fecha)
                              VALUES (@cod_emisor, @cod_tarjeta, @registro_usuario, dbo.MyGetdate())";
                connection.Execute(query, new
                {
                    cod_emisor = asignacion.cod_emisor,
                    cod_tarjeta = asignacion.cod_tarjeta,
                    registro_usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Asignaci�n tarjeta: {asignacion.cod_tarjeta} al emisor: {asignacion.cod_emisor}",
                    Movimiento = "Asigna Tarjeta - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Desasigna (elimina) una tarjeta de un emisor.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="asignacion"></param>
        /// <returns></returns>
        public ErrorDto SIF_EmisorTarjeta_Desasignar(int CodEmpresa, string usuario, SifEmisorTarjetaData asignacion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"DELETE FROM sif_emisores_tarjetas WHERE cod_tarjeta = @cod_tarjeta AND cod_emisor = @cod_emisor";
                connection.Execute(query, new
                {
                    cod_emisor = asignacion.cod_emisor,
                    cod_tarjeta = asignacion.cod_tarjeta
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Desasignaci�n tarjeta: {asignacion.cod_tarjeta} del emisor: {asignacion.cod_emisor}",
                    Movimiento = "Desasigna Tarjeta - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        private static bool TryAddEmisoresFiltro(FiltrosLazyLoadData filtros, DynamicParameters p)
        {
            string q = (filtros?.filtro ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(q))
                return false;

            p.Add("@Q", $"%{q}%");
            return true;
        }

        private static string NormalizeEmisoresSortField(string? sortField)
        {
            string sf = (sortField ?? string.Empty).Trim().ToUpperInvariant();

            return sf switch
            {
                "COD_EMISOR" => "COD_EMISOR",
                "DESCRIPCION" => "DESCRIPCION",
                "ACTIVO" => "ACTIVO",
                _ => "COD_EMISOR"
            };
        }
    }
}