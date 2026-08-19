using Dapper;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.DataBaseTier
{
    public class FrmCprSolicitudCotizacionValidacionDB
    {
        private readonly PortalDB _portalDb;

        public FrmCprSolicitudCotizacionValidacionDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

       public ErrorDto<CprSolicitudCotizacionPrvBsLista> CprValidarCotizacionBs_Obtener(int codEmpresa, int? cpr_id, int? cod_unidad)
        {
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var list = conn.Query<CprSolicitudCotizacionPrvBs>(
                    "spCPR_ValidarCotizacion_Consultar",
                    new { cpr_id = cpr_id, cod_proveedor = cod_unidad },
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();

                return new CprSolicitudCotizacionPrvBsLista { cotizaciones = list };
            });

            if (r.Code != 0)
                return DbHelper.CreateErrorResponse<CprSolicitudCotizacionPrvBsLista>(
                    r.Description ?? "Error",
                    r.Code ?? -1,
                    new CprSolicitudCotizacionPrvBsLista { cotizaciones = new List<CprSolicitudCotizacionPrvBs>() });

            return DbHelper.CreateOkResponse(r.Result ?? new CprSolicitudCotizacionPrvBsLista { cotizaciones = new List<CprSolicitudCotizacionPrvBs>() });
        }

        public ErrorDto CprValidarContizacionBs_Guardar(int codEmpresa, CprSolicitusCotizacionGuardar datos)
        {
            // Varias escrituras => transacción
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();
                using var tx = conn.BeginTransaction();

                try
                {
                    foreach (var cotizacion in datos.listacotizacion)
                    {
                        cotizacion.proveedor_codigo = datos.proveedor_codigo;
                        cotizacion.no_cotizacion = datos.no_cotizacion;

                        var xml = MProGrXAuxiliarDB.fxConvertModelToXml<CprSolicitudCotizacionPrvBs>(cotizacion);

                        conn.Execute(
                            "spCPR_ValidarCotizacion_Guardar",
                            new { detalle = xml }, // si el SP usa otro nombre, cámbialo aquí
                            transaction: tx,
                            commandType: System.Data.CommandType.StoredProcedure
                        );
                    }

                    tx.Commit();
                    return true;
                }
                catch
                {
                    tx.Rollback();
                    throw; // DbHelper lo captura y rellena ErrorDto
                }
            });

            return r.Code == 0
                ? DbHelper.CreateOkResponse()
                : DbHelper.ErrorResponse(r.Description ?? "Error", r.Code ?? -1);
        }

        /// <summary>
        /// Inactiva una línea de validación de cotización por id de línea o por llave funcional.
        /// </summary>
        public ErrorDto CprValidacionCotizacionBs_Eliminar(
            int codEmpresa,
            int id_cotizacion_linea,
            int? cpr_id,
            int? proveedor_codigo,
            string? codigo,
            string? cod_producto)
        {
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();
                using var tx = conn.BeginTransaction();

                try
                {
                    var affected = 0;
                    var hasLineaId = id_cotizacion_linea > 0;

                    if (hasLineaId)
                    {
                        affected += conn.Execute(
                            "UPDATE CPR_SOLICITUD_PROV_COTIZA_LINEAS SET SELECCIONADO = 0 WHERE CODIGO = @pCodigo AND COD_PRODUCTO = @pProducto AND id_cotizacion_linea = @IdLinea",
                            new { pCodigo = proveedor_codigo, pProducto = cod_producto, IdLinea = id_cotizacion_linea },
                            transaction: tx
                        );

                        //conn.Execute(
                        //    @"DELETE B
                        //      FROM CPR_SOLICITUD_PROV_BS B
                        //      INNER JOIN CPR_SOLICITUD_PROV_COTIZA_LINEAS L
                        //              ON L.ID_COTIZACION_LINEA = @IdLinea
                        //      INNER JOIN CPR_SOLICITUD_PROV_COTIZA C
                        //              ON C.ID_COTIZACION = L.ID_COTIZACION
                        //      WHERE B.CPR_ID = C.CPR_ID
                        //        AND B.PROVEEDOR_CODIGO = C.PROVEEDOR_CODIGO
                        //        AND ISNULL(B.COD_PRODUCTO, '') = ISNULL(L.COD_PRODUCTO, '')
                        //        AND ISNULL(B.CODIGO, '') = ISNULL(L.CODIGO, '')
                        //        AND ISNULL(B.NO_COTIZACION, '') = ISNULL(C.NO_COTIZACION, ISNULL(C.COTIZA_NUMERO, ''))",
                        //    new { IdLinea = id_cotizacion_linea },
                        //    transaction: tx
                        //);
                    }

                    if (affected == 0)
                    {
                        var cprId = cpr_id ?? 0;
                        var proveedor = proveedor_codigo ?? 0;
                        var codigoLinea = (codigo ?? string.Empty).Trim();
                        var codProducto = (cod_producto ?? string.Empty).Trim();

                        if (cprId <= 0 || proveedor <= 0 || string.IsNullOrWhiteSpace(codigoLinea) || string.IsNullOrWhiteSpace(codProducto))
                        {
                            throw new InvalidOperationException("No se encontró línea de cotización para inactivar y no se recibieron llaves funcionales válidas.");
                        }

                        affected += conn.Execute(
                            @"UPDATE L
                              SET L.SELECCIONADO = 0
                              FROM CPR_SOLICITUD_PROV_COTIZA_LINEAS L
                              INNER JOIN CPR_SOLICITUD_PROV_COTIZA C
                                      ON C.ID_COTIZACION = L.ID_COTIZACION
                              WHERE C.CPR_ID = @CprId
                                AND C.PROVEEDOR_CODIGO = @Proveedor
                                AND ISNULL(L.CODIGO, '') = @Codigo
                                AND ISNULL(L.COD_PRODUCTO, '') = @CodProducto",
                            new
                            {
                                CprId = cprId,
                                Proveedor = proveedor,
                                Codigo = codigoLinea,
                                CodProducto = codProducto,
                            },
                            transaction: tx
                        );

                        conn.Execute(
                            @"DELETE FROM CPR_SOLICITUD_PROV_BS
                              WHERE CPR_ID = @CprId
                                AND PROVEEDOR_CODIGO = @Proveedor
                                AND ISNULL(CODIGO, '') = @Codigo
                                AND ISNULL(COD_PRODUCTO, '') = @CodProducto",
                            new
                            {
                                CprId = cprId,
                                Proveedor = proveedor,
                                Codigo = codigoLinea,
                                CodProducto = codProducto,
                            },
                            transaction: tx
                        );
                    }

                    if (affected == 0)
                    {
                        throw new InvalidOperationException("No se encontró línea para inactivar.");
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
    }
}
