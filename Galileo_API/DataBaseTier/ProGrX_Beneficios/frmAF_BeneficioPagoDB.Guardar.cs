using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficioPagoDB
    {
        /// <summary>
        /// Genera el pago de los beneficios creando el maestro de tesorería, actualizando estados
        /// y registrando el detalle contable por cada ítem.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="usuario">Usuario que genera el pago.</param>
        /// <param name="tabla">Lista de pagos a procesar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiBeneficioPago_Generar(int CodCliente, string usuario, List<AfiBenePago> tabla)
        {
            if (tabla == null || tabla.Count == 0)
            {
                return DbHelper.ErrorResponse("No hay pagos para procesar");
            }

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                var cod_beneficio = tabla[0].cod_beneficio.Trim();
                var detalle = ObtenerDetalleBeneficio(connection, cod_beneficio);
                var (detalle1, detalle2) = PartirDetalle(detalle);

                foreach (var item in tabla)
                {
                    ProcesarPagoItem(CodCliente, connection, usuario, item, detalle1, detalle2);
                }

                return DbHelper.OkResponse("Pagos generados correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la descripción del beneficio para el detalle de tesorería.
        /// </summary>
        private static string ObtenerDetalleBeneficio(SqlConnection connection, string cod_beneficio)
        {
            const string sql = "SELECT descripcion FROM afi_beneficios WHERE cod_beneficio = @cod_beneficio";
            return connection.QueryFirstOrDefault<string>(sql, new { cod_beneficio }) ?? string.Empty;
        }

        /// <summary>
        /// Divide el detalle en dos líneas de máximo 26 caracteres.
        /// </summary>
        private static (string detalle1, string detalle2) PartirDetalle(string detalle)
        {
            if (string.IsNullOrEmpty(detalle) || detalle.Length <= 26)
            {
                return (detalle ?? string.Empty, string.Empty);
            }

            return (detalle.Substring(0, 26), detalle.Substring(26));
        }

        /// <summary>
        /// Procesa un ítem de pago: genera maestro tesorería, actualiza estados y crea el detalle contable.
        /// </summary>
        private void ProcesarPagoItem(int CodCliente, SqlConnection connection, string usuario,
            AfiBenePago item, string detalle1, string detalle2)
        {
            var vTesoreria = _mTes.fxgTesoreriaMaestro(CodCliente, usuario, new TesoreriaMaestroModel
            {
                vTipoDocumento = MTesFuncionesDb.fxTipoDocumento(item.tipo_emision),
                vBanco = item.cod_banco,
                vMonto = item.monto,
                vBeneficiario = item.nombre ?? string.Empty,
                vCodigo = item.cedula,
                vOP = 0,
                vDetalle1 = detalle1,
                vReferencia = 0,
                vDetalle2 = detalle2,
                vCuenta = item.cta_bancaria,
                vFecha = DateTime.Now.ToString("yyyy/MM/dd")
            });

            ActualizarEstadoOtorga(connection, usuario, item);
            ActualizarEstadoPago(connection, usuario, item, vTesoreria);
            CrearDetallesTesoreria(CodCliente, item, vTesoreria);
        }

        /// <summary>
        /// Actualiza el estado en afi_bene_otorga a 'E' (Enviado).
        /// </summary>
        private static void ActualizarEstadoOtorga(SqlConnection connection, string usuario, AfiBenePago item)
        {
            const string sql = @"UPDATE afi_bene_otorga
                                 SET estado = 'E', autoriza_user = @usuario, autoriza_fecha = GETDATE()
                                 WHERE cedula = @cedula AND cod_beneficio = @cod_beneficio AND consec = @consec";

            connection.Execute(sql, new { usuario, item.cedula, item.cod_beneficio, item.consec });
        }

        /// <summary>
        /// Actualiza el estado en afi_bene_pago a 'E' con el número de tesorería generado.
        /// </summary>
        private static void ActualizarEstadoPago(SqlConnection connection, string usuario, AfiBenePago item, long vTesoreria)
        {
            const string sql = @"UPDATE afi_bene_pago
                                 SET estado = 'E', tesoreria = @vTesoreria, envio_user = @usuario, envio_fecha = GETDATE()
                                 WHERE cedula = @cedula AND cod_beneficio = @cod_beneficio AND consec = @consec";

            connection.Execute(sql, new { vTesoreria, usuario, item.cedula, item.cod_beneficio, item.consec });
        }

        /// <summary>
        /// Crea los detalles contables (Haber/Débito) de la tesorería generada.
        /// </summary>
        private void CrearDetallesTesoreria(int CodCliente, AfiBenePago item, long vTesoreria)
        {
            _mTes.sbgTesoreriaDetalle(CodCliente, new TesoreriaDetalleModel
            {
                vSolicitud = vTesoreria,
                vCtaConta = item.cta_bancaria,
                vMonto = item.monto,
                vDH = "H",
                vLinea = 1
            });

            _mTes.sbgTesoreriaDetalle(CodCliente, new TesoreriaDetalleModel
            {
                vSolicitud = vTesoreria,
                vCtaConta = item.cta_bancaria,
                vMonto = item.monto,
                vDH = "D",
                vLinea = 2
            });
        }
    }
}
