using System.Data;
using Dapper;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_ControlTramites
{
    public sealed partial class FrmAfRecepcionAfiliacionesTagsDb
    {
        /// <summary>
        /// Aplica las etiquetas dentro de una transaccion ya iniciada.
        /// </summary>
        /// <param name="connection">Conexion SQL abierta.</param>
        /// <param name="transaction">Transaccion activa.</param>
        /// <param name="request">Datos validados del proceso.</param>
        /// <param name="movimiento">Movimiento normalizado.</param>
        /// <returns>Cantidad de afiliaciones procesadas.</returns>
        private static int
            AF_frmAF_RecepcionAfiliacionesTags_Aplicar_Transaccion(
                SqlConnection connection,
                SqlTransaction transaction,
                AfRecepcionAfiliacionesTagsAplicarRequest request,
                string movimiento)
        {
            var tags = AF_frmAF_RecepcionAfiliacionesTags_Tags_Obtener(
                connection,
                transaction);
            string tag = movimiento == MovimientoRecepcion
                ? tags.tag_recepcion
                : tags.tag_devolucion;
            string observacion = movimiento == MovimientoRecepcion
                ? "Recibida la documentacion de la afiliacion"
                : "Devolucion de la documentacion de la afiliacion";
            var afiliaciones = request.afiliaciones
                .GroupBy(item => new
                {
                    Cedula = item.cedula.Trim(),
                    item.consec
                })
                .Select(group => group.First());

            int aplicados = 0;
            foreach (var item in afiliaciones)
            {
                var afiliacion =
                    AF_frmAF_RecepcionAfiliacionesTags_Afiliacion_Validada_Obtener(
                        connection,
                        transaction,
                        item,
                        movimiento,
                        tags);
                AF_frmAF_RecepcionAfiliacionesTags_Tag_Registrar(
                    connection,
                    transaction,
                    afiliacion,
                    request.usuario,
                    tag,
                    observacion);
                aplicados++;
            }

            return aplicados;
        }

        /// <summary>
        /// Valida nuevamente y obtiene una afiliacion antes de registrar el tag.
        /// </summary>
        /// <param name="connection">Conexion SQL abierta.</param>
        /// <param name="transaction">Transaccion activa.</param>
        /// <param name="item">Afiliacion solicitada.</param>
        /// <param name="movimiento">Movimiento normalizado.</param>
        /// <param name="tags">Configuracion de etiquetas.</param>
        /// <returns>Afiliacion vigente para el proceso.</returns>
        private static AfRecepcionAfiliacionesTagsAfiliacionResponse
            AF_frmAF_RecepcionAfiliacionesTags_Afiliacion_Validada_Obtener(
                SqlConnection connection,
                SqlTransaction transaction,
                AfRecepcionAfiliacionesTagsAplicarItem item,
                string movimiento,
                TagsConfiguracion tags)
        {
            string cedula = item.cedula.Trim();
            string? error = AF_frmAF_RecepcionAfiliacionesTags_Tag_Validar(
                connection,
                transaction,
                cedula,
                item.consec,
                movimiento,
                tags);
            if (error is not null)
            {
                throw new InvalidOperationException(error);
            }

            return AF_frmAF_RecepcionAfiliacionesTags_Afiliacion_Consultar(
                    connection,
                    transaction,
                    cedula,
                    item.consec,
                    movimiento)
                ?? throw new InvalidOperationException(
                    $"La cedula {cedula} y boleta {item.consec} ya no cumplen el estado requerido.");
        }

        /// <summary>
        /// Registra la etiqueta de una afiliacion validada.
        /// </summary>
        /// <param name="connection">Conexion SQL abierta.</param>
        /// <param name="transaction">Transaccion activa.</param>
        /// <param name="afiliacion">Afiliacion validada.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <param name="tag">Etiqueta a registrar.</param>
        /// <param name="observacion">Detalle del movimiento.</param>
        private static void
            AF_frmAF_RecepcionAfiliacionesTags_Tag_Registrar(
                SqlConnection connection,
                SqlTransaction transaction,
                AfRecepcionAfiliacionesTagsAfiliacionResponse afiliacion,
                string usuario,
                string tag,
                string observacion)
        {
            var parametros = new DynamicParameters();
            parametros.Add("Codigo", afiliacion.cedula);
            parametros.Add("Tag", tag);
            parametros.Add("Usuario", usuario.Trim());
            parametros.Add("Observacion", observacion);
            parametros.Add("Documento", afiliacion.consec.ToString());
            parametros.Add("Modulo", Modulo);
            parametros.Add("Llave_01", afiliacion.cedula);
            parametros.Add("Llave_02", afiliacion.consec.ToString());
            parametros.Add("Llave_03", string.Empty);
            connection.Execute(
                "spSIFRegistraTags",
                parametros,
                transaction,
                commandType: CommandType.StoredProcedure);
        }
    }
}
