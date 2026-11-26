using System.Data;
using Dapper;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public class FrmCprOrdenesAutorizacionDB
    {
        private readonly PortalDB _portalDb;

        public FrmCprOrdenesAutorizacionDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las órdenes pendientes con paginación y ordenamiento validado.
        /// </summary>
        public ErrorDto<OrdenCompraDto> OrdenesCompra_Autorizacion_Obtener(
            int codEmpresa,
            OrdenCompraRequestDto req)
        {
            var campoOrden = (req.sortField ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "tipoordendesc" or "tipoorden" => "tipo_orden",
                "proceso" => "proceso",
                "total" => "total",
                "genera_user" => "genera_user",
                "genera_fecha" => "genera_fecha",
                "nota" => "nota",
                _ => "cod_orden"
            };
            var direccionOrden = req.sortOrder == -1 ? -1 : 1;

            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (conn.State != ConnectionState.Open) conn.Open();

                string fechaIni = MProGrXAuxiliarDB.validaFechaGlobal(req.fechaInicio, "yyyy-MM-dd" + " 00:00:00") ?? "";
                string fechaFin = MProGrXAuxiliarDB.validaFechaGlobal(req.fechaCorte, "yyyy-MM-dd" + " 23:59:59") ?? "";

                var like = NormalizeLike(req.filtro);
                var (offset, fetch) = NormalizePaging(req.pagina, req.paginacion);

                const string sqlTotal = @"
SELECT COUNT(O.cod_orden)
FROM cpr_ordenes O
INNER JOIN cpr_Tipo_Orden C ON O.Tipo_Orden = C.Tipo_Orden
WHERE O.autoriza_fecha IS NULL
  AND O.estado = 'S'
  AND O.tipo_orden = @Tipo
  AND UPPER(O.genera_user) IN (
        SELECT UPPER(usuario_asignado)
        FROM cpr_orden_autousers
        WHERE UPPER(usuario) = @Usuario
  )
  AND (@TodosPendientes = 1 OR O.genera_fecha BETWEEN @FechaInicio AND @FechaCorte)
  AND (
        @F IS NULL
        OR CAST(O.cod_orden AS NVARCHAR(50)) LIKE @F
        OR C.Descripcion LIKE @F
        OR O.nota LIKE @F
        OR O.proceso LIKE @F
  );
";
                

      
                var total = conn.QueryFirstOrDefault<int>(
                    sqlTotal,
                    new
                    {
                        Tipo = req.tipo,
                        Usuario = req.usuario.ToUpper(),
                        TodosPendientes = req.todosPendientes ? 1 : 0,
                        FechaInicio = fechaIni,
                        FechaCorte = fechaFin,
                        F = like
                    }
                );

                const string sqlLista = @"
SELECT
    O.cod_orden,
    C.Descripcion AS TipoOrdenDesc,
    O.total,
    O.genera_user,
    O.genera_fecha,
    C.Descripcion AS TipoOrden,
    O.nota,
    O.proceso
FROM cpr_ordenes O
INNER JOIN cpr_Tipo_Orden C ON O.Tipo_Orden = C.Tipo_Orden
WHERE O.autoriza_fecha IS NULL
  AND O.estado = 'S'
  AND O.tipo_orden = @Tipo
  AND UPPER(O.genera_user) IN (
        SELECT UPPER(usuario_asignado)
        FROM cpr_orden_autousers
        WHERE UPPER(usuario) = @Usuario
  )
  AND (@TodosPendientes = 1 OR O.genera_fecha BETWEEN @FechaInicio AND @FechaCorte)
  AND (
        @F IS NULL
        OR CAST(O.cod_orden AS NVARCHAR(50)) LIKE @F
        OR C.Descripcion LIKE @F
        OR O.nota LIKE @F
        OR O.proceso LIKE @F
  )
ORDER BY
    CASE WHEN @SortField = 'cod_orden' AND @SortOrder = 1
        THEN O.cod_orden END ASC,
    CASE WHEN @SortField = 'cod_orden' AND @SortOrder = -1
        THEN O.cod_orden END DESC,
    CASE WHEN @SortField = 'tipo_orden' AND @SortOrder = 1
        THEN C.Descripcion END ASC,
    CASE WHEN @SortField = 'tipo_orden' AND @SortOrder = -1
        THEN C.Descripcion END DESC,
    CASE WHEN @SortField = 'proceso' AND @SortOrder = 1
        THEN O.proceso END ASC,
    CASE WHEN @SortField = 'proceso' AND @SortOrder = -1
        THEN O.proceso END DESC,
    CASE WHEN @SortField = 'total' AND @SortOrder = 1
        THEN O.total END ASC,
    CASE WHEN @SortField = 'total' AND @SortOrder = -1
        THEN O.total END DESC,
    CASE WHEN @SortField = 'genera_user' AND @SortOrder = 1
        THEN O.genera_user END ASC,
    CASE WHEN @SortField = 'genera_user' AND @SortOrder = -1
        THEN O.genera_user END DESC,
    CASE WHEN @SortField = 'genera_fecha' AND @SortOrder = 1
        THEN O.genera_fecha END ASC,
    CASE WHEN @SortField = 'genera_fecha' AND @SortOrder = -1
        THEN O.genera_fecha END DESC,
    CASE WHEN @SortField = 'nota' AND @SortOrder = 1
        THEN O.nota END ASC,
    CASE WHEN @SortField = 'nota' AND @SortOrder = -1
        THEN O.nota END DESC,
    O.cod_orden ASC
OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;
";

                var ordenes = conn.Query<OrdenCompra>(
                    sqlLista,
                    new
                    {
                        Tipo = req.tipo,
                        Usuario = req.usuario.ToUpper(),
                        TodosPendientes = req.todosPendientes ? 1 : 0,
                        FechaInicio = fechaIni,
                        FechaCorte = fechaFin,
                        F = like,
                        Offset = offset,
                        Fetch = fetch,
                        SortField = campoOrden,
                        SortOrder = direccionOrden
                    }
                ).ToList();

                return new OrdenCompraDto
                {
                    total = total,
                    ordenes = ordenes
                };
            });

            if (r.Code != 0)
                return DbHelper.CreateErrorResponse<OrdenCompraDto>(
                    r.Description ?? "Error",
                    r.Code ?? -1,
                    new OrdenCompraDto { total = 0, ordenes = new List<OrdenCompra>() });

            return DbHelper.CreateOkResponse(r.Result ?? new OrdenCompraDto { total = 0, ordenes = new List<OrdenCompra>() });
        }

        public ErrorDto OrdenCompra_Autorizar(int codEmpresa, OrdenCompraResolucionRequestDto req)
            => ResolverOrdenes(codEmpresa, req, estadoFinal: "A");

        public ErrorDto OrdenCompra_Rechazar(int codEmpresa, OrdenCompraResolucionRequestDto req)
            => ResolverOrdenes(codEmpresa, req, estadoFinal: "R");

        // ----------------- Resolver (Autorizar/Rechazar) -----------------

        private ErrorDto ResolverOrdenes(int codEmpresa, OrdenCompraResolucionRequestDto req, string estadoFinal)
        {
            var codigos = ParseCodigos(req.codigosOrden);
            if (codigos.Count == 0)
                return DbHelper.ErrorResponse("Debe indicar códigos de orden", -1);

            var res = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                EjecutarResolucionEnTx(conn, req, estadoFinal, codigos)
            );

            return MapTxResult(res);
        }

        private ErrorDto EjecutarResolucionEnTx(
            SqlConnection conn,
            OrdenCompraResolucionRequestDto req,
            string estadoFinal,
            List<string> codigos)
        {
            if (conn.State != ConnectionState.Open) conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                var valida = ValidarOrdenes(conn, tx, req.usuario, estadoFinal, codigos);
                if (valida.Code != 0)
                {
                    tx.Rollback();
                    return valida;
                }

                var upd = ActualizarOrdenes(conn, tx, req.usuario, estadoFinal, codigos);
                if (upd.Code != 0)
                {
                    tx.Rollback();
                    return upd;
                }

                tx.Commit();
                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        // Sin resultados parciales: se revisan todas las órdenes antes de actualizar alguna.
        private static ErrorDto ValidarOrdenes(
            SqlConnection conn,
            SqlTransaction tx,
            string usuario,
            string estadoFinal,
            List<string> codigos)
        {
            var tipoCambio = CprOrdenPermisos.ObtenerTipoCambio(conn, tx);
            if (tipoCambio <= 0)
                return DbHelper.ErrorResponse("Tipo de cambio inválido", -1);

            var fallas = new List<string>();
            foreach (var codOrden in codigos)
            {
                var faltantes = ObtenerFaltantes(conn, tx, usuario, codOrden, tipoCambio);
                if (faltantes.Count > 0)
                    fallas.Add($"Orden #{codOrden}: {string.Join("; ", faltantes)}.");
            }

            if (fallas.Count == 0)
                return DbHelper.CreateOkResponse();

            var accion = estadoFinal == "A" ? "autorizó" : "rechazó";
            return DbHelper.ErrorResponse(
                $"No se {accion} ninguna orden. {string.Join(" ", fallas)} Solicítelo en {CprOrdenPermisos.RutaNivelesAutorizacion}.",
                CprOrdenPermisos.CodigoSinPermiso);
        }

        private static List<string> ObtenerFaltantes(
            SqlConnection conn,
            SqlTransaction tx,
            string usuario,
            string codOrden,
            decimal tipoCambio)
        {
            var montoDolares = CprOrdenPermisos.ObtenerMontoOrden(conn, tx, codOrden) / tipoCambio;
            if (montoDolares == 0)
                return new List<string> { "el monto de la orden de compra no puede ser 0" };

            var codUnidad = CprOrdenPermisos.ObtenerCodUnidad(conn, tx, codOrden);

            return new[]
                {
                    CprOrdenPermisos.FaltantePorUen(conn, tx, usuario, codUnidad, montoDolares),
                    CprOrdenPermisos.FaltanteUsuarioACargo(conn, tx, usuario, codOrden)
                }
                .OfType<string>()
                .ToList();
        }

        private ErrorDto ActualizarOrdenes(
            SqlConnection conn,
            SqlTransaction tx,
            string usuario,
            string estadoFinal,
            List<string> codigos)
        {
            foreach (var codOrden in codigos)
            {
                var rows = conn.Execute(
                    @"UPDATE cpr_ordenes
                      SET autoriza_fecha = GETDATE(),
                          autoriza_user  = @Usuario,
                          estado         = @Estado
                      WHERE cod_orden = @CodOrden",
                    new { Usuario = usuario, Estado = estadoFinal, CodOrden = codOrden },
                    transaction: tx
                );

                if (rows <= 0)
                    return DbHelper.ErrorResponse("Error al actualizar órdenes de compra", -1);
            }

            return DbHelper.CreateOkResponse();
        }

        private static ErrorDto MapTxResult(ErrorDto<ErrorDto> r)
        {
            if (r.Code != 0 || r.Result == null)
                return DbHelper.ErrorResponse(r.Description ?? "Error", r.Code ?? -1);

            return r.Result;
        }




        // ----------------- Helpers SQL -----------------

        private static string? NormalizeLike(string? filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return string.Empty;

            var f = filtro.Trim();
            return f.Length == 0 ? string.Empty : $"%{f}%";
        }

        private static (int Offset, int Fetch) NormalizePaging(int pagina, int paginacion)
        {
            if (pagina < 0 || paginacion <= 0)
                return (0, int.MaxValue);

            return (pagina, paginacion);
        }

        private static List<string> ParseCodigos(string? codigosOrden)
        {
            if (string.IsNullOrWhiteSpace(codigosOrden))
                return new List<string>();

            return codigosOrden
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }
    }
}
