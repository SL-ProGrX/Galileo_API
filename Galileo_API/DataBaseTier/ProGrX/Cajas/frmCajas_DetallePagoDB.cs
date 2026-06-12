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
        /// <param name="codEmpresa"></param>
        /// <param name="codDivisa"></param>
        /// <returns></returns>
        public ErrorDto<decimal> Cajas_TipoCambio(int codEmpresa, string codDivisa)
        {
            const string sql = @"SELECT dbo.fxCajas_TipoCambio(2, @Divisa, dbo.MyGetDate(), 'C') AS TipoCambio";
            var result = DbHelper.ExecuteSingleQuery<decimal>(_portalDb, codEmpresa, sql, 0, new { Divisa = codDivisa });

            return result.Code == 0
     ? DbHelper.CreateOkResponse(result.Result)
     : DbHelper.CreateErrorResponse<decimal>($"Error en Cajas_TipoCambio: {result.Description}");
        }

        /// <summary>
        /// Elimina del desgloce de pago
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <param name="codApertura"></param>
        /// <param name="ticket"></param>
        /// <param name="linea"></param>
        /// <returns></returns>
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
        /// <param name="codEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <param name="codApertura"></param>
        /// <param name="ticket"></param>
        /// <param name="codPlan"></param>
        /// <param name="codContrato"></param>
        /// <returns></returns>
        public ErrorDto<CajasDisponibleFondosDto> Cajas_DisponibleFondos(int codEmpresa, string codCaja, int codApertura, string ticket, string codPlan, int codContrato)
        {
            var response = DbHelper.CreateOkResponse<CajasDisponibleFondosDto>(default!);

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
        /// <param name="codEmpresa"></param>
        /// <param name="clienteid"></param>
        /// <param name="referencia"></param>
        /// <param name="referencia_texto"></param>
        /// <returns></returns>
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
        /// <param name="codEmpresa"></param>
        /// <param name="enlace"></param>
        /// <returns></returns>
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
        /// <param name="codEmpresa"></param>
        /// <param name="formaPago"></param>
        /// <returns></returns>
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
        /// <param name="codEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <param name="ticket"></param>
        /// <param name="codApertura"></param>
        /// <param name="linea"></param>
        /// <returns></returns>
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
        /// <param name="codEmpresa"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
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

                // 1?? Obtener la l�nea siguiente
                var sqlLinea = @"SELECT ISNULL(MAX(linea), 0) + 1 
                         FROM CAJAS_DESGLOCE_PAGO 
                         WHERE Cod_Caja = @cod_caja AND Ticket = @ticket AND Cod_Apertura = @cod_apertura";

                dto.linea = connection.ExecuteScalar<int>(sqlLinea, new { dto.cod_caja, dto.ticket, dto.cod_apertura });

                // 2?? Insertar el registro
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
        /// <param name="codEmpresa"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto Cajas_DesglocePago_Update(int codEmpresa, CajasDesglocePagoDto dto)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                var validacion = ValidarDesglocePago(connection, ToGuardarRequest(dto), dto.linea);
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
        /// <param name="codEmpresa"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto Cajas_DistribuyeSaldoFavor(int codEmpresa, DistribuyeSaldoFavorDto dto)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok",
            };

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                string sql = @"exec spCajas_DistribuyeSaldoFavor @CodCaja, @CodApertura, @Ticket, @Usuario, @TotalAplicar, @Divisa";

                connection.Execute(sql, new
                {
                    CodCaja = dto.cod_caja,
                    CodApertura = dto.cod_apertura,
                    Ticket = dto.ticket,
                    Usuario = dto.usuario,
                    TotalAplicar = dto.total_aplicar,
                    Divisa = dto.divisa
                });

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Guarda desgloce de pago
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Cajas_DesglocePago_Guardar(int CodEmpresa, CajasDesglocePagoRequest request)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                connection.Open();

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
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al guardar desgloce de pago: {ex.Message}");
            }
        }

        private static CajasDesglocePagoRequest ToGuardarRequest(CajasDesglocePagoDto dto)
        {
            return new CajasDesglocePagoRequest
            {
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

            var validacionReferencia = ValidarReferenciaDocumento(connection, request, lineaActual);
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
                    Cedula = request.usuario,
                    Monto = request.monto
                });

            return valDeposito == 0
                ? DbHelper.ErrorResponse("Deposito no registrado en Tesoreria")
                : DbHelper.OkResponse("OK");
        }

        private static ErrorDto ValidarReferenciaDocumento(SqlConnection connection, CajasDesglocePagoRequest request, int? lineaActual)
        {
            if (string.IsNullOrWhiteSpace(request.num_referencia) || EsMismaReferencia(connection, request, lineaActual))
            {
                return DbHelper.OkResponse("OK");
            }

            var valDoc = connection.ExecuteScalar<int>(
                "select dbo.fxCajas_DocumentoVerifica(@FormaPago,@Documento,@Banco,@Cuenta)",
                new
                {
                    FormaPago = request.cod_forma_pago,
                    Documento = request.num_referencia,
                    Banco = request.dp_banco,
                    Cuenta = request.cuenta_bancaria
                });

            if (valDoc > 0)
            {
                return DbHelper.ErrorResponse("Documento ya existe (duplicado)");
            }

            var valRegistrado = connection.ExecuteScalar<int>(
                "select dbo.fxCajas_FP_Registada(@Ticket,@FormaPago,@Doc,@Banco,@Cuenta)",
                new
                {
                    Ticket = request.ticket,
                    FormaPago = request.cod_forma_pago,
                    Doc = request.num_referencia,
                    Banco = request.dp_banco,
                    Cuenta = request.cuenta_bancaria
                });

            return valRegistrado > 0
                ? DbHelper.ErrorResponse("Este movimiento ya fue registrado")
                : DbHelper.OkResponse("OK");
        }

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

        private static bool EsMismaReferencia(SqlConnection connection, CajasDesglocePagoRequest request, int? lineaActual)
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
                    and isnull(Cuenta_Bancaria, '') = isnull(@cuenta_bancaria, '')",
                new
                {
                    request.cod_caja,
                    request.ticket,
                    request.cod_apertura,
                    linea = lineaActual.Value,
                    request.cod_forma_pago,
                    request.num_referencia,
                    dp_banco = request.dp_banco ?? 0,
                    request.cuenta_bancaria
                });

            return total > 0;
        }


        /// <summary>
        /// Obtiene datos de cat�logos de Cajas (Divisas, Emisores, Tarjetas, Pagadores, Origen Recursos, etc.)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codCliente"></param>
        /// <param name="codCaja"></param>
        /// <param name="apertura"></param>
        /// <param name="tiquete"></param>
        /// <param name="productoCodigo"></param>
        /// <param name="productoNumero"></param>
        /// <returns></returns>
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
        /// <param name="CodEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <returns></returns>
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
        /// <param name="CodEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <param name="tiquete"></param>
        /// <param name="apertura"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasTiqueteDto>> Cajas_Tiquete_Obtener(int CodEmpresa, string codCaja, string tiquete, int apertura)
        {
            const string sql = @"select C.Linea, F.DESCRIPCION as Forma_Pago_Desc, F.TIPO, C.Monto, C.Saldo_Favor, 
                           D.descripcion as Divisa, C.Tipo_Cambio, C.Num_Referencia, C.Cheque_Numero, 
                           C.Tarjeta_Numero, C.Cod_Plan, C.Cod_Contrato
                    from CAJAS_DESGLOCE_PAGO C
                    inner join SIF_FORMAS_PAGO F on C.COD_FORMA_PAGO = F.COD_FORMA_PAGO
                    inner join CNTX_Divisas D on C.cod_Divisa = D.cod_Divisa and D.cod_Contabilidad = 2
                    where C.cod_caja = @codCaja and C.Ticket = @tiquete and C.Cod_Apertura = @apertura";

            var result = DbHelper.ExecuteListQuery<CajasTiqueteDto>(_portalDb, CodEmpresa, sql, new { codCaja, tiquete, apertura, CodEmpresa });
            if (result.Code != 0)
            {
                result.Description = $"Error al obtener tiquete: {result.Description}";
            }

            return result;
        }


    }

}
