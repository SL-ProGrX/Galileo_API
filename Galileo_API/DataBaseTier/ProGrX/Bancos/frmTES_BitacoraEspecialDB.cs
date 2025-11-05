using PgxAPI.Models.TES;
using Dapper;
using PgxAPI.Models.ERROR;
using Microsoft.Data.SqlClient;
using System.Text;
using PgxAPI.Models;
using System.Drawing;

namespace PgxAPI.DataBaseTier.TES
{
    public class frmTES_BitacoraEspecialDB
    {
        private readonly IConfiguration? _config;

        public frmTES_BitacoraEspecialDB(IConfiguration? config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene las cuentas de la base de datos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de cuentas.</returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> TES_Bancos_Obtener(int codEmpresa)
        {
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                if (_config == null)
                    throw new ArgumentNullException(nameof(_config), "La configuración no puede ser nula.");

                string connectionString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                var sql = @"
                            SELECT 
                                id_Banco AS item,
                                RTRIM(Descripcion) AS descripcion
                            FROM Tes_Bancos
                            WHERE estado = 'A'";
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                response.Result = connection.Query<DropDownListaGenericaModel>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al obtener las cuentas: {ex.Message}";
                response.Result = null;
            }

            return response;
        }


        /// <summary>
        /// Obtiene los tipos de documentos de la base de datos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de tipos de documentos.</returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> TES_Tipos_Doc_Obtener(int codEmpresa)
        {
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                if (_config == null)
                    throw new ArgumentNullException(nameof(_config), "La configuración no puede ser nula.");

                string connectionString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                var sql = @"
                            SELECT 
                                TIPO as item,
                                RTRIM(DESCRIPCION) AS descripcion
                            FROM TES_TIPOS_DOC";

                using var connection = new SqlConnection(connectionString);

                response.Result = connection.Query<DropDownListaGenericaModel>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al obtener los tipos de documentos: {ex.Message}";
                response.Result = null;
            }

            return response;
        }


        /// <summary>
        /// Obtiene los tipos de movimientos de la base de datos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ErrorDTO<List<DropDownListaGenericaModel>> TES_Tipos_Movimientos_Obtener(int codEmpresa)
        {
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                if (_config == null)
                    throw new ArgumentNullException(nameof(_config), "La configuración no puede ser nula.");

                string connectionString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                var sql = @"
                            SELECT 
                                COD_MOVIMIENTO as item,
                                RTRIM(DESCRIPCION) AS descripcion
                            FROM TES_TIPOS_MOVIMIENTOS";

                using var connection = new SqlConnection(connectionString);

                response.Result = connection.Query<DropDownListaGenericaModel>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al obtener los tipos de movimientos: {ex.Message}";
                response.Result = null;
            }

