using System.Data;
using System.Text;
using Dapper;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier
{
    public class FrmCprSolicitudCotizaValoraDB
    {
        private readonly PortalDB _portalDb;
        private readonly EnvioCorreoDB _envioCorreoDB;
        private readonly FrmCprSolicitudDB _frmCprSolicitud;
        private readonly IConfiguration _config;

        private const string PlantillaCotizacion = "CPR_COTIZ01";
        private const string PlantillaTerminos = "CPR_TERM01";
        private readonly string _notificaciones;

        private const string DefaultErrorMsg = "Error";
        private const string EstadoVigente = "V";
        private const string EstadoEnviado = "E";

        private sealed record CorreoData(CprProveedorDto Proveedor, List<CprSolicitudBsDto> Productos, string Recepcion, string Multa, bool Terminos, List<string> Anexos, string LugarEntrega);
        private sealed record InvitarData(int CprId, int ProveedorCodigo, string Usuario);
        private sealed record ProvPuntajeRow(int PROVEEDOR_CODIGO, decimal PUNTAJE);
        private sealed record ProvPresenciaTablas(HashSet<int> ConValora, HashSet<int> ConCotiza, HashSet<int> ConBs);
        private sealed class SolicitudCorreoFila
        {
            public DateTime? recepcion_ofertas { get; set; }
            public string? int_sitio_entrega { get; set; }
            public decimal? porc_multa { get; set; }
            public int? terminos_condiciones { get; set; }
        }

        public FrmCprSolicitudCotizaValoraDB(IConfiguration config)
        {
            _config = config;
            _portalDb = new PortalDB(config);
            _envioCorreoDB = new EnvioCorreoDB(config);
            _frmCprSolicitud = new FrmCprSolicitudDB(config);

            _notificaciones = config.GetSection("AppSettings:Notificaciones").Value ?? string.Empty;
        }

        public ErrorDto<List<CprValoracionLista>> CprSolicitudProveedoresLista_Obtener(int codEmpresa, int consulta, int cpr_id)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Query<CprValoracionLista>(
                    "spCPR_SolicitudProveedores_Obtener",
                    new { compra = consulta, cpr_id = cpr_id },
                    commandType: CommandType.StoredProcedure
                ).ToList()
            );
        }

        private sealed record InvitarTxResult(bool Ok, string? Msg, bool EsCompraDirecta, CprSolicitudDto? Solicitud);

        public ErrorDto CprSolicitudProveedor_Invitar(int codEmpresa, CprSolicitudProvDto proveedor)
        {
            var dataR = ParseInvitarData(proveedor);
            if (dataR.Code != 0 || dataR.Result == null)
                return DbHelper.ErrorResponse(dataR.Description ?? "Datos inválidos", dataR.Code ?? -1);

            var tipoExcepcion = _frmCprSolicitud.CprSolicitud_TipoExcepcion(codEmpresa).Description ?? string.Empty;

            // ✅ OJO: WithConn devuelve ErrorDto<InvitarTxResult>
            var txR = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                InvitarProveedorEnTx(conn, dataR.Result, tipoExcepcion)
            );

            // Error real (SQL/Exception capturada por helper)
            if (txR.Code != 0 || txR.Result == null)
                return DbHelper.ErrorResponse(txR.Description ?? "Error", txR.Code ?? -1);

            // Error de negocio (sin excepciones)
            if (!txR.Result.Ok)
                return DbHelper.ErrorResponse(txR.Result.Msg ?? "Error", -1);

            // Post-proceso compra directa
            if (txR.Result.EsCompraDirecta && txR.Result.Solicitud != null)
                return _frmCprSolicitud.CompraDirectaProv_Agregar(codEmpresa, dataR.Result.CprId, txR.Result.Solicitud);

            return DbHelper.CreateOkResponse();
        }

        private static InvitarTxResult InvitarProveedorEnTx(SqlConnection conn, InvitarData data, string tipoExcepcion)
        {
            if (conn.State != ConnectionState.Open) conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                var solicitud = ObtenerSolicitud(conn, tx, data.CprId);
                if (solicitud == null)
                {
                    tx.Rollback();
                    return new InvitarTxResult(false, "Solicitud no encontrada", false, null);
                }

                var esCompraDirecta = !string.IsNullOrEmpty(tipoExcepcion) && solicitud.tipo_orden == tipoExcepcion;

                if (esCompraDirecta)
                {
                    var count = ContarProveedores(conn, tx, data.CprId);
                    if (count >= 1)
                    {
                        tx.Rollback();
                        return new InvitarTxResult(false, "Una compra directa no permite mas de un proveedor", true, solicitud);
                    }
                }

                InvitarProveedorSp(conn, tx, data);
                tx.Commit();
                return new InvitarTxResult(true, "OK", esCompraDirecta, solicitud);
            }
            catch (Exception ex)
            {
                tx.Rollback();
                // ✅ Esto sí es correcto: error REAL se propaga al helper
                throw new InvalidOperationException(ex.Message, ex);
            }
        }
        
        public ErrorDto CprSolicitudProveedor_Eliminar(int codEmpresa, int proveedor_codigo, int cpr_id)
        {
            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                @"DELETE FROM CPR_SOLICITUD_PROV
                  WHERE PROVEEDOR_CODIGO = @Proveedor
                    AND CPR_ID = @CprId",
                new { Proveedor = proveedor_codigo, CprId = cpr_id }
            );
        }

        /// <summary>
        /// Obtiene la lista de proveedores invitados para una solicitud CPR.
        /// El estado y el puntaje se derivan dinámicamente de las tablas de proceso
        /// para reflejar siempre la fase real: Registrada, Tránsito o Valorada.
        /// </summary>
        public ErrorDto<List<CprSolicitudProvDto>> CprSolicitudProvInvitados_Obtener(int codEmpresa, int cpr_id)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var lista = conn.Query<CprSolicitudProvDto>(
                    "spCPR_SolicitudProvInvitados_Obtener",
                    new { cpr_id },
                    commandType: CommandType.StoredProcedure
                ).ToList();

                var puntajes = conn.Query<ProvPuntajeRow>(
                    @"SELECT PROVEEDOR_CODIGO,
                             ISNULL(SUM(PUNTAJE), 0) AS PUNTAJE
                      FROM   CPR_SOLICITUD_PROV_VALORA
                      WHERE  CPR_ID = @cpr_id
                      GROUP  BY PROVEEDOR_CODIGO",
                    new { cpr_id }
                ).ToDictionary(x => x.PROVEEDOR_CODIGO, x => x.PUNTAJE);

                var presencia = ObtenerPresenciaTablas(conn, cpr_id);

                foreach (var item in lista)
                {
                    item.valora_puntaje = puntajes.TryGetValue(item.proveedor_codigo, out var p)
                        ? p.ToString("0.##")
                        : "0";
                    item.estado = ResolverEstado(item.estado, item.proveedor_codigo, presencia);
                }

                return lista;
            });
        }

        /// <summary>Consulta en qué tablas de proceso tiene registros cada proveedor para el CPR.</summary>
        private static ProvPresenciaTablas ObtenerPresenciaTablas(SqlConnection conn, int cpr_id)
        {
            var conValora = conn.Query<int>(
                "SELECT DISTINCT PROVEEDOR_CODIGO FROM CPR_SOLICITUD_PROV_VALORA WHERE CPR_ID = @cpr_id",
                new { cpr_id }
            ).ToHashSet();

            var conCotiza = conn.Query<int>(
                "SELECT DISTINCT PROVEEDOR_CODIGO FROM CPR_SOLICITUD_PROV_COTIZA WHERE CPR_ID = @cpr_id",
                new { cpr_id }
            ).ToHashSet();

            var conBs = conn.Query<int>(
                "SELECT DISTINCT PROVEEDOR_CODIGO FROM CPR_SOLICITUD_PROV_BS WHERE CPR_ID = @cpr_id",
                new { cpr_id }
            ).ToHashSet();

            return new ProvPresenciaTablas(conValora, conCotiza, conBs);
        }

        /// <summary>
        /// Determina el estado real del proveedor según las tablas de proceso.
        /// No sobreescribe estados finales (S, A, C, F).
        /// </summary>
        private static string? ResolverEstado(string? estadoActual, int proveedorCodigo, ProvPresenciaTablas presencia)
        {
            if (estadoActual is "S" or "A" or "C" or "F") return estadoActual;
            if (presencia.ConValora.Contains(proveedorCodigo)) return "V";
            if (presencia.ConCotiza.Contains(proveedorCodigo)) return "T";
            if (presencia.ConBs.Contains(proveedorCodigo)) return "R";
            return estadoActual;
        }

        public ErrorDto<List<CprSolicitudPrvBs>> CprSolicitudProvContizacionLista_Obtener(int codEmpresa, int cpr_id, string cod_proveedor)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Query<CprSolicitudPrvBs>(
                    "spCPR_SolicitudProvCotiLista_Obtener",
                    new { cpr_id = cpr_id, cod_proveedor = cod_proveedor },
                    commandType: CommandType.StoredProcedure
                ).ToList()
            );
        }

        public ErrorDto<List<CprSolicitudProvValItemData>> CprSolicitudProvValItemData_Obtener(int codEmpresa, string parametros)
        {
            var parametro = JsonConvert.DeserializeObject<CprParametrosValBusqueda>(parametros);
            if (parametro is null)
                return DbHelper.CreateErrorResponse("Parámetros inválidos", -1, new List<CprSolicitudProvValItemData>());

            // Convert.ToInt32(null) => 0 (evita CS1503 y nulls)
            var cprId = Convert.ToInt32(parametro.crp_id);
            var prov = Convert.ToInt32(parametro.proveedor);
            var valId = Convert.ToString(parametro.val_id) ?? string.Empty;

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Query<CprSolicitudProvValItemData>(
                    @"SELECT
                          ISNULL(P.ID_VALORACION, 0) AS ID_VALORACION,
                          I.val_item, I.DESCRIPCION, I.PESO,
                          ISNULL(P.NOTA, 0) AS NOTA,
                          ISNULL(P.PUNTAJE, 0) AS PUNTAJE
                      FROM CPR_VALORA_ITEMS I
                      LEFT JOIN CPR_SOLICITUD_PROV_VALORA P
                             ON P.VAL_ITEM = I.VAL_ITEM
                            AND P.CPR_ID = @CprId
                            AND P.PROVEEDOR_CODIGO = @Proveedor
                      WHERE I.VAL_ID = @ValId",
                    new { CprId = cprId, Proveedor = prov, ValId = valId }
                ).ToList()
            );
        }

        public async Task<ErrorDto> CprSolicitudProvCotizacion_Enviar(int codEmpresa, int cpr_id, string cod_proveedor)
        {
            var proveedorCodigo = Convert.ToInt32(cod_proveedor);
            if (proveedorCodigo <= 0)
                return DbHelper.ErrorResponse("cod_proveedor inválido", -1);

            var data = ObtenerDataParaCorreo(codEmpresa, cpr_id, proveedorCodigo);
            if (data.Code != 0 || data.Result == null)
                return DbHelper.ErrorResponse(data.Description ?? DefaultErrorMsg, data.Code ?? -1);

            var correoData = data.Result;

            var mail = await CorreoSolicitaCotizacion_Enviar(codEmpresa, cpr_id, correoData);
            if (mail.Code != 0)
                return mail;

            var upd = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                @"UPDATE CPR_SOLICITUD_PROV
                  SET ESTADO = @Estado
                  WHERE CPR_ID = @CprId
                    AND PROVEEDOR_CODIGO = @Proveedor",
                new { Estado = EstadoEnviado, CprId = cpr_id, Proveedor = proveedorCodigo }
            );

            return upd.Code == 0
                ? DbHelper.OkResponse("Solicitud de cotización enviada")
                : upd;
        }

        public ErrorDto CprSolicitudValoracion_Guardar(int codEmpresa, CprSolicitusValoracionGuardar datos)
        {
            var cprId = Convert.ToInt32(datos.cotizacion.cpr_id);
            var proveedorCodigo = Convert.ToInt32(datos.cotizacion.proveedor_codigo);
            var usuario = Convert.ToString(datos.cotizacion.valora_usuario) ?? string.Empty;

            if (cprId <= 0 || proveedorCodigo <= 0 || string.IsNullOrWhiteSpace(usuario))
                return DbHelper.ErrorResponse("Datos de cotización inválidos", -1);

            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (conn.State != ConnectionState.Open) conn.Open();
                using var tx = conn.BeginTransaction();

                try
                {
                    GuardarValoracionProveedores(conn, tx, datos, cprId, proveedorCodigo, usuario);
                    GuardarValoracionProductos(conn, tx, datos, cprId, proveedorCodigo, usuario);
                    ActualizarProveedorEstado(conn, tx, datos, cprId, proveedorCodigo, usuario);
                    ActualizarCotizaEstado(conn, tx, cprId, proveedorCodigo);

                    tx.Commit();
                    return true;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            });

            return r.Code == 0
                ? DbHelper.CreateOkResponse()
                : DbHelper.ErrorResponse(r.Description ?? DefaultErrorMsg, r.Code ?? -1);
        }

        // ----------------- Helpers (bajan S3776 + evitan CS1503/CS1061) -----------------

        private static ErrorDto<InvitarData> ParseInvitarData(CprSolicitudProvDto proveedor)
        {
            var cprId = Convert.ToInt32(proveedor.cpr_id);
            var prov = Convert.ToInt32(proveedor.proveedor_codigo);
            var usuario = Convert.ToString(proveedor.registro_usuario) ?? string.Empty;

            if (cprId <= 0) return DbHelper.CreateErrorResponse<InvitarData>("cpr_id inválido", -1, new InvitarData(0, 0, string.Empty));
            if (prov <= 0) return DbHelper.CreateErrorResponse<InvitarData>("proveedor_codigo inválido", -1, new InvitarData(0, 0, string.Empty));
            if (string.IsNullOrWhiteSpace(usuario)) return DbHelper.CreateErrorResponse<InvitarData>("registro_usuario inválido", -1, new InvitarData(0, 0, string.Empty));

            return DbHelper.CreateOkResponse(new InvitarData(cprId, prov, usuario));
        }

        private static CprSolicitudDto? ObtenerSolicitud(SqlConnection conn, SqlTransaction tx, int cprId)
        {
            return conn.QueryFirstOrDefault<CprSolicitudDto>(
                "SELECT * FROM CPR_SOLICITUD WHERE CPR_ID = @CprId",
                new { CprId = cprId },
                transaction: tx
            );
        }

        private static int ContarProveedores(SqlConnection conn, SqlTransaction tx, int cprId)
        {
            return conn.QueryFirstOrDefault<int>(
                "SELECT COUNT(*) FROM CPR_SOLICITUD_PROV WHERE CPR_ID = @CprId",
                new { CprId = cprId },
                transaction: tx
            );
        }

        private static void InvitarProveedorSp(SqlConnection conn, SqlTransaction tx, InvitarData data)
        {
            conn.Execute(
                "spCPR_SolicitudProv_Invitar",
                new { cod_proveedor = data.ProveedorCodigo, cpr_id = data.CprId, usuario = data.Usuario },
                transaction: tx,
                commandType: CommandType.StoredProcedure
            );
        }

        private ErrorDto<CorreoData> ObtenerDataParaCorreo(int codEmpresa, int cpr_id, int proveedorCodigo)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var proveedor = conn.QueryFirstOrDefault<CprProveedorDto>(
                    @"SELECT DESCRIPCION, CEDJUR, EMAIL
                      FROM CPR_PROVEEDORES_TEMPO
                      WHERE COD_PROVEEDOR = @CodProveedor",
                    new { CodProveedor = proveedorCodigo }
                ) ?? new CprProveedorDto();

                var productos = conn.Query<CprSolicitudBsDto>(
                    @"SELECT B.CPR_ID, B.COD_PRODUCTO, P.DESCRIPCION, B.CANTIDAD, B.MONTO,
                             (B.CANTIDAD * B.MONTO) AS TOTAL, P.COD_UNIDAD
                      FROM CPR_SOLICITUD_BS B
                      LEFT JOIN PV_PRODUCTOS P ON P.COD_PRODUCTO = B.COD_PRODUCTO
                      WHERE B.CPR_ID = @CprId",
                    new { CprId = cpr_id }
                ).ToList();

                var solicitud = conn.QueryFirstOrDefault<SolicitudCorreoFila>(
                    @"SELECT recepcion_ofertas, porc_multa, terminos_condiciones, int_sitio_entrega
                      FROM CPR_SOLICITUD
                      WHERE cpr_id = @CprId",
                    new { CprId = cpr_id }
                ) ?? new SolicitudCorreoFila();

                return new CorreoData(
                    proveedor,
                    productos,
                    FormatoFechaLimite(solicitud.recepcion_ofertas),
                    FormatoMulta(solicitud.porc_multa),
                    (solicitud.terminos_condiciones ?? 0) != 0,
                    ObtenerAnexos(cpr_id),
                    solicitud.int_sitio_entrega ?? string.Empty);
            });
        }

        /// <summary>Nombres de los documentos anexos del concurso (GA_Files, módulo CPR).</summary>
        private List<string> ObtenerAnexos(int cprId)
        {
            using var conn = new SqlConnection(_config.GetConnectionString("GAConnString"));
            return conn.Query<string>(
                "SELECT filename FROM GA_Files WHERE moduloid = @Modulo AND llave_01 = @Llave ORDER BY filename;",
                new { Modulo = "CPR", Llave = cprId.ToString() }).ToList();
        }

        private static string FormatoFechaLimite(DateTime? fecha) =>
            fecha?.ToString("dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.GetCultureInfo("es-CR")) ?? "Sin fecha";

        private static string FormatoMulta(decimal? porcentaje) =>
            porcentaje is null ? "No establecida" : $"{porcentaje.Value:0.##}%";

        private async Task<ErrorDto> CorreoSolicitaCotizacion_Enviar(int codEmpresa, int cprId, CorreoData datos)
        {
            try
            {
                var terminos = datos.Terminos
                    ? await _envioCorreoDB.ObtenerCuerpoPlantillaAsync(codEmpresa, PlantillaTerminos) ?? string.Empty
                    : string.Empty;

                return await _envioCorreoDB.QueuePlantillaAsync(
                    codEmpresa,
                    _notificaciones,
                    PlantillaCotizacion,
                    datos.Proveedor.email ?? string.Empty,
                    new Dictionary<string, string>
                    {
                        ["NO_SOLICITUD"] = cprId.ToString(),
                        ["PROVEEDOR"] = datos.Proveedor.descripcion ?? string.Empty,
                        ["CEDJUR"] = datos.Proveedor.cedjur ?? string.Empty,
                        ["FECHA_LIMITE"] = datos.Recepcion,
                        ["MULTA"] = datos.Multa,
                        ["LUGAR_ENTREGA"] = datos.LugarEntrega,
                        ["ANEXOS"] = datos.Anexos.Count > 0 ? string.Join(", ", datos.Anexos) : "Sin anexos",
                    },
                    new Dictionary<string, string>
                    {
                        ["PRODUCTOS"] = ConstruirFilasProductos(datos.Productos),
                        ["TERMINOS"] = terminos,
                    });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        private static string ConstruirFilasProductos(List<CprSolicitudBsDto> info)
        {
            var sb = new StringBuilder();
            foreach (var p in info)
            {
                sb.Append("<tr>")
                  .Append(Celda(p.cantidad))
                  .Append(Celda((p.cod_unidad ?? string.Empty).ToUpper()))
                  .Append(Celda(p.cod_producto))
                  .Append(Celda(p.descripcion))
                  .Append(Celda(p.monto))
                  .Append(Celda(p.total))
                  .Append("</tr>");
            }
            return sb.ToString();
        }

        private static string Celda(object? valor) =>
            $"<td>{System.Net.WebUtility.HtmlEncode(Convert.ToString(valor) ?? string.Empty)}</td>";

        private static void GuardarValoracionProveedores(SqlConnection conn, SqlTransaction tx, CprSolicitusValoracionGuardar datos, int cprId, int proveedorCodigo, string usuario)
        {
            foreach (var item in datos.valoracion)
            {
                var xml = MProGrXAuxiliarDB.fxConvertModelToXml<CprSolicitudProvValItemData>(item);

                conn.Execute(
                    "spCPR_CprSolicitudProvValora_Guardar",
                    new { datos = xml, cpr_id = cprId, proveedor = proveedorCodigo, usuario },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        private static void GuardarValoracionProductos(SqlConnection conn, SqlTransaction tx, CprSolicitusValoracionGuardar datos, int cprId, int proveedorCodigo, string usuario)
        {
            var notas = Convert.ToString(datos.cotizacion.valora_notas) ?? string.Empty;

            foreach (var producto in datos.productos)
            {
                conn.Execute(
                    @"UPDATE dbo.CPR_SOLICITUD_PROV_BS
                      SET VALORA_PUNTAJE = @Puntaje,
                          VALORA_FECHA = GETDATE(),
                          VALORA_USUARIO = @Usuario,
                          VALORA_NOTAS = @Notas,
                          ESTADO = @Estado
                      WHERE CPR_ID = @CprId
                        AND COD_PRODUCTO = @CodProducto
                        AND PROVEEDOR_CODIGO = @Proveedor",
                    new
                    {
                        Puntaje = producto.valora_puntaje,
                        Usuario = usuario,
                        Notas = notas,
                        Estado = EstadoVigente,
                        CprId = cprId,
                        CodProducto = producto.cod_producto,
                        Proveedor = proveedorCodigo
                    },
                    transaction: tx
                );
            }
        }

        private static void ActualizarProveedorEstado(SqlConnection conn, SqlTransaction tx, CprSolicitusValoracionGuardar datos, int cprId, int proveedorCodigo, string usuario)
        {
            var notas = Convert.ToString(datos.cotizacion.valora_notas) ?? string.Empty;

            conn.Execute(
                @"UPDATE CPR_SOLICITUD_PROV
                  SET ESTADO = @Estado,
                      VALORA_PUNTAJE = @Puntaje,
                      VALORA_FECHA = GETDATE(),
                      VALORA_USUARIO = @Usuario,
                      NOTAS = @Notas
                  WHERE CPR_ID = @CprId
                    AND PROVEEDOR_CODIGO = @Proveedor",
                new
                {
                    Estado = EstadoVigente,
                    Puntaje = datos.cotizacion.valora_puntaje,
                    Usuario = usuario,
                    Notas = notas,
                    CprId = cprId,
                    Proveedor = proveedorCodigo
                },
                transaction: tx
            );
        }

        private static void ActualizarCotizaEstado(SqlConnection conn, SqlTransaction tx, int cprId, int proveedorCodigo)
        {
            conn.Execute(
                @"UPDATE CPR_SOLICITUD_PROV_COTIZA
                  SET ESTADO = @Estado
                  WHERE CPR_ID = @CprId
                    AND PROVEEDOR_CODIGO = @Proveedor",
                new { Estado = EstadoVigente, CprId = cprId, Proveedor = proveedorCodigo },
                transaction: tx
            );
        }

        public static ErrorDto CprSolicitudProv_GastoMenor(int codEmpresa)
            => DbHelper.CreateOkResponse();
    }
}
