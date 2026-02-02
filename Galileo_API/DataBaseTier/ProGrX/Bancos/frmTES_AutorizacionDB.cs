using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.TES;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesAutorizacionDb
    {
        private readonly VerificadorCoreFactory _factory;
        private readonly MTesoreria _mTesoreria;
        private readonly PortalDB _portalDB;

        private const string SQL_UPDATE_EMISION =
     @"UPDATE Tes_Transacciones 
            SET Autoriza='S',
                Fecha_Autorizacion = dbo.MyGetdate(), 
                User_Autoriza = @usuario,
                ESTADO_SINPE = @estado_sinpe,
                TIPO_GIROSINPE = @tipo_giro_sinpe,
                USUARIO_AUTORIZA_ESPECIAL = @usuarioEspecial
          WHERE Nsolicitud = @nsolicitud";

        private const string SQL_UPDATE_FIRMAS =
            @"UPDATE Tes_Transacciones 
            SET FIRMAS_AUTORIZA_FECHA = dbo.MyGetdate(),
                FIRMAS_AUTORIZA_USUARIO = @usuario
          WHERE Nsolicitud = @nsolicitud";

        private const string SQL_TES_AUTORIZACIONES_RANGOS = @"
SELECT rango_gen_Inicio, rango_gen_corte, firmas_gen_inicio, firmas_gen_corte
FROM TES_AUTORIZACIONES
WHERE NOMBRE = @usuario";

        private const string SQL_BITACORA_EMISION = "EXEC spTesBitacora @nsolicitud,'02','',@usuario";
        private const string SQL_BITACORA_FIRMAS = "EXEC spTesBitacora @nsolicitud,'04','',@usuario";


        public FrmTesAutorizacionDb(IConfiguration config)
        {
            _mTesoreria = new MTesoreria(config);
            _portalDB = new PortalDB(config);
            _factory = new VerificadorCoreFactory(config);
        }

        /// <summary>
        /// Obtener solicitudes pendientes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TesSolicitudesLista> TES_SolicitudesPendientes_Obtener(int CodEmpresa, string filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var filtro = ParseFiltros(filtros);
            var response = NewOkResponse();

            // Rango de fecha (inicio del día / fin del día)
            var fechaInicio = filtro.fecha_inicio.Date;
            var fechaCorte = filtro.fecha_corte.Date.AddDays(1).AddTicks(-1);
            try
            {
                // 1) Ajustar rangos de montos por usuario (si existen)
                AjustarRangosPorUsuario(conn, filtro);

                // 2) Supervisión banco e interbancaria
                var lenInter = GetInterbancariaLength(conn, filtro.id_banco);

                // 3) Revisión automática (SP)
                EjecutarRevisionAutomatica(conn, filtro.id_banco);

                // 4) Conteo total
                response.Result!.total = GetConteoPendientes(conn, filtro.id_banco, filtro.tipo_doc);

                // 5) Construcción de query dinámica
                var baseQuery = @"
                SELECT 
                    T.nsolicitud, T.codigo, T.beneficiario, T.monto, T.fecha_solicitud, T.cta_Ahorros,
                    CASE WHEN @Duplicados = 1
                         THEN dbo.fxTesSupervisa(CODIGO,BENEFICIARIO,monto,0,'T')
                         ELSE 0
                    END AS duplicado,
                    dbo.fxTes_Cuenta_Verifica(T.id_banco,T.codigo,T.cta_ahorros) AS Cta_Verifica,
                    T.Detalle1 + T.detalle2 AS Detalle, ISNULL(T.cod_App,'') AS AppId,
                    IIF(T.user_hold IS NULL, 0, 1) AS Bloqueo, S.ESTADOACTUAL
                FROM Tes_Transacciones T 
                INNER JOIN Tes_Bancos B ON T.id_banco = B.id_banco
                INNER JOIN Socios S ON T.CODIGO = S.CEDULA
                WHERE T.estado = 'P' AND B.id_banco = @Banco AND T.Tipo = @TipoDoc"; 
                var (query, sqlParams) = BuildFinalQueryAndParams(
                    conn, baseQuery, filtro, fechaInicio, fechaCorte, lenInter);

                // 6) Ejecución
                response.Result.solicitudes = conn
                    .Query<Galileo.Models.TES.TesSolicitudesData>(query, sqlParams)
                    .ToList();

                if(filtro.tipo_doc == "TS" && filtro.activaCuentaSinpe)
                {
                    foreach (var solicitud in response.Result.solicitudes)
                    {
                        var valida = _factory.CrearServicio(CodEmpresa, filtro.usuario)
                           .fxValidacionSinpe(CodEmpresa, solicitud.nsolicitud.ToString(), filtro.usuario);
                        if (valida.Code != 0 && valida.Code != 1)
                        {
                            solicitud.bloqueo = true;
                            solicitud.detalle = valida.Description;
                        }
                    }

                }

                return response;
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TesSolicitudesLista>($"Error al obtener las solicitudes pendientes: {ex.Message}");
            }
            
        }

        // ====== Helpers de TES_SolicitudesPendientes_Obtener ======
        private static TesAutorizacionFiltros ParseFiltros(string? json)
            => JsonConvert.DeserializeObject<TesAutorizacionFiltros>(json ?? "{}")
               ?? new TesAutorizacionFiltros();

        private static ErrorDto<TesSolicitudesLista> NewOkResponse() => new()
        {
            Result = new TesSolicitudesLista
            {
                solicitudes = new List<Galileo.Models.TES.TesSolicitudesData>(),
                total = 0
            },
            Code = 0,
            Description = "OK"
        };

        private static void AjustarRangosPorUsuario(SqlConnection conn, TesAutorizacionFiltros filtro)
        {
            const string sql = SQL_TES_AUTORIZACIONES_RANGOS;

            var r = conn.Query<TesAutorizacionData>(sql, new { filtro.usuario }).FirstOrDefault();
            if (r != null)
            {
                filtro.monto_inicio = r.rango_gen_inicio ?? 0;
                filtro.monto_fin = r.rango_gen_corte ?? 0;
            }
        }

        private static int GetInterbancariaLength(SqlConnection conn, int idBanco)
        {
            const string sql = @"
            SELECT Bg.LCTA_INTERBANCARIA 
            FROM TES_BANCOS Tb 
            INNER JOIN TES_BANCOS_GRUPOS Bg ON Tb.COD_GRUPO = Bg.COD_GRUPO
            WHERE Tb.ID_BANCO = @Banco";
            return conn.Query<int?>(sql, new { Banco = idBanco }).FirstOrDefault() ?? 0;
        }

        private static void EjecutarRevisionAutomatica(SqlConnection conn, int idBanco)
        {
            const string sql = "EXEC spTes_Cuentas_Revision_Automatica @Banco";
            conn.Execute(sql, new { Banco = idBanco });
        }

        private static int GetConteoPendientes(SqlConnection conn, int idBanco, string tipoDoc)
        {
            const string sql = @"
            SELECT COUNT(T.nsolicitud) 
            FROM Tes_Transacciones T 
            INNER JOIN Tes_Bancos B ON T.id_banco = B.id_banco
            WHERE T.estado = 'P' AND B.id_banco = @Banco AND T.Tipo = @TipoDoc";
            return conn.Query<int>(sql, new { Banco = idBanco, TipoDoc = tipoDoc }).FirstOrDefault();
        }


        private static (string sql, DynamicParameters param) BuildFinalQueryAndParams(
        SqlConnection conn,
        string baseQuery,
        TesAutorizacionFiltros filtro,
        DateTime fechaInicio,
        DateTime fechaCorte,
        int lenInterbancaria)
        {
            var sb = new StringBuilder(baseQuery);
            var p = BuildBaseParams(filtro, fechaInicio, fechaCorte);

            AppendDateFilter(sb, filtro.todas_fechas);
            AppendSolicitudFilter(sb, filtro.todas_solicitudes);
            AppendBloqueoFilter(sb, filtro.casos_bloqueados);

            if (EsTransferencia(filtro.tipo_doc))
            {
                AppendCuentaTipoFilter(sb, filtro.tipo_cuenta, lenInterbancaria);
                AppendMismoBancoFilter(conn, sb, filtro.mismo_banco, filtro.id_banco, lenInterbancaria);
            }

            AppendAutorizacionFilter(sb,  filtro);
            AppendDetalleFilter(sb,  filtro.detalle);
            AppendAppFilter(sb, filtro.appid);

            sb.Append(" ORDER BY T.nsolicitud ASC, T.fecha_solicitud ASC");

            return (sb.ToString(), p);
        }

        private static DynamicParameters BuildBaseParams(TesAutorizacionFiltros f, DateTime ini, DateTime fin)
        {
            var p = new DynamicParameters();
            p.Add("Banco", f.id_banco);
            p.Add("TipoDoc", f.tipo_doc);
            p.Add("Usuario", f.usuario);
            p.Add("FechaInicio", ini);
            p.Add("FechaFin", fin);
            p.Add("SolicitudInicio", f.solicitud_inicio);
            p.Add("SolicitudCorte", f.solicitud_corte);
            p.Add("MontoInicio", f.monto_inicio);
            p.Add("MontoFin", f.monto_fin);
            p.Add("Token", f.token);
            p.Add("Detalle", $"%{f.detalle}%");
            p.Add("CodigoApp", $"%{f.appid}%");
            p.Add("Duplicados", f.duplicados ? 1 : 0);
            return p;
        }

        private static void AppendDateFilter(StringBuilder sb, bool todasFechas)
        {
            if (!todasFechas)
                sb.Append(" AND T.fecha_solicitud BETWEEN @FechaInicio AND @FechaFin ");
        }

        private static void AppendSolicitudFilter(StringBuilder sb, bool todasSolicitudes)
        {
            if (!todasSolicitudes)
                sb.Append(" AND (T.nsolicitud >= @SolicitudInicio AND T.nsolicitud <= @SolicitudCorte) ");
        }

        private static void AppendBloqueoFilter(StringBuilder sb, bool incluirBloqueados)
        {
            if (!incluirBloqueados)
                sb.Append(" AND T.fecha_hold IS NULL ");
        }

        private static bool EsTransferencia(string? tipoDoc)
            => string.Equals(tipoDoc, "TE", StringComparison.OrdinalIgnoreCase);

        private static void AppendCuentaTipoFilter(StringBuilder sb, string? tipoCuenta, int lenInter)
        {
            if (string.IsNullOrWhiteSpace(tipoCuenta)) return;

            switch (tipoCuenta.ToUpperInvariant())
            {
                case "L": // Locales
                    sb.Append($" AND LEN(RTRIM(T.cta_Ahorros)) <> {lenInter} ");
                    break;
                case "I": // Interbancarias
                    sb.Append($" AND LEN(RTRIM(T.cta_Ahorros)) = {lenInter} ");
                    break;
                default:
                    // Todas: sin filtro
                    break;
            }
        }

        private static void AppendMismoBancoFilter(
            SqlConnection conn,
            StringBuilder sb,
            bool mismoBanco,
            int idBanco,
            int lenInter)
        {
            if (!mismoBanco) return;

            const string sqlGrupo = "SELECT dbo.fxTes_BancoSFN(@Banco) AS Codigo";
            var grupo = conn.Query<int?>(sqlGrupo, new { Banco = idBanco }).FirstOrDefault() ?? 0;

            // SUBSTRING(...,1,10) LIKE '%grupo%' y largo interbancario
            sb.Append($" AND (SUBSTRING(RTRIM(T.cta_Ahorros), 1, 10) LIKE '%{grupo}%' AND LEN(RTRIM(T.cta_Ahorros)) = {lenInter}) ");
        }

        private static void AppendAutorizacionFilter(StringBuilder sb, TesAutorizacionFiltros f)
        {
            // 0 = Emisión (usa rango y token opcional)
            if (f.tipo_autorizacion == 0)
            {
                sb.Append(" AND T.fecha_autorizacion IS NULL AND T.monto BETWEEN @MontoInicio AND @MontoFin ");
                if (!string.IsNullOrWhiteSpace(f.token))
                    sb.Append(" AND T.id_token = @Token ");
                return;
            }

            // Firmas
            sb.Append(@" AND T.FIRMAS_AUTORIZA_FECHA IS NULL 
                     AND T.monto > B.firmas_hasta 
                     AND dbo.fxTesAutorizaFirmaAcceso(@Usuario, @Banco, T.monto) = 1 ");
        }

        private static void AppendDetalleFilter(StringBuilder sb,  string? detalle)
        {
            if (!string.IsNullOrWhiteSpace(detalle))
                sb.Append(" AND (T.DETALLE1 + T.DETALLE2) LIKE @Detalle ");
        }

        private static void AppendAppFilter(StringBuilder sb, string? appId)
        {
            if (!string.IsNullOrWhiteSpace(appId))
                sb.Append(" AND ISNULL(T.COD_APP,'') LIKE @CodigoApp ");
        }


        /// <summary>
        /// Aplicar autorizaci�n de solicitudes pendientes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="clave"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo_autorizacion"></param>
        /// <param name="solicitudesLista"></param>
        /// <returns></returns>
        public ErrorDto TES_Autorizacion_Aplicar(TesAutorizaParametros nsolicitud)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, nsolicitud.codEmpresa);

            var solicitudes = DeserializeLista(nsolicitud.solicitudesLista);
            try
            {
                if (!UsuarioAutorizado(conn, nsolicitud))
                    return DbHelper.ErrorResponse("Contrase&ntilde;a Incorrecta, o no Existe Nivel de Autorizaci&oacute;n", -2);


                var resultado = ProcesarSolicitudes(conn, nsolicitud, solicitudes);

                var mensaje = resultado.codigo == 0
                ? "Autorización procesada correctamente!"
                : $"{resultado.mensaje} - Solicitud(es): {resultado.mensaje} no puede(n) ser autorizada(s) por el mismo usuario";

                return DbHelper.OkResponse(mensaje);

            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al procesar la autorizaci&oacute;n: {ex.Message}");
            }
        }

        // ====== Helpers de TES_Autorizacion_Aplicar ======
        private static List<int> DeserializeLista(string? jsonLista)
            => JsonConvert.DeserializeObject<List<int>>(jsonLista ?? "[]") ?? new List<int>();

        private static bool UsuarioAutorizado(SqlConnection conn, TesAutorizaParametros p)
        {
            var SQL_AUTH = @"Select * From Tes_Autorizaciones Where Clave = @clave and nombre = @usuario and estado = 'A'";
            var auth = conn.QueryFirstOrDefault<TesAutorizacionData>(
                SQL_AUTH, new { p.clave, p.usuario });
            return auth != null;
        }

        private (int codigo, string mensaje) ProcesarSolicitudes(
        SqlConnection conn,
        TesAutorizaParametros p,
        IEnumerable<int> solicitudes)
        {
            var bloqueadasPorMismoUsuario = new List<int>();

            foreach (var id in solicitudes)
            {
                if (BloqueaPorMismoUsuario(conn, p.codEmpresa, id, p.usuario!))
                {
                    bloqueadasPorMismoUsuario.Add(id);
                    continue;
                }

                EjecutarAutorizacion(conn, id, p);
            }

            var codigo = bloqueadasPorMismoUsuario.Any() ? -1 : 0;
            var msg = bloqueadasPorMismoUsuario.Any()
                ? string.Join(",", bloqueadasPorMismoUsuario)
                : string.Empty;

            return (codigo, msg);
        }

        private bool BloqueaPorMismoUsuario(SqlConnection conn, int codEmpresa, int nsolicitud, string usuario)
        {
            // Si el parámetro 12 está en "S", no se permite auto-autorización
            if (_mTesoreria.fxTesParametro(codEmpresa, "12") != "S")
                return false;
            var SQL_USER_SOLICITA = "SELECT USER_SOLICITA FROM TES_TRANSACCIONES WHERE NSOLICITUD = @nsolicitud";
            var userSolicita = conn.QueryFirstOrDefault<string>(
                SQL_USER_SOLICITA, new { nsolicitud });

            if (string.IsNullOrWhiteSpace(userSolicita))
                return false;

            return string.Equals(userSolicita, usuario, StringComparison.OrdinalIgnoreCase);
        }

        private static void EjecutarAutorizacion(SqlConnection conn, int nsolicitud, TesAutorizaParametros p)
        {
            var (updateSql, bitacoraSql) = ConstruirQueries(p.tipo_autorizacion);
            var (estadoSinpeDb, tipoGiroSinpeDb) = NormalizarSinpe(p.estadoSinpe, p.tipoDocumento, p.tipoGiroSinpe);

            // Nota: Para Firmas, los parámetros SINPE no se usan por la query,
            // pero pasar un objeto único simplifica la firma.
            var parametros = new
            {
                usuario = p.usuario,
                nsolicitud,
                estado_sinpe = estadoSinpeDb,
                tipo_giro_sinpe = tipoGiroSinpeDb,
                usuarioEspecial = p.autorizacionEspecialUsuario
            };

            conn.Execute(updateSql, parametros);
            conn.Execute(bitacoraSql, new { usuario = p.usuario, nsolicitud });
        }

        private static (string update, string bitacora) ConstruirQueries(int tipoAutorizacion)
        {
            // 0 = Emisión; distinto de 0 = Firmas
            return tipoAutorizacion == 0
                ? (SQL_UPDATE_EMISION, SQL_BITACORA_EMISION)
                : (SQL_UPDATE_FIRMAS, SQL_BITACORA_FIRMAS);
        }

        private static (int? estadoSinpeDb, string tipoGiroSinpeDb) NormalizarSinpe(bool? estadoSinpe, string? tipoDocumento, string? tipoGiroSinpe)
        {
            // Si el documento no es "TS", no aplica SINPE
            if (!string.Equals(tipoDocumento, "TS", StringComparison.OrdinalIgnoreCase))
                return (null, "NA");

            // Documento TS: mapear bool? a int? (1/0)
            int? estado = null;
            if (estadoSinpe != null)
            {
                estado = estadoSinpe.Value ? 1 : 0;
            }
            var giro = string.IsNullOrWhiteSpace(tipoGiroSinpe) ? "NA" : tipoGiroSinpe!;
            return (estado, giro);
        }

        /// <summary>
        /// Obtener rangos de montos de autorizaci�n de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<TesAutorizacionData> TES_AutorizacionDoc_Obtener(int CodEmpresa, string usuario)
        {

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = SQL_TES_AUTORIZACIONES_RANGOS;

                return conn.Query<TesAutorizacionData>(query, new { usuario }).FirstOrDefault() ?? new TesAutorizacionData();
            });
        }

        /// <summary>
        /// Obtener rango de montos de autorizaci�n de firmas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto<TesFirmasAutData> TES_AutorizacionFirma_Obtener(int CodEmpresa, string usuario, int banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select firmas_autoriza_inicio,firmas_autoriza_corte from TES_BANCO_FIRMASAUT 
                        where USUARIO = @usuario and ID_BANCO = @banco and aplica_rango_autorizacion = 1";

                return conn.Query<TesFirmasAutData>(query, new { usuario, banco }).FirstOrDefault() ?? new TesFirmasAutData();
            });
        }


        /// <summary>
        /// Método para buscar y obtener los usuarios activos de la empresa especificada, con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TesAccesosUsuariosLista> TES_AutorizacionBuscar_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var result = new ErrorDto<TesAccesosUsuariosLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new TesAccesosUsuariosLista
                {
                    total = 0,
                    lista = new List<DropDownListaGenericaModel>()
                }
            };

            try
            {
                filtros ??= new FiltrosLazyLoadData();

                // --- Parámetros y saneo ---
                var hasFilter = !string.IsNullOrWhiteSpace(filtros.filtro);
                var filtroValor = hasFilter ? $"%{filtros.filtro}%" : null;

                // Whitelist: solo columnas permitidas
                var sortField = (filtros.sortField ?? "item").Trim();
                var sortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    // codificamos la columna a un entero para usar CASE en ORDER BY
                    ["item"] = 1,          // t.item
                    ["descripcion"] = 2,   // t.descripcion
                    ["nombre"] = 1         // "nombre" mapea a 'item'
                };
                if (!sortMap.TryGetValue(sortField, out var sortCode))
                    sortCode = 1; // default seguro

                var isAsc = filtros.sortOrder != 0; // true=ASC, false=DESC

                // Paginación (ajusta si 'pagina' es número de página y no offset)
                var pageSize = Math.Max(1, filtros.paginacion);
                var offset = Math.Max(0, filtros.pagina);

                var p = new DynamicParameters();
                p.Add("@hasFilter", hasFilter ? 1 : 0, DbType.Int32);
                p.Add("@filtro", filtroValor, DbType.String);
                p.Add("@sortCode", sortCode, DbType.Int32);
                p.Add("@isAsc", isAsc ? 1 : 0, DbType.Int32);
                p.Add("@offset", offset, DbType.Int32);
                p.Add("@pageSize", pageSize, DbType.Int32);

                // --- COUNT: SQL 100% estático ---
                var sqlCount = @"
            SELECT COUNT(1)
            FROM usuarios
            WHERE Estado = 'A'
              AND (
                    @hasFilter = 0
                 OR  Nombre      LIKE @filtro
                 OR  descripcion LIKE @filtro
              );";
                result.Result.total = conn.ExecuteScalar<int>(sqlCount, p);

                // --- DATA: SQL 100% estático; ORDER BY con CASE + flags ---
                var sqlData = @"
            WITH base AS (
                SELECT
                    Nombre       AS item,
                    RTRIM(descripcion) AS descripcion
                FROM usuarios
                WHERE Estado = 'A'
                  AND (
                        @hasFilter = 0
                     OR  Nombre      LIKE @filtro
                     OR  descripcion LIKE @filtro
                  )
            )
            SELECT item, descripcion
            FROM base t
            ORDER BY
                -- item ASC/DESC
                CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN t.item END ASC,
                CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN t.item END DESC,
                -- descripcion ASC/DESC
                CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN t.descripcion END ASC,
                CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN t.descripcion END DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;";

                result.Result.lista = conn.Query<DropDownListaGenericaModel>(sqlData, p).ToList();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TesAccesosUsuariosLista>($"Error al obtener las solicitudes pendientes: {ex.Message}");
            }

            return result;
        }

    }
}