using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public partial class FrmAfLiquidacionwDb
    {

        /// <summary>
        /// Inserta un nuevo registro de renuncia SIF en la base de datos, utilizando la información proporcionada en el modelo AfRenunciaSifModel. Este método es esencial para registrar las renuncias SIF de los socios durante el proceso de liquidación, permitiendo una gestión adecuada de estas renuncias y facilitando la toma de decisiones informadas sobre las acciones a tomar en relación con cada socio. La inserción de la renuncia SIF es un paso importante en el proceso de liquidación, ya que puede afectar el monto a liquidar y las acciones relacionadas con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ErrorDto<int> AF_Liquidacion_RenunciaSIF_Insertar(int CodEmpresa, AfRenunciaSifModel model)
        {
            var result = new ErrorDto<int> { Code = 0, Description = "Ok", Result = 0 };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

                using var connection = new SqlConnection(stringConn);

                string query = @"
                    INSERT INTO Renuncias (ID_Causa, Cedula, fecha, tipo)
                    VALUES (@IdCausa, @Cedula, @FechaSistema, @TipoRenFlag)";

                result.Result = connection.Execute(query, model);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }


        /// <summary>
        /// Inserta un nuevo registro de renuncia ASE en la base de datos, utilizando la información proporcionada en el modelo AfRenunciaAseModel. Este método es esencial para registrar las renuncias ASE de los socios durante el proceso de liquidación, permitiendo una gestión adecuada de estas renuncias y facilitando la toma de decisiones informadas sobre las acciones a tomar en relación con cada socio. La inserción de la renuncia ASE es un paso importante en el proceso de liquidación, ya que puede afectar el monto a liquidar y las acciones relacionadas con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ErrorDto<int> AF_Liquidacion_RenunciaASE_Insertar(int CodEmpresa, AfRenunciaAseModel model)
        {
            var result = new ErrorDto<int> { Code = 0, Description = "Ok", Result = 0 };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

                using var connection = new SqlConnection(stringConn);

                string query = @"
                    INSERT INTO Renuncias (
                        ID_Causa, ID_Promotor, Cedula, Id_Boleta,
                        FechaRenA, FechaRenP, TipoRen, Nacta, NCausaRen, RenMor
                    )
                    VALUES (
                        @IdCausa, @IdPromotor, @Cedula, @IdBoletaAf,
                        CASE WHEN @TipoRenFlag='A' THEN @FechaSistema ELSE NULL END,
                        CASE WHEN @TipoRenFlag='P' THEN @FechaSistema ELSE NULL END,
                        @TipoRenFlag,
                        ISNULL(@Nacta, 0),
                        0,
                        @Mortalidad
                    )";

                result.Result = connection.Execute(query, model);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }


        /// <summary>
        /// Actualiza el estado de las renuncias a 'V' (Vencida) para aquellas renuncias cuyo ID de causa coincide con el proporcionado y cuya fecha de vencimiento es menor a la fecha actual. Este método es fundamental para mantener actualizada la información de las renuncias en el sistema, asegurando que las renuncias que han vencido se marquen adecuadamente como vencidas, lo que puede afectar el proceso de liquidación y las acciones relacionadas con cada socio. La actualización del estado de las renuncias es un paso importante para garantizar la precisión de la información y para facilitar la toma de decisiones informadas durante el proceso de liquidación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ErrorDto<int> AF_Liquidacion_Insertar(int CodEmpresa, AfLiquidacionInsertModel model)
        {
            var result = new ErrorDto<int> { Code = 0, Description = "Ok", Result = 0 };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

                using var connection = new SqlConnection(stringConn);

                string query = @"
                    INSERT INTO liquidacion (
                        cedula, ahorro, aporte, custodia, capitaliza, extra, montofci, retenido,
                        fechaingreso, fecliq, estadoActLiq, estadoactual,
                        aplAhorro, aplAporte, aplCapitalizado, aplExtra,
                        TotalBruto, TNeto,
                        Ahorro_Liq, Aporte_liq, custodia_liq, Capitalizado_liq, extra_liq,
                        tdocumento, cod_banco, CTA_AHORROS, ubicacion, liq_tcon, estadoAsiento, estado, mortalidad,
                        id_causa, observacion, usuario, cod_oficina, ac_boleta, ac_fecha, fecha_pago,
                        EXCEDENTE_PERIODO, EXCEDENTE_IR, EXCEDENTE_LIQ, EXCEDENTE_IR_LIQ, APL_EXCEDENTE,
                        COD_DIVISA, TIPO_CAMBIO
                    )
                    VALUES (
                        @Cedula,
                        @Ahorro,
                        @Aporte,
                        @Custodia,
                        @Capitaliza,
                        @Extra,
                        @FCI,
                        @Retenido,
                        @FechaIngreso,
                        dbo.MyGetdate(),
                        @TipoRenFlag,
                        @EstadoActual,
                        @AplAhorro,
                        @AplAporte,
                        @AplCapitalizado,
                        @AplExtra,
                        @TotalBrutoUI,
                        @MontoAGirar,
                        @Ahorro_Liq,
                        @Aporte_Liq,
                        @Custodia_Liq,
                        @Capitaliza_Liq,
                        @Extra_Liq,
                        @TipoDoc,
                        @CodBanco,
                        @CuentaAhorros,
                        CASE WHEN @MontoAGirar > 0 THEN 'T' ELSE 'C' END,
                        5,
                        CASE WHEN @Mortalidad=1 THEN 'G' ELSE 'P' END,
                        'P',
                        @Mortalidad,
                        @IdCausa,
                        LEFT(@Observacion,250),
                        @Usuario,
                        @CodOficina,
                        @AcBoleta,
                        @AcFecha,
                        @FechaPago,
                        @EXC_PERIODO,
                        @EXC_IR,
                        @EXC_LIQ,
                        @EXC_IR_LIQ,
                        @APL_EXCEDENTE,
                        @COD_DIVISA,
                        @TIPO_CAMBIO
                    )";

                result.Result = connection.Execute(query, model);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }


        /// <summary>
        /// Actualiza el estado de las renuncias a 'V' (Vencida) para aquellas renuncias cuyo ID de causa coincide con el proporcionado y cuya fecha de vencimiento es menor a la fecha actual. Este método es fundamental para mantener actualizada la información de las renuncias en el sistema, asegurando que las renuncias que han vencido se marquen adecuadamente como vencidas, lo que puede afectar el proceso de liquidación y las acciones relacionadas con cada socio. La actualización del estado de las renuncias es un paso importante para garantizar la precisión de la información y para facilitar la toma de decisiones informadas durante el proceso de liquidación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public ErrorDto<bool> AF_Liquidacion_Patrimonio_Aplicar(int CodEmpresa, AfLiquidacionPatrimonioInput input)
        {
            if (input is null)
            {
                return DbHelper.CreateErrorResponse("Los datos de patrimonio son requeridos.", -2, false);
            }

            return EjecutarStoredProcedureBool(
                CodEmpresa,
                SpLiquidacionPatrimonio,
                new
                {
                    Liq = input.LiqConsec,
                    Usuario = NormalizarTexto(input.Usuario)
                },
                "Error al aplicar patrimonio.");
        }


        /// <summary>
        /// Inserta un nuevo registro de liquidación de fondos en la base de datos, utilizando la información proporcionada en el modelo AfLiquidaFondosInsertModel. Este método es esencial para registrar las liquidaciones de fondos de los socios durante el proceso de liquidación, permitiendo una gestión adecuada de estas liquidaciones y facilitando la toma de decisiones informadas sobre las acciones a tomar en relación con cada socio. La inserción de la liquidación de fondos es un paso importante en el proceso de liquidación, ya que puede afectar el monto a liquidar y las acciones relacionadas con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ErrorDto<int> AF_LiquidaFondos_Insertar(int CodEmpresa, AfLiquidaFondosInsertModel model)
        {
            var result = new ErrorDto<int> { Code = 0, Description = "Ok", Result = 0 };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

                using var connection = new SqlConnection(stringConn);

                string query = @"
                    INSERT INTO LIQUIDA_FONDOS (
                        CONSEC, COD_CONTRATO, COD_OPERADORA, COD_PLAN, DISPONIBLE, MULTA, REND_PENDIENTE, LIQ_FND,
                        APORTES, RENDIMIENTOS, COD_DIVISA, TIPO_CAMBIO
                    )
                    VALUES (
                        @LiqConsec,
                        @CodContrato,
                        @CodOperadora,
                        @CodPlan,
                        @Disponible,
                        @Multa,
                        @RendPendiente,
                        0,
                        @Aportes,
                        @Rendimientos,
                        @CodDivisa,
                        @TipoCambio
                    )";

                result.Result = connection.Execute(query, model);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }


        /// <summary>
        /// Ejecuta el proceso de liquidación de planes para una liquidación específica, utilizando la información proporcionada en el modelo AfLiquidaPlanesInput. Este método es esencial para llevar a cabo la liquidación de planes de los socios durante el proceso de liquidación, permitiendo una gestión adecuada de estas liquidaciones y facilitando la toma de decisiones informadas sobre las acciones a tomar en relación con cada socio. La ejecución de la liquidación de planes es un paso importante en el proceso de liquidación, ya que puede afectar el monto a liquidar y las acciones relacionadas con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public ErrorDto<bool> AF_LiquidaPlanes_Ejecutar(int CodEmpresa, AfLiquidaPlanesInput input)
        {
            if (input is null)
            {
                return DbHelper.CreateErrorResponse("Los datos de liquidación de planes son requeridos.", -2, false);
            }

            return EjecutarStoredProcedureBool(
                CodEmpresa,
                SpLiquidaPlanes,
                new
                {
                    NumLiq = input.LiqConsec,
                    Usuario = NormalizarTexto(input.Usuario),
                    Oficina = input.OficinaTitular
                },
                "Error al ejecutar liquidación de planes.");
        }


        /// <summary>
        /// Inserta un nuevo registro de detalle de liquidación en la base de datos, utilizando la información proporcionada en el modelo AfLiquidaDetalleInsertModel. Este método es esencial para registrar los detalles de las liquidaciones de los socios durante el proceso de liquidación, permitiendo una gestión adecuada de estos detalles y facilitando la toma de decisiones informadas sobre las acciones a tomar en relación con cada socio. La inserción del detalle de liquidación es un paso importante en el proceso de liquidación, ya que puede afectar el monto a liquidar y las acciones relacionadas con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ErrorDto<int> AF_LiquidaDetalle_Insertar(int CodEmpresa, AfLiquidaDetalleInsertModel model)
        {
            var result = new ErrorDto<int> { Code = 0, Description = "Ok", Result = 0 };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"
                    INSERT INTO liquida_detalle (
                        CONSEC, ID_SOLICITUD, CODIGO, LIQ_ABONO, LIQ_FECHA,
                        LIQ_SALDO, LIQ_INTCOR, LIQ_INTMOR, LIQ_AMORTIZA,
                        COD_DIVISA, TIPO_CAMBIO
                    )
                    VALUES (
                        @LiqConsec, @IdSolicitud, @Codigo, @AbonoFila, dbo.MyGetDate(),
                        @SaldoFila, 0, 0, 0,
                        @CodDivisa, @TipoCambio
                    )";
                result.Result = connection.Execute(query, model);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Ejecuta el proceso de abonos de plan de pagos para una liquidación específica, utilizando la información proporcionada en el modelo AfLiquidacionPatrimonioInput. Este método es esencial para llevar a cabo los abonos de plan de pagos de los socios durante el proceso de liquidación, permitiendo una gestión adecuada de estos abonos y facilitando la toma de decisiones informadas sobre las acciones a tomar en relación con cada socio. La ejecución de los abonos de plan de pagos es un paso importante en el proceso de liquidación, ya que puede afectar el monto a liquidar y las acciones relacionadas con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public ErrorDto<bool> AF_Liquidacion_AbonosPlanPagos_Ejecutar(int CodEmpresa, AfLiquidacionPatrimonioInput input)
        {
            if (input is null)
            {
                return DbHelper.CreateErrorResponse("Los datos de abono son requeridos.", -2, false);
            }

            return EjecutarStoredProcedureBool(
                CodEmpresa,
                SpAbonosPlanPagos,
                new
                {
                    Liquidacion = input.LiqConsec,
                    Usuario = NormalizarTexto(input.Usuario)
                },
                "Error al ejecutar abonos de plan de pagos.");
        }


        /// <summary>
        /// Actualiza la información de morosidad para una solicitud específica, utilizando la información proporcionada en el modelo AfMorosidadModel. Este método es esencial para mantener actualizada la información de morosidad en el sistema durante el proceso de liquidación, permitiendo una gestión adecuada de esta morosidad y facilitando la toma de decisiones informadas sobre las acciones a tomar en relación con cada socio. La actualización de la morosidad es un paso importante en el proceso de liquidación, ya que puede afectar el monto a liquidar y las acciones relacionadas con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ErrorDto<int> AF_Morosidad_Actualizar(int CodEmpresa, AfMorosidadModel model)
        {
            var result = new ErrorDto<int> { Code = 0, Description = "Ok", Result = 0 };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"
                    UPDATE morosidad
                    SET abintc = intc,
                        abintm = intm,
                        abamortiza = amortiza,
                        estado = 'C',
                        Tcon = 'LIQ',
                        Ncon = @LiqConsec,
                        fecult = dbo.MyGetdate(),
                        cod_concepto = 'CRD001',
                        cod_caja = '',
                        usuario = @Usuario
                    WHERE estado = 'A' AND id_solicitud = @IdSolicitud";
                result.Result = connection.Execute(query, model);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Actualiza la información de morosidad para una solicitud específica, utilizando la información proporcionada en el modelo AfMorosidadPorMoraModel. Este método es esencial para mantener actualizada la información de morosidad en el sistema durante el proceso de liquidación, permitiendo una gestión adecuada de esta morosidad y facilitando la toma de decisiones informadas sobre las acciones a tomar en relación con cada socio. La actualización de la morosidad es un paso importante en el proceso de liquidación, ya que puede afectar el monto a liquidar y las acciones relacionadas con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ErrorDto<int> AF_Morosidad_Actualizar_Mora(int CodEmpresa, AfMorosidadPorMoraModel model)
        {
            var result = new ErrorDto<int> { Code = 0, Description = "Ok", Result = 0 };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"
                    UPDATE morosidad
                    SET abintc      = @AbIntC,
                        abintm      = @AbIntM,
                        abamortiza  = @AbAmortiza,
                        estado      = 'C',
                        Tcon        = 'LIQ',
                        Ncon        = @LiqConsec,
                        fecult      = dbo.MyGetdate(),
                        usuario     = @Usuario,
                        Cod_Caja    = '',
                        cod_Concepto= 'CRD001'
                    WHERE id_moro   = @id_moro";
                result.Result = connection.Execute(query, model);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Inserta un nuevo registro de morosidad para una solicitud específica, utilizando la información proporcionada en el modelo AfMorosidadInsertModel. Este método es esencial para registrar la morosidad de los socios durante el proceso de liquidación, permitiendo una gestión adecuada de esta morosidad y facilitando la toma de decisiones informadas sobre las acciones a tomar en relación con cada socio. La inserción de la morosidad es un paso importante en el proceso de liquidación, ya que puede afectar el monto a liquidar y las acciones relacionadas con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ErrorDto<int> AF_Morosidad_Insertar(int CodEmpresa, AfMorosidadInsertModel model)
        {
            var result = new ErrorDto<int> { Code = 0, Description = "Ok", Result = 0 };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"
                    INSERT INTO morosidad(
                        id_solicitud, codigo, estado, estadoi,
                        intc, intm, amortiza,
                        abintc, abintm, abamortiza,
                        tcon, ncon,
                        fechap, fecap, fecult, cuota_morosa,
                        usuario, cod_concepto, cod_caja
                    ) VALUES (
                        @IdSolicitud, @Codigo, 'A', 'A',
                        (@IntC - @AbIntC), (@IntM - @AbIntM), (@Amortiza - @AbAmortiza),
                        0, 0, 0,
                        'LIQ', @LiqConsec,
                        @Fechap, @Fecap, dbo.MyGetdate(),
                        (@IntC + @IntM + @Amortiza) - (@AbIntC + @AbIntM + @AbAmortiza),
                        @Usuario, 'CRD001', ''
                    )";
                result.Result = connection.Execute(query, model);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Actualiza la información de cartera para una solicitud específica, utilizando la información proporcionada en el modelo AfRegCreditosActualizarModel. Este método es esencial para mantener actualizada la información de cartera en el sistema durante el proceso de liquidación, permitiendo una gestión adecuada de esta cartera y facilitando la toma de decisiones informadas sobre las acciones a tomar en relación con cada socio. La actualización de la cartera es un paso importante en el proceso de liquidación, ya que puede afectar el monto a liquidar y las acciones relacionadas con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ErrorDto<int> AF_RegCreditos_Actualizar_Cartera(int CodEmpresa, AfRegCreditosActualizarModel model)
        {
            var result = new ErrorDto<int> { Code = 0, Description = "Ok", Result = 0 };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"
                    UPDATE reg_creditos
                    SET saldo     = saldo - @CurPrin,
                        amortiza  = amortiza + @CurPrin,
                        saldo_mes = saldo_mes - @CurPrin,
                        interesc  = interesc + (@CurIntC + @CurIntM),
                        estado    = CASE WHEN (saldo - @CurPrin) <= 0 THEN 'C' ELSE 'A' END
                    WHERE id_solicitud = @IdSolicitud";
                result.Result = connection.Execute(query, model);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Actualiza la información de retención a plazo para una solicitud específica, utilizando la información proporcionada en el modelo AfRegCreditosActualizarModel. Este método es esencial para mantener actualizada la información de retención a plazo en el sistema durante el proceso de liquidación, permitiendo una gestión adecuada de esta retención y facilitando la toma de decisiones informadas sobre las acciones a tomar en relación con cada socio. La actualización de la retención a plazo es un paso importante en el proceso de liquidación, ya que puede afectar el monto a liquidar y las acciones relacionadas con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ErrorDto<int> AF_RegCreditos_Actualizar_RetenPlazo(int CodEmpresa, AfRegCreditosActualizarModel model)
        {
            var result = new ErrorDto<int> { Code = 0, Description = "Ok", Result = 0 };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"
                    UPDATE reg_creditos
                    SET amortiza  = amortiza + @CurPrin,
                        interesc  = interesc + (@CurIntC + @CurIntM),
                        estado    = CASE WHEN (saldo - @CurPrin) <= 0 THEN 'C' ELSE 'A' END
                    WHERE id_solicitud = @IdSolicitud";
                result.Result = connection.Execute(query, model);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Actualiza la información de retención indefinida para una solicitud específica, utilizando la información proporcionada en el modelo AfRegCreditosActualizarModel. Este método es esencial para mantener actualizada la información de retención indefinida en el sistema durante el proceso de liquidación, permitiendo una gestión adecuada de esta retención y facilitando la toma de decisiones informadas sobre las acciones a tomar en relación con cada socio. La actualización de la retención indefinida es un paso importante en el proceso de liquidación, ya que puede afectar el monto a liquidar y las acciones relacionadas con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ErrorDto<int> AF_RegCreditos_Actualizar_RetenIndefinida(int CodEmpresa, AfRegCreditosActualizarModel model)
        {
            var result = new ErrorDto<int> { Code = 0, Description = "Ok", Result = 0 };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"
                    UPDATE reg_creditos
                    SET amortiza  = amortiza + @CurPrin,
                        interesc  = interesc + (@CurIntC + @CurIntM)
                    WHERE id_solicitud = @IdSolicitud";
                result.Result = connection.Execute(query, model);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Inserta un nuevo registro de detalle de crédito en la base de datos, utilizando la información proporcionada en el modelo AfCreditosDtInsertModel. Este método es esencial para registrar los detalles de los créditos de los socios durante el proceso de liquidación, permitiendo una gestión adecuada de estos detalles y facilitando la toma de decisiones informadas sobre las acciones a tomar en relación con cada socio. La inserción del detalle de crédito es un paso importante en el proceso de liquidación, ya que puede afectar el monto a liquidar y las acciones relacionadas con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ErrorDto<int> AF_CreditosDt_Insertar(int CodEmpresa, AfCreditosDtInsertModel model)
        {
            var result = new ErrorDto<int> { Code = 0, Description = "Ok", Result = 0 };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"
                    INSERT INTO creditos_dt(
                        id_solicitud, codigo, cuota, abono, intcp, amortiza,
                        fechap, fechas, tcon, ncon, estado, usuario, cod_concepto, cod_caja
                    ) VALUES (
                        @IdSolicitud, @Codigo, 0, @curAbono, 0, @curAbono,
                        @FechaCR, dbo.MyGetdate(), 'LIQ', @LiqConsec,
                        'A', @Usuario, 'CRD002', ''
                    )";
                result.Result = connection.Execute(query, model);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Ejecuta el proceso de asiento de liquidación para una liquidación específica, utilizando la información proporcionada en el modelo AfLiquidacionAsientoInput. Este método es esencial para llevar a cabo el asiento de liquidación de los socios durante el proceso de liquidación, permitiendo una gestión adecuada de este asiento y facilitando la toma de decisiones informadas sobre las acciones a tomar en relación con cada socio. La ejecución del asiento de liquidación es un paso importante en el proceso de liquidación, ya que puede afectar el monto a liquidar y las acciones relacionadas con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="LiqConsec"></param>
        /// <returns></returns>
        public ErrorDto<bool> AF_Liquidacion_Asiento_Ejecutar(int CodEmpresa, int LiqConsec)
        {
            return EjecutarStoredProcedureBool(
                CodEmpresa,
                SpLiquidacionAsiento,
                new { Liq = LiqConsec },
                "Error al ejecutar asiento de liquidación.");
        }


        /// <summary>
        /// Ejecuta el proceso de traslado de gastos operativos para una liquidación específica, utilizando la información proporcionada en el modelo AfLiquidacionTrasladoOpExInput. Este método es esencial para llevar a cabo el traslado de gastos operativos de los socios durante el proceso de liquidación, permitiendo una gestión adecuada de este traslado y facilitando la toma de decisiones informadas sobre las acciones a tomar en relación con cada socio. La ejecución del traslado de gastos operativos es un paso importante en el proceso de liquidación, ya que puede afectar el monto a liquidar y las acciones relacionadas con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="LiqConsec"></param>
        /// <returns></returns>
        public ErrorDto<bool> AF_Liquidacion_Traslado_OpEx_Ejecutar(int CodEmpresa, int LiqConsec)
        {
            return EjecutarStoredProcedureBool(
                CodEmpresa,
                SpTrasladoOpEx,
                new { Liq = LiqConsec },
                "Error al ejecutar traslado OpEx.");
        }


        /// <summary>
        /// Ejecuta el proceso de devolución de fondos para una liquidación específica, utilizando la información proporcionada en el modelo AfLiquidacionDevolucionFondosInput. Este método es esencial para llevar a cabo la devolución de fondos de los socios durante el proceso de liquidación, permitiendo una gestión adecuada de esta devolución y facilitando la toma de decisiones informadas sobre las acciones a tomar en relación con cada socio. La ejecución de la devolución de fondos es un paso importante en el proceso de liquidación, ya que puede afectar el monto a liquidar y las acciones relacionadas con cada socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="LiqConsec"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<bool> AF_Liquidacion_Fondos_Devolucion_Ejecutar(int CodEmpresa, int LiqConsec, string Usuario)
        {
            return EjecutarStoredProcedureBool(
                CodEmpresa,
                SpFondosDevolucion,
                new
                {
                    NumLiq = LiqConsec,
                    Usuario = NormalizarTexto(Usuario)
                },
                "Error al ejecutar devolución de fondos.");
        }

    }
}