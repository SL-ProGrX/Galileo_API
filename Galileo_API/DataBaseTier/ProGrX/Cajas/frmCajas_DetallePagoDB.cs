using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Cajas;
using System.Data;
using static PgxAPI.Models.ProGrX.Cajas.CajasDesglocePagoRequest;

namespace PgxAPI.DataBaseTier
{
    public class frmCajas_DetallePagoDB
    {
        private readonly IConfiguration _config;

        public frmCajas_DetallePagoDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene el tipo de cambio
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codDivisa"></param>
        /// <returns></returns>
        public ErrorDto<decimal> Cajas_TipoCambio(int codEmpresa, string codDivisa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<decimal>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"SELECT dbo.fxCajas_TipoCambio(2, @Divisa, dbo.MyGetDate(), 'C') AS TipoCambio";

                    var result = connection.QueryFirstOrDefault<decimal>(
                        query,
                        new
                        {
                            CodEnlace = codEmpresa,
                            Divisa = codDivisa
                        }
                    );

                    response.Result = result;
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error en Cajas_TipoCambio: {ex.Message}";
            }

            return response;
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
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok",
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                using var connection = new SqlConnection(stringConn);
                string sql = @"
                            DELETE FROM CAJAS_DESGLOCE_PAGO
                            WHERE Cod_Caja = @CodCaja
                              AND Cod_Apertura = @CodApertura
                              AND Ticket = @Ticket
                              AND Linea = @Linea";

                connection.Execute(sql, new { CodCaja = codCaja, CodApertura = codApertura, Ticket = ticket, Linea = linea });
            }

            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
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
            var response = new ErrorDto<CajasDisponibleFondosDto>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                using var connection = new SqlConnection(stringConn);
                string sql = "exec spCajas_DisponibleFondos @CodCaja, @CodApertura, @Ticket, @CodPlan, @CodContrato";

                var result = connection.QueryFirstOrDefault<CajasDisponibleFondosDto>(sql,
                    new { CodCaja = codCaja, CodApertura = codApertura, Ticket = ticket, CodPlan = codPlan, CodContrato = codContrato });

