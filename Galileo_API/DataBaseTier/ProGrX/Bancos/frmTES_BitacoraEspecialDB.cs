using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.Data.SqlClient;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using System.Text;

namespace Galileo_API.DataBaseTier.TES
{
    public class FrmTesBitacoraEspecialDB
    {
        private readonly PortalDB _portalDB;

        public FrmTesBitacoraEspecialDB(IConfiguration? config)
        {
            _portalDB = new PortalDB(config!);
        }

        /// <summary>
        /// Obtiene las cuentas de la base de datos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de cuentas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string query = @"
                            SELECT 
                                id_Banco AS item,
                                RTRIM(Descripcion) AS descripcion
                            FROM Tes_Bancos
                            WHERE estado = 'A'";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }


        /// <summary>
        /// Obtiene los tipos de documentos de la base de datos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de tipos de documentos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Tipos_Doc_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string query = @"
                            SELECT 
                                TIPO as item,
                                RTRIM(DESCRIPCION) AS descripcion
                            FROM TES_TIPOS_DOC";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }


        /// <summary>
        /// Obtiene los tipos de movimientos de la base de datos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Tipos_Movimientos_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string query = @"
                            SELECT 
                                COD_MOVIMIENTO as item,
                                RTRIM(DESCRIPCION) AS descripcion
                            FROM TES_TIPOS_MOVIMIENTOS";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }


        /// <summary>
        /// Busca en la base de datos según los filtros proporcionados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<BitacoraEspecialDto>> BitacoraEspecial_Buscar(int codEmpresa, FiltrosBitacoraEspecial filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var filtroCuentas = filtros.cuentas?.Count > 0
                    ? " AND C.id_banco IN @Cuentas "
                    : string.Empty;
                

                var filtroTiposDocumento = filtros.tipos_documento?.Count > 0
                    ? " AND C.Tipo IN @TiposDocumento "
                    : string.Empty;
               

                var filtroMovimientos = filtros.movimientos?.Count > 0
                    ? " AND M.cod_movimiento IN @Movimientos "
                    : string.Empty;
                

                var usarRevision = filtros.chk_revision;

                var filtroFechaHistorial = usarRevision
                    ? " AND H.Revisado_fecha BETWEEN @MovFecInicio AND @MovFecCorte "
                    : " AND H.Fecha BETWEEN @MovFecInicio AND @MovFecCorte ";

                var campoUsuario = usarRevision
                        ? "H.Revisado_Usuario"
                        : "H.Usuario";

                var filtroUsuario = string.IsNullOrWhiteSpace(filtros.usuario)
                    ? string.Empty
                    : $" AND {campoUsuario} = @Usuario ";

                var parameters = FiltrosWhereTextos(filtros);

                // Tipo Fecha (sin if para construir string; solo if para parámetros cuando aplique)
                var filtroTipoFecha = (filtros.tipo_fecha ?? string.Empty) switch
                {
                    "E" => " AND C.fecha_emision BETWEEN @FechaInicio AND @FechaCorte ",
                    "A" => " AND C.fecha_anula BETWEEN @FechaInicio AND @FechaCorte ",
                    "S" => " AND C.fecha_solicitud BETWEEN @FechaInicio AND @FechaCorte ",
                    _ => string.Empty
                };
                if (!string.IsNullOrEmpty(filtroTipoFecha))
                {
                    parameters.Add("@FechaInicio", filtros.fecha_inicio!.Value);
                    parameters.Add("@FechaCorte", filtros.fecha_corte!.Value.AddDays(1).AddTicks(-1));
                }


                var filtroEstado = (filtros.estado ?? string.Empty) switch
                {
                    "E" => " AND C.estado IN ('I','T','E') ",
                    "A" => " AND C.estado = 'A' ",
                    "S" => " AND C.estado = 'P' ",
                    _ => string.Empty
                };

