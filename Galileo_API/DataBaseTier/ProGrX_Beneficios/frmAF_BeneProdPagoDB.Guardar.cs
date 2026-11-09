using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneProdPagoDB
    {
        private const string MensajeRegistrosRequeridos =
            "Debe seleccionar al menos un beneficio.";
        private const string MensajeUsuarioRequerido =
            "El usuario que realiza la entrega es requerido.";
        private const int MaximoPendientesMensaje = 20;

        private sealed record EntregaClave(
            string CodBeneficio,
            int Consec,
            string Cedula);

        private sealed class EntregaResultado
        {
            public ErrorDto? Error { get; set; }

            public string Boleta { get; set; } = string.Empty;

            public List<BitacoraBeneInsertarDto> Bitacora { get; } = new();
        }

        /// <summary>
        /// Procesa la entrega de los beneficios seleccionados.
        /// Actualiza el otorgamiento, genera la boleta de salida de
        /// inventario, marca los pagos como enviados y registra la bitácora.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Usuario y beneficios seleccionados.</param>
        /// <returns>Resultado del proceso de entrega.</returns>
        public ErrorDto AF_BeneProdPago_Entrega_Procesar(
            int CodEmpresa,
            AfiBeneProdPagoEntregaRequest request)
        {
            var validacion =
                AF_BeneProdPago_Entrega_Validar(request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            var usuario = request.usuario
                .Trim()
                .ToUpperInvariant();

            var claves = AF_BeneProdPago_Entrega_Claves_Crear(
                request.registros);

            var resultado = new EntregaResultado();

            using var connection = DbHelper.OpenConnection(
                AF_BeneProdPago_Portal_Crear(),
                CodEmpresa);

            try
            {
                connection.Open();

                using var transaction =
                    connection.BeginTransaction();

                try
                {
                    AF_BeneProdPago_Entrega_Registrar(
                        connection,
                        transaction,
                        usuario,
                        claves,
                        resultado);

                    if (resultado.Error != null)
                    {
                        transaction.Rollback();
                        return resultado.Error;
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }

            return AF_BeneProdPago_Entrega_Finalizar(
                CodEmpresa,
                usuario,
                resultado);
        }

        /// <summary>
        /// Ejecuta todos los pasos transaccionales de la entrega.
        /// </summary>
        private void AF_BeneProdPago_Entrega_Registrar(
            SqlConnection connection,
            SqlTransaction transaction,
            string usuario,
            List<EntregaClave> claves,
            EntregaResultado resultado)
        {
            var pendientes = AF_BeneProdPago_Otorga_Actualizar(
                connection,
                transaction,
                usuario,
                claves);

            if (pendientes.Count > 0)
            {
                resultado.Error = DbHelper.ErrorResponse(
                    AF_BeneProdPago_Pendientes_Mensaje(pendientes),
                    CodigoValidacion);
                return;
            }

            var productos = AF_BeneProdPago_Productos_Obtener(
                connection,
                transaction,
                claves);

            resultado.Error = AF_BeneProdPago_Existencia_Validar(
                connection,
                transaction,
                productos);

            if (resultado.Error != null) return;

            resultado.Error = AF_BeneProdPago_Boleta_Crear(
                connection,
                transaction,
                usuario,
                productos,
                resultado);

            if (resultado.Error != null) return;

            AF_BeneProdPago_Pago_Actualizar(
                connection,
                transaction,
                usuario,
                claves);

            AF_BeneProdPago_Bitacora_Preparar(
                usuario,
                productos,
                resultado);
        }

        /// <summary>
        /// Marca el otorgamiento como entregado y devuelve los registros
        /// que ya no estaban pendientes.
        /// </summary>
        private static List<string> AF_BeneProdPago_Otorga_Actualizar(
            SqlConnection connection,
            SqlTransaction transaction,
            string usuario,
            List<EntregaClave> claves)
        {
            const string sql = """
                UPDATE afi_bene_otorga
                SET
                    estado = 'E',
                    autoriza_user = @usuario,
                    autoriza_fecha = dbo.MyGetdate()
                WHERE cedula = @cedula
                    AND cod_beneficio = @cod_beneficio
                    AND consec = @consec
                    AND estado = 'S'
                """;

            var pendientes = new List<string>();

            foreach (var clave in claves)
            {
                var actualizados = connection.Execute(
                    sql,
                    new
                    {
                        usuario,
                        cedula = clave.Cedula,
                        cod_beneficio = clave.CodBeneficio,
                        consec = clave.Consec
                    },
                    transaction);

                if (actualizados == 0)
                {
                    pendientes.Add(
                        $"{clave.CodBeneficio}-{clave.Consec}");
                }
            }

            return pendientes;
        }

        /// <summary>
        /// Marca los pagos de los beneficios como enviados.
        /// </summary>
        private static void AF_BeneProdPago_Pago_Actualizar(
            SqlConnection connection,
            SqlTransaction transaction,
            string usuario,
            List<EntregaClave> claves)
        {
            const string sql = """
                UPDATE afi_bene_pago
                SET
                    estado = 'E',
                    envio_user = @usuario,
                    envio_fecha = GETDATE()
                WHERE cedula = @cedula
                    AND cod_beneficio = @cod_beneficio
                    AND consec = @consec
                    AND estado <> 'E'
                """;

            foreach (var clave in claves)
            {
                connection.Execute(
                    sql,
                    new
                    {
                        usuario,
                        cedula = clave.Cedula,
                        cod_beneficio = clave.CodBeneficio,
                        consec = clave.Consec
                    },
                    transaction);
            }
        }

        /// <summary>
        /// Prepara los movimientos de bitácora de cada producto entregado.
        /// </summary>
        private static void AF_BeneProdPago_Bitacora_Preparar(
            string usuario,
            List<AfiBeneProdEntregaProducto> productos,
            EntregaResultado resultado)
        {
            foreach (var producto in productos)
            {
                resultado.Bitacora.Add(new BitacoraBeneInsertarDto
                {
                    cod_beneficio = producto.cod_beneficio,
                    consec = producto.consec,
                    movimiento = "Actualiza",
                    detalle =
                        $"Entrega de Producto COD: [{producto.cod_producto}], " +
                        $"Monto: [{producto.costo_unidad * producto.cantidad}]",
                    registro_usuario = usuario
                });
            }
        }

        /// <summary>
        /// Registra la bitácora y procesa la boleta de inventario una vez
        /// confirmada la transacción.
        /// </summary>
        private ErrorDto AF_BeneProdPago_Entrega_Finalizar(
            int CodEmpresa,
            string usuario,
            EntregaResultado resultado)
        {
            var errorBitacora = AF_BeneProdPago_Bitacora_Registrar(
                CodEmpresa,
                resultado.Bitacora);

            if (!string.IsNullOrEmpty(resultado.Boleta))
            {
                var procesa = new FrmInvTransacProcesaDB(_config)
                    .InvTransacProcesa_SP(
                        CodEmpresa,
                        new InvTransacProcesa
                        {
                            tipo = "S",
                            boleta = resultado.Boleta,
                            usuario = usuario
                        });

                if (procesa.Code != 0)
                {
                    return DbHelper.ErrorResponse(
                        "Los beneficios fueron entregados, pero no se pudo " +
                        $"procesar la boleta de salida {resultado.Boleta}: " +
                        procesa.Description);
                }
            }

            var mensaje =
                "Los beneficios fueron entregados correctamente.";

            if (!string.IsNullOrEmpty(errorBitacora))
            {
                mensaje +=
                    " No se pudo registrar la bit&aacute;cora: " +
                    errorBitacora;
            }

            return DbHelper.OkResponse(mensaje);
        }

        /// <summary>
        /// Inserta los movimientos de bitácora.
        /// </summary>
        /// <returns>Mensaje de error o una cadena vacía.</returns>
        private string AF_BeneProdPago_Bitacora_Registrar(
            int CodEmpresa,
            List<BitacoraBeneInsertarDto> movimientos)
        {
            try
            {
                var bitacora = new MBeneficiosDB(_config);

                foreach (var movimiento in movimientos)
                {
                    movimiento.EmpresaId = CodEmpresa;
                    bitacora.BitacoraBeneficios(movimiento);
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Crea las claves únicas de los registros seleccionados.
        /// </summary>
        private static List<EntregaClave> AF_BeneProdPago_Entrega_Claves_Crear(
            List<AfiBeneProdAsgData> registros)
        {
            return registros
                .Select(registro => new EntregaClave(
                    registro.cod_beneficio.Trim(),
                    registro.consec,
                    registro.cedula.Trim()))
                .DistinctBy(clave => (clave.CodBeneficio, clave.Consec))
                .ToList();
        }

        /// <summary>
        /// Crea el mensaje con los beneficios que ya no están pendientes.
        /// </summary>
        private static string AF_BeneProdPago_Pendientes_Mensaje(
            List<string> pendientes)
        {
            var detalle = string.Join(
                ", ",
                pendientes.Take(MaximoPendientesMensaje));

            if (pendientes.Count > MaximoPendientesMensaje)
            {
                detalle +=
                    $" y {pendientes.Count - MaximoPendientesMensaje} m&aacute;s";
            }

            return "No se entreg&oacute; ning&uacute;n beneficio. " +
                "Los siguientes ya no est&aacute;n pendientes de entrega " +
                $"(beneficio-id): {detalle}. " +
                "Actualice la lista e intente de nuevo.";
        }

        /// <summary>
        /// Valida la solicitud de entrega de beneficios.
        /// </summary>
        /// <param name="request">Solicitud que se debe validar.</param>
        /// <returns>Mensaje de validación o una cadena vacía.</returns>
        private static string AF_BeneProdPago_Entrega_Validar(
            AfiBeneProdPagoEntregaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return MensajeUsuarioRequerido;
            }

            if (request.registros.Count == 0)
            {
                return MensajeRegistrosRequeridos;
            }

            if (request.registros.Any(
                registro =>
                    registro.consec <= 0 ||
                    string.IsNullOrWhiteSpace(
                        registro.cod_beneficio) ||
                    string.IsNullOrWhiteSpace(
                        registro.cedula)))
            {
                return "Los datos del beneficio seleccionado no son v&aacute;lidos.";
            }

            return string.Empty;
        }
    }
}