                response.Result = result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
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
        public ErrorDto<List<CajasSaldoFavorDto>> Cajas_SaldoFavor_Obtener(int codEmpresa, string clienteid, int referencia, string referencia_texto)
        {
            var response = new ErrorDto<List<CajasSaldoFavorDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CajasSaldoFavorDto>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                using var connection = new SqlConnection(stringConn);
                string sql = @"exec spCajas_SaldoFavor @clienteid, @referencia, @referenciatexto";

                var result = connection.Query<CajasSaldoFavorDto>(
                    sql,
                    new { clienteid, referencia, referenciatexto = referencia_texto }
                );

                response.Result = result.ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "Error en Cajas_SaldoFavor_Obtener: " + ex.Message;
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
            var response = new ErrorDto<CajasDivisaFuncionalDto>
            {
                Code = 0,
                Description = "Ok",
                Result = new CajasDivisaFuncionalDto()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                using var connection = new SqlConnection(stringConn);
                string sql = @"select dbo.fxCajas_DivisaFuncional(@enlace) as Divisa";

                var result = connection.QueryFirstOrDefault<CajasDivisaFuncionalDto>(
                    sql,
                    new { enlace }
                );

                if (result != null)
                    response.Result = result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "Error en Cajas_DivisaFuncional_Obtener: " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene los depositos bancarios
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="formaPago"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasDepositosCuentasBancariasDto>> Cajas_DepositosCuentasBancariasAut_Obtener(int codEmpresa, string formaPago)
        {
            var response = new ErrorDto<List<CajasDepositosCuentasBancariasDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CajasDepositosCuentasBancariasDto>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                using var connection = new SqlConnection(stringConn);
                string sql = @"exec spCajas_DepositosCuentasBancariasAut @formaPago";


                var result = connection.Query<CajasDepositosCuentasBancariasDto>(
                    sql,
                    new { formaPago }
                );


                foreach (var item in result)
                {
                    item.itmx = $"{item.cta} - {item.itmx}";
                }

                response.Result = result.ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "Error en Cajas_DepositosCuentasBancariasAut_Obtener: " + ex.Message;
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
            var response = new ErrorDto<List<CajasDesglocePagoDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CajasDesglocePagoDto>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                using var connection = new SqlConnection(stringConn);
                string sql = @"
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

                var result = connection.Query<CajasDesglocePagoDto>(
                    sql,
                    new { CodCaja = codCaja, Ticket = ticket, CodApertura = codApertura, Linea = linea }
                );

                response.Result = result.ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "Error en Cajas_DesglocePago_Obtener: " + ex.Message;
            }

            return response;
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
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                using var connection = new SqlConnection(stringConn);

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
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok",
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                using var connection = new SqlConnection(stringConn);
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
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
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
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

                using var connection = new SqlConnection(stringConn);
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
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK",
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

                using (var connection = new SqlConnection(stringConn))
                {
                    connection.Open();

                    // 1. VALIDACIONES

                    // Validar dep�sito
                    var valDeposito = connection.ExecuteScalar<int>(
                        "select dbo.fxTes_DP_Cargado(@Banco,@Documento,@Cedula,@Monto)",
                        new
                        {
                            Banco = request.dp_banco,
                            Documento = request.num_referencia,
                            Cedula = request.usuario,
                            Monto = request.monto
                        }
                    );
                    if (valDeposito == 0)
                    {
                        response.Code = -1;
                        response.Description = "Dep�sito no registrado en Tesorer�a";
                        return response;
                    }

                    // Validar documento duplicado
                    var valDoc = connection.ExecuteScalar<int>(
                        "select dbo.fxCajas_DocumentoVerifica(@FormaPago,@Documento,@Banco,@Cuenta)",
                        new
                        {
                            FormaPago = request.cod_forma_pago,
                            Documento = request.num_referencia,
                            Banco = request.dp_banco,
                            Cuenta = request.cuenta_bancaria
                        }
                    );
                    if (valDoc > 0)
                    {
                        response.Code = -1;
                        response.Description = "Documento ya existe (duplicado)";
                        return response;
                    }

                    // Validar si ya fue registrado
                    var valRegistrado = connection.ExecuteScalar<int>(
                        "select dbo.fxCajas_FP_Registada(@Ticket,@FormaPago,@Doc,@Banco,@Cuenta)",
                        new
                        {
                            Ticket = request.ticket,
                            FormaPago = request.cod_forma_pago,
                            Doc = request.num_referencia,
                            Banco = request.dp_banco,
                            Cuenta = request.cuenta_bancaria
                        }
                    );
                    if (valRegistrado > 0)
                    {
                        response.Code = -1;
                        response.Description = "Este movimiento ya fue registrado";
                        return response;
                    }

                    // Validar fondos
                    if (!string.IsNullOrEmpty(request.cod_plan))
                    {
                        var valFondos = connection.ExecuteScalar<string>(
                            "select dbo.fxCajas_FondosDivisa(@Plan)",
                            new { Plan = request.cod_plan }
                        );
                        if (string.IsNullOrEmpty(valFondos))
                        {
                            response.Code = -1;
                            response.Description = "El fondo no es v�lido";
                            return response;
                        }
                    }

                    // Validar saldo a favor
                    if (request.saldo_favor_id.HasValue)
                    {
                        var valSaldoFavor = connection.ExecuteScalar<string>(
                            "select dbo.fxCajas_SaldoFavorDivisa(@IdSaldo)",
                            new { IdSaldo = request.saldo_favor_id }
                        );
                        if (string.IsNullOrEmpty(valSaldoFavor))
                        {
                            response.Code = -1;
                            response.Description = "Saldo a favor no v�lido";
                            return response;
                        }
                    }

                    // 2. GUARDAR
                    var sql = @"
                insert into CAJAS_DESGLOCE_PAGO
                (ticket,cod_caja,cod_apertura,monto,cod_divisa,tipo_cambio,registro_fecha,registro_usuario,
                 cod_tarjeta,tarjeta_numero,tarjeta_autorizacion,cheque_emisor,cheque_numero,
                 cuenta_bancaria,num_referencia,cod_cuenta,cod_forma_pago,dp_banco,dp_fecha,cod_plan,cod_contrato)
                values
                (@ticket,@cod_caja,@cod_apertura,@monto,@cod_divisa,@tipo_cambio,GETDATE(),@usuario,
                 @cod_tarjeta,@tarjeta_numero,@tarjeta_autorizacion,@cheque_emisor,@cheque_numero,
                 @cuenta_bancaria,@num_referencia,@cod_cuenta,@cod_forma_pago,@dp_banco,@dp_fecha,@cod_plan,@cod_contrato)";

                    connection.Execute(sql, request);

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al guardar desgloce de pago: {ex.Message}";
            }

            return response;
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<CajasCatalogosDto>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new CajasCatalogosDto()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

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
            var response = new ErrorDto<List<CajasFormaPagoDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CajasFormaPagoDto>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

                using var connection = new SqlConnection(stringConn);
                var sql = @"select F.COD_FORMA_PAGO, F.DESCRIPCION, F.TIPO, F.COD_CUENTA, F.APLICA_SALDOS_FAVOR, F.OR_APLICA
                    from CAJAS_FORMAS_PAGO C
                    inner join SIF_FORMAS_PAGO F on C.COD_FORMA_PAGO = F.COD_FORMA_PAGO
                    where C.COD_CAJA = @codCaja
                    order by F.EFECTIVO desc, F.tipo asc, F.COD_FORMA_PAGO asc";

                response.Result = connection.Query<CajasFormaPagoDto>(sql, new { codCaja }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al obtener formas de pago: {ex.Message}";
            }

            return response;
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
            var response = new ErrorDto<List<CajasTiqueteDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CajasTiqueteDto>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

                using var connection = new SqlConnection(stringConn);
                var sql = @"select C.Linea, F.DESCRIPCION as Forma_Pago_Desc, F.TIPO, C.Monto, C.Saldo_Favor, 
                           D.descripcion as Divisa, C.Tipo_Cambio, C.Num_Referencia, C.Cheque_Numero, 
                           C.Tarjeta_Numero, C.Cod_Plan, C.Cod_Contrato
                    from CAJAS_DESGLOCE_PAGO C
                    inner join SIF_FORMAS_PAGO F on C.COD_FORMA_PAGO = F.COD_FORMA_PAGO
                    inner join CNTX_Divisas D on C.cod_Divisa = D.cod_Divisa and D.cod_Contabilidad = 2
                    where C.cod_caja = @codCaja and C.Ticket = @tiquete and C.Cod_Apertura = @apertura";

                response.Result = connection.Query<CajasTiqueteDto>(sql, new { codCaja, tiquete, apertura, CodEmpresa }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al obtener tiquete: {ex.Message}";
            }

            return response;
        }


    }

}