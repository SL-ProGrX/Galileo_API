using Dapper;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

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
                return DbHelper.CreateErrorResponse<CprSolicitudCotizacionPrvBsLista>(r.Description ?? "Error", r.Code ?? -1, null!);

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

        /// <summary>Desmarca como seleccionada la línea de cotización indicada.</summary>
        public ErrorDto CprValidacionCotizacionBs_Eliminar(int codEmpresa, int id_cotizacion_linea)
        {
            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                "UPDATE CPR_SOLICITUD_PROV_COTIZA_LINEAS SET SELECCIONADO = 0 WHERE ID_COTIZACION_LINEA = @IdLinea",
                new { IdLinea = id_cotizacion_linea }
            );
        }
    }
}