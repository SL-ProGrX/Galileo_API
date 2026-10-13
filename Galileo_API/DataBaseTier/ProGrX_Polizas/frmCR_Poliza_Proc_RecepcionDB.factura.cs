using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using static Galileo_API.Models.ProGrX_Polizas.FrmCrPolizaProcRecepcionModels;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    /// <summary>
    /// Eliminar registros seleccionados (VB6: btnEliminar_Click) y generar la factura en Cuentas por Pagar (VB6: btnFactura_Click).
    /// </summary>
    public partial class FrmCrPolizaProcRecepcionDB
    {
        private const string MotivoDescarte = "Poliza Descartada por Cancelacion";

        /// <summary>
        /// Elimina los registros seleccionados del detalle, registra bitácora por cada uno y recalcula el resumen del corte.
        /// El Result contiene el mensaje de advertencia de spPoliza_Aseguradora_Corte_Actualiza cuando Pass = 0 (vacío si no hay advertencia).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<string> Crd_PolizaProcRecepcion_Detalle_Eliminar(int codEmpresa, string usuario, PolizaDetalleEliminarRequest request)
        {
            if (request.corte is null || string.IsNullOrWhiteSpace(request.cod_poliza))
            {
                return DbHelper.CreateErrorResponse<string>("Indique la póliza y el corte.");
            }

            if (request.registros.Count == 0)
            {
                return DbHelper.CreateErrorResponse<string>("Seleccione los Casos a Eliminar!");
            }

            try
            {
                using var conn = _portalDb.CreateConnection(codEmpresa);
                conn.Open();

                var corte = request.corte.Value;
                var idFactura = request.id_factura ?? 0;

                var valida = ResultadoSp(SpCorteValida(conn, corte, request.cod_poliza, idFactura));
                if (valida.Code == -1)
                {
                    return DbHelper.CreateErrorResponse<string>(valida.Description);
                }

                EliminarRegistros(conn, codEmpresa, usuario, request.registros);

                var actualiza = SpCorteActualiza(conn, corte, request.cod_poliza, idFactura);
                var advertencia = actualiza?.pass == 0 ? actualiza.mensaje : string.Empty;

                return DbHelper.CreateOkResponse(advertencia);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<string>(ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta spPoliza_Aseguradora_Corte_Detalle_Elimina por cada registro y deja bitácora cuando Pass = 1 y hay mensaje.
        /// </summary>
        private void EliminarRegistros(SqlConnection conn, int codEmpresa, string usuario, IEnumerable<long> registros)
        {
            foreach (var idRegistro in registros)
            {
                var resultado = conn.QueryFirstOrDefault<PolizaAseguradoraCorte>(
                    "exec dbo.spPoliza_Aseguradora_Corte_Detalle_Elimina @registro, @usuario, @motivo",
                    new { registro = idRegistro, usuario, motivo = MotivoDescarte });

                if (resultado?.pass == 1 && !string.IsNullOrEmpty(resultado.mensaje))
                {
                    RegistrarBitacora(codEmpresa, usuario, resultado.movimiento, resultado.mensaje);
                }
            }
        }

        /// <summary>
        /// Actualiza el encabezado y genera la factura en Cuentas por Pagar (VB6: btnFactura_Click).
        /// Orden VB6: Valida, Corte_Add (actualiza datos), Corte_Pago y Bitácora.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto<PolizaAseguradoraCorte?> Crd_PolizaProcRecepcion_Factura_Generar(int codEmpresa, string usuario, PolizaAseguradoraCorteData datos)
        {
            var errorEncabezado = ValidarEncabezado(datos);
            if (errorEncabezado is not null)
            {
                return DbHelper.CreateErrorResponse<PolizaAseguradoraCorte?>(errorEncabezado);
            }

            if ((datos.id_factura ?? 0) == 0)
            {
                return DbHelper.CreateErrorResponse<PolizaAseguradoraCorte?>(
                    "El Corte No ha Indicado Datos a Facturar, no puede enviar a Cuentas por Pagar");
            }

            try
            {
                using var conn = _portalDb.CreateConnection(codEmpresa);
                conn.Open();
                return GenerarFactura(conn, codEmpresa, usuario, datos);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<PolizaAseguradoraCorte?>(ex.Message);
            }
        }

        /// <summary>
        /// Flujo secuencial de generación de factura: Valida, Add, Pago y Bitácora.
        /// </summary>
        private ErrorDto<PolizaAseguradoraCorte?> GenerarFactura(SqlConnection conn, int codEmpresa, string usuario, PolizaAseguradoraCorteData datos)
        {
            var corte = datos.corte!.Value;

            var valida = ResultadoSp(SpCorteValida(conn, corte, datos.cod_poliza, datos.id_factura ?? 0));
            if (valida.Code == -1)
            {
                return valida;
            }

            var actualiza = ResultadoSp(SpCorteAdd(conn, datos, usuario));
            if (actualiza.Code == -1)
            {
                return actualiza;
            }

            var idFactura = actualiza.Result!.idfactura ?? 0;
            var pago = conn.QueryFirstOrDefault<PolizaAseguradoraCorte>(
                "exec dbo.spPoliza_Aseguradora_Corte_Pago @corte, @poliza, @factura, @usuario",
                new { corte = corte.Date, poliza = datos.cod_poliza, factura = idFactura, usuario });

            var resultado = ResultadoSp(pago);
            if (resultado.Code == 0)
            {
                resultado.Result!.idfactura ??= idFactura;
                RegistrarBitacora(codEmpresa, usuario, resultado.Result.movimiento, resultado.Result.mensaje);
            }

            return resultado;
        }
    }
}
