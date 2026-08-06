using System.Data;
using System.Globalization;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrSeguimientoTramitesDb
    {
        /// <summary>
        /// Actualiza el monto no gravable de la operación conservando el flujo de
        /// btnMtnNoGravable_Click del formulario VB6.
        /// </summary>
        public ErrorDto Cr_SeguimientoTramites_MontoNoGravable_Actualizar(
            int codEmpresa,
            CrSeguimientoTramitesMontoNoGravableRequest request)
        {
            if (request is null || request.operacion <= 0)
            {
                return DbHelper.ErrorResponse("No se ha indicado ninguna operación?", -2);
            }

            if (request.monto_no_gravable > request.monto)
            {
                return DbHelper.ErrorResponse(
                    "El Monto No Gravable supera al monto del crédito!",
                    -2);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();

                using (IDbTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        Cr_SeguimientoTramites_MontoNoGravable_Registrar(
                            conn,
                            transaction,
                            request);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                Cr_SeguimientoTramites_MontoNoGravable_Bitacora_Registrar(codEmpresa, request);

                return DbHelper.OkResponse(
                    "Monto No Gravable, actualizado satisfactoriamente!");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        private static void Cr_SeguimientoTramites_MontoNoGravable_Registrar(
            IDbConnection conn,
            IDbTransaction transaction,
            CrSeguimientoTramitesMontoNoGravableRequest request)
        {
            conn.Execute(
                """
                update reg_creditos
                set IVA_Monto = @MontoNoGravable
                where id_solicitud = @Operacion;
                """,
                new
                {
                    MontoNoGravable = request.monto_no_gravable,
                    Operacion = request.operacion
                },
                transaction);

            // El VB6 solo recalcula los cargos mientras la operación no esté resuelta.
            if (!Cr_SeguimientoTramites_MontoNoGravable_RecalculaCargos(request.estado_solicitud))
            {
                return;
            }

            conn.Execute(
                "exec spCRDOperacionCargosAdd @Operacion, @Codigo, @Monto;",
                new
                {
                    Operacion = request.operacion,
                    Codigo = Cr_SeguimientoTramites_Filtro_Normalizar(request.codigo, 10),
                    Monto = request.monto
                },
                transaction);
        }

        private static bool Cr_SeguimientoTramites_MontoNoGravable_RecalculaCargos(
            string? estadoSolicitud)
        {
            return (estadoSolicitud ?? string.Empty).Trim().ToUpperInvariant()
                is "R" or "P" or "A";
        }

        private void Cr_SeguimientoTramites_MontoNoGravable_Bitacora_Registrar(
            int codEmpresa,
            CrSeguimientoTramitesMontoNoGravableRequest request)
        {
            string estado = (request.estado_solicitud ?? string.Empty).Trim();

            MCredito.SbBitacoraCredito(
                _portalDb,
                codEmpresa,
                new MCredito.CrBitacoraCreditoRequest
                {
                    usuario = request.usuario,
                    tipo = "C",
                    movimiento = "28",
                    detalle = string.Create(
                        CultureInfo.InvariantCulture,
                        $"Monto: {request.monto_no_gravable:N2}"),
                    operacion = request.operacion,
                    codigo = request.codigo,
                    notas = $"Estado de la Operación [ {estado} ]"
                });
        }
    }
}
