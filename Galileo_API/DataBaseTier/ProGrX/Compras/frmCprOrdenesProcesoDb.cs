using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public class FrmCprOrdenesProcesoDB
    {
        private const string ErrorLiteral = "Error";
        private readonly PortalDB _portalDb;

        public FrmCprOrdenesProcesoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        public ErrorDto<List<ProveedorOrdenesData>> ProveedorOrden_Obtener(int codEmpresa, string codOrden)
        {
            return DbHelper.ExecuteListQuery<ProveedorOrdenesData>(
                _portalDb,
                codEmpresa,
                @"SELECT Prov.COD_PROVEEDOR, Prov.DESCRIPCION, Op.*
                  FROM CPR_ORDENES O
                  INNER JOIN CPR_ORDENES_PROCESO Op ON O.COD_ORDEN = Op.COD_ORDEN
                  INNER JOIN CXP_PROVEEDORES Prov ON Op.cod_proveedor = Prov.COD_PROVEEDOR
                  WHERE O.COD_ORDEN = @CodOrden",
                new { CodOrden = codOrden }
            );
        }

        public ErrorDto Cpr_Orden_Proceso(int codEmpresa, OrdenProceso orden)
        {
            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                @"EXEC spCpr_Orden_Proceso @Orden, @Usuario, @Movimiento, @Proveedor, @Notas",
                new
                {
                    Orden = orden.cod_orden,
                    Usuario = orden.genera_user,
                    Movimiento = orden.funcion,
                    Proveedor = orden.cod_proveedor,
                    Notas = string.Empty
                }
            );
        }

        public ErrorDto OrdenProceso_ReemplazarPin(int codEmpresa, bool pinIngreso, string pin, string codOrden)
        {
            var ingresoPin = pinIngreso ? 1 : 0;

            var r = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                codEmpresa,
                @"UPDATE cpr_ordenes
                  SET pin_entrada = @Pin,
                      pin_autorizacion = @IngresoPin
                  WHERE cod_orden = @CodOrden
                    AND proceso NOT IN ('D','X')",
                new { Pin = pin, IngresoPin = ingresoPin, CodOrden = codOrden }
            );

            var mensaje = r.Result > 0
                ? "Pin reemplazado satisfactoriamente."
                : "No se pudo reemplazar el pin (orden no elegible).";

            return r.Code == 0
                ? DbHelper.OkResponse(mensaje)
                : DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);
        }

        public ErrorDto Orden_Autoriza(int codEmpresa, string codOrden, string usuario, int index)
        {
            // index: 0 = Autoriza, otro = Rechaza (conservando tu regla)
            var estadoFinal = index == 0 ? "A" : "R";

            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (conn.State != ConnectionState.Open) conn.Open();
                using var tx = conn.BeginTransaction();

                try
                {
                    var valida = ValidarAutorizacion(conn, tx, codOrden, usuario);
                    if (valida.Code != 0)
                    {
                        tx.Rollback();
                        return valida;
                    }

                    var upd = ActualizarOrdenEstado(conn, tx, codOrden, usuario, estadoFinal);
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
            });

            // WithConn devuelve ErrorDto<ErrorDto>
            if (r.Code != 0 || r.Result == null)
                return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            return r.Result;
        }

        public ErrorDto Orden_Cerrar(int codEmpresa, string codOrden)
        {
            var r = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                codEmpresa,
                @"UPDATE CPR_Ordenes
                  SET Proceso = 'Y'
                  WHERE cod_Orden = @CodOrden
                    AND Estado = 'A'
                    AND Proceso IN ('A','D','X')",
                new { CodOrden = codOrden }
            );

            var mensaje = r.Result > 0
                ? "Orden cerrada satisfactoriamente."
                : "No se pudo cerrar la orden (estado/proceso no permite).";

            return r.Code == 0
                ? DbHelper.OkResponse(mensaje)
                : DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, -1);
        }

        public ErrorDto ProveedorEstado_Obtener(int codEmpresa, int codProveedor)
        {
            var r = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                @"SELECT Estado FROM CxP_Proveedores WHERE cod_proveedor = @CodProveedor",
                defaultValue: string.Empty,
                parameters: new { CodProveedor = codProveedor }
            );

            return r.Code == 0
                ? DbHelper.OkResponse(r.Result ?? string.Empty)
                : DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);
        }

        // ----------------- Helpers (bajan S3776) -----------------

        private  static ErrorDto ValidarAutorizacion(SqlConnection conn, SqlTransaction tx, string codOrden, string usuario)
        {
            var codUnidad = CprOrdenPermisos.ObtenerCodUnidad(conn, tx, codOrden);

            var montoColones = CprOrdenPermisos.ObtenerMontoOrden(conn, tx, codOrden);
            var tipoCambio = CprOrdenPermisos.ObtenerTipoCambio(conn, tx);
            if (tipoCambio <= 0)
                return DbHelper.ErrorResponse("Tipo de cambio inválido.", -1);

            var montoDolares = montoColones / tipoCambio;
            if (montoDolares == 0)
                return DbHelper.ErrorResponse("El monto de la orden de compra no puede ser 0.", -1);

            var faltante = CprOrdenPermisos.FaltantePorUen(conn, tx, usuario, codUnidad, montoDolares);
            if (faltante != null)
                return DbHelper.ErrorResponse(
                    $"No tiene permiso para resolver esta orden: {faltante}. Solicítelo en {CprOrdenPermisos.RutaNivelesAutorizacion}.",
                    CprOrdenPermisos.CodigoSinPermiso);

            if (!UsuarioPuedeAutorizarPendientes(conn, tx, usuario))
                return DbHelper.ErrorResponse(
                    $"No tiene permiso para resolver esta orden: no hay órdenes pendientes de usuarios a su cargo. Solicítelo en {CprOrdenPermisos.RutaNivelesAutorizacion}, pestaña Usuarios a Cargo.",
                    CprOrdenPermisos.CodigoSinPermiso);

            return DbHelper.CreateOkResponse();
        }

        private static bool UsuarioPuedeAutorizarPendientes(SqlConnection conn, SqlTransaction tx, string usuario)
        {
            var count = conn.QueryFirstOrDefault<int>(
                @"SELECT COUNT(*)
                  FROM cpr_ordenes O
                  INNER JOIN cpr_Tipo_Orden C ON O.Tipo_Orden = C.Tipo_Orden
                  WHERE O.autoriza_fecha IS NULL
                    AND O.estado = 'S'
                    AND O.genera_user IN (
                        SELECT usuario_asignado
                        FROM cpr_orden_autousers
                        WHERE usuario = @Usuario
                    )",
                new { Usuario = usuario },
                transaction: tx
            );

            return count > 0;
        }

        private ErrorDto ActualizarOrdenEstado(SqlConnection conn, SqlTransaction tx, string codOrden, string usuario, string estado)
        {
            var rows = conn.Execute(
                @"UPDATE cpr_ordenes
                  SET autoriza_fecha = GETDATE(),
                      autoriza_user  = @Usuario,
                      estado         = @Estado
                  WHERE cod_orden = @CodOrden",
                new { Usuario = usuario, Estado = estado, CodOrden = codOrden },
                transaction: tx
            );

            return rows > 0
                ? DbHelper.CreateOkResponse()
                : DbHelper.ErrorResponse("No se pudo actualizar la orden (no encontrada o no elegible).", -1);
        }
    }
}