                // Revisión (SIN if extra)
                var revChar = string.IsNullOrEmpty(filtros.revision) ? '\0' : filtros.revision[0];
                var filtroRevision = revChar switch
                {
                    'P' => " AND H.Revisado_Fecha IS NULL ",
                    'R' => " AND H.Revisado_Fecha IS NOT NULL ",
                    _ => string.Empty
                };

                var orderBy = usarRevision
                    ? " ORDER BY H.Revisado_fecha "
                    : " ORDER BY H.Fecha ";

                var sql = new StringBuilder();
                sql.Append($@"
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
                            CASE WHEN H.revisado_fecha IS NULL THEN 0 ELSE 1 END AS Revisado
                        FROM Tes_Transacciones C
                        INNER JOIN Tes_Bancos B ON C.id_banco = B.id_Banco
                        INNER JOIN TES_HISTORIAL H ON C.NSOLICITUD = H.NSOLICITUD
                        INNER JOIN TES_TIPOS_MOVIMIENTOS M ON H.COD_MOVIMIENTO = M.COD_MOVIMIENTO
                        WHERE 1 = 1
                       ");
                string where = @$"{ filtroCuentas }
                        { filtroTiposDocumento}
                { filtroMovimientos}
                { filtroFechaHistorial}
                { filtroUsuario}
                { filtroTipoFecha}
                { filtroEstado}
                { filtroRevision}
                { orderBy}";
                sql.Append(where);

                var resultado = conn.Query<BitacoraEspecialDto>(sql.ToString(),parameters).ToList();
                return DbHelper.CreateOkResponse(resultado);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<BitacoraEspecialDto>>(ex.Message);
            }
        }

        public static DynamicParameters FiltrosWhereTextos(FiltrosBitacoraEspecial filtros)
        {
            try
            {
                var parameters = new DynamicParameters();
                if (filtros.cuentas?.Count > 0)
                    parameters.Add("@Cuentas", filtros.cuentas.Select(x => x.item).ToList());

                if (filtros.tipos_documento?.Count > 0)
                    parameters.Add("@TiposDocumento", filtros.tipos_documento.Select(x => x.item).ToList());

                if (filtros.movimientos?.Count > 0)
                    parameters.Add("@Movimientos", filtros.movimientos.Select(x => x.item).ToList());

                parameters.Add("@MovFecInicio", filtros.mov_fecha_inicio!.Value);
                parameters.Add("@MovFecCorte", filtros.mov_fecha_corte!.Value.AddDays(1).AddTicks(-1));

                if (!string.IsNullOrWhiteSpace(filtros.usuario))
                    parameters.Add("@Usuario", filtros.usuario);

                

                return parameters;
            }
            catch (Exception)
            {
                return new DynamicParameters();
            }
            
        }

        /// <summary>
        /// Actualiza el historial de una solicitud en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="id">ID del historial a actualizar.</param>
        /// <param name="usuario">Usuario que realiza la actualización.</param>
        /// <param name="nsolicitud">Número de solicitud asociado al historial.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto TES_Historial_Actualizar(int CodEmpresa, string id, string usuario, string nsolicitud)
        {


            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
               
                // Verificar si el registro existe
                string querySelect = "SELECT 1 FROM TES_HISTORIAL WHERE id = @id AND nsolicitud = @nsolicitud";
                var exists = conn.QueryFirstOrDefault<int?>(querySelect, new { id, nsolicitud });

                if (exists == null)
                {
                    return DbHelper.ErrorResponse("Este registro no existe.", -2);
                }

                // Actualizar los campos
                string queryUpdate = @"
                                    UPDATE TES_HISTORIAL 
                                    SET revisado_usuario = @usuario, revisado_fecha = @fecha 
                                    WHERE id = @id AND nsolicitud = @nsolicitud";

                conn.Execute(queryUpdate, new
                {
                    id,
                    nsolicitud,
                    usuario,
                    fecha = DateTime.Now
                });

                return DbHelper.OkResponse("Revisión satisfactoria.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al actualizar el historial: {ex.Message}", -1);
            }
        }

    }
}