using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Nucleo;
using PgxAPI.Models;

namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSIF_TarjetasDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 10; // Modulo de Tesorería
        private readonly mSecurityMainDb _Security_MainDB;

        public frmSIF_TarjetasDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene una lista de tarjetas con paginación y filtros (lazy loading).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<SifTarjetasLista> SIF_TarjetasLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<SifTarjetasLista>()
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
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    // Busco Total
                    query = $@"SELECT COUNT(cod_tarjeta) FROM sif_tarjetas";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (!string.IsNullOrEmpty(filtros.filtro))
                    {
                        filtros.filtro = " WHERE ( cod_tarjeta LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (string.IsNullOrEmpty(filtros.sortField))
                    {
                        filtros.sortField = "cod_tarjeta";
                    }

                    query = $@"SELECT cod_tarjeta, descripcion, activa
                                FROM sif_tarjetas
                                {filtros.filtro}
                                ORDER BY {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                OFFSET {filtros.pagina} ROWS 
                                FETCH NEXT {filtros.paginacion} ROWS ONLY ";

                    result.Result.lista = connection.Query<SifTarjetasData>(query).ToList();
                }
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
        /// Obtiene una lista de tarjetas con filtros aplicados (sin paginación).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<List<SifTarjetasData>> SIF_Tarjetas_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<List<SifTarjetasData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifTarjetasData>()
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    if (!string.IsNullOrEmpty(filtros.filtro))
                    {
                        filtros.filtro = " WHERE ( cod_tarjeta LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' ) ";
                    }
                    query = $@"SELECT cod_tarjeta, descripcion, activa
                                FROM sif_tarjetas
                                {filtros.filtro}
                                ORDER BY cod_tarjeta";
                    result.Result = connection.Query<SifTarjetasData>(query).ToList();
                }
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
        public ErrorDTO SIF_Tarjetas_Guardar(int CodEmpresa, string usuario, SifTarjetasData tarjeta)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);

                // Validar si existe la tarjeta
                var queryExiste = @"SELECT COUNT(*) FROM sif_tarjetas WHERE UPPER(cod_tarjeta) = @cod_tarjeta";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { cod_tarjeta = tarjeta.cod_tarjeta.ToUpper() });

                if (existe == 0)
                {
                    result = SIF_Tarjetas_Insertar(connection, CodEmpresa, usuario, tarjeta);
                }
                else
                {
                    result = SIF_Tarjetas_Actualizar(connection, CodEmpresa, usuario, tarjeta);
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
        /// Inserta una nueva tarjeta y registra en bitácora.
        /// </summary>
        private ErrorDTO SIF_Tarjetas_Insertar(SqlConnection connection, int CodEmpresa, string usuario, SifTarjetasData tarjeta)
        {
            var result = new ErrorDTO()
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
                    cod_tarjeta = tarjeta.cod_tarjeta.ToUpper(),
                    tarjeta.descripcion,
                    tarjeta.activa,
                    registro_usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
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
        /// Actualiza una tarjeta existente y registra en bitácora.
        /// </summary>
        private ErrorDTO SIF_Tarjetas_Actualizar(SqlConnection connection, int CodEmpresa, string usuario, SifTarjetasData tarjeta)
        {
            var result = new ErrorDTO()
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
                    cod_tarjeta = tarjeta.cod_tarjeta.ToUpper(),
                    tarjeta.descripcion,
                    tarjeta.activa
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
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
        /// Elimina una tarjeta por su código y registra en bitácora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_tarjeta"></param>
        /// <returns></returns>
        public ErrorDTO SIF_Tarjetas_Eliminar(int CodEmpresa, string usuario, string cod_tarjeta)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO()
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
                    result.Description = $"La tarjeta con el código {cod_tarjeta} no existe.";
                    return result;
                }

                var queryDelete = @"DELETE FROM sif_tarjetas WHERE UPPER(cod_tarjeta) = @cod_tarjeta";
                connection.Execute(queryDelete, new { cod_tarjeta = cod_tarjeta.ToUpper() });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
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
        /// Valida si un código o descripción de tarjeta ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tarjeta"></param>
        /// <returns></returns>
        public ErrorDTO SIF_Tarjetas_Valida(int CodEmpresa, SifTarjetasData tarjeta)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"SELECT COUNT(*) FROM sif_tarjetas 
                                  WHERE UPPER(cod_tarjeta) = @cod_tarjeta
                                     OR UPPER(descripcion) = @descripcion";
                    var existe = connection.QueryFirstOrDefault<int>(query, new
                    {
                        cod_tarjeta = tarjeta.cod_tarjeta.ToUpper(),
                        descripcion = tarjeta.descripcion.ToUpper()
                    });

                    if (existe > 0)
                    {
                        result.Code = -1;
                        result.Description = "Ya existe una tarjeta con ese código o descripción.";
                    }
                    else
                    {
                        result.Code = 0;
                        result.Description = "El código y la descripción de tarjeta son válidos.";
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
        /// Obtiene la lista de emisores y su asignación para una tarjeta.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_tarjeta"></param>
        /// <returns></returns>
        public ErrorDTO<List<SifEmisoresAsignadosData>> SIF_TarjetasEmisores_Obtener(int CodEmpresa, string cod_tarjeta)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<List<SifEmisoresAsignadosData>>()
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