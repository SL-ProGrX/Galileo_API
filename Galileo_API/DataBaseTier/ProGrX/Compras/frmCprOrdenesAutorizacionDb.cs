using System.Data;
using Dapper;
using Galileo.Models;
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

        public ErrorDto<OrdenCompraDto> OrdenesCompra_Autorizacion_Obtener(
            int codEmpresa,
            int pagina,
            int paginacion,
            string? filtro,
            OrdenCompraRequestDto req)
        {
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var (baseWhere, p) = BuildBaseWhere(req);
                var total = conn.QueryFirstOrDefault<int>(
                    $@"SELECT COUNT(O.cod_orden)
                       FROM cpr_ordenes O
                       INNER JOIN cpr_Tipo_Orden C ON O.Tipo_Orden = C.Tipo_Orden
                       {baseWhere}",
                    p
                );

                var (filtroWhere, filtroParams) = BuildFiltroListado(filtro);
                var (pagingSql, pagingParams) = BuildPaging(pagina, paginacion);

                var prms = MergeParams(p, filtroParams, pagingParams);

                // Nota: filtro se aplica sobre el resultset "T" (como hacías), pero parametrizado.
                var sql = $@"
                    SELECT *
                    FROM (
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
                        {baseWhere}
                        ORDER BY O.cod_orden
                        {pagingSql}
                    ) T
                    {filtroWhere}";

                var ordenes = conn.Query<OrdenCompra>(sql, prms).ToList();

                return new OrdenCompraDto
                {
                    total = total,
                    ordenes = ordenes
                };
            });

            if (r.Code != 0)
                return DbHelper.CreateErrorResponse<OrdenCompraDto>(r.Description ?? "Error", r.Code ?? -1, null!);

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
    catch
    {
        tx.Rollback();
        throw;
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
    // OJO: r.Code es int (no int?) en tu modelo, no uses ?? aquí.
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

        private static (string whereSql, DynamicParameters p) BuildBaseWhere(OrdenCompraRequestDto req)
        {
            var p = new DynamicParameters();

            // Base: pendientes de autorización
            var where = @"
                WHERE O.autoriza_fecha IS NULL
                  AND O.estado = 'S'
                  AND O.tipo_orden = @Tipo
                  AND O.genera_user IN (
                        SELECT usuario_asignado
                        FROM cpr_orden_autousers
                        WHERE usuario = @Usuario
                  )";

            p.Add("Tipo", req.tipo, DbType.String);
            p.Add("Usuario", req.usuario, DbType.String);

            if (!req.todosPendientes)
            {
                where += " AND O.genera_fecha BETWEEN @FechaInicio AND @FechaCorte";
                p.Add("FechaInicio", req.fechaInicio, DbType.DateTime);
                p.Add("FechaCorte", req.fechaCorte, DbType.DateTime);
            }

            return (where, p);
        }

        private static (string whereSql, object parameters) BuildFiltroListado(string? filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return (string.Empty, new { });

            var like = $"%{filtro.Trim()}%";
            return ("WHERE cod_orden LIKE @F OR TipoOrdenDesc LIKE @F OR nota LIKE @F OR proceso LIKE @F", new { F = like });
        }

        private static (string pagingSql, object parameters) BuildPaging(int pagina, int paginacion)
        {
            if (pagina < 0 || paginacion <= 0)
                return (string.Empty, new { });

            return ("OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY", new { Offset = pagina, Fetch = paginacion });
        }

        private static object MergeParams(params object[] parts)
        {
            var p = new DynamicParameters();
            foreach (var part in parts) p.AddDynamicParams(part);
            return p;
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