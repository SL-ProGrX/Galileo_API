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
                var valida = ValidarSiAplica(conn, tx, req.usuario, estadoFinal, codigos);
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

        private ErrorDto ValidarSiAplica(
            SqlConnection conn,
            SqlTransaction tx,
            string usuario,
            string estadoFinal,
            List<string> codigos)
        {
            if (estadoFinal != "A")
                return DbHelper.CreateOkResponse();

            // Regla original: validar usando el primer código
            return ValidarRangoAutorizacion(conn, tx, codigos[0], usuario);
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




        private ErrorDto ValidarRangoAutorizacion(IDbConnection conn, IDbTransaction tx, string codOrden, string usuario)
        {
            // UEN (COD_UNIDAD) por orden: tu query original usaba CPR_SOLICITUD.ADJUDICA_ORDEN = {codigosOrden}
            var codUnidad = conn.QueryFirstOrDefault<string>(
                @"SELECT TOP 1 COD_UNIDAD
                  FROM CPR_SOLICITUD
                  WHERE ADJUDICA_ORDEN = @CodOrden",
                new { CodOrden = codOrden },
                transaction: tx
            ) ?? string.Empty;

            var montoColones = conn.QueryFirstOrDefault<decimal>(
                @"SELECT TOTAL
                  FROM CPR_ORDENES
                  WHERE COD_ORDEN = @CodOrden",
                new { CodOrden = codOrden },
                transaction: tx
            );

            var tcStr = conn.QueryFirstOrDefault<string>(
                @"SELECT VALOR
                  FROM SIF_PARAMETROS
                  WHERE COD_PARAMETRO = 'TC'",
                transaction: tx
            ) ?? "0";

            if (!decimal.TryParse(tcStr, out var tipoCambio) || tipoCambio <= 0)
                return DbHelper.ErrorResponse("Tipo de cambio inválido", -1);

            var montoDolares = montoColones / tipoCambio;

            if (montoDolares == 0)
                return DbHelper.ErrorResponse("El monto de la orden de compra no puede ser 0.", -1);

            var montoMinimo = conn.QueryFirstOrDefault<decimal>(
                @"SELECT TOP 1 MONTO_MINIMO
                  FROM cpr_orden_rangos r
                  JOIN CPR_RANGO_USUARIO u ON r.cod_rango = u.cod_rango
                  WHERE u.USUARIO = @Usuario AND u.ACTIVO = 1 AND u.UEN = @UEN",
                new { Usuario = usuario, UEN = codUnidad },
                transaction: tx
            );

            var montoMaximo = conn.QueryFirstOrDefault<decimal>(
                @"SELECT TOP 1 MONTO_MAXIMO
                  FROM cpr_orden_rangos r
                  JOIN CPR_RANGO_USUARIO u ON r.cod_rango = u.cod_rango
                  WHERE u.USUARIO = @Usuario AND u.ACTIVO = 1 AND u.UEN = @UEN",
                new { Usuario = usuario, UEN = codUnidad },
                transaction: tx
            );

            if (montoDolares < montoMinimo || montoDolares > montoMaximo)
                return DbHelper.ErrorResponse("El Usuario actual no está dentro del rango para esta Gestión.", -1);

            return DbHelper.CreateOkResponse();
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
