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

                var cprId = cotizacion.cpr_id;
                if (!cprId.HasValue) continue;

                var solicitud = ObtenerSolicitud(conn, tx, cprId.Value);
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
            var cprId = solicitud.cpr_id ?? cotizacion.cpr_id.Value;
            var proveedorCodigo = cotizacion.proveedor_codigo.Value;
            var noCotizacion = cotizacion.cotiza_numero;

            MarcarCotizaVigente(conn, tx, cprId, proveedorCodigo, idCotizacion);
            EliminarLineasBsPrevias(conn, tx, cprId, proveedorCodigo, noCotizacion);
            InsertarDetalleBsPorLinea(conn, tx, cprId, proveedorCodigo, noCotizacion, idCotizacion);
            ActualizarSolicitudProv(conn, tx, cotizacion.cpr_id.Value, proveedorCodigo, cotizacion.registro_usuario);
        }

        private static void EliminarLineasBsPrevias(
            IDbConnection conn,
            IDbTransaction tx,
            int cprId,
            int proveedorCodigo,
            string noCotizacion)
        {
            conn.Execute(
                @"DELETE FROM CPR_SOLICITUD_PROV_BS
          WHERE CPR_ID = @CprId
            AND PROVEEDOR_CODIGO = @Proveedor
            AND NO_COTIZACION = @NoCotizacion",
                new
                {
                    CprId = cprId,
                    Proveedor = proveedorCodigo,
                    NoCotizacion = noCotizacion
                },
                transaction: tx
            );
        }

        private static void InsertarDetalleBsPorLinea(
    IDbConnection conn,
    IDbTransaction tx,
    int cprId,
    int proveedorCodigo,
    string noCotizacion,
    int idCotizacion)
        {
            conn.Execute(
                @"INSERT INTO CPR_SOLICITUD_PROV_BS
          (
              ID_COTIZACION_LINEA,
              CPR_ID,
              COD_PRODUCTO,
              PROVEEDOR_CODIGO,
              CODIGO,
              MONTO,
              CANTIDAD,
              TOTAL,
              IVA_PORC,
              IVA_MONTO,
              DESC_PORC,
              DESC_MONTO,
              REGISTRO_FECHA,
              REGISTRO_USUARIO,
              ESTADO,
              NO_COTIZACION
          )
          SELECT
              spcl.ID_COTIZACION_LINEA,
              @CprId,
              spcl.COD_PRODUCTO,
              @Proveedor,
              spcl.CODIGO,
              spcl.MONTO,
              spcl.CANTIDAD,
              spcl.TOTAL,
              spcl.IVA_PORC,
              spcl.IVA_MONTO,
              spcl.DESC_PORC,
              spcl.DESC_MONTO,
              GETDATE(),
              cspc.REGISTRO_USUARIO,
              'V',
              @NoCotizacion
          FROM CPR_SOLICITUD_PROV_COTIZA_LINEAS spcl
          INNER JOIN CPR_SOLICITUD_PROV_COTIZA cspc
              ON cspc.ID_COTIZACION = spcl.ID_COTIZACION
          WHERE spcl.ID_COTIZACION = @IdCotizacion
            AND ISNULL(spcl.SELECCIONADO, 0) = 1;",
                new
                {
                    CprId = cprId,
                    Proveedor = proveedorCodigo,
                    NoCotizacion = noCotizacion,
                    IdCotizacion = idCotizacion
                },
                transaction: tx
            );
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



        private static void InsertarDetalleBs(
            IDbConnection conn,
            IDbTransaction tx,
            int cprId,
            int proveedorCodigo,
            string noCotizacion,
            int idCotizacion)
        {
            conn.Execute(
                @"INSERT INTO CPR_SOLICITUD_PROV_BS
                  (
                      CPR_ID,
                      COD_PRODUCTO,
                      PROVEEDOR_CODIGO,
                      CODIGO,
                      MONTO,
                      CANTIDAD,
                      TOTAL,
                      IVA_PORC,
                      IVA_MONTO,
                      DESC_PORC,
                      DESC_MONTO,
                      REGISTRO_FECHA,
                      REGISTRO_USUARIO,
                      ESTADO,
                      NO_COTIZACION
                  )
                  SELECT
                      @CprId,
                      spcl.COD_PRODUCTO,
                      @Proveedor,
                      spcl.CODIGO,
                      spcl.MONTO,
                      spcl.CANTIDAD,
                      spcl.TOTAL,
                      spcl.IVA_PORC,
                      spcl.IVA_MONTO,
                      spcl.DESC_PORC,
                      spcl.DESC_MONTO,
                      GETDATE(),
                      cspc.REGISTRO_USUARIO,
                      'V',
                      @NoCotizacion
                  FROM CPR_SOLICITUD_PROV_COTIZA_LINEAS spcl
                  INNER JOIN CPR_SOLICITUD_PROV_COTIZA cspc
                      ON cspc.ID_COTIZACION = spcl.ID_COTIZACION
                  WHERE spcl.ID_COTIZACION = @IdCotizacion
                    AND ISNULL(spcl.SELECCIONADO, 0) = 1;",
                new
                {
                    CprId = cprId,
                    Proveedor = proveedorCodigo,
                    NoCotizacion = noCotizacion,
                    IdCotizacion = idCotizacion
                },
                transaction: tx
            );
        }

        /// <summary>Actualiza selección de una línea y sincroniza CPR_SOLICITUD_PROV_BS.</summary>
        public ErrorDto CprSolicitudCotizacionLineaSel_Actualizar(int codEmpresa, int id_cotizacion_linea, int seleccionado)
        {
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();
                using var tx = conn.BeginTransaction();
                try
                {
                    conn.Execute(
                        "UPDATE CPR_SOLICITUD_PROV_COTIZA_LINEAS SET SELECCIONADO = @Sel WHERE ID_COTIZACION_LINEA = @Id",
                        new { Sel = seleccionado, Id = id_cotizacion_linea },
                        transaction: tx
                    );

                    conn.Execute(
                        "DELETE FROM CPR_SOLICITUD_PROV_BS WHERE ID_COTIZACION_LINEA = @Id",
                        new { Id = id_cotizacion_linea },
                        transaction: tx
                    );

                    if (seleccionado == 1)
                    {
                        conn.Execute(
                            @"INSERT INTO CPR_SOLICITUD_PROV_BS
                              (ID_COTIZACION_LINEA, CPR_ID, COD_PRODUCTO, PROVEEDOR_CODIGO, CODIGO,
                               MONTO, CANTIDAD, TOTAL, IVA_PORC, IVA_MONTO, DESC_PORC, DESC_MONTO,
                               REGISTRO_FECHA, REGISTRO_USUARIO, ESTADO, NO_COTIZACION)
                              SELECT CL.ID_COTIZACION_LINEA, SPC.CPR_ID, CL.COD_PRODUCTO, SPC.PROVEEDOR_CODIGO,
                                     CL.CODIGO, CL.MONTO, CL.CANTIDAD, CL.TOTAL,
                                     CL.IVA_PORC, CL.IVA_MONTO, CL.DESC_PORC, CL.DESC_MONTO,
                                     GETDATE(), SPC.REGISTRO_USUARIO, 'V', SPC.NO_COTIZACION
                              FROM CPR_SOLICITUD_PROV_COTIZA_LINEAS CL
                              INNER JOIN CPR_SOLICITUD_PROV_COTIZA SPC ON SPC.ID_COTIZACION = CL.ID_COTIZACION
                              WHERE CL.ID_COTIZACION_LINEA = @Id",
                            new { Id = id_cotizacion_linea },
                            transaction: tx
                        );
                    }

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
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                // Eliminar registro relacionado en BS antes de borrar la línea de cotización
                conn.Execute(
                    "DELETE FROM CPR_SOLICITUD_PROV_BS WHERE ID_COTIZACION_LINEA = @id;",
                    new { id = id_cotizacion_linea }
                );

                conn.Execute(
                    "spCPR_SolicitudCotizacion_Eliminar",
                    new { id_cotizacion_linea },
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
