using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using static Galileo_API.Models.ProGrX.Cajas.CajasDesglocePagoRequest;

namespace Galileo.DataBaseTier
{
    public class FrmCajasDetallePagoDB
    {
        private readonly PortalDB _portalDb;

        public FrmCajasDetallePagoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config ?? throw new ArgumentNullException(nameof(config)));
        }

        /// <summary>
        /// Obtiene el tipo de cambio
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que contiene la configuración de cajas.</param>
        /// <param name="enlace">Código de contabilidad usado por la función de tipo de cambio.</param>
        /// <param name="codDivisa">Código de la divisa consultada.</param>
        /// <returns>Respuesta con el tipo de cambio vigente o el error producido.</returns>
        public ErrorDto<decimal> Cajas_TipoCambio(int codEmpresa, int enlace, string codDivisa)
        {
            const string sql = @"SELECT dbo.fxCajas_TipoCambio(@Enlace, @Divisa, dbo.MyGetDate(), 'C') AS TipoCambio";
            var result = DbHelper.ExecuteSingleQuery<decimal>(_portalDb, codEmpresa, sql, 0, new { Enlace = enlace, Divisa = codDivisa });

            return result.Code == 0
     ? DbHelper.CreateOkResponse(result.Result)
     : DbHelper.CreateErrorResponse<decimal>($"Error en Cajas_TipoCambio: {result.Description}");
        }

        /// <summary>
        /// Elimina del desgloce de pago
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codCaja">Código de la caja.</param>
        /// <param name="codApertura">Número de apertura.</param>
        /// <param name="ticket">Número del tiquete.</param>
        /// <param name="linea">Línea del desglose que se eliminará.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto Cajas_DesglocePago_Eliminar(int codEmpresa, string codCaja, int codApertura, string ticket, int linea)
        {
            const string sql = @"
                            DELETE FROM CAJAS_DESGLOCE_PAGO
                            WHERE Cod_Caja = @CodCaja
                              AND Cod_Apertura = @CodApertura
                              AND Ticket = @Ticket
                              AND Linea = @Linea";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new { CodCaja = codCaja, CodApertura = codApertura, Ticket = ticket, Linea = linea });
        }

        /// <summary>
        /// Obtiene el disponible de fondos de la caja
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codCaja">Código de la caja.</param>
        /// <param name="codApertura">Número de apertura.</param>
        /// <param name="ticket">Número del tiquete.</param>
        /// <param name="codPlan">Código del plan de fondos.</param>
        /// <param name="codContrato">Número del contrato.</param>
        /// <returns>Fondo y monto disponibles para aplicar.</returns>
        public ErrorDto<CajasDisponibleFondosDto> Cajas_DisponibleFondos(int codEmpresa, string codCaja, int codApertura, string ticket, string codPlan, int codContrato)
        {
            var response = DbHelper.CreateOkResponse<CajasDisponibleFondosDto>(default);

            try
            {
                var connectionString = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);
                var result = DbHelper.ExecuteStoredProcedureSingle<CajasDisponibleFondosDto>(
                    connectionString,
                    "spCajas_DisponibleFondos",
                    default,
                    new { CodCaja = codCaja, CodApertura = codApertura, Ticket = ticket, CodPlan = codPlan, CodContrato = codContrato });

                response.Code = result.Code;
                response.Description = result.Description;
                response.Result = result.Result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        /// Obtiene saldos a favor
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="clienteid">Identificación del cliente.</param>
        /// <param name="referencia">Identificador numérico del saldo.</param>
        /// <param name="referencia_texto">Texto de referencia del saldo.</param>
        /// <returns>Saldos a favor encontrados para el cliente.</returns>
        public ErrorDto<List<CajasSaldoFavorDetDto>> Cajas_SaldoFavor_Obtener(int codEmpresa, string clienteid, int referencia, string referencia_texto)
        {
            var response = DbHelper.CreateOkResponse(new List<CajasSaldoFavorDetDto>());

            try
            {
                var connectionString = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);
                var result = DbHelper.ExecuteStoredProcedureList<CajasSaldoFavorDetDto>(
                    connectionString,
                    "spCajas_SaldoFavor",
                    new { clienteid, referencia, referenciatexto = referencia_texto });

                response.Code = result.Code;
                response.Description = result.Description;
                response.Result = result.Result ?? new List<CajasSaldoFavorDetDto>();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "Error en Cajas_SaldoFavor_Obtener: " + ex.Message;
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        /// Obtiene la divisa funcional
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="enlace">Código de contabilidad.</param>
        /// <returns>Divisa funcional de la contabilidad indicada.</returns>
        public ErrorDto<CajasDivisaFuncionalDto> Cajas_DivisaFuncional_Obtener(int codEmpresa, string enlace)
        {
            const string sql = @"select dbo.fxCajas_DivisaFuncional(@enlace) as Divisa";
            var result = DbHelper.ExecuteSingleQuery<CajasDivisaFuncionalDto?>(_portalDb, codEmpresa, sql, default, new { enlace });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CajasDivisaFuncionalDto>("Error en Cajas_DivisaFuncional_Obtener: " + result.Description);
            }

            return DbHelper.CreateOkResponse(result.Result ?? new CajasDivisaFuncionalDto());
        }

        /// <summary>
        /// Obtiene los depositos bancarios
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="formaPago">Código de la forma de pago.</param>
        /// <returns>Cuentas bancarias autorizadas para depósitos.</returns>
        public ErrorDto<List<CajasDepositosCuentasBancariasDto>> Cajas_DepositosCuentasBancariasAut_Obtener(int codEmpresa, string formaPago)
        {
            var response = DbHelper.CreateOkResponse(new List<CajasDepositosCuentasBancariasDto>());

            try
            {
                var connectionString = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);
                var result = DbHelper.ExecuteStoredProcedureList<CajasDepositosCuentasBancariasDto>(
                    connectionString,
                    "spCajas_DepositosCuentasBancariasAut",
                    new { formaPago });

                if (result.Code != 0)
                {
                    response.Code = result.Code;
                    response.Description = "Error en Cajas_DepositosCuentasBancariasAut_Obtener: " + result.Description;
                    response.Result = null;
                    return response;
                }

                var items = result.Result ?? new List<CajasDepositosCuentasBancariasDto>();
                foreach (var item in items)
                {
                    item.itmx = $"{item.cta} - {item.itmx}";
                }

                response.Result = items;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "Error en Cajas_DepositosCuentasBancariasAut_Obtener: " + ex.Message;
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        /// Desgloce de Pago Obtener
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codCaja">Código de la caja.</param>
        /// <param name="ticket">Número del tiquete.</param>
        /// <param name="codApertura">Número de apertura.</param>
        /// <param name="linea">Línea del desglose consultado.</param>
        /// <returns>Detalle de la línea solicitada.</returns>
        public ErrorDto<List<CajasDesglocePagoDto>> Cajas_DesglocePago_Obtener(int codEmpresa, string codCaja, string ticket, int codApertura, int linea)
        {
            const string sql = @"
            select 
                C.*, 
                F.DESCRIPCION as FormaPagoDesc,
                F.TIPO
            from CAJAS_DESGLOCE_PAGO C
            inner join SIF_FORMAS_PAGO F 
                on C.COD_FORMA_PAGO = F.COD_FORMA_PAGO
            where C.Cod_Caja = @CodCaja
              and C.Ticket = @Ticket
              and C.Cod_Apertura = @CodApertura
              and C.Linea = @Linea";

            return DbHelper.ExecuteListQuery<CajasDesglocePagoDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { CodCaja = codCaja, Ticket = ticket, CodApertura = codApertura, Linea = linea });
        }

        /// <summary>
        /// Inserta desgloce de pago
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="dto">Datos del desglose que se insertará.</param>
        /// <returns>Resultado de la inserción.</returns>
        public ErrorDto Cajas_DesglocePago_Insert(int codEmpresa, CajasDesglocePagoDto dto)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok",
            };

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

                // 1. Obtener la línea siguiente.
                var sqlLinea = @"SELECT ISNULL(MAX(linea), 0) + 1 
                         FROM CAJAS_DESGLOCE_PAGO 
                         WHERE Cod_Caja = @cod_caja AND Ticket = @ticket AND Cod_Apertura = @cod_apertura";

                dto.linea = connection.ExecuteScalar<int>(sqlLinea, new { dto.cod_caja, dto.ticket, dto.cod_apertura });

                // 2. Insertar el registro.
                string sql = @"
            INSERT INTO CAJAS_DESGLOCE_PAGO
            (linea, Ticket, Cod_Caja, cod_Apertura, Monto, cod_Divisa, Tipo_Cambio, registro_fecha, registro_usuario,
             Cod_Tarjeta, Tarjeta_Numero, Tarjeta_Autorizacion, Cheque_Emisor, Cheque_Numero, Cuenta_Bancaria,
             Num_Referencia, Cod_Cuenta, Aplica_Saldo_Favor, Saldo_Favor, Saldo_Favor_Id, Observaciones, cod_forma_pago,
             DP_Banco, DP_Fecha, COD_PLAN, COD_CONTRATO, COD_ORIGEN_RECURSOS)
            VALUES
            (@linea, @ticket, @cod_caja, @cod_apertura, @monto, @cod_divisa, @tipo_cambio, @registro_fecha, @registro_usuario,
             @cod_tarjeta, @tarjeta_numero, @tarjeta_autorizacion, @cheque_emisor, @cheque_numero, @cuenta_bancaria,
             @num_referencia, @cod_cuenta, @aplica_saldo_favor, @saldo_favor, @saldo_favor_id, @observaciones, @cod_forma_pago,
             @dp_banco, @dp_fecha, @cod_plan, @cod_contrato, @cod_origen_recursos)";

                connection.Execute(sql, dto);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Actualiza el desgloce de pago
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="dto">Datos actualizados del desglose.</param>
        /// <returns>Resultado de la actualización.</returns>
        public ErrorDto Cajas_DesglocePago_Update(int codEmpresa, CajasDesglocePagoDto dto)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                var request = ToGuardarRequest(dto);
                request.cod_cuenta = ResolverCuentaContable(connection, request);
                dto.cod_cuenta = request.cod_cuenta;

                var validacion = ValidarDesglocePago(connection, request, dto.linea);
                if (validacion.Code != 0)
                {
                    return validacion;
                }

                string sql = @"
                        UPDATE CAJAS_DESGLOCE_PAGO SET
                            Monto = @monto,
                            cod_divisa = @cod_divisa,
                            Tipo_Cambio = @tipo_cambio,
                            cod_cuenta = @cod_cuenta,
                            Observaciones = @observaciones,
                            Num_Referencia = @num_referencia,
                            Cuenta_Bancaria = @cuenta_bancaria,
                            Aplica_Saldo_Favor = @aplica_saldo_favor,
                            Saldo_Favor = @saldo_favor,
                            Saldo_Favor_Id = @saldo_favor_id,
                            cod_forma_pago = @cod_forma_pago,
                            Cod_Tarjeta = @cod_tarjeta,
                            Tarjeta_Numero = @tarjeta_numero,
                            Tarjeta_Autorizacion = @tarjeta_autorizacion,
                            Cheque_Emisor = @cheque_emisor,
                            Cheque_Numero = @cheque_numero,
                            DP_Banco = @dp_banco,
                            DP_Fecha = @dp_fecha,
                            COD_PLAN = @cod_plan,
                            COD_CONTRATO = @cod_contrato,
                            COD_ENTIDAD_PAGO = @cod_entidad_pago,
                            COD_ORIGEN_RECURSOS = @cod_origen_recursos
                        WHERE Cod_Caja = @cod_caja
                          AND Cod_Apertura = @cod_apertura
                          AND Ticket = @ticket
                          AND Linea = @linea";

                connection.Execute(sql, dto);

                return DbHelper.OkResponse("OK");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al actualizar desgloce de pago: {ex.Message}");
            }
        }

        /// <summary>
        /// Distribuye saldo a favor
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="dto">Datos para distribuir el saldo a favor.</param>
        /// <returns>Resultado de la distribución.</returns>
        public ErrorDto Cajas_DistribuyeSaldoFavor(int codEmpresa, DistribuyeSaldoFavorDto dto)
        {
            var result = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                codEmpresa,
                @"exec spCajas_DistribuyeSaldoFavor
                    @CodCaja, @CodApertura, @Ticket, @Usuario, @TotalAplicar, @Divisa",
                new
                {
                    CodCaja = dto.cod_caja,
                    CodApertura = dto.cod_apertura,
                    Ticket = dto.ticket,
                    Usuario = dto.usuario,
                    TotalAplicar = dto.total_aplicar,
                    Divisa = dto.divisa
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse($"Error al distribuir el saldo a favor: {result.Description}");
        }

        /// <summary>
        /// Guarda desgloce de pago
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos y contexto del desglose que se guardará.</param>
        /// <returns>Resultado del guardado y sus validaciones.</returns>
        public ErrorDto Cajas_DesglocePago_Guardar(int CodEmpresa, CajasDesglocePagoRequest request)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                connection.Open();

                request.cod_cuenta = ResolverCuentaContable(connection, request);

                var validacion = ValidarDesglocePago(connection, request);
                if (validacion.Code != 0)
                {
                    return validacion;
                }

                var linea = connection.ExecuteScalar<int>(
                    @"select isnull(max(linea), 0) + 1
                      from CAJAS_DESGLOCE_PAGO
                      where Cod_Caja = @cod_caja
                        and Ticket = @ticket
                        and Cod_Apertura = @cod_apertura",
                    request);

                const string sql = @"
                    insert into CAJAS_DESGLOCE_PAGO
                    (linea,ticket,cod_caja,cod_apertura,monto,cod_divisa,tipo_cambio,registro_fecha,registro_usuario,
                    cod_tarjeta,tarjeta_numero,tarjeta_autorizacion,cheque_emisor,cheque_numero,
                    cuenta_bancaria,num_referencia,cod_cuenta,aplica_saldo_favor,saldo_favor,saldo_favor_id,
                    observaciones,cod_forma_pago,dp_banco,dp_fecha,cod_plan,cod_contrato,cod_entidad_pago,cod_origen_recursos)
                    values
                    (@linea,@ticket,@cod_caja,@cod_apertura,@monto,@cod_divisa,@tipo_cambio,GETDATE(),@usuario,
                    @cod_tarjeta,@tarjeta_numero,@tarjeta_autorizacion,@cheque_emisor,@cheque_numero,
                    @cuenta_bancaria,@num_referencia,@cod_cuenta,@aplica_saldo_favor,@saldo_favor,@saldo_favor_id,
                    @notas,@cod_forma_pago,@dp_banco,@dp_fecha,@cod_plan,@cod_contrato,@cod_entidad_pago,@cod_origen_recursos)";

                connection.Execute(sql, new
                {
                    linea,
                    request.ticket,
                    request.cod_caja,
                    request.cod_apertura,
                    request.monto,
                    request.cod_divisa,
                    request.tipo_cambio,
                    request.usuario,
                    request.cod_tarjeta,
                    request.tarjeta_numero,
                    request.tarjeta_autorizacion,
                    request.cheque_emisor,
                    request.cheque_numero,
                    request.cuenta_bancaria,
                    request.num_referencia,
                    request.cod_cuenta,
                    aplica_saldo_favor = request.aplica_saldo_favor ?? 0,
                    saldo_favor = request.saldo_favor ?? 0,
                    saldo_favor_id = request.saldo_favor_id ?? 0,
                    request.notas,
                    request.cod_forma_pago,
                    dp_banco = request.dp_banco ?? 0,
                    request.dp_fecha,
                    request.cod_plan,
                    request.cod_contrato,
                    request.cod_entidad_pago,
                    request.cod_origen_recursos
                });

                return DbHelper.OkResponse("OK");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse($"Error al guardar desgloce de pago: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse($"Error al guardar desgloce de pago: {ex.Message}");
            }
        }

        /// <summary>Convierte el DTO de edición al contrato común de guardado.</summary>
        /// <param name="dto">Detalle de pago recibido para actualización.</param>
        /// <returns>Solicitud normalizada para ejecutar las validaciones y persistencia.</returns>
        private static CajasDesglocePagoRequest ToGuardarRequest(CajasDesglocePagoDto dto)
        {
            return new CajasDesglocePagoRequest
            {
                clienteid = dto.clienteid,
                ticket = dto.ticket,
                cod_caja = dto.cod_caja,
                cod_apertura = dto.cod_apertura,
                monto = dto.monto,
                cod_divisa = dto.cod_divisa,
                tipo_cambio = dto.tipo_cambio,
                usuario = dto.registro_usuario,
                cod_tarjeta = dto.cod_tarjeta,
                tarjeta_numero = dto.tarjeta_numero,
                tarjeta_autorizacion = dto.tarjeta_autorizacion,
                cheque_emisor = dto.cheque_emisor,
                cheque_numero = dto.cheque_numero,
                cuenta_bancaria = dto.cuenta_bancaria,
                num_referencia = dto.num_referencia,
                cod_cuenta = dto.cod_cuenta,
                cod_forma_pago = dto.cod_forma_pago,
                dp_banco = dto.dp_banco,
                dp_fecha = dto.dp_fecha,
                cod_plan = dto.cod_plan,
                cod_contrato = dto.cod_contrato,
                saldo_favor_id = dto.saldo_favor_id,
                aplica_saldo_favor = dto.aplica_saldo_favor,
                saldo_favor = dto.saldo_favor,
                cod_entidad_pago = dto.cod_entidad_pago,
                cod_origen_recursos = dto.cod_origen_recursos,
                notas = dto.observaciones
            };
        }

        /// <summary>Resuelve la cuenta contable dinámica de fondos y saldos a favor.</summary>
        /// <param name="connection">Conexión abierta a la empresa.</param>
        /// <param name="request">Solicitud con plan o saldo a favor.</param>
        /// <returns>Cuenta contable que se almacenará en el desglose.</returns>
        private static string? ResolverCuentaContable(SqlConnection connection, CajasDesglocePagoRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.cod_plan))
            {
                return connection.ExecuteScalar<string?>(
                    "select top (1) rtrim(cuenta_conta) from FND_PLANES where cod_plan = @Plan",
                    new { Plan = request.cod_plan });
            }

            if (request.saldo_favor_id.GetValueOrDefault() > 0)
            {
                return connection.ExecuteScalar<string?>(
                    "select dbo.fxCajas_SaldoFavorCuenta(@SaldoFavorId)",
                    new { SaldoFavorId = request.saldo_favor_id });
            }

            return request.cod_cuenta;
        }

        /// <summary>Ejecuta las validaciones de negocio comunes del desglose.</summary>
        /// <param name="connection">Conexión abierta a la empresa.</param>
        /// <param name="request">Solicitud de desglose que se validará.</param>
        /// <param name="lineaActual">Línea actual cuando se está modificando un registro.</param>
        /// <returns>Resultado consolidado de las validaciones.</returns>
        private static ErrorDto ValidarDesglocePago(SqlConnection connection, CajasDesglocePagoRequest request, int? lineaActual = null)
        {
            var tipoFormaPago = connection.ExecuteScalar<string?>(
                "select top 1 Tipo from SIF_FORMAS_PAGO where COD_FORMA_PAGO = @FormaPago",
                new { FormaPago = request.cod_forma_pago })?.Trim() ?? string.Empty;

            var validacionDeposito = ValidarDepositoRegistrado(connection, request, tipoFormaPago);
            if (validacionDeposito.Code != 0)
            {
                return validacionDeposito;
            }

            var validacionReferencia = ValidarFormaPagoRegistrada(connection, request, tipoFormaPago, lineaActual);
            if (validacionReferencia.Code != 0)
            {
                return validacionReferencia;
            }

            var validacionFondos = ValidarFormaPagoFondos(connection, request, tipoFormaPago);
            if (validacionFondos.Code != 0)
            {
                return validacionFondos;
            }

            return ValidarSaldoFavor(connection, request, tipoFormaPago);
        }

        /// <summary>Valida el estado del depósito conforme al parámetro 10 de Cajas.</summary>
        /// <param name="connection">Conexión abierta a la empresa.</param>
        /// <param name="request">Solicitud que contiene banco, referencia, cliente y monto.</param>
        /// <param name="tipoFormaPago">Tipo de la forma de pago seleccionada.</param>
        /// <returns>Resultado de la validación del depósito en Tesorería.</returns>
        private static ErrorDto ValidarDepositoRegistrado(SqlConnection connection, CajasDesglocePagoRequest request, string tipoFormaPago)
        {
            if (tipoFormaPago != "B")
            {
                return DbHelper.OkResponse("OK");
            }

            if (!request.dp_banco.HasValue || request.dp_banco.Value <= 0)
            {
                return DbHelper.ErrorResponse("Debe seleccionar la cuenta bancaria del deposito");
            }

            var valDeposito = connection.ExecuteScalar<int>(
                "select dbo.fxTes_DP_Cargado(@Banco,@Documento,@Cedula,@Monto)",
                new
                {
                    Banco = request.dp_banco,
                    Documento = request.num_referencia,
                    Cedula = request.clienteid,
                    Monto = request.monto
                });

            var controlaDepositos = string.Equals(
                connection.ExecuteScalar<string?>(
                    "select top (1) ltrim(rtrim(valor)) from CAJAS_PARAMETROS where cod_parametro = '10'"),
                "S",
                StringComparison.OrdinalIgnoreCase);

            return valDeposito switch
            {
                0 when controlaDepositos => DbHelper.ErrorResponse(
                    "Se encuentra activado el control de depósitos y este depósito no ha sido registrado"),
                0 or 1 => DbHelper.OkResponse("OK"),
                2 => DbHelper.ErrorResponse(
                    "Este depósito ya fue identificado; búsquelo como saldo a favor del cliente"),
                3 => DbHelper.ErrorResponse("Este depósito pertenece a otra persona"),
                4 => DbHelper.ErrorResponse(
                    "El monto no coincide con el depósito registrado en Control de Depósitos"),
                _ => DbHelper.ErrorResponse("No fue posible validar el depósito en Tesorería")
            };
        }

        /// <summary>Valida la duplicidad usando los parámetros específicos de cada forma de pago.</summary>
        /// <param name="connection">Conexión abierta a la empresa.</param>
        /// <param name="request">Solicitud con los datos del medio de pago.</param>
        /// <param name="tipoFormaPago">Tipo de la forma de pago seleccionada.</param>
        /// <param name="lineaActual">Línea excluida durante una modificación.</param>
        /// <returns>Resultado de las validaciones global y del tiquete.</returns>
        private static ErrorDto ValidarFormaPagoRegistrada(
            SqlConnection connection,
            CajasDesglocePagoRequest request,
            string tipoFormaPago,
            int? lineaActual)
        {
            if (EsMismaFormaPago(connection, request, lineaActual))
            {
                return DbHelper.OkResponse("OK");
            }

            var documento = request.num_referencia ?? string.Empty;
            var banco = string.Empty;
            var cuenta = string.Empty;
            var validaDocumentoGlobal = false;

            switch (tipoFormaPago)
            {
                case "B":
                    banco = request.dp_banco?.ToString() ?? string.Empty;
                    validaDocumentoGlobal = !string.IsNullOrWhiteSpace(documento);
                    break;
                case "D":
                    validaDocumentoGlobal = !string.IsNullOrWhiteSpace(documento);
                    break;
                case "C":
                    documento = request.cheque_numero ?? string.Empty;
                    banco = request.cheque_emisor ?? string.Empty;
                    cuenta = request.cuenta_bancaria ?? string.Empty;
                    validaDocumentoGlobal = !string.IsNullOrWhiteSpace(documento);
                    break;
                case "T":
                    documento = request.cod_tarjeta ?? string.Empty;
                    banco = request.tarjeta_numero ?? string.Empty;
                    cuenta = request.tarjeta_autorizacion ?? string.Empty;
                    break;
                case "F":
                    documento = $"{request.cod_plan}..{request.cod_contrato.GetValueOrDefault()}";
                    banco = request.cod_plan ?? string.Empty;
                    cuenta = request.cod_contrato?.ToString() ?? string.Empty;
                    break;
                case "S":
                    banco = request.saldo_favor_id?.ToString() ?? string.Empty;
                    break;
                default:
                    return DbHelper.OkResponse("OK");
            }

            if (validaDocumentoGlobal)
            {
                var valDoc = connection.ExecuteScalar<int>(
                    "select dbo.fxCajas_DocumentoVerifica(@FormaPago,@Documento,@Banco,@Cuenta)",
                    new { FormaPago = request.cod_forma_pago, Documento = documento, Banco = banco, Cuenta = cuenta });

                if (valDoc > 0)
                {
                    return DbHelper.ErrorResponse("Este medio de pago ya se encuentra registrado");
                }
            }

            var valRegistrado = connection.ExecuteScalar<int>(
                "select dbo.fxCajas_FP_Registada(@Ticket,@FormaPago,@Doc,@Banco,@Cuenta)",
                new
                {
                    Ticket = request.ticket,
                    FormaPago = request.cod_forma_pago,
                    Doc = documento,
                    Banco = banco,
                    Cuenta = cuenta
                });

            return valRegistrado > 0
                ? DbHelper.ErrorResponse("Este movimiento ya fue registrado")
                : DbHelper.OkResponse("OK");
        }

        /// <summary>Valida disponibilidad y duplicidad de fondos.</summary>
        /// <param name="connection">Conexión abierta a la empresa.</param>
        /// <param name="request">Solicitud con plan, contrato y monto.</param>
        /// <param name="tipoFormaPago">Tipo de la forma de pago seleccionada.</param>
        /// <returns>Resultado de la validación de fondos.</returns>
        private static ErrorDto ValidarFormaPagoFondos(SqlConnection connection, CajasDesglocePagoRequest request, string tipoFormaPago)
        {
            if (tipoFormaPago != "F" || string.IsNullOrEmpty(request.cod_plan))
            {
                return DbHelper.OkResponse("OK");
            }

            var valFondos = connection.ExecuteScalar<string>(
                "select dbo.fxCajas_FondosDivisa(@Plan)",
                new { Plan = request.cod_plan });

            return string.IsNullOrEmpty(valFondos)
                ? DbHelper.ErrorResponse("El fondo no es valido")
                : DbHelper.OkResponse("OK");
        }

        /// <summary>Valida el monto disponible del saldo a favor.</summary>
        /// <param name="connection">Conexión abierta a la empresa.</param>
        /// <param name="request">Solicitud con la referencia y monto del saldo.</param>
        /// <param name="tipoFormaPago">Tipo de la forma de pago seleccionada.</param>
        /// <returns>Resultado de la validación del saldo a favor.</returns>
        private static ErrorDto ValidarSaldoFavor(SqlConnection connection, CajasDesglocePagoRequest request, string tipoFormaPago)
        {
            if (tipoFormaPago == "S" && request.saldo_favor_id.HasValue && request.saldo_favor_id.Value > 0)
            {
                var valSaldoFavor = connection.ExecuteScalar<string>(
                    "select dbo.fxCajas_SaldoFavorDivisa(@IdSaldo)",
                    new { IdSaldo = request.saldo_favor_id });

                if (string.IsNullOrEmpty(valSaldoFavor))
                {
                    return DbHelper.ErrorResponse("Saldo a favor no valido");
                }
            }

            return DbHelper.OkResponse("OK");
        }

        /// <summary>Determina si los identificadores del medio de pago pertenecen a la misma línea editada.</summary>
        /// <param name="connection">Conexión abierta a la empresa.</param>
        /// <param name="request">Solicitud con la referencia consultada.</param>
        /// <param name="lineaActual">Línea actual de edición.</param>
        /// <returns><see langword="true"/> cuando la referencia pertenece a la línea actual.</returns>
        private static bool EsMismaFormaPago(SqlConnection connection, CajasDesglocePagoRequest request, int? lineaActual)
        {
            if (!lineaActual.HasValue)
            {
                return false;
            }

            var total = connection.ExecuteScalar<int>(
                @"select count(1)
                  from CAJAS_DESGLOCE_PAGO
                  where Cod_Caja = @cod_caja
                    and Ticket = @ticket
                    and Cod_Apertura = @cod_apertura
                    and Linea = @linea
                    and Cod_Forma_Pago = @cod_forma_pago
                    and isnull(Num_Referencia, '') = isnull(@num_referencia, '')
                    and isnull(DP_Banco, 0) = isnull(@dp_banco, 0)
                    and isnull(Cuenta_Bancaria, '') = isnull(@cuenta_bancaria, '')
                    and isnull(Cod_Tarjeta, '') = isnull(@cod_tarjeta, '')
                    and isnull(Tarjeta_Numero, '') = isnull(@tarjeta_numero, '')
                    and isnull(Tarjeta_Autorizacion, '') = isnull(@tarjeta_autorizacion, '')
                    and isnull(Cheque_Emisor, '') = isnull(@cheque_emisor, '')
                    and isnull(Cheque_Numero, '') = isnull(@cheque_numero, '')
                    and isnull(Cod_Plan, '') = isnull(@cod_plan, '')
                    and isnull(Cod_Contrato, 0) = isnull(@cod_contrato, 0)
                    and isnull(Saldo_Favor_Id, 0) = isnull(@saldo_favor_id, 0)",
                new
                {
                    request.cod_caja,
                    request.ticket,
                    request.cod_apertura,
                    linea = lineaActual.Value,
                    request.cod_forma_pago,
                    request.num_referencia,
                    dp_banco = request.dp_banco ?? 0,
                    request.cuenta_bancaria,
                    request.cod_tarjeta,
                    request.tarjeta_numero,
                    request.tarjeta_autorizacion,
                    request.cheque_emisor,
                    request.cheque_numero,
                    request.cod_plan,
                    cod_contrato = request.cod_contrato ?? 0,
                    saldo_favor_id = request.saldo_favor_id ?? 0
                });

            return total > 0;
        }


        /// <summary>
        /// Obtiene datos de catálogos de Cajas (Divisas, Emisores, Tarjetas, Pagadores, Origen Recursos, etc.).
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="codCliente">Identificación del cliente.</param>
        /// <param name="codCaja">Código de la caja.</param>
        /// <param name="apertura">Número de apertura.</param>
        /// <param name="tiquete">Número del tiquete.</param>
        /// <param name="productoCodigo">Código del producto relacionado.</param>
        /// <param name="productoNumero">Número del producto relacionado.</param>
        /// <returns>Catálogos requeridos por la pantalla.</returns>
        public ErrorDto<CajasCatalogosDto> Cajas_Catalogos_Obtener(int CodEmpresa, string codCliente, string codCaja,
            int apertura, string? tiquete, string? productoCodigo, int? productoNumero)
        {
            var connectionString = _portalDb.ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<CajasCatalogosDto>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new CajasCatalogosDto()
            };

            try
            {
                using var connection = new SqlConnection(connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@ClienteId", codCliente);
                parameters.Add("@CodCaja", codCaja);
                parameters.Add("@Apertura", apertura);
                parameters.Add("@Tiquete", tiquete);
                parameters.Add("@ProductoCodigo", productoCodigo);
                parameters.Add("@ProductoNumero", productoNumero);

                using var multi = connection.QueryMultiple(
                    "spCajas_CatalogosCarga",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                response.Result.Divisas = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Emisores = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Tarjetas = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Pagadores = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.OrigenRecursos = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.SaldosFavor = multi.Read<CajasSaldoaFavorDto>().ToList();
                response.Result.Fondos = multi.Read<DropDownListaGenericaModel>().ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Obtiene formas de pagos
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="codCaja">Código de la caja.</param>
        /// <returns>Formas de pago habilitadas para la caja.</returns>
        public ErrorDto<List<CajasFormaPagoDto>> Cajas_FormasPago_Obtener(int CodEmpresa, string codCaja)
        {
            const string sql = @"select F.COD_FORMA_PAGO, F.DESCRIPCION, F.TIPO, F.COD_CUENTA, F.APLICA_SALDOS_FAVOR, F.OR_APLICA
                    from CAJAS_FORMAS_PAGO C
                    inner join SIF_FORMAS_PAGO F on C.COD_FORMA_PAGO = F.COD_FORMA_PAGO
                    where C.COD_CAJA = @codCaja
                    order by F.EFECTIVO desc, F.tipo asc, F.COD_FORMA_PAGO asc";

            var result = DbHelper.ExecuteListQuery<CajasFormaPagoDto>(_portalDb, CodEmpresa, sql, new { codCaja });
            if (result.Code != 0)
            {
                result.Description = $"Error al obtener formas de pago: {result.Description}";
            }

            return result;
        }

        /// <summary>
        /// Obtiene tiquete de caja
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa que contiene el tiquete.</param>
        /// <param name="enlace">Código de contabilidad usado para resolver la divisa.</param>
        /// <param name="codCaja">Código de la caja.</param>
        /// <param name="tiquete">Número del tiquete.</param>
        /// <param name="apertura">Número de apertura de caja.</param>
        /// <returns>Respuesta con los valores registrados en el tiquete.</returns>
        public ErrorDto<List<CajasTiqueteDto>> Cajas_Tiquete_Obtener(int CodEmpresa, int enlace, string codCaja, string tiquete, int apertura)
        {
            const string sql = @"select C.Linea, F.DESCRIPCION as Forma_Pago_Desc, F.TIPO, C.Monto, C.Saldo_Favor,
                           D.descripcion as Divisa, rtrim(C.cod_divisa) as cod_divisa,
                           dbo.fxCajas_TipoCambio(@enlace, C.cod_divisa, dbo.MyGetdate(), 'C') as Tipo_Cambio,
                           C.Num_Referencia, C.Cheque_Numero,
                           C.Tarjeta_Numero, C.Cod_Plan, C.Cod_Contrato
                    from CAJAS_DESGLOCE_PAGO C
                    inner join SIF_FORMAS_PAGO F on C.COD_FORMA_PAGO = F.COD_FORMA_PAGO
                    inner join CNTX_Divisas D on C.cod_Divisa = D.cod_Divisa and D.cod_Contabilidad = @enlace
                    where C.cod_caja = @codCaja and C.Ticket = @tiquete and C.Cod_Apertura = @apertura";

            var result = DbHelper.ExecuteListQuery<CajasTiqueteDto>(_portalDb, CodEmpresa, sql, new { enlace, codCaja, tiquete, apertura });
            if (result.Code != 0)
            {
                result.Description = $"Error al obtener tiquete: {result.Description}";
            }

            return result;
        }

        /// <summary>Determina si el tiquete permite generar un recibo digital.</summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="codCaja">Código de la caja.</param>
        /// <param name="apertura">Número de apertura de caja.</param>
        /// <param name="tiquete">Número del tiquete consultado.</param>
        /// <returns>Indicador de recibo digital del tiquete.</returns>
        public ErrorDto<bool> Cajas_ReciboDigital(int CodEmpresa, string codCaja, int apertura, string tiquete)
        {
            const string sql = @"
                select cast(case when dbo.fxCajas_ReciboDigital(@codCaja, @apertura, @tiquete) = 1
                    then 1 else 0 end as bit)";

            var result = DbHelper.ExecuteSingleQuery<bool>(
                _portalDb,
                CodEmpresa,
                sql,
                false,
                new { codCaja, apertura, tiquete });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result)
                : DbHelper.CreateErrorResponse<bool>($"Error al consultar recibo digital: {result.Description}");
        }


    }

}
