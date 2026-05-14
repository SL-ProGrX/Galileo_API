using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaSubCreditoDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly MProGrxMain _trazabilidad;
        private readonly int vModulo = 3;

        public FrmPreaSubCreditoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
            _trazabilidad = new MProGrxMain(config);
        }

        /// <summary>
        /// Carga la información inicial de frmPreaSubCredito: validación del expediente,
        /// bancos, operaciones, tipos de documento y cuentas iniciales del primer banco.
        /// </summary>
        public ErrorDto<FrmPreaSubCreditoCargarResponse> Prea_frmPreaSubCredito_Cargar(
            int codEmpresa,
            FrmPreaSubCreditoCargarRequest request)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                var codPreanalisis = request.cod_preanalisis?.Trim() ?? string.Empty;
                var usuario = request.usuario?.Trim() ?? string.Empty;

                var validacion = connection.QueryFirstOrDefault<FrmPreaSubCreditoValidacionData>(
                    @"EXEC spCRDPreaSolicitudValida @Expediente",
                    new { Expediente = codPreanalisis },
                    commandType: CommandType.Text
                ) ?? new FrmPreaSubCreditoValidacionData();

                var persona = connection.QueryFirstOrDefault<FrmPreaSubCreditoPersonaData>(
                    @"
                    SELECT TOP 1
                        RTRIM(ISNULL(CEDULA, '')) AS cedula
                    FROM CRD_PREA_PREANALISIS
                    WHERE COD_PREANALISIS = @cod_preanalisis
                       OR COD_PREANALISIS_REF = @cod_preanalisis;",
                    new { cod_preanalisis = codPreanalisis },
                    commandType: CommandType.Text
                ) ?? new FrmPreaSubCreditoPersonaData();

                var bancos = connection.Query<dynamic>(
                    @"EXEC spCrd_SGT_Bancos @Usuario",
                    new { Usuario = usuario },
                    commandType: CommandType.Text
                ).Select(x => new DropDownListaGenericaModel
                {
                    item = Convert.ToInt32(x.IdX),
                    descripcion = Convert.ToString(x.ItmX) ?? string.Empty
                }).ToList();

                var operaciones = connection.Query<dynamic>(
                    @"EXEC spCrdPrea_Operacion_Vincular @Expediente",
                    new { Expediente = codPreanalisis },
                    commandType: CommandType.Text
                ).Select(x => new DropDownListaGenericaModel
                {
                    item = Convert.ToInt32(x.IdX),
                    descripcion = Convert.ToString(x.ItmX) ?? string.Empty
                }).ToList();

                var tiposDocumento = new List<FrmPreaSubCreditoTipoDocumentoItem>
                {
                    new() { codigo = "RE", descripcion = "Efectivo", emitir_transferencia = 0 },
                    new() { codigo = "CK", descripcion = "Cheque", emitir_transferencia = 0 },
                    new() { codigo = "TE", descripcion = "Transferencia", emitir_transferencia = 1 },
                    new() { codigo = "TS", descripcion = "Transferencia SINPE", emitir_transferencia = 0 },
                    new() { codigo = "ND", descripcion = "Nota Debito", emitir_transferencia = 0 }
                };

                var cuentas = new List<FrmPreaSubCreditoCuentaItem>();
                var primerBanco = bancos.FirstOrDefault();

                if (primerBanco is not null && Convert.ToInt32(primerBanco.item) > 0)
                {
                    cuentas = connection.Query<dynamic>(
                        @"EXEC spSys_Cuentas_Bancarias @Cedula, @BancoId, @Tipo",
                        new
                        {
                            Cedula = persona.cedula,
                            BancoId = primerBanco!.item,
                            Tipo = 1
                        },
                        commandType: CommandType.Text
                    ).Select(x => new FrmPreaSubCreditoCuentaItem
                    {
                        codigo = Convert.ToString(x.IdX) ?? string.Empty,
                        descripcion = Convert.ToString(x.ItmX) ?? string.Empty
                    }).ToList();
                }

                var result = new FrmPreaSubCreditoCargarResponse
                {
                    cod_preanalisis = codPreanalisis,
                    aprobado = validacion.aprobado,
                    pendiente = validacion.pendiente,
                    maestro = validacion.maestro,
                    comite = validacion.comite,
                    mensaje_validacion = ConstruirMensajeValidacion(validacion),
                    bancos = bancos,
                    operaciones = operaciones,
                    tipos_documento = tiposDocumento,
                    cuentas = cuentas
                };

                return DbHelper.CreateOkResponse<FrmPreaSubCreditoCargarResponse>(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaSubCreditoCargarResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta la creación de la solicitud de crédito del expediente validando nuevamente
        /// el estado del preanálisis antes de invocar el proceso de registro.
        /// </summary>
        public ErrorDto<FrmPreaSubCreditoAplicarResponse> Prea_frmPreaSubCredito_Aplicar(
            int codEmpresa,
            FrmPreaSubCreditoAplicarRequest request)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                var codPreanalisis = request.cod_preanalisis?.Trim() ?? string.Empty;
                var usuario = request.usuario?.Trim() ?? string.Empty;
                var tipoDocumento = request.tipo_documento?.Trim() ?? string.Empty;

                var validacion = connection.QueryFirstOrDefault<FrmPreaSubCreditoValidacionData>(
                    @"EXEC spCRDPreaSolicitudValida @Expediente",
                    new { Expediente = codPreanalisis },
                    commandType: CommandType.Text
                ) ?? new FrmPreaSubCreditoValidacionData();

                var mensajeValidacion = ConstruirMensajeValidacion(validacion);
                if (!string.IsNullOrWhiteSpace(mensajeValidacion))
                {
                    return DbHelper.CreateErrorResponse<FrmPreaSubCreditoAplicarResponse>(mensajeValidacion);
                }

                var operacion = connection.QueryFirstOrDefault<FrmPreaSubCreditoOperacionData>(
                    @"
                    EXEC spCRDPreaSolicitudCrd
                        @Expediente,
                        @Comite,
                        @Banco,
                        @Transferencia,
                        @Emitir,
                        @Cuenta,
                        @Documento,
                        @Usuario,
                        @Operacion;",
                    new
                    {
                        Expediente = codPreanalisis,
                        Comite = validacion.comite,
                        Banco = request.banco,
                        Transferencia = request.emitir_transferencia,
                        Emitir = tipoDocumento,
                        Cuenta = request.cuenta?.Trim() ?? string.Empty,
                        Documento = string.Empty,
                        usuario,
                        Operacion = request.operacion
                    },
                    commandType: CommandType.Text
                ) ?? new FrmPreaSubCreditoOperacionData();

                var idSolicitud = operacion.operacion?.Trim() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(idSolicitud))
                {
                    RegistrarBitacora(
                        codEmpresa,
                        usuario,
                        "Registra - WEB",
                        $"Recepción de la Operacion : {idSolicitud}"
                    );

                    _trazabilidad.sbTrazabilidad_Inserta(
                        codEmpresa,
                        "01",
                        idSolicitud,
                        idSolicitud,
                        usuario
                    );
                }

                var result = new FrmPreaSubCreditoAplicarResponse
                {
                    cod_preanalisis = codPreanalisis,
                    id_solicitud = idSolicitud,
                    mensaje = string.IsNullOrWhiteSpace(idSolicitud)
                        ? "No fue posible generar la solicitud de crédito."
                        : $"Solicitud de Credito Generada Satisfactoriamente, Solicitud # {idSolicitud}"
                };

                return DbHelper.CreateOkResponse<FrmPreaSubCreditoAplicarResponse>(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaSubCreditoAplicarResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene las cuentas bancarias disponibles para la cédula y banco seleccionados
        /// en frmPreaSubCredito.
        /// </summary>
        public ErrorDto<FrmPreaSubCreditoCuentasResponse> Prea_frmPreaSubCredito_Cuentas_Obtener(
            int codEmpresa,
            FrmPreaSubCreditoCuentasRequest request)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                var cedula = request.cedula?.Trim() ?? string.Empty;
                var banco = request.banco;

                var cuentas = new List<FrmPreaSubCreditoCuentaItem>();

                if (!string.IsNullOrWhiteSpace(cedula) && banco > 0)
                {
                    cuentas = connection.Query<dynamic>(
                        @"EXEC spSys_Cuentas_Bancarias @Cedula, @BancoId, @Tipo",
                        new
                        {
                            Cedula = cedula,
                            BancoId = banco,
                            Tipo = 1
                        },
                        commandType: CommandType.Text
                    ).Select(x => new FrmPreaSubCreditoCuentaItem
                    {
                        codigo = Convert.ToString(x.IdX) ?? string.Empty,
                        descripcion = Convert.ToString(x.ItmX) ?? string.Empty
                    }).ToList();
                }

                var result = new FrmPreaSubCreditoCuentasResponse
                {
                    banco = banco,
                    cuentas = cuentas
                };

                return DbHelper.CreateOkResponse<FrmPreaSubCreditoCuentasResponse>(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaSubCreditoCuentasResponse>(ex.Message);
            }
        }

        private static string ConstruirMensajeValidacion(FrmPreaSubCreditoValidacionData validacion)
        {
            var mensajes = new List<string>();

            if (validacion.aprobado == 0)
                mensajes.Add(" - El Expediente no se encuentra aprobado");

            if (validacion.pendiente > 0)
                mensajes.Add(" - La Solicitud de Crédito ya fue realizada");

            if (validacion.maestro == 0)
                mensajes.Add(" - Este es un SubExpediente, verifique...");

            if (validacion.comite == 0)
                mensajes.Add(" - No está asignado un comité a evaluación para el expediente");

            return string.Join(Environment.NewLine, mensajes);
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