            return response;
        }


        /// <summary>
        /// Busca en la base de datos según los filtros proporcionados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<List<BitacoraEspecialDTO>> BitacoraEspecial_Buscar(int codEmpresa, FiltrosBitacoraEspecial filtros)
        {
            var response = new ErrorDTO<List<BitacoraEspecialDTO>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<BitacoraEspecialDTO>()
            };

            try
            {
                if (_config == null)
                    throw new ArgumentNullException(nameof(_config), "La configuración no puede ser nula.");

                string connectionString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                var sql = new StringBuilder();
                sql.Append(@"
                            SELECT 
                                C.nsolicitud,
                                ISNULL(C.ndocumento,0) AS NDocumento,
                                C.Tipo,
                                C.monto,
                                CASE 
                                    WHEN C.estado IN ('I','E','T') THEN 'Emitido'
                                    WHEN C.estado = 'A' THEN 'Anulado'
                                    WHEN C.estado = 'P' THEN 'Pendiente'
                                END AS Estado,
                                H.FECHA,
                                M.DESCRIPCION,
                                H.DETALLE,
                                H.USUARIO,
                                H.revisado_usuario,
                                H.revisado_Fecha,
                                H.ID,
                                CASE 
                                    WHEN H.revisado_fecha IS NULL THEN 0 
                                    ELSE 1 
                                END AS Revisado
                            FROM Tes_Transacciones C
                            INNER JOIN Tes_Bancos B ON C.id_banco = B.id_Banco
                            INNER JOIN TES_HISTORIAL H ON C.NSOLICITUD = H.NSOLICITUD
                            INNER JOIN TES_TIPOS_MOVIMIENTOS M ON H.COD_MOVIMIENTO = M.COD_MOVIMIENTO
                            WHERE 1=1
                        ");

                var parameters = new DynamicParameters();

                // Lista de bancos
                if (filtros.cuentas?.Count > 0)
                {
                    sql.Append(" AND C.id_banco IN @Cuentas ");
                    parameters.Add("@Cuentas", filtros.cuentas.Select(x => x.item).ToList());
                }

                // Tipos de documento
                if (filtros.tipos_documento?.Count > 0)
                {
                    sql.Append(" AND C.Tipo IN @TiposDocumento ");
                    parameters.Add("@TiposDocumento", filtros.tipos_documento.Select(x => x.item).ToList());
                }

                // Tipos de movimientos
                if (filtros.movimientos?.Count > 0)
                {
                    sql.Append(" AND M.cod_movimiento IN @Movimientos ");
                    parameters.Add("@Movimientos", filtros.movimientos.Select(x => x.item).ToList());
                }

                // Fechas del movimiento (historial)
                if (filtros.chk_revision)
                {
                    sql.Append(" AND H.Revisado_fecha BETWEEN @MovFecInicio AND @MovFecCorte ");
                }
                else
                {
                    sql.Append(" AND H.Fecha BETWEEN @MovFecInicio AND @MovFecCorte ");
                }
                parameters.Add("@MovFecInicio", filtros.mov_fecha_inicio.Date);
                parameters.Add("@MovFecCorte", filtros.mov_fecha_corte.Date.AddDays(1).AddTicks(-1));

                // Usuario
                if (!string.IsNullOrWhiteSpace(filtros.usuario))
                {
                    if (filtros.chk_revision)
                    {
                        sql.Append(" AND H.Revisado_Usuario = @Usuario ");
                    }
                    else
                    {
                        sql.Append(" AND H.Usuario = @Usuario ");
                    }
                    parameters.Add("@Usuario", filtros.usuario);
                }

                // Tipo Fecha principal
                if (!string.IsNullOrEmpty(filtros.tipo_fecha))
                {
                    switch (filtros.tipo_fecha)
                    {
                        case "E":
                            sql.Append(" AND C.fecha_emision BETWEEN @FechaInicio AND @FechaCorte ");
                            break;
                        case "A":
                            sql.Append(" AND C.fecha_anula BETWEEN @FechaInicio AND @FechaCorte ");
                            break;
                        case "S":
                            sql.Append(" AND C.fecha_solicitud BETWEEN @FechaInicio AND @FechaCorte ");
                            break;
                    }
                    parameters.Add("@FechaInicio", filtros.fecha_inicio.Date);
                    parameters.Add("@FechaCorte", filtros.fecha_corte.Date.AddDays(1).AddTicks(-1));
                }

                // Estado
                if (!string.IsNullOrEmpty(filtros.estado))
                {
                    switch (filtros.estado)
                    {
                        case "E":
                            sql.Append(" AND C.estado IN ('I','T','E') ");
                            break;
                        case "A":
                            sql.Append(" AND C.estado = 'A' ");
                            break;
                        case "S":
                            sql.Append(" AND C.estado = 'P' ");
                            break;
                    }
                }

                // Revisión
                if (!string.IsNullOrEmpty(filtros.revision))
                {
                    switch (filtros.revision[0])
                    {
                        case 'P':
                            sql.Append(" AND H.Revisado_Fecha IS NULL ");
                            break;
                        case 'R':
                            sql.Append(" AND H.Revisado_Fecha IS NOT NULL ");
                            break;
                            // 'T' -> todos, no filtra
                    }
                }

                // Orden
                if (filtros.chk_revision)
                    sql.Append(" ORDER BY H.Revisado_fecha ");
                else
                    sql.Append(" ORDER BY H.Fecha ");

                using var connection = new SqlConnection(connectionString);
                var resultado = connection.Query<BitacoraEspecialDTO>(sql.ToString(), parameters);
                response.Result = resultado.ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al buscar bitácora especial: {ex.Message}";
                response.Result = null;
            }

            return response;
        }


        /// <summary>
        /// Actualiza el historial de una solicitud en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="id">ID del historial a actualizar.</param>
        /// <param name="usuario">Usuario que realiza la actualización.</param>
        /// <param name="nsolicitud">Número de solicitud asociado al historial.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDTO TES_Historial_Actualizar(int CodEmpresa, string id, string usuario, string nsolicitud)
        {
            if (_config == null)
            {
                throw new ArgumentNullException(nameof(_config), "Configuración es nula");
            }

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                // Verificar si el registro existe
                string querySelect = "SELECT 1 FROM TES_HISTORIAL WHERE id = @id AND nsolicitud = @nsolicitud";
                var exists = connection.QueryFirstOrDefault<int?>(querySelect, new { id, nsolicitud });

                if (exists == null)
                {
                    response.Code = -2;
                    response.Description = "Este registro no existe.";
                    return response;
                }

                // Actualizar los campos
                string queryUpdate = @"
                                    UPDATE TES_HISTORIAL 
                                    SET revisado_usuario = @usuario, revisado_fecha = @fecha 
                                    WHERE id = @id AND nsolicitud = @nsolicitud";

                connection.Execute(queryUpdate, new
                {
                    id,
                    nsolicitud,
                    usuario,
                    fecha = DateTime.Now
                });

                response.Description = "Revisión satisfactoria.";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

    }
}