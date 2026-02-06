using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSifTarjetasDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 10; // Modulo de Tesorer�a
        private readonly MSecurityMainDb _Security_MainDB;
        public FrmSifTarjetasDB(IConfiguration config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene una lista de tarjetas con paginaci�n y filtros (lazy loading).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SifTarjetasLista> SIF_TarjetasLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SifTarjetasLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new SifTarjetasLista()
                {
                    total = 0,
                    lista = new List<SifTarjetasData>()
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var search = filtros?.filtro?.Trim();
                string? searchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";

                var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                var sortOrder = filtros?.sortOrder ?? 0; // 0=DESC, 1=ASC

                var offset = filtros?.pagina ?? 0;
                var fetch = filtros?.paginacion ?? 0;
                if (fetch <= 0)
                {
                    fetch = int.MaxValue;
                }

                // Busco Total (mantiene comportamiento anterior: total sin filtro)
                const string totalQuery = @"SELECT COUNT(cod_tarjeta) FROM sif_tarjetas";
                result.Result.total = connection.Query<int>(totalQuery).FirstOrDefault();

                const string query = @"
                    SELECT cod_tarjeta, descripcion, activa
                    FROM sif_tarjetas
                    WHERE (@search IS NULL
                           OR cod_tarjeta LIKE @search
                           OR descripcion LIKE @search)
                    ORDER BY
                        -- ASC
                        CASE WHEN @sortOrder = 1 AND @sortField = 'cod_tarjeta' THEN cod_tarjeta END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'descripcion' THEN descripcion END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'activa' THEN CONVERT(int, activa) END ASC,

                        -- DESC
                        CASE WHEN @sortOrder = 0 AND @sortField = 'cod_tarjeta' THEN cod_tarjeta END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'descripcion' THEN descripcion END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'activa' THEN CONVERT(int, activa) END DESC,

                        -- Fallback determinístico
                        cod_tarjeta ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

                result.Result.lista = connection.Query<SifTarjetasData>(query, new
                {
                    search = searchLike,
                    sortField,
                    sortOrder,
                    offset,
                    fetch
                }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = null;
            }
            return result;
        }

        /// <summary>
        /// Obtiene una lista de tarjetas con filtros aplicados (sin paginaci�n).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SifTarjetasData>> SIF_Tarjetas_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SifTarjetasData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifTarjetasData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var search = filtros?.filtro?.Trim();
                string? searchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";

                const string query = @"SELECT cod_tarjeta, descripcion, activa
                                FROM sif_tarjetas
                                WHERE (@search IS NULL
                                       OR cod_tarjeta LIKE @search
                                       OR descripcion LIKE @search)
                                ORDER BY cod_tarjeta";

                result.Result = connection.Query<SifTarjetasData>(query, new { search = searchLike }).ToList();
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
        /// Inserta o actualiza una tarjeta.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tarjeta"></param>
        /// <returns></returns>
        public ErrorDto SIF_Tarjetas_Guardar(int CodEmpresa, string usuario, SifTarjetasData tarjeta)
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

                // Validar si existe la tarjeta
                var queryExiste = @"SELECT COUNT(*) FROM sif_tarjetas WHERE UPPER(cod_tarjeta) = @cod_tarjeta";
                var codTarjetaUpper = tarjeta?.cod_tarjeta?.ToUpper() ?? string.Empty;
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { cod_tarjeta = codTarjetaUpper });

                if (existe == 0)
                {
                    if (tarjeta == null)
                    {
                        result.Code = -2;
                        result.Description = "El objeto tarjeta no puede ser nulo.";
                    }
                    else
                    {
                        result = SIF_Tarjetas_Insertar(connection, CodEmpresa, usuario, tarjeta);
                    }
                }
                else
                {
                    if (tarjeta == null)
                    {
                        result.Code = -2;
                        result.Description = "El objeto tarjeta no puede ser nulo.";
                    }
                    else
                    {
                        result = SIF_Tarjetas_Actualizar(connection, CodEmpresa, usuario, tarjeta);
                    }
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
        /// Inserta una nueva tarjeta y registra en bit�cora.
        /// </summary>
        private ErrorDto SIF_Tarjetas_Insertar(SqlConnection connection, int CodEmpresa, string usuario, SifTarjetasData tarjeta)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Tarjeta registrada correctamente."
            };
            try
            {
                var queryInsert = @"INSERT INTO sif_tarjetas (cod_tarjeta, descripcion, activa, registro_usuario, registro_fecha)
                                    VALUES (@cod_tarjeta, @descripcion, @activa, @registro_usuario, GETDATE())";
                connection.Execute(queryInsert, new
                {
                    cod_tarjeta = tarjeta.cod_tarjeta?.ToUpper() ?? string.Empty,
                    tarjeta.descripcion,
                    tarjeta.activa,
                    registro_usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Mantenimiento Tarjetas: {tarjeta.cod_tarjeta} - {tarjeta.descripcion}",
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
        /// Actualiza una tarjeta existente y registra en bit�cora.
        /// </summary>
        private ErrorDto SIF_Tarjetas_Actualizar(SqlConnection connection, int CodEmpresa, string usuario, SifTarjetasData tarjeta)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Tarjeta actualizada correctamente."
            };
            try
            {
                var queryUpdate = @"UPDATE sif_tarjetas
                                    SET descripcion = @descripcion,
                                        activa = @activa
                                    WHERE UPPER(cod_tarjeta) = @cod_tarjeta";
                connection.Execute(queryUpdate, new
                {
                    cod_tarjeta = tarjeta.cod_tarjeta?.ToUpper() ?? string.Empty,
                    tarjeta.descripcion,
                    tarjeta.activa
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Mantenimiento Tarjetas: {tarjeta.cod_tarjeta} - {tarjeta.descripcion}",
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
        /// Elimina una tarjeta por su c�digo y registra en bit�cora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_tarjeta"></param>
        /// <returns></returns>
        public ErrorDto SIF_Tarjetas_Eliminar(int CodEmpresa, string usuario, string cod_tarjeta)
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

                // Verifica que exista la tarjeta antes de eliminar
                var queryExiste = @"SELECT COUNT(*) FROM sif_tarjetas WHERE UPPER(cod_tarjeta) = @cod_tarjeta";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { cod_tarjeta = cod_tarjeta.ToUpper() });

                if (existe == 0)
                {
                    result.Code = -2;
                    result.Description = $"La tarjeta con el c�digo {cod_tarjeta} no existe.";
                    return result;
                }

                var queryDelete = @"DELETE FROM sif_tarjetas WHERE UPPER(cod_tarjeta) = @cod_tarjeta";
                connection.Execute(queryDelete, new { cod_tarjeta = cod_tarjeta.ToUpper() });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Mantenimiento Tarjetas: {cod_tarjeta}",
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
        /// Valida si un c�digo o descripci�n de tarjeta ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tarjeta"></param>
        /// <returns></returns>
        public ErrorDto SIF_Tarjetas_Valida(int CodEmpresa, SifTarjetasData tarjeta)
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
                    var query = @"SELECT COUNT(*) FROM sif_tarjetas 
                                  WHERE UPPER(cod_tarjeta) = @cod_tarjeta
                                     OR UPPER(descripcion) = @descripcion";
                    var existe = connection.QueryFirstOrDefault<int>(query, new
                    {
                        cod_tarjeta = tarjeta.cod_tarjeta?.ToUpper() ?? string.Empty,
                        descripcion = tarjeta.descripcion?.ToUpper() ?? string.Empty
                    });

                    if (existe > 0)
                    {
                        result.Code = -1;
                        result.Description = "Ya existe una tarjeta con ese c�digo o descripci�n.";
                    }
                    else
                    {
                        result.Code = 0;
                        result.Description = "El c�digo y la descripci�n de tarjeta son v�lidos.";
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
        /// Obtiene la lista de emisores y su asignaci�n para una tarjeta.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_tarjeta"></param>
        /// <returns></returns>
        public ErrorDto<List<SifEmisoresAsignadosData>> SIF_TarjetasEmisores_Obtener(int CodEmpresa, string cod_tarjeta)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SifEmisoresAsignadosData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifEmisoresAsignadosData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"SELECT E.cod_emisor AS Codigo, E.descripcion, X.cod_emisor AS Asignado
                              FROM sif_emisores E
                              LEFT JOIN sif_emisores_tarjetas X ON E.cod_emisor = X.cod_emisor
                                AND X.cod_tarjeta = @cod_tarjeta
                              ORDER BY X.cod_emisor DESC, E.cod_emisor";
                result.Result = connection.Query<SifEmisoresAsignadosData>(query, new { cod_tarjeta }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }
    }
}