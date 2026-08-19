using Dapper;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using System.Data;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmCprSolicitudDB
    {
        private const string ErrorLiteral = "Ocurrió un error inesperado.";

        private readonly IConfiguration _config;
        private readonly PortalDB _portalDb;

        private readonly MSecurityMainDb _dbBitacora;
        private readonly EnvioCorreoDB _envioCorreoDB;

        private readonly string _sendEmail;
        private readonly string _notificaciones;

        public FrmCprSolicitudDB(IConfiguration config)
        {
            _config = config;
            _portalDb = new PortalDB(_config);

            _dbBitacora = new MSecurityMainDb(config);
            _envioCorreoDB = new EnvioCorreoDB(_config);

            _sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value?.ToString() ?? "";
            _notificaciones = _config.GetSection("AppSettings").GetSection("Notificaciones").Value?.ToString() ?? "";
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data) => _dbBitacora.Bitacora(data);

        // ===========================
        //  LISTA SOLICITUDES
        // ===========================

        public ErrorDto<CprSolicitudLista> CprSolicitudLista_Obtener(int codEmpresa, string filtros)
        {
            var filtro = JsonConvert.DeserializeObject<CprSolicitudFiltro>(filtros) ?? new CprSolicitudFiltro();

            var r = DbHelper.WithConn<CprSolicitudLista>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                var p = new DynamicParameters();

                // Filtro texto
                var like = NormalizeLike(filtro.filtro);
                var hasFiltro = !string.IsNullOrWhiteSpace(like);
                p.Add("HasFiltro", hasFiltro ? 1 : 0);
                p.Add("Like", hasFiltro ? like : null);

                // Solicitantes
                var solicitantes = (filtro.solicitante ?? new List<string>())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var hasSolicitantes = solicitantes.Count > 0 ? 1 : 0;
                if (solicitantes.Count == 0) solicitantes.Add(string.Empty); // evita IN ()

                p.Add("HasSolicitantes", hasSolicitantes);
                p.Add("Solicitantes", solicitantes);

                // Encargados
                var encargados = (filtro.encargado ?? new List<string>())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var hasEncargados = encargados.Count > 0 ? 1 : 0;
                if (encargados.Count == 0) encargados.Add(string.Empty); // evita IN ()

                p.Add("HasEncargados", hasEncargados);
                p.Add("Encargados", encargados);

                // Paginación (si no viene, usa defaults)
                var (offset, fetch, _) = GetPaging(filtro.pagina, filtro.paginacion);
                p.Add("Offset", offset);
                p.Add("Fetch", fetch);

                var sortOrder = filtro.sort_order == 1 ? 1 : -1;

                var sortField = filtro.sort_field?
                .Trim()
                .ToLowerInvariant() ?? string.Empty;
                p.Add("SortField", sortField);
                p.Add("SortOrder", sortOrder);

                const string qCount = @"
                            SELECT COUNT(DISTINCT S.CPR_ID)
                             from CPR_SOLICITUD S LEFT JOIN CPR_SOLICITUD_PROV P ON S.CPR_ID = P.CPR_ID
                                    AND P.ADJUDICA_ORDEN is not null;
                            ";

                var total = conn.QueryFirstOrDefault<int>(qCount, p);

                const string qList = @"
    SELECT 
        S.CPR_ID,
        P.ADJUDICA_ORDEN,
        S.DOCUMENTO,
        U.DESCRIPCION AS COD_UNIDAD_SOLICITANTE,
        S.ESTADO,
        S.REGISTRO_USUARIO,
        S.ENCARGADO_USUARIO
    FROM CPR_SOLICITUD S
    LEFT JOIN CPR_SOLICITUD_PROV P
        ON S.CPR_ID = P.CPR_ID AND P.ADJUDICA_ORDEN IS NOT NULL
    LEFT JOIN CORE_UENS U
        ON U.COD_UNIDAD = S.COD_UNIDAD_SOLICITANTE
    WHERE
        (@HasFiltro = 0 OR (
            CAST(S.CPR_ID AS VARCHAR(20)) LIKE @Like OR
            ISNULL(P.ADJUDICA_ORDEN,'') LIKE @Like OR
            ISNULL(S.REGISTRO_USUARIO,'') LIKE @Like OR
            ISNULL(U.DESCRIPCION,'') LIKE @Like
        ))
        AND (@HasSolicitantes = 0 OR S.REGISTRO_USUARIO IN @Solicitantes)
        AND (@HasEncargados = 0 OR S.ENCARGADO_USUARIO IN @Encargados)
    ORDER BY
        CASE WHEN @SortField = 'cpr_id' AND @SortOrder = 1 THEN S.CPR_ID END ASC,
        CASE WHEN @SortField = 'cpr_id' AND @SortOrder <> 1 THEN S.CPR_ID END DESC,

        CASE WHEN @SortField = 'adjudica_orden' AND @SortOrder = 1 THEN P.ADJUDICA_ORDEN END ASC,
        CASE WHEN @SortField = 'adjudica_orden' AND @SortOrder <> 1 THEN P.ADJUDICA_ORDEN END DESC,

        CASE WHEN @SortField = 'estado' AND @SortOrder = 1 THEN S.ESTADO END ASC,
        CASE WHEN @SortField = 'estado' AND @SortOrder <> 1 THEN S.ESTADO END DESC,

        CASE WHEN @SortField = 'registro_usuario' AND @SortOrder = 1 THEN S.REGISTRO_USUARIO END ASC,
        CASE WHEN @SortField = 'registro_usuario' AND @SortOrder <> 1 THEN S.REGISTRO_USUARIO END DESC,

        CASE WHEN @SortField = 'encargado_usuario' AND @SortOrder = 1 THEN S.ENCARGADO_USUARIO END ASC,
        CASE WHEN @SortField = 'encargado_usuario' AND @SortOrder <> 1 THEN S.ENCARGADO_USUARIO END DESC,

        CASE WHEN @SortField = 'cod_unidad_solicitante' AND @SortOrder = 1 THEN U.DESCRIPCION END ASC,
        CASE WHEN @SortField = 'cod_unidad_solicitante' AND @SortOrder <> 1 THEN U.DESCRIPCION END DESC,

        S.CPR_ID DESC
    OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

                var solicitudes = conn.Query<CprSolicitudDto>(qList, p).ToList();

                return new CprSolicitudLista
                {
                    total = total,
                    solicitudes = solicitudes
                };
            });

            return WrapRequired(r);
        }


        private static (int Offset, int Fetch, bool UsePaging) GetPaging(int? pagina, int? paginacion)
        {
            // Always return a usable paging tuple so the SQL can be constant.
            if (pagina == null || paginacion == null || pagina < 0 || paginacion <= 0)
                return (0, int.MaxValue, true);

            return (pagina.Value, paginacion.Value, true);
        }

        // ===========================
        //  OBTENER SOLICITUD
        // ===========================

        public ErrorDto<CprSolicitudDto> CprSolicitud_Obtener(int codEmpresa, int cpr_id, string usuario)
        {
            var db = DbHelper.WithConn<CprSolicitudDto>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                ActualizarEstadoSolicitudSiAplica(conn, cpr_id);

                var solicitud = ObtenerSolicitud(conn, cpr_id);
                if (solicitud == null) return new CprSolicitudDto();

                EnriquecerCompraDirectaSiAplica(conn, codEmpresa, cpr_id, solicitud);
                return solicitud;
            });

            var baseResp = MapDbResult(db);
            if (baseResp.Code != 0) return baseResp;

            if (baseResp.Result == null)
                return new ErrorDto<CprSolicitudDto>
                {
                    Code = -1,
                    Description = "Solicitud no encontrada",
                    Result = null
                };

            return ValidarPermisoConsulta(codEmpresa, usuario, baseResp.Result);
        }

        private ErrorDto<CprSolicitudDto> MapDbResult(ErrorDto<CprSolicitudDto> db)
        {
            if (db.Code != 0)
                return new ErrorDto<CprSolicitudDto>
                {
                    Code = db.Code ?? -1,
                    Description = db.Description ?? ErrorLiteral,
                    Result = null
                };

            if (db.Result == null)
                return new ErrorDto<CprSolicitudDto>
                {
                    Code = -1,
                    Description = "Solicitud no encontrada",
                    Result = null
                };

            return new ErrorDto<CprSolicitudDto> { Code = 0, Result = db.Result };
        }

        private ErrorDto<CprSolicitudDto> ValidarPermisoConsulta(int codEmpresa, string usuario, CprSolicitudDto solicitud)
        {
            var permiso = ValidaUsuarioSolicitud(codEmpresa, usuario, "C", solicitud.cod_unidad);
            if (permiso.Code == -1)
                return new ErrorDto<CprSolicitudDto>
                {
                    Code = -1,
                    Description = "El usuario no tiene permisos para realizar esta acción",
                    Result = null
                };

            return new ErrorDto<CprSolicitudDto> { Code = 0, Result = solicitud };
        }

        private void ActualizarEstadoSolicitudSiAplica(IDbConnection conn, int cprId)
        {
            if (!TodosProveedoresEnV(conn, cprId)) return;

            var estadoSolicitud = ObtenerEstadoSolicitud(conn, cprId);
            if (!PuedeSubirAV(estadoSolicitud)) return;

            conn.Execute("UPDATE CPR_SOLICITUD SET estado = 'V' WHERE CPR_ID = @Id;", new { Id = cprId });
        }

        private bool TodosProveedoresEnV(IDbConnection conn, int cprId)
        {
            var estados = conn.Query<string>(
                "SELECT ISNULL(ESTADO,'N') FROM CPR_SOLICITUD_PROV WHERE CPR_ID = @Id;",
                new { Id = cprId }).ToList();

            return estados.Count > 0 &&
                estados.All(e => string.Equals(e, "V", StringComparison.OrdinalIgnoreCase));
        }

        private string? ObtenerEstadoSolicitud(IDbConnection conn, int cprId)
        {
            return conn.QueryFirstOrDefault<string>(
                "SELECT estado FROM CPR_SOLICITUD WHERE CPR_ID = @Id;",
                new { Id = cprId });
        }

        private static bool PuedeSubirAV(string? estadoSolicitud)
        {
            if (string.IsNullOrWhiteSpace(estadoSolicitud)) return false;
            if (string.Equals(estadoSolicitud, "D", StringComparison.OrdinalIgnoreCase)) return false;
            if (string.Equals(estadoSolicitud, "F", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        private CprSolicitudDto? ObtenerSolicitud(IDbConnection conn, int cprId)
        {
            return conn.QueryFirstOrDefault<CprSolicitudDto>(
                "SELECT * FROM CPR_SOLICITUD WHERE CPR_ID = @Id;",
                new { Id = cprId });
        }

        private void EnriquecerCompraDirectaSiAplica(IDbConnection conn, int codEmpresa, int cprId, CprSolicitudDto solicitud)
        {
            if (!EsCompraDirecta(codEmpresa, solicitud.tipo_orden)) return;

            const string qProv = @"
        SELECT TOP 1
            P.PROVEEDOR_CODIGO AS com_dir_cod_proveedor,
            cp.DESCRIPCION AS com_dir_des_proveedor
        FROM CPR_SOLICITUD_PROV P
        LEFT JOIN CXP_PROVEEDORES cp ON cp.COD_PROVEEDOR = P.PROVEEDOR_CODIGO
        WHERE P.CPR_ID = @Id;";

            var prov = conn.QueryFirstOrDefault<CprSolicitudDto>(qProv, new { Id = cprId });
            if (prov == null) return;

            solicitud.com_dir_cod_proveedor = prov.com_dir_cod_proveedor;
            solicitud.com_dir_des_proveedor = prov.com_dir_des_proveedor;
        }

        private bool EsCompraDirecta(int codEmpresa, string? tipoOrdenSolicitud)
        {
            if (string.IsNullOrWhiteSpace(tipoOrdenSolicitud)) return false;

            var tipoEx = CprSolicitud_TipoExcepcion(codEmpresa).Description ?? "";
            if (string.IsNullOrWhiteSpace(tipoEx)) return false;

            return string.Equals(tipoOrdenSolicitud, tipoEx, StringComparison.OrdinalIgnoreCase);
        }

        private ErrorDto CompraDirectaProveedor_UpsertSiNoAutorizada(IDbConnection conn, int codEmpresa, CprSolicitudDto solicitud)
        {
            if (!EsCompraDirecta(codEmpresa, solicitud.tipo_orden))
                return DbHelper.CreateOkResponse();

            if ((solicitud.cpr_id ?? 0) <= 0)
                return DbHelper.ErrorResponse("No se pudo identificar la solicitud para compra directa", -1);

            var codProveedor = solicitud.com_dir_cod_proveedor ?? 0;
            if (codProveedor <= 0)
                return DbHelper.ErrorResponse("Debe seleccionar un proveedor para la compra directa", -1);

            var cprId = solicitud.cpr_id ?? 0;
            var estadoSolicitud = conn.QueryFirstOrDefault<string>(
                "SELECT ESTADO FROM CPR_SOLICITUD WHERE CPR_ID = @Id;",
                new { Id = cprId }) ?? string.Empty;

            if (string.Equals(estadoSolicitud, "A", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(estadoSolicitud, "F", StringComparison.OrdinalIgnoreCase))
            {
                return DbHelper.ErrorResponse("La solicitud ya está autorizada/finalizada y no permite cambiar proveedor en compra directa", -1);
            }

            var usuario = string.IsNullOrWhiteSpace(solicitud.modifica_usuario)
                ? (solicitud.registro_usuario ?? string.Empty)
                : solicitud.modifica_usuario;

            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.ErrorResponse("No se pudo identificar el usuario para actualizar proveedor de compra directa", -1);

            var existe = conn.QueryFirstOrDefault<int>(
                "SELECT COUNT(*) FROM CPR_SOLICITUD_PROV WHERE CPR_ID = @Id;",
                new { Id = cprId });

            if (existe == 0)
            {
                conn.Execute(@"
INSERT INTO CPR_SOLICITUD_PROV
(
    CPR_ID,
    PROVEEDOR_CODIGO,
    PROVEEDOR_ESTADO,
    REGISTRO_FECHA,
    REGISTRO_USUARIO,
    VALORA_PUNTAJE
)
VALUES
(
    @CprId,
    @CodProveedor,
    'I',
    GETDATE(),
    @Usuario,
    0
);",
                new { CprId = cprId, CodProveedor = codProveedor, Usuario = usuario });
            }
            else
            {
                conn.Execute(@"
UPDATE CPR_SOLICITUD_PROV
SET PROVEEDOR_CODIGO = @CodProveedor,
    PROVEEDOR_ESTADO = 'I',
    REGISTRO_FECHA = GETDATE(),
    REGISTRO_USUARIO = @Usuario,
    VALORA_PUNTAJE = 0
WHERE CPR_ID = @CprId;",
                new { CprId = cprId, CodProveedor = codProveedor, Usuario = usuario });
            }

            conn.Execute(@"
UPDATE CPR_SOLICITUD_PROV
SET ESTADO = 'V'
WHERE CPR_ID = @CprId;",
                new { CprId = cprId });

            return DbHelper.CreateOkResponse();
        }

        public ErrorDto<CprSolicitudDto> CprSolicitud_Scroll(int codEmpresa, int scroll, string usuario, string? codigo)
        {
            if (!int.TryParse(codigo ?? "", out var codId))
                return new ErrorDto<CprSolicitudDto> { Code = -1, Description = "Código inválido", Result = null };

            var r = DbHelper.WithConn<CprSolicitudDto>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                const string sqlNext = @"
SELECT TOP 1 *
FROM CPR_SOLICITUD
WHERE CPR_ID > @Codigo
  AND COD_UNIDAD IN (
        SELECT R.COD_UNIDAD
        FROM CORE_UENS_USUARIOS_ROLES R
        LEFT JOIN CORE_UENS U ON R.COD_UNIDAD = U.COD_UNIDAD
        WHERE R.CORE_USUARIO = @Usuario
          AND (R.ROL_CONSULTA = 1 OR R.ROL_ENCARGADO = 1)
  )
ORDER BY CPR_ID ASC;";

                const string sqlPrev = @"
SELECT TOP 1 *
FROM CPR_SOLICITUD
WHERE CPR_ID < @Codigo
  AND COD_UNIDAD IN (
        SELECT R.COD_UNIDAD
        FROM CORE_UENS_USUARIOS_ROLES R
        LEFT JOIN CORE_UENS U ON R.COD_UNIDAD = U.COD_UNIDAD
        WHERE R.CORE_USUARIO = @Usuario
          AND (R.ROL_CONSULTA = 1 OR R.ROL_ENCARGADO = 1)
  )
ORDER BY CPR_ID DESC;";

                var sql = scroll == 1 ? sqlNext : sqlPrev;

                return conn.QueryFirstOrDefault<CprSolicitudDto>(
                    sql,
                    new { Codigo = codId, Usuario = usuario }
                ) ?? new CprSolicitudDto();
            });

            if (r.Code != 0)
                return new ErrorDto<CprSolicitudDto> { Code = r.Code ?? -1, Description = r.Description ?? ErrorLiteral, Result = null };

            if (r.Result == null)
                return new ErrorDto<CprSolicitudDto> { Code = 0, Result = null };

            // proveedor si compra directa
            var tipoEx = CprSolicitud_TipoExcepcion(codEmpresa).Description ?? "";
            if (!string.IsNullOrWhiteSpace(r.Result.tipo_orden) &&
                string.Equals(r.Result.tipo_orden, tipoEx, StringComparison.OrdinalIgnoreCase))
            {
                var provR = DbHelper.WithConn<CprSolicitudDto>(_portalDb, codEmpresa, conn =>
                {
                    EnsureOpen(conn);

                    const string qProv = @"
SELECT TOP 1
    P.PROVEEDOR_CODIGO AS com_dir_cod_proveedor,
    cp.DESCRIPCION AS com_dir_des_proveedor
FROM CPR_SOLICITUD_PROV P
LEFT JOIN CXP_PROVEEDORES cp ON cp.COD_PROVEEDOR = P.PROVEEDOR_CODIGO
WHERE P.CPR_ID = @Id;";

                    return conn.QueryFirstOrDefault<CprSolicitudDto>(qProv, new { Id = r.Result.cpr_id }) ?? new CprSolicitudDto();
                });

                if (provR.Code == 0 && provR.Result != null)
                {
                    r.Result.com_dir_cod_proveedor = provR.Result.com_dir_cod_proveedor;
                    r.Result.com_dir_des_proveedor = provR.Result.com_dir_des_proveedor;
                }
            }

            var permiso = ValidaUsuarioSolicitud(codEmpresa, usuario, "C", r.Result.cod_unidad);
            if (permiso.Code == -1)
                return new ErrorDto<CprSolicitudDto> { Code = -1, Description = "El usuario no tiene permisos para realizar esta acción", Result = null };

            return new ErrorDto<CprSolicitudDto> { Code = 0, Result = r.Result };
        }

        // ===========================
        //  GUARDAR SOLICITUD
        // ===========================

        public ErrorDto CprSolicitud_Guardar(int codEmpresa, bool edita, CprSolicitudDto solicitud)
        {
            return edita ? CprSolicitud_Actualizar(codEmpresa, solicitud) : CprSolicitud_Insertar(codEmpresa, solicitud);
        }

        private ErrorDto CprSolicitud_Insertar(int codEmpresa, CprSolicitudDto solicitud)
        {
            // reglas de negocio
            var v = ValidarMontoVsTipo(codEmpresa, solicitud);
            if (v.Code != 0) return v;

            var r = DbHelper.WithConn<ErrorDto>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                // consecutivo
                var secuencia = conn.Query<int>("SELECT ISNULL(MAX(CPR_ID),0) + 1 FROM CPR_SOLICITUD;").FirstOrDefault();
                solicitud.cpr_id = secuencia;

                // xml -> sp (parametrizado)
                var xmlOutput = MProGrXAuxiliarDB.fxConvertModelToXml<CprSolicitudDto>(solicitud);
                conn.Execute("exec spCPR_Solicitud_Insertar @Xml;", new { Xml = xmlOutput });

                var proveedorR = CompraDirectaProveedor_UpsertSiNoAutorizada(conn, codEmpresa, solicitud);
                if (proveedorR.Code != 0)
                    return proveedorR;

                // asigna encargado
                _ = AsignaEncargado_Solicitud(codEmpresa, solicitud.cod_unidad_solicitante ?? string.Empty, secuencia);

                // email encargado
                var usuarioEnc = conn.QueryFirstOrDefault<string>(
                    "SELECT ENCARGADO_USUARIO FROM CPR_SOLICITUD WHERE CPR_ID = @Id;",
                    new { Id = secuencia }) ?? "";

                _ = CorreoNotificaSolicitud_Enviar(codEmpresa, secuencia, usuarioEnc);

                return DbHelper.OkResponse(secuencia.ToString());
            });

            if (r.Code != 0 || r.Result == null)
                return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            return r.Result;
        }

        private ErrorDto ValidarMontoVsTipo(int codEmpresa, CprSolicitudDto solicitud)
        {
            var gmMonto = CprSolicitud_gastoMenorMonto(codEmpresa).Result;
            var tipoEx = CprSolicitud_TipoExcepcion(codEmpresa).Description ?? "";
            var tipoGm = CprSolicitud_TipoExcepcionGM(codEmpresa).Description ?? "";

            // si monto < gmMonto y NO es excepción GM ni compra directa
            if (solicitud.monto < gmMonto &&
                !string.Equals(solicitud.tipo_orden, tipoGm, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(solicitud.tipo_orden, tipoEx, StringComparison.OrdinalIgnoreCase))
            {
                return DbHelper.ErrorResponse("El monto de la orden clasifica como gasto menor", -1);
            }

            // si monto > gmMonto y es de tipo GM (no debería)
            if (solicitud.monto > gmMonto &&
                string.Equals(solicitud.tipo_orden, tipoGm, StringComparison.OrdinalIgnoreCase))
            {
                return DbHelper.ErrorResponse("El monto de la orden es muy alto para esta clasificación", -1);
            }

            return DbHelper.CreateOkResponse();
        }

        private ErrorDto CprSolicitud_Actualizar(int codEmpresa, CprSolicitudDto solicitud)
        {
            var r = DbHelper.WithConn<ErrorDto>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                var xmlOutput = MProGrXAuxiliarDB.fxConvertModelToXml<CprSolicitudDto>(solicitud);
                conn.Execute("exec spCPR_Solicitud_Actualizar @Xml;", new { Xml = xmlOutput });

                var proveedorR = CompraDirectaProveedor_UpsertSiNoAutorizada(conn, codEmpresa, solicitud);
                if (proveedorR.Code != 0)
                    return proveedorR;

                return DbHelper.CreateOkResponse();
            });

            if (r.Code != 0 || r.Result == null)
                return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            if (r.Result.Code != 0)
                return DbHelper.ErrorResponse(r.Result.Description ?? ErrorLiteral, r.Result.Code ?? -1);

            return DbHelper.OkResponse((solicitud.cpr_id ?? 0).ToString());
        }

        public ErrorDto CprSolicitud_Eliminar(int codEmpresa, int cpr_id)
        {
            var r = DbHelper.WithConn<int>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);
                return conn.Execute("exec spCPR_Solicitud_Eliminar @Id;", new { Id = cpr_id });
            });

            if (r.Code != 0)
                return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            return DbHelper.OkResponse(cpr_id.ToString());
        }

        // ===========================
        //  DETALLE BS
        // ===========================

        public ErrorDto<List<CprSolicitudBsDto>> CprSolicitudBs_Obtener(int codEmpresa, int? cpr_id, string? cod_unidad)
        {
            var r = DbHelper.WithConn<List<CprSolicitudBsDto>>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                var sql = "exec spCPR_SolicitudDetalle_Consultar @CprId, @CodUnidad;";
                var items = conn.Query<CprSolicitudBsDto>(sql, new { CprId = cpr_id, CodUnidad = cod_unidad }).ToList();

                // batch unidades (evita N+1)
                var productos = items.Select(x => x.cod_producto).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                if (productos.Count > 0)
                {
                    const string qUnidades = "SELECT COD_PRODUCTO, COD_UNIDAD FROM PV_PRODUCTOS WHERE COD_PRODUCTO IN @Productos;";
                    var map = conn.Query<(string COD_PRODUCTO, string COD_UNIDAD)>(qUnidades, new { Productos = productos })
                                  .ToDictionary(x => x.COD_PRODUCTO, x => x.COD_UNIDAD);

                    foreach (var it in items.Where(it => !string.IsNullOrWhiteSpace(it.cod_producto) && map.ContainsKey(it.cod_producto)))
                    {
                        it.unidad = map[it.cod_producto];
                    }
                }

                return items;
            });

            return WrapList(r);
        }

        public ErrorDto CprSolicitudBs_Guardar(int codEmpresa, bool editaBs, CprSolicitudBsDto solicitud)
        {
            var r = DbHelper.WithConn<int>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                var xmlOutput = MProGrXAuxiliarDB.fxConvertModelToXml<CprSolicitudBsDto>(solicitud);
                var affected = conn.Execute("exec spCPR_SolicitudDetalle_Guardar @Xml;", new { Xml = xmlOutput });

                // valida compra directa
                var tipoOrden = conn.QueryFirstOrDefault<string>(
                    "SELECT TIPO_ORDEN FROM CPR_SOLICITUD WHERE CPR_ID = @Id;",
                    new { Id = solicitud.cpr_id });

                var tipoEx = CprSolicitud_TipoExcepcion(codEmpresa).Description ?? "";
                if (!string.IsNullOrWhiteSpace(tipoOrden) &&
                    string.Equals(tipoOrden, tipoEx, StringComparison.OrdinalIgnoreCase))
                {
                    CompraDirectaProvBs_Guardar(codEmpresa, solicitud);
                }

                return affected;
            });

            if (r.Code != 0)
                return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            return DbHelper.OkResponse((solicitud.cpr_id).ToString());
        }

        public ErrorDto CprSolicitudBs_Eliminar(int codEmpresa, int cpr_id, string cod_producto, string cod_unidad)
        {
            var r = DbHelper.WithConn<int>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);
                return conn.Execute("exec spCPR_SolicitudDetalle_Eliminar @CprId, @CodProducto, @CodUnidad;",
                    new { CprId = cpr_id, CodProducto = cod_producto, CodUnidad = cod_unidad });
            });

            if (r.Code != 0)
                return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            return DbHelper.OkResponse(cpr_id.ToString());
        }

        // ===========================
        //  LISTAS SIMPLES
        // ===========================

        public ErrorDto<List<CprValoracionLista>> CprValoracionesLista_Obtener(int codEmpresa)
        {
            var r = DbHelper.WithConn<List<CprValoracionLista>>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);
                return conn.Query<CprValoracionLista>("select VAL_ID as item, descripcion from CPR_VALORA_ESQUEMA;").ToList();
            });

            return WrapList(r);
        }

        public ErrorDto<List<CprUensLista>> CprUens_Obtener(int codEmpresa, string usuario)
        {
            var r = DbHelper.WithConn<List<CprUensLista>>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                const string sql = @"
                            SELECT
                                R.COD_UNIDAD item,
                                U.DESCRIPCION,
                                (select TOP 1 DESCRIPCION from CNTX_UNIDADES WHERE COD_UNIDAD = U.CNTX_UNIDAD) AS CNTX_UNIDAD,
                                (select TOP 1 DESCRIPCION from CNTX_CENTRO_COSTOS WHERE COD_CENTRO_COSTO = U.CNTX_CENTRO_COSTO) AS CNTX_CENTRO_COSTO
                            FROM CORE_UENS_USUARIOS_ROLES R
                            LEFT JOIN CORE_UENS U ON R.COD_UNIDAD = U.COD_UNIDAD
                            WHERE R.CORE_USUARIO = @Usuario;";

                return conn.Query<CprUensLista>(sql, new { Usuario = usuario }).ToList();
            });

            return WrapList(r);
        }

        public ErrorDto<List<CprValoracionLista>> CprSolicitudUens_Obtener(int codEmpresa)
        {
            var r = DbHelper.WithConn<List<CprValoracionLista>>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);
                return conn.Query<CprValoracionLista>(@"
                                SELECT [COD_UNIDAD] AS ITEM, [DESCRIPCION]
                                FROM [dbo].[CORE_UENS];").ToList();
            });

            return WrapList(r);
        }

        // ===========================
        //  PLAN / CANT PLAN
        // ===========================

        public ErrorDto CprSolicitudBuscaProdPlan_Obtener(int codEmpresa, string cod_producto)
        {
            var r = DbHelper.WithConn<int>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);
                return conn.Query<int>("exec spCPR_Solicitud_ProductoPlan_Consultar @Cod;", new { Cod = cod_producto }).FirstOrDefault();
            });

            if (r.Code != 0)
                return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            return DbHelper.OkResponse(r.Result.ToString());
        }

        public ErrorDto CprSolicitudBuscaProdCantPlan_Obtener(int codEmpresa, string cod_producto, float cantidad)
        {
            var r = DbHelper.WithConn<string>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                var parametros = new DynamicParameters();
                parametros.Add("@cod_producto", cod_producto);
                parametros.Add("@cantidad", cantidad);
                parametros.Add("@ValorSalida", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                conn.Execute("spCPR_SolicitudProductoCantPlan_Consultar", parametros, commandType: CommandType.StoredProcedure);

                return parametros.Get<string>("@ValorSalida") ?? "";
            });

            if (r.Code != 0)
                return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            return DbHelper.OkResponse(r.Result ?? "");
        }

        // ===========================
        //  SEGUIMIENTO
        // ===========================

        public ErrorDto<List<CprSolicitudSeguimientoDto>> Segumiento_Obtener(int codEmpresa, int cod_solicitud)
        {
            var r = DbHelper.WithConn<List<CprSolicitudSeguimientoDto>>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                const string sql = @"
                                SELECT
                                    REGISTRO_FECHA, REGISTRO_USUARIO,
                                    AUTORIZA_FECHA, AUTORIZA_USUARIO,
                                    MODIFICA_FECHA, MODIFICA_USUARIO,
                                    PRESUPUESTO_USUARIO, PRESUPUESTO_FECHA,
                                    ADJUDICA_USUARIO, ADJUDICA_FECHA,
                                    DETALLE_SEGUIMIENTO
                                FROM CPR_SOLICITUD
                                WHERE CPR_ID = @Id;";

                return conn.Query<CprSolicitudSeguimientoDto>(sql, new { Id = cod_solicitud }).ToList();
            });

            return WrapList(r);
        }

        // ===========================
        //  COTIZACIONES
        // ===========================

        public ErrorDto<CprSolicitudCotizacionPrvBsLista> CprSolicitudCotizacionBs_Obtener(int codEmpresa, int? cpr_id, string? cod_unidad, string cod_cotizacion)
        {
            var r = DbHelper.WithConn<CprSolicitudCotizacionPrvBsLista>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                var lista = new CprSolicitudCotizacionPrvBsLista();

                var hasCot = !string.IsNullOrWhiteSpace(cod_cotizacion);

                var sql = hasCot
                    ? "exec [spCPR_SolicitudCotizacion_Consultar] @CprId, @CodUnidad, @CodCot;"
                    : "exec [spCPR_SolicitudCotizacion_Consultar] @CprId, @CodUnidad;";

                lista.cotizaciones = conn.Query<CprSolicitudCotizacionPrvBs>(sql, new
                {
                    CprId = cpr_id,
                    CodUnidad = cod_unidad,
                    CodCot = cod_cotizacion
                }).ToList();

                // batch unidades
                var productos = lista.cotizaciones.Select(x => x.cod_producto).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                if (productos.Count > 0)
                {
                    const string qUnidades = "SELECT COD_PRODUCTO, COD_UNIDAD FROM PV_PRODUCTOS WHERE COD_PRODUCTO IN @Productos;";
                    var map = conn.Query<(string COD_PRODUCTO, string COD_UNIDAD)>(qUnidades, new { Productos = productos })
                                  .ToDictionary(x => x.COD_PRODUCTO, x => x.COD_UNIDAD);

                    foreach (var entry in lista.cotizaciones
                        .Select(it => new { Item = it, CodProducto = it.cod_producto })
                        .Where(x => !string.IsNullOrWhiteSpace(x.CodProducto))
                        .Select(x =>
                        {
                            var codProducto = x.CodProducto!;
                            bool found = map.TryGetValue(codProducto, out string? unidad);
                            return new { x.Item, Found = found, Unidad = unidad };
                        })
                        .Where(x => x.Found))
                    {
                        entry.Item.unidad = entry.Unidad;
                    }
                }

                return lista;
            });

            if (r.Code != 0 || r.Result == null)
                return new ErrorDto<CprSolicitudCotizacionPrvBsLista> { Code = r.Code ?? -1, Description = r.Description ?? ErrorLiteral, Result = null };

            return new ErrorDto<CprSolicitudCotizacionPrvBsLista> { Code = 0, Result = r.Result };
        }

        // ===========================
        //  AUTORIZAR / DENEGAR
        // ===========================

        public ErrorDto AutorizaSolicitud(int codEmpresa, int cprId, string usuario)
        {
            var r = DbHelper.WithConn<ErrorDto>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                var estado = conn.QueryFirstOrDefault<string>(
                    "SELECT ESTADO FROM CPR_SOLICITUD WHERE CPR_ID = @Id;",
                    new { Id = cprId }) ?? "";

                if (estado == "A" || estado == "F")
                    return DbHelper.ErrorResponse("El estado de la solicitud no permite autorizarla", -1);

                conn.Execute(@"
UPDATE CPR_SOLICITUD
SET estado = 'A', AUTORIZA_FECHA = GETDATE(), AUTORIZA_USUARIO = @Usuario
WHERE CPR_ID = @Id;",
                    new { Usuario = usuario, Id = cprId });

                return DbHelper.CreateOkResponse();
            });

            if (r.Code != 0 || r.Result == null)
                return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            return r.Result;
        }

        public ErrorDto DeniegaSolicitud(int codEmpresa, int cprId, string usuario, string detalle_seguimiento)
        {
            var r = DbHelper.WithConn<int>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                return conn.Execute(@"
UPDATE CPR_SOLICITUD
SET estado = 'D',
    AUTORIZA_FECHA = GETDATE(),
    MODIFICA_USUARIO = @Usuario,
    detalle_seguimiento = @Detalle
WHERE CPR_ID = @Id;",
                    new { Usuario = usuario, Detalle = detalle_seguimiento ?? "", Id = cprId });
            });

            if (r.Code != 0)
                return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            _ = CorreoNotificacionDevolucion_Enviar(codEmpresa, cprId);
            return DbHelper.CreateOkResponse();
        }

        // ===========================
        //  VALIDAR PERMISOS
        // ===========================

        public ErrorDto ValidaUsuarioSolicitud(int codEmpresa, string usuario, string permiso, string? cod_unidad)
        {
            var r = DbHelper.WithConn<string>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                const string sql = @"
SELECT TOP 1 'X'
FROM CORE_UENS_USUARIOS_ROLES R
LEFT JOIN CORE_UENS U ON R.COD_UNIDAD = U.COD_UNIDAD
WHERE R.CORE_USUARIO = @Usuario
  AND (@CodUnidad IS NULL OR @CodUnidad = '' OR R.COD_UNIDAD = @CodUnidad)
  AND (
        (@Permiso = 'C' AND R.ROL_CONSULTA = 1)
     OR (@Permiso = 'A' AND R.ROL_AUTORIZA  = 1)
     OR (@Permiso = 'S' AND R.ROL_SOLICITA  = 1)
     OR (@Permiso = 'E' AND R.ROL_ENCARGADO = 1)
     OR (@Permiso = 'L' AND R.ROL_LIDER     = 1)
     OR (@Permiso IS NULL OR @Permiso = '')
  );";

                return conn.QueryFirstOrDefault<string>(
                    sql,
                    new
                    {
                        Usuario = usuario,
                        CodUnidad = cod_unidad ?? string.Empty,
                        Permiso = permiso
                    }
                ) ?? string.Empty;
            });

            if (r.Code != 0)
                return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            if (string.IsNullOrWhiteSpace(r.Result))
                return DbHelper.ErrorResponse("El usuario no tiene permisos para realizar esta acción", -1);

            return DbHelper.CreateOkResponse();
        }

        // ===========================
        //  ARTICULOS PLAN
        // ===========================

        public ErrorDto<ArticuloDataLista> Articulos_Obtener(int codEmpresa, int? pagina, int? paginacion, string? filtro, string? cod_unidad)
        {
            var codUnidad = (cod_unidad ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(codUnidad))
                return DbHelper.CreateErrorResponse<ArticuloDataLista>("Debe indicar la unidad solicitante para consultar artículos del plan de compras");

            var filtroBusqueda = (filtro ?? string.Empty).Trim();
            var usaPaginacion = pagina.HasValue && paginacion.HasValue && pagina.Value >= 0 && paginacion.Value > 0;

            var r = DbHelper.WithConn<ArticuloDataLista>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                var p = new DynamicParameters();
                p.Add("CodUnidad", codUnidad);
                p.Add("Filtro", filtroBusqueda);
                p.Add("Offset", usaPaginacion ? pagina!.Value : 0);
                p.Add("Fetch", usaPaginacion ? paginacion!.Value : int.MaxValue);

                const string qCount = @"
                            SELECT COUNT(*) FROM (
                                SELECT DISTINCT D.COD_PRODUCTO
                                FROM CPR_PLAN_DT D
                                INNER JOIN CPR_PLAN_COMPRAS C ON D.ID_PC = C.ID_PC
                                    AND (RTRIM(LTRIM(C.COD_UNIDAD)) = @CodUnidad OR RTRIM(LTRIM(C.COD_UNIDAD_DESTINO)) = @CodUnidad)
                                INNER JOIN CPR_PLAN_DT_CORTES S ON D.ID_PLAN = S.ID_PLAN
                                INNER JOIN PV_PRODUCTOS P ON D.COD_PRODUCTO = P.COD_PRODUCTO
                                WHERE S.CORTE >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
                                  AND (
                                        NULLIF(@Filtro, '') IS NULL
                                        OR D.COD_PRODUCTO LIKE '%' + @Filtro + '%'
                                        OR P.DESCRIPCION LIKE '%' + @Filtro + '%'
                                      )
                            ) T";

                var total = conn.Query<int>(qCount, p).FirstOrDefault();

                const string qList = @"
                            SELECT DISTINCT
                                D.COD_PRODUCTO,
                                P.DESCRIPCION,
                                P.COSTO_REGULAR,
                                P.EXISTENCIA,
                                P.COD_BARRAS,
                                P.CABYS,
                                P.PRECIO_REGULAR,
                                P.IMPUESTO_VENTAS,
                                P.COD_FABRICANTE,
                                P.I_STOCK
                            FROM CPR_PLAN_DT D
                            INNER JOIN CPR_PLAN_COMPRAS C ON D.ID_PC = C.ID_PC
                                AND (RTRIM(LTRIM(C.COD_UNIDAD)) = @CodUnidad OR RTRIM(LTRIM(C.COD_UNIDAD_DESTINO)) = @CodUnidad)
                            INNER JOIN CPR_PLAN_DT_CORTES S ON D.ID_PLAN = S.ID_PLAN
                            INNER JOIN PV_PRODUCTOS P ON D.COD_PRODUCTO = P.COD_PRODUCTO
                            WHERE S.CORTE >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
                              AND (
                                    NULLIF(@Filtro, '') IS NULL
                                    OR D.COD_PRODUCTO LIKE '%' + @Filtro + '%'
                                    OR P.DESCRIPCION LIKE '%' + @Filtro + '%'
                                  )
                            ORDER BY D.COD_PRODUCTO
                            OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

                var articulos = conn.Query<ArticuloData>(qList, p).ToList();

                return new ArticuloDataLista
                {
                    Total = total,
                    Articulos = articulos
                };
            });

            return WrapRequired(r);
        }

        /// <summary>
        /// Obtiene artículos generales desde PV_PRODUCTOS para búsqueda reutilizable.
        /// </summary>
        public ErrorDto<ArticuloDataLista> CprSolicitud_ArticulosGenerales_Obtener(int codEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            var filtroBusqueda = (filtro ?? string.Empty).Trim();
            var usaPaginacion = pagina.HasValue && paginacion.HasValue && pagina.Value >= 0 && paginacion.Value > 0;

            var r = DbHelper.WithConn<ArticuloDataLista>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                var p = new DynamicParameters();
                p.Add("Filtro", filtroBusqueda);
                p.Add("Offset", usaPaginacion ? pagina!.Value : 0);
                p.Add("Fetch", usaPaginacion ? paginacion!.Value : int.MaxValue);

                const string qCount = @"
                            SELECT COUNT(*)
                            FROM PV_PRODUCTOS P
                            WHERE
                                NULLIF(@Filtro, '') IS NULL
                                OR P.COD_PRODUCTO LIKE '%' + @Filtro + '%'
                                OR P.DESCRIPCION LIKE '%' + @Filtro + '%'
                                OR P.COD_BARRAS LIKE '%' + @Filtro + '%';";

                var total = conn.Query<int>(qCount, p).FirstOrDefault();

                const string qList = @"
                            SELECT
                                P.COD_PRODUCTO,
                                P.CABYS,
                                P.DESCRIPCION,
                                P.COD_BARRAS,
                                P.TIPO_PRODUCTO AS TIPO,
                                P.PRECIO_COMPRA AS PRECIO_REGULAR,
                                P.EXISTENCIA,
                                CAST(0 AS DECIMAL(18,2)) AS IMPUESTO_VENTAS,
                                ISNULL(P.COD_UNIDAD, '') AS UNIDAD,
                                CAST('' AS VARCHAR(50)) AS CODIGO,
                                CAST('' AS VARCHAR(50)) AS COD_FABRICANTE,
                                CAST(0 AS BIT) AS I_STOCK
                            FROM PV_PRODUCTOS P
                            WHERE
                                NULLIF(@Filtro, '') IS NULL
                                OR P.COD_PRODUCTO LIKE '%' + @Filtro + '%'
                                OR P.DESCRIPCION LIKE '%' + @Filtro + '%'
                                OR P.COD_BARRAS LIKE '%' + @Filtro + '%'
                            ORDER BY P.COD_PRODUCTO
                            OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

                var articulos = conn.Query<ArticuloData>(qList, p).ToList();

                return new ArticuloDataLista
                {
                    Total = total,
                    Articulos = articulos
                };
            });

            return WrapRequired(r);
        }

        // ===========================
        //  COMPRA DIRECTA PROV
        // ===========================

        public ErrorDto CompraDirectaProv_Agregar(int codEmpresa, int cpr_id, CprSolicitudDto solicitud)
        {
            var r = DbHelper.WithConn<int>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                return conn.Execute(@"
UPDATE CPR_SOLICITUD_PROV
SET PROVEEDOR_ESTADO = 'I',
    ESTADO = 'V'
WHERE CPR_ID = @Id;", new { Id = solicitud.cpr_id ?? cpr_id });
            });

            if (r.Code != 0)
                return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            return DbHelper.CreateOkResponse();
        }

        private void CompraDirectaProvBs_Guardar(int codEmpresa, CprSolicitudBsDto solicitud)
        {
            _ = DbHelper.WithConn<int>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                var existeProv = conn.Query<int>(
                    "SELECT COUNT(*) FROM CPR_SOLICITUD_PROV WHERE CPR_ID = @Id;",
                    new { Id = solicitud.cpr_id }).FirstOrDefault();

                if (existeProv == 0)
                {
                    var enc = new CprSolicitudDto
                    {
                        cpr_id = solicitud.cpr_id,
                        com_dir_cod_proveedor = solicitud.comp_dir_cod_proveedor,
                        registro_usuario = solicitud.registro_usuario
                    };

                    _ = CompraDirectaProv_Agregar(codEmpresa, solicitud.cpr_id, enc);
                }

                conn.Execute("DELETE FROM CPR_SOLICITUD_PROV_BS WHERE CPR_ID = @Id;", new { Id = solicitud.cpr_id });

                const string qDetalle = @"
INSERT INTO CPR_SOLICITUD_PROV_BS
(
    CPR_ID, COD_PRODUCTO, PROVEEDOR_CODIGO, CODIGO, MONTO, CANTIDAD, TOTAL,
    IVA_PORC, IVA_MONTO, DESC_PORC, DESC_MONTO,
    registro_fecha, registro_usuario, ESTADO, NO_COTIZACION
)
SELECT
    CPR_ID,
    COD_PRODUCTO,
    @CodProveedor AS COD_PROVEEDOR,
    NULL AS CODIGO,
    MONTO,
    CANTIDAD,
    TOTAL,
    IVA_PORC,
    IVA_MONTO,
    DESC_PORC,
    DESC_MONTO,
    GETDATE() AS registro_fecha,
    registro_usuario,
    'V' AS ESTADO,
    @NoCotizacion AS NO_COTIZACION
FROM CPR_SOLICITUD_BS
WHERE CPR_ID = @Id;";

                conn.Execute(qDetalle, new
                {
                    Id = solicitud.cpr_id,
                    CodProveedor = solicitud.comp_dir_cod_proveedor,
                    NoCotizacion = solicitud.comp_dir_documento ?? ""
                });

                return 1;
            });
        }

        // ===========================
        //  CORREOS
        // ===========================

        public async Task<ErrorDto> CorreoNotificacionDevolucion_Enviar(int codEmpresa, int cprId)
        {
            var info = new ErrorDto { Code = 0 };

            try
            {
                // Config correo
                var cfg = _envioCorreoDB.CorreoConfig(codEmpresa, _notificaciones);
                if (cfg == null || cfg.Code != 0 || cfg.Result == null)
                    return DbHelper.ErrorResponse("No se pudo obtener la configuración de correo.", -1);

                var eConfig = cfg.Result;

                // datos correo
                var datos = DbHelper.WithConn<(string Detalle, string Usuario, string Correo)>(_portalDb, codEmpresa, conn =>
                {
                    EnsureOpen(conn);

                    var detalle = conn.QueryFirstOrDefault<string>(
                        "SELECT detalle_seguimiento FROM CPR_SOLICITUD WHERE cpr_id = @Id;",
                        new { Id = cprId }) ?? "";

                    var usuario = conn.QueryFirstOrDefault<string>(
                        "SELECT autoriza_usuario FROM CPR_SOLICITUD WHERE cpr_id = @Id;",
                        new { Id = cprId }) ?? "";

                    // OJO: en tu código original esta consulta se veía rara (USUARIOS y cpr_id = usuario).
                    // La dejo igual, pero parametrizada.
                    var correo = conn.QueryFirstOrDefault<string>(
                        "SELECT email FROM USUARIOS WHERE cpr_id = @Usuario;",
                        new { Usuario = usuario }) ?? "";

                    return (detalle, usuario, correo);
                });

                if (datos.Code != 0) return DbHelper.ErrorResponse(datos.Description ?? ErrorLiteral, datos.Code ?? -1);

                var body = @$"<html lang=""es""><body>
<div style=""font-family: Arial, sans-serif;"">
  <h2><strong>Anulación Solicitud de Compra</strong></h2>
  <p>No. Solicitud <strong>{cprId}</strong></p>
  <p>Mediante la presente se le comunica la anulación de la Solicitud de Compra #{cprId}</p>
  <p>Justificación: {datos.Result.Detalle}</p>
</div>
</body></html>";

                if (_sendEmail == "Y")
                {
                    var emailRequest = new EmailRequest
                    {
                        To = datos.Result.Correo,
                        From = eConfig.User,
                        Subject = "Anulación de Solicitud",
                        Body = body
                    };

                    await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, info);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;
        }

        private async Task CorreoNotificaSolicitud_Enviar(int codEmpresa, int cpr_id, string usuario)
        {
            var resp = new ErrorDto();
            var solicitudMascara = cpr_id.ToString("D10");

            try
            {
                // correo del usuario asignado
                var emailR = DbHelper.WithConn<string>(_portalDb, codEmpresa, conn =>
                {
                    EnsureOpen(conn);
                    return conn.QueryFirstOrDefault<string>(
                        "SELECT EMAIL FROM CORE_USUARIOS WHERE CORE_USUARIO = @Usuario;",
                        new { Usuario = usuario }) ?? "";
                });

                if (emailR.Code != 0 || string.IsNullOrWhiteSpace(emailR.Result))
                    return;

                var cfg = _envioCorreoDB.CorreoConfig(codEmpresa, _notificaciones);
                if (cfg == null || cfg.Code != 0 || cfg.Result == null)
                    return;

                var eConfig = cfg.Result;

                var body = @$"<html lang=""es""><body>
<div style=""font-family: Arial, sans-serif;"">
  <h2><strong>Notificación de asignación de Solicitud de Compra</strong></h2>
  <p>Estimado/a {usuario} se le comunica la asignación de la solicitud de compra #{solicitudMascara}</p>
</div>
</body></html>";

                if (_sendEmail == "Y")
                {
                    var emailRequest = new EmailRequest
                    {
                        To = emailR.Result,
                        From = eConfig.User,
                        Subject = "Asignación de Solicitud de Compra",
                        Body = body,
                        Attachments = new List<IFormFile>()
                    };

                    await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, resp);
                }
            }
            catch
            {
                // si falla correo no tumbamos el flujo
            }
        }

        // ===========================
        //  CONFIG / PARAMS
        // ===========================

        public ErrorDto CprSolicitud_TipoExcepcion(int codEmpresa)
        {
            try
            {
                return new ErrorDto
                {
                    Code = 0,
                    Description = _config.GetSection("Crp_Compras").GetSection("CrpCompraDirecta").Value?.ToString() ?? ""
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        public ErrorDto CprSolicitud_TipoExcepcionGM(int codEmpresa)
        {
            try
            {
                return new ErrorDto
                {
                    Code = 0,
                    Description = _config.GetSection("Crp_Compras").GetSection("CrpCompraGastoMenor").Value?.ToString() ?? ""
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        public ErrorDto<decimal> CprSolicitud_gastoMenorMonto(int codEmpresa)
        {
            var info = new ErrorDto<decimal>();
            try
            {
                info.Code = 0;
                var val = _config.GetSection("Crp_Compras").GetSection("CrpCompraGM_Monto").Value?.ToString() ?? "0";
                info.Result = decimal.Parse(val);
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = 0;
            }
            return info;
        }

        // ===========================
        //  ASIGNAR ENCARGADO
        // ===========================

        public ErrorDto AsignaEncargado_Solicitud(int codEmpresa, string cod_unidad, int cpr_id)
        {
            var r = DbHelper.WithConn<int>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);
                return conn.Execute("exec spCpr_AsignaEncargadoPorSolicitud @Id, @CodUnidad;",
                    new { Id = cpr_id, CodUnidad = cod_unidad });
            });

            if (r.Code != 0)
                return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            return DbHelper.OkResponse("Registro actualizado satisfactoriamente");
        }

        // ===========================
        //  ENCARGADOS / USUARIOS
        // ===========================

        public ErrorDto<List<EncargadosDto>> Encargados_Obtener(int codEmpresa, int cod_unidad)
        {
            var r = DbHelper.WithConn<List<EncargadosDto>>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                const string sql = @"
                            SELECT u.CORE_USUARIO, u.NOMBRE, u.EMAIL, ur.COD_UNIDAD
                            FROM CORE_UENS_USUARIOS_ROLES ur
                            INNER JOIN CORE_USUARIOS u ON ur.CORE_USUARIO = u.CORE_USUARIO
                            WHERE ur.ROL_ENCARGADO = 1 AND ur.COD_UNIDAD = @CodUnidad;";

                return conn.Query<EncargadosDto>(sql, new { CodUnidad = cod_unidad }).ToList();
            });

            return WrapList(r);
        }

        public ErrorDto<List<string>> CprSolicitud_UsuariosSolicitantes_Obtener(int codEmpresa)
        {
            var r = DbHelper.WithConn<List<string>>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);
                return conn.Query<string>(@"
                                SELECT REGISTRO_USUARIO
                                FROM CPR_SOLICITUD
                                GROUP BY REGISTRO_USUARIO;").ToList();
            });

            return WrapList(r);
        }

        public ErrorDto<List<string>> CprSolicitud_UsuariosEncargados_Obtener(int codEmpresa)
        {
            var r = DbHelper.WithConn<List<string>>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);
                return conn.Query<string>(@"
                            SELECT ENCARGADO_USUARIO
                            FROM CPR_SOLICITUD
                            WHERE ENCARGADO_USUARIO IS NOT NULL
                            GROUP BY ENCARGADO_USUARIO;").ToList();
            });

            return WrapList(r);
        }

        // ===========================
        //  HELPERS
        // ===========================

        private ErrorDto<List<T>> WrapList<T>(ErrorDto<List<T>> r)
        {
            if (r.Code != 0)
                return new ErrorDto<List<T>>
                {
                    Code = r.Code ?? -1,
                    Description = r.Description ?? ErrorLiteral,
                    Result = null
                };

            return new ErrorDto<List<T>>
            {
                Code = 0,
                Result = r.Result ?? new List<T>()
            };
        }

        private ErrorDto<T> WrapRequired<T>(ErrorDto<T> r, string? nullMessage = null) where T : class
        {
            if (r.Code != 0)
                return new ErrorDto<T>
                {
                    Code = r.Code ?? -1,
                    Description = r.Description ?? ErrorLiteral,
                    Result = null
                };

            if (r.Result == null)
                return new ErrorDto<T>
                {
                    Code = -1,
                    Description = nullMessage ?? ErrorLiteral,
                    Result = null
                };

            return new ErrorDto<T> { Code = 0, Result = r.Result };
        }

        private static string NormalizeLike(string? filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return string.Empty;

            var f = filtro.Trim();
            return f.Length == 0 ? string.Empty : $"%{f}%";
        }

        private static void EnsureOpen(IDbConnection conn)
        {
            if (conn.State != ConnectionState.Open) conn.Open();
        }
    }
}
