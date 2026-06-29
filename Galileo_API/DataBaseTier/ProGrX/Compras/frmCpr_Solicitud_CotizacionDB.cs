using System.Data;
using Dapper;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmCprSolicitudCotizacionDB
    {
        private readonly PortalDB _portalDb;
        private readonly FrmCprSolicitudDB _solicitudDB;

        public FrmCprSolicitudCotizacionDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _solicitudDB = new FrmCprSolicitudDB(config);
        }

        public ErrorDto CprSolicitudContizacionBs_Guardar(int codEmpresa, CprSolicitusCotizacionGuardar datos)
        {
            var tipoExcepcion = _solicitudDB.CprSolicitud_TipoExcepcion(codEmpresa).Description ?? string.Empty;
            var tipoExcepcionGM = _solicitudDB.CprSolicitud_TipoExcepcionGM(codEmpresa).Description ?? string.Empty;

            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (conn.State != ConnectionState.Open) conn.Open();
                using var tx = conn.BeginTransaction();

                try
                {
                    ProcesarCotizaciones(conn, tx, datos, tipoExcepcion, tipoExcepcionGM);
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
                : DbHelper.ErrorResponse(r.Description ?? "Error", r.Code ?? -1);
        }

        private static void ProcesarCotizaciones(
            IDbConnection conn,
            IDbTransaction tx,
            CprSolicitusCotizacionGuardar datos,
            string tipoExcepcion,
            string tipoExcepcionGM)
        {
            foreach (var cotizacion in datos.listacotizacion)
            {
                if (!TieneCamposMinimos(cotizacion, datos))
                    continue;

                PrepararCotizacion(cotizacion, datos);

                var idCotizacion = EjecutarSpGuardarCotizacion(conn, tx, cotizacion);

                var solicitud = ObtenerSolicitud(conn, tx, cotizacion.cpr_id!.Value);
                if (solicitud is null) continue;

                if (!EsExcepcion(solicitud, tipoExcepcion, tipoExcepcionGM)) continue;

                AplicarExcepcion(conn, tx, cotizacion, solicitud, idCotizacion);
            }
        }

        private static bool TieneCamposMinimos(CprSolicitudCotizacionPrvBs cotizacion, CprSolicitusCotizacionGuardar datos)
        {
            // Evita operar con 0 / empty “por default” (no es SQLi, es consistencia)
            if (!cotizacion.cpr_id.HasValue) return false;
            if (datos.proveedor_codigo == 0) return false;
            if (string.IsNullOrWhiteSpace(datos.cotiza_numero)) return false;
            return true;
        }

        private static void PrepararCotizacion(CprSolicitudCotizacionPrvBs cotizacion, CprSolicitusCotizacionGuardar datos)
        {
            cotizacion.seleccionado = (cotizacion.sel == true) ? 1 : 0;

            cotizacion.proveedor_codigo = datos.proveedor_codigo;
            cotizacion.cotiza_numero = datos.cotiza_numero;
            cotizacion.plazo = datos.plazo;
            cotizacion.garantia = datos.garantia;
            cotizacion.tipo_cambio = datos.tipo_cambio;
        }

        private static int EjecutarSpGuardarCotizacion(IDbConnection conn, IDbTransaction tx, CprSolicitudCotizacionPrvBs cotizacion)
        {
            var xmlOutput = MProGrXAuxiliarDB.fxConvertModelToXml<CprSolicitudCotizacionPrvBs>(cotizacion);

            var spParams = new DynamicParameters();
            spParams.Add("@detalle", xmlOutput, DbType.String, ParameterDirection.Input);
            spParams.Add("@ID_COTIZACION_OUTPUT", dbType: DbType.Int32, direction: ParameterDirection.Output);

            conn.Execute(
                "spCPR_SolicitudCotizacion_Guardar",
                spParams,
                transaction: tx,
                commandType: CommandType.StoredProcedure
            );

            return spParams.Get<int>("@ID_COTIZACION_OUTPUT");
        }

        private static CprSolicitudDto? ObtenerSolicitud(IDbConnection conn, IDbTransaction tx, int cprId)
        {
            // Parametrizado => sin SQL injection
            return conn.QueryFirstOrDefault<CprSolicitudDto>(
                "SELECT * FROM CPR_SOLICITUD WHERE CPR_ID = @CprId",
                new { CprId = cprId },
                transaction: tx
            );
        }

        private static bool EsExcepcion(CprSolicitudDto solicitud, string tipoExcepcion, string tipoExcepcionGM)
            => solicitud.tipo_orden == tipoExcepcion || solicitud.tipo_orden == tipoExcepcionGM;

        private static void AplicarExcepcion(
            IDbConnection conn,
            IDbTransaction tx,
            CprSolicitudCotizacionPrvBs cotizacion,
            CprSolicitudDto solicitud,
            int idCotizacion)
        {
            // Requeridos ya validados en TieneCamposMinimos
            var cprId = solicitud.cpr_id ?? cotizacion.cpr_id!.Value;
            var proveedorCodigo = cotizacion.proveedor_codigo!.Value;
            var noCotizacion = cotizacion.cotiza_numero!;

            MarcarCotizaVigente(conn, tx, cotizacion.cpr_id!.Value, proveedorCodigo, idCotizacion);
            MarcarLineasSeleccionadas(conn, tx, idCotizacion);
            EliminarLineasBsPrevias(conn, tx, cprId, proveedorCodigo);
            InsertarDetalleBs(conn, tx, cprId, proveedorCodigo, noCotizacion);
            ActualizarSolicitudProv(conn, tx, cotizacion.cpr_id.Value, proveedorCodigo, cotizacion.registro_usuario);
        }

        private static void MarcarCotizaVigente(IDbConnection conn, IDbTransaction tx, int cprId, int proveedorCodigo, int idCotizacion)
        {
            conn.Execute(
                @"UPDATE CPR_SOLICITUD_PROV_COTIZA
                  SET ESTADO = 'V'
                  WHERE CPR_ID = @CprId
                    AND PROVEEDOR_CODIGO = @Proveedor
                    AND ID_COTIZACION = @IdCotizacion",
                new { CprId = cprId, Proveedor = proveedorCodigo, IdCotizacion = idCotizacion },
                transaction: tx
            );
        }

        private static void MarcarLineasSeleccionadas(IDbConnection conn, IDbTransaction tx, int idCotizacion)
        {
            conn.Execute(
                @"UPDATE CPR_SOLICITUD_PROV_COTIZA_LINEAS
                  SET SELECCIONADO = 1
                  WHERE ID_COTIZACION = @IdCotizacion",
                new { IdCotizacion = idCotizacion },
                transaction: tx
            );
        }

        private static void EliminarLineasBsPrevias(IDbConnection conn, IDbTransaction tx, int cprId, int proveedorCodigo)
        {
            conn.Execute(
                @"DELETE FROM CPR_SOLICITUD_PROV_BS
                  WHERE CPR_ID = @CprId
                    AND PROVEEDOR_CODIGO = @Proveedor",
                new { CprId = cprId, Proveedor = proveedorCodigo },
                transaction: tx
            );
        }

        private static void InsertarDetalleBs(IDbConnection conn, IDbTransaction tx, int cprId, int proveedorCodigo, string noCotizacion)
        {
            conn.Execute(
                @"INSERT INTO CPR_SOLICITUD_PROV_BS
                  (
                      CPR_ID, COD_PRODUCTO, PROVEEDOR_CODIGO, CODIGO, MONTO, CANTIDAD, TOTAL,
                      IVA_PORC, IVA_MONTO, DESC_PORC, DESC_MONTO, registro_fecha, registro_usuario,
                      ESTADO, NO_COTIZACION
                  )
                  SELECT
                      csb.CPR_ID,
                      csb.COD_PRODUCTO,
                      @Proveedor AS PROVEEDOR_CODIGO,
                      NULL AS CODIGO,
                      spcl.MONTO,
                      spcl.CANTIDAD,
                      spcl.TOTAL,
                      spcl.IVA_PORC,
                      spcl.IVA_MONTO,
                      spcl.DESC_PORC,
                      spcl.DESC_MONTO,
                      GETDATE() AS registro_fecha,
                      csb.registro_usuario,
                      'V' AS ESTADO,
                      @NoCotizacion AS NO_COTIZACION
                  FROM CPR_SOLICITUD_BS csb
                  LEFT JOIN CPR_SOLICITUD_PROV_COTIZA cspc
                         ON csb.CPR_ID = cspc.CPR_ID
                        AND cspc.PROVEEDOR_CODIGO = @Proveedor
                  LEFT JOIN CPR_SOLICITUD_PROV_COTIZA_LINEAS spcl
                         ON spcl.ID_COTIZACION = cspc.ID_COTIZACION
                  WHERE csb.CPR_ID = @CprId",
                new { CprId = cprId, Proveedor = proveedorCodigo, NoCotizacion = noCotizacion },
                transaction: tx
            );
        }

        private static void ActualizarSolicitudProv(IDbConnection conn, IDbTransaction tx, int cprId, int proveedorCodigo, string? usuario)
        {
            conn.Execute(
                @"UPDATE CPR_SOLICITUD_PROV
                  SET ESTADO = 'V',
                      VALORA_PUNTAJE = 1000,
                      COTIZA_FECHA = GETDATE(),
                      COTIZA_USUARIO = @Usuario
                  WHERE PROVEEDOR_CODIGO = @Proveedor
                    AND CPR_ID = @CprId",
                new { Usuario = usuario, Proveedor = proveedorCodigo, CprId = cprId },
                transaction: tx
            );
        }

        public ErrorDto CprSolicitudCotizacionBs_Eliminar(int codEmpresa, int id_cotizacion_linea)
        {
            // Llamada segura: StoredProcedure + parámetros (sin EXEC string)
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(
                    "spCPR_SolicitudCotizacion_Eliminar",
                    new { id_cotizacion_linea = id_cotizacion_linea }, // usa el nombre real del parámetro si difiere
                    commandType: CommandType.StoredProcedure
                );

                return true;
            });

            return r.Code == 0
                ? DbHelper.CreateOkResponse()
                : DbHelper.ErrorResponse(r.Description ?? "Error", r.Code ?? -1);
        }

        public ErrorDto<List<CprSolicitudProvCotiza>> CprSolicitudContizacionLista_Obtener(int codEmpresa, int cpr_id, string cod_proveedor)
        {
            // Llamada segura: StoredProcedure + parámetros
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var list = conn.Query<CprSolicitudProvCotiza>(
                    "spCPR_SolicitudCotiLista_Obtener",
                    new { cpr_id = cpr_id, cod_proveedor = cod_proveedor }, // usa nombres reales del SP si difieren
                    commandType: CommandType.StoredProcedure
                ).AsList();

                return list;
            });
        }
    }
}