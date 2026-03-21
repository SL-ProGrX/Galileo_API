using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXAsientosDb
    {
        private const string AsientoModificadoPorOtroUsuario = "El asiento actual ha sido modificado por otro usuario o proceso.";
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _Bitacora;
        private readonly MCntXCalculosDb _mCalculos;
        private readonly int vModulo = 20;

        public FrmCntXAsientosDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config),
                 new MCntXCalculosDb(config))
        {
        }

        public FrmCntXAsientosDb(PortalDB portalDB, MSecurityMainDb dbBitacora, MCntXCalculosDb mCalculos)
        {
            _portalDb = portalDB;
            _Bitacora = dbBitacora;
            _mCalculos = mCalculos;
        }

        /// <summary>
        /// Obtiene un asiento contable 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="tipoAsiento"></param>
        /// <param name="numAsiento"></param>
        /// <returns></returns>
        public ErrorDto<CntXAsientoData?> CntXAsientos_Obtener(int codEmpresa, int codConta, string tipoAsiento, string numAsiento)
        {
            const string sql = @"select * from Cntx_Asientos 
                where cod_contabilidad = @codConta
                  and tipo_asiento = @tipoAsiento
                  and num_asiento = @numAsiento;";

            return DbHelper.ExecuteSingleQuery<CntXAsientoData>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new { codConta, tipoAsiento, numAsiento }
            );
        }

        /// <summary>
        /// Obtiene el detalle de un asiento contable
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="tipoAsiento"></param>
        /// <param name="numAsiento"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXAsientoDetalleData>> CntXAsientos_Detalle_Obtener(int codEmpresa, int codConta, string tipoAsiento, string numAsiento)
        {
            const string query = @"select A.cod_cuenta,B.descripcion,A.documento,A.detalle,A.monto_debito,A.monto_credito,A.num_linea
                ,isnull(M.saldo_inicial,0) as Saldo_Inicial,isnull(M.total_debitos,0) as Total_debitos
                ,isnull(M.total_creditos,0) as Total_creditos,A.cod_unidad,U.descripcion as UniDes
                ,A.cod_divisa,Y.descripcion as Divisa,isnull(A.Tipo_Cambio,1) as TC,isnull(A.Tipo_Cambio_Ajuste,0) as TC_Ajuste
                ,A.cod_Centro_Costo,Cc.Descripcion as CentroCosto
                from Cntx_Asientos_detalle A inner join Cntx_Asientos X on A.cod_contabilidad = X.cod_contabilidad
                and A.tipo_asiento = X.tipo_asiento and A.num_asiento = X.num_asiento
                inner join CntX_Cuentas B on A.cod_cuenta = B.cod_cuenta and A.cod_contabilidad = B.cod_contabilidad
                inner join CntX_Unidades U on A.cod_unidad = U.cod_unidad and A.cod_contabilidad = U.cod_contabilidad
                inner join CntX_Divisas Y on A.cod_divisa = Y.cod_divisa and A.cod_contabilidad = Y.cod_contabilidad
                left join vCntX_Mov_Cuentas_General M on A.cod_cuenta = M.cod_cuenta and A.cod_contabilidad = M.cod_contabilidad
                and M.anio = X.anio and M.mes = X.mes
                left join CntX_Centro_Costos Cc on A.cod_centro_costo = Cc.cod_centro_costo and A.cod_contabilidad = Cc.cod_contabilidad
                and A.cod_contabilidad = M.cod_contabilidad and X.anio = M.anio and X.mes = M.mes
                where A.cod_contabilidad = @codConta and A.tipo_asiento = @tipoAsiento and A.num_asiento = @numAsiento 
                order by A.num_linea;";

            return DbHelper.ExecuteListQuery<CntXAsientoDetalleData>(
                _portalDb,
                codEmpresa,
                query,
                new { codConta, tipoAsiento, numAsiento }
            );
        }

        /// <summary>
        /// Navegacion por scroll en asientos contables
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="scrollCode"></param>
        /// <returns></returns>
        public ErrorDto<CntXAsientoData?> CntXAsientos_Scroll_Obtener(int codEmpresa, CntXAsientoData request, int scrollCode)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string query = @"
                    select top 1 num_asiento
                    from Cntx_Asientos
                    where cod_contabilidad = @codConta
                      and tipo_asiento = @tipoAsiento
                      and anio = @anio
                      and mes = @mes
                      and (
                            (@scrollCode = 1 and num_asiento > @numAsiento)
                         or (@scrollCode <> 1 and num_asiento < @numAsiento)
                      )
                    order by
                        case when @scrollCode = 1 then num_asiento end asc,
                        case when @scrollCode <> 1 then num_asiento end desc;";

                var asientoDestino = conn.QueryFirstOrDefault<string>(query, new
                {
                    codConta = request.cod_contabilidad,
                    tipoAsiento = request.tipo_asiento,
                    anio = request.anio,
                    mes = request.mes,
                    numAsiento = request.num_asiento,
                    scrollCode
                });

                var numeroObjetivo = string.IsNullOrWhiteSpace(asientoDestino)
                    ? request.num_asiento
                    : asientoDestino;

                return CntXAsientos_Obtener(codEmpresa, request.cod_contabilidad, request.tipo_asiento, numeroObjetivo);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CntXAsientoData?>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista de asientos contables
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="tipoAsiento"></param>
        /// <param name="periodoActual"></param>
        /// <param name="anio"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXAsientos_Lista_Obtener(int codEmpresa, int codConta, string tipoAsiento, bool periodoActual, int anio, int mes)
        {
            string query = @"select Num_asiento as item, descripcion 
                from Cntx_Asientos 
                where cod_contabilidad = @codConta 
                  and tipo_asiento = @tipoAsiento ";

            if (periodoActual)
            {
                query += " and anio = @anio and mes = @mes";
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query,
                new { codConta, tipoAsiento, anio, mes }
            );
        }

        /// <summary>
        /// Obtiene la lista de tipos de asientos contables
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXTiposAsientos_Lista_Obtener(int codEmpresa, int codConta)
        {
            const string query = @"select rtrim(tipo_asiento) as item, rtrim(descripcion) as descripcion 
                from CntX_Tipos_Asientos 
                where cod_contabilidad = @codConta 
                order by tipo_asiento;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query,
                new { codConta }
            );
        }

        /// <summary>
        /// Obtiene la descripcion de un tipo de asiento contable
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="tipoAsiento"></param>
        /// <returns></returns>
        public ErrorDto<string?> CntXTiposAsientos_Descripcion_Obtener(int codEmpresa, int codConta, string tipoAsiento)
        {
            const string query = @"select top 1 rtrim(descripcion) 
                from CntX_Tipos_Asientos 
                where cod_contabilidad = @codConta
                  and tipo_asiento = @tipoAsiento;";

            return DbHelper.ExecuteSingleQuery<string>(_portalDb, codEmpresa, query, null, new { codConta, tipoAsiento });
        }

        /// <summary>
        /// Obtiene la lista de centros de costo por unidad para asientos contables
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="codUnidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXCentroCostosporUnidad_Lista_Obtener(int codEmpresa, int codConta, string codUnidad)
        {
            const string query = @"select cod_centro_costo as item, descripcion 
                from CntX_Centro_Costos 
                where cod_contabilidad = @codConta 
                  and cod_centro_costo in (
                        select cod_centro_costo 
                        from cntX_unidades_cc 
                        where cod_unidad = @codUnidad 
                          and cod_contabilidad = @codConta
                  );";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query,
                new { codConta, codUnidad }
            );
        }

        /// <summary>
        /// Obtiene el consecutivo para un nuevo asiento contable
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="tipoAsiento"></param>
        /// <returns></returns>
        public ErrorDto<string?> CntXAsientos_Consecutivo_Obtener(int codEmpresa, int codConta, string tipoAsiento)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string querySelect = @"select isnull(consecutivo,0) + 1 
                    from CntX_Tipos_Asientos
                    where cod_contabilidad = @codConta 
                      and tipo_asiento = @tipoAsiento;";

                var consecutivo = conn.QueryFirstOrDefault<int>(querySelect, new { codConta, tipoAsiento });

                const string queryUpdate = @"update CntX_Tipos_Asientos
                    set consecutivo = isnull(consecutivo,0) + 1
                    where cod_contabilidad = @codConta
                      and tipo_asiento = @tipoAsiento;";

                conn.Execute(queryUpdate, new { codConta, tipoAsiento });

                return new ErrorDto<string?>
                {
                    Code = 0,
                    Result = consecutivo.ToString("00000000")
                };
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<string?>(ex.Message);
            }
        }

        /// <summary>
        /// Guarda un asiento contable
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="edita"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntXAsientos_Guardar(int codEmpresa, string usuario, bool edita, CntXAsientoGuardarRequest request)
        {
            try
            {
                var validacion = FxVerificarAsiento(codEmpresa, edita, request);
                if (validacion.Code != 0)
                {
                    return validacion;
                }

                var periodoAbiert = _mCalculos.FxCntX_PeriodoVerifica(
                    codEmpresa,
                    request.asiento.cod_contabilidad,
                    request.asiento.anio,
                    request.asiento.mes
                );

                if (!periodoAbiert)
                {
                    return new ErrorDto
                    {
                        Code = -2,
                        Description = "El período indicado se encuentra cerrado o no se ha creado."
                    };
                }

                if (edita)
                {
                    var concurrencia = _mCalculos.FxCntX_AsientoConcurrencia(
                        codEmpresa,
                        request.asiento.cod_contabilidad,
                        request.asiento.num_asiento,
                        request.asiento.tipo_asiento
                    );

                    bool concurrenciaResult = TsSonIguales(concurrencia, request.asiento.ts);
                    if (!concurrenciaResult)
                    {
                        return new ErrorDto
                        {
                            Code = -2,
                            Description = AsientoModificadoPorOtroUsuario
                        };
                    }

                    var updateRows = ActualizarAsiento(codEmpresa, usuario, request);
                    if (updateRows.Code == -1)
                    {
                        return updateRows;
                    }
                }
                else
                {
                    var insertAsiento = InsertarAsiento(codEmpresa, usuario, request);

                    if (insertAsiento.Code == -1)
                    {
                        return insertAsiento;
                    }
                }

                var eliminarDetalle = EliminarDetalle(
                    codEmpresa,
                    request.asiento.cod_contabilidad,
                    request.asiento.tipo_asiento,
                    request.asiento.num_asiento
                );
                if (eliminarDetalle.Code == -1)
                {
                    return eliminarDetalle;
                }

                var insertarDetalle = InsertarDetalle(codEmpresa, request);
                if (insertarDetalle.Code == -1)
                {
                    return insertarDetalle;
                }

                RegistrarBitacora(
                    codEmpresa,
                    usuario,
                    edita ? "Modifica - WEB" : "Registra - WEB",
                    $"Asiento : {request.asiento.tipo_asiento}-{request.asiento.num_asiento} Conta.{request.asiento.cod_contabilidad}"
                );

                return new ErrorDto
                {
                    Code = 0,
                    Description = "Información guardada satisfactoriamente..."
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Elimina un asiento contable
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="tipoAsiento"></param>
        /// <param name="numAsiento"></param>
        /// <param name="ts"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CntXAsientos_Eliminar(
            int codEmpresa, int codConta, string tipoAsiento, string numAsiento, byte[] ts, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var concurrenc = _mCalculos.FxCntX_AsientoConcurrencia(
                    codEmpresa,
                    codConta,
                    numAsiento,
                    tipoAsiento
                );

                bool concurrencResult = TsSonIguales(concurrenc, ts);
                if (!concurrencResult)
                {
                    return new ErrorDto
                    {
                        Code = -2,
                        Description = AsientoModificadoPorOtroUsuario
                    };
                }

                conn.Execute(
                    @"delete from Cntx_Asientos_detalle
                      where cod_contabilidad = @codConta
                        and tipo_asiento = @tipoAsiento
                        and num_asiento = @numAsiento;",
                    new { codConta, tipoAsiento, numAsiento }
                );

                var rows = conn.Execute(
                    @"delete from Cntx_Asientos
                      where cod_contabilidad = @codConta
                        and tipo_asiento = @tipoAsiento
                        and num_asiento = @numAsiento
                        and ts = @ts;",
                    new { codConta, tipoAsiento, numAsiento, ts }
                );

                if (rows == 0)
                {
                    return new ErrorDto
                    {
                        Code = -2,
                        Description = AsientoModificadoPorOtroUsuario
                    };
                }

                RegistrarBitacora(
                    codEmpresa,
                    usuario,
                    "Elimina - WEB",
                    $"Asiento : {tipoAsiento}-{numAsiento} Conta.{codConta}"
                );

                return new ErrorDto
                {
                    Code = 0,
                    Description = "Información eliminada satisfactoriamente..."
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Autoriza un asiento contable
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="tipoAsiento"></param>
        /// <param name="numAsiento"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CntXAsientos_Autorizar(int codEmpresa, int codConta, string tipoAsiento, string numAsiento, string usuario)
        {
            const string sql = @"update Cntx_Asientos
                set user_autoriza = @usuario,
                    fecha_autoriza = getdate()
                where cod_contabilidad = @codConta
                  and tipo_asiento = @tipoAsiento
                  and num_asiento = @numAsiento
                  and fecha_autoriza is null
                  and modulo <> 20;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new { usuario, codConta, tipoAsiento, numAsiento }
            );

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                "Autoriza - WEB",
                $"Asiento foráneo : {tipoAsiento}-{numAsiento} Conta.{codConta}"
            );

            return resp;
        }

        /// <summary>
        /// Copia un asiento contable
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntXAsientos_Copiar(int codEmpresa, string usuario, CntXAsientoCopiarRequest request)
        {
            const string sql = @"exec spCntX_Asiento_Copia
                @codConta,
                @tipoAsiento,
                @numAsiento,
                @nuevoNumAsiento,
                @descripcion,
                @fecha,
                @usuario,
                @notas,
                @copiarDetalles,
                @documento,
                @detalle,
                @referencia,
                @asReversion;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    codConta = request.cod_contabilidad,
                    tipoAsiento = request.tipo_asiento,
                    numAsiento = request.num_asiento,
                    nuevoNumAsiento = request.nuevo_num_asiento,
                    descripcion = request.descripcion,
                    fecha = request.fecha,
                    usuario,
                    notas = request.notas,
                    copiarDetalles = request.copiar_detalles ? 1 : 0,
                    documento = FxTruncar(request.documento, 35),
                    detalle = FxTruncar(request.detalle, 100),
                    referencia = FxTruncar(request.referencia, 200),
                    asReversion = request.as_reversion ? 1 : 0
                }
            );

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                "Copia - WEB",
                $"Asiento : {request.tipo_asiento}-{request.num_asiento} -> {request.nuevo_num_asiento} Conta.{request.cod_contabilidad}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = "Copia de asiento realizada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Reversa un asiento contable
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntXAsientos_Reversar(int codEmpresa, CntXAsientoData request)
        {
            var periodoAbi = _mCalculos.FxCntX_PeriodoVerifica(
                    codEmpresa,
                    request.cod_contabilidad,
                    request.anio,
                    request.mes
                );
            if (!periodoAbi)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "No se puede reversar este asiento porque el periodo se encuentra cerrado..."
                };
            }

            var concurren = _mCalculos.FxCntX_AsientoConcurrencia(
                    codEmpresa,
                    request.cod_contabilidad,
                    request.num_asiento,
                    request.tipo_asiento
                );

            bool concurrenResult = TsSonIguales(concurren, request.ts);
            if (!concurrenResult)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = AsientoModificadoPorOtroUsuario
                };
            }

            const string query = @"exec spCntX_AsientoReversa @codConta, @usuario, @tipoAsiento, @numAsiento;";
            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, 
                new { 
                    codConta = request.cod_contabilidad, 
                    usuario = request.user_modifica, 
                    numAsiento = request.num_asiento, 
                    tipoAsiento = request.tipo_asiento });
        }

        /// <summary>
        /// Mayoriza un asiento contable
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntXAsientos_Mayorizar(int codEmpresa, CntXAsientoData request)
        {
            var periodoAb = _mCalculos.FxCntX_PeriodoVerifica(
                    codEmpresa,
                    request.cod_contabilidad,
                    request.anio,
                    request.mes
                );
            if (!periodoAb)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "No se puede mayorizar este asiento porque el periodo se encuentra cerrado..."
                };
            }

            var concur = _mCalculos.FxCntX_AsientoConcurrencia(
                    codEmpresa,
                    request.cod_contabilidad,
                    request.num_asiento,
                    request.tipo_asiento
                );

            bool concurResult = TsSonIguales(concur, request.ts);
            if (!concurResult)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = AsientoModificadoPorOtroUsuario
                };
            }

            const string query = @"exec spCntX_AsientoMayoriza @codConta, @usuario, @tipoAsiento, @numAsiento;";
            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query,
                new
                {
                    codConta = request.cod_contabilidad,
                    usuario = request.user_modifica,
                    numAsiento = request.num_asiento,
                    tipoAsiento = request.tipo_asiento
                });
        }

        /// <summary>
        /// Obtiene la nota de cuenta para un asiento contable
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="vCuenta"></param>
        /// <param name="anio"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
        public ErrorDto CntXAsientos_FxNotaCuenta_Obtener(int codEmpresa, int codConta, string vCuenta, int anio, int mes)
        {
            const string query = @"
            select saldo_inicial, total_debitos, total_creditos
            from vCntX_Mov_Cuentas_General
            where cod_contabilidad = @codConta
              and cod_cuenta = @codCuenta
              and anio = @anio
              and mes = @mes;";

            var dataResult = DbHelper.ExecuteSingleQuery<dynamic>(_portalDb, codEmpresa, query, null,
                new
                {
                    codConta = codConta,
                    codCuenta = vCuenta,
                    anio,
                    mes
                });

            if (dataResult.Code == -1)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = dataResult.Description
                };
            }

            if (dataResult.Result == null)
            {
                return new ErrorDto
                {
                    Code = 0,
                    Description = "No disponible por el momento"
                };
            }
            dynamic data = dataResult.Result;
            decimal saldoInicial = data.saldo_inicial ?? 0m;
            decimal totalDebitos = data.total_debitos ?? 0m;
            decimal totalCreditos = data.total_creditos ?? 0m;

            return new ErrorDto
            {
                Code = 0,
                Description = "Estado del Periodo:" + Environment.NewLine
                 + "___________________" + Environment.NewLine
                 + " Saldo Inicial : " + saldoInicial.ToString("N2") + Environment.NewLine
                 + " Total Debitos : " + Math.Abs(totalDebitos).ToString("N2") + Environment.NewLine
                 + " Total Creditos: " + Math.Abs(totalCreditos).ToString("N2") + Environment.NewLine
                 + " Mensual       : " + (totalDebitos + totalCreditos).ToString("N2") + Environment.NewLine
                 + " Acumulado     : " + (saldoInicial + totalDebitos + totalCreditos).ToString("N2") + Environment.NewLine
                 + "___________________"
            };
        }

        /// <summary>
        /// Actualiza un asiento contable
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto ActualizarAsiento(int codEmpresa,  string usuario, CntXAsientoGuardarRequest request)
        {
            const string sql = @"update Cntx_Asientos
                set descripcion = @descripcion,
                    anio = @anio,
                    mes = @mes,
                    fecha_asiento = @fechaAsiento,
                    balanceado = @balanceado,
                    user_modifica = @usuario,
                    notas = @notas,
                    referencia = @referencia
                where cod_contabilidad = @codConta
                  and tipo_asiento = @tipoAsiento
                  and num_asiento = @numAsiento
                  and ts = @ts;";

            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, new 
            {
                descripcion = request.asiento.descripcion,
                anio = request.asiento.anio,
                mes = request.asiento.mes,
                fechaAsiento = request.asiento.fecha_asiento,
                balanceado = request.balanceado ? "S" : "N",
                usuario = usuario.Trim().ToUpper(),
                notas = (request.asiento.notas ?? string.Empty).Trim(),
                referencia = Tuncar(request.asiento.referencia, 200),
                codConta = request.asiento.cod_contabilidad,
                tipoAsiento = request.asiento.tipo_asiento.ToUpper(),
                numAsiento = request.asiento.num_asiento,
                ts = request.asiento.ts
            });
        }

        /// <summary>
        /// Inserta un nuevo asiento contable
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto InsertarAsiento(int codEmpresa,  string usuario, CntXAsientoGuardarRequest request)
        {
            const string sql = @"insert into Cntx_Asientos
                (tipo_asiento, cod_contabilidad, num_asiento, anio, mes, fecha_asiento,
                 descripcion, balanceado, user_crea, modulo, notas, referencia)
                values
                (@tipoAsiento, @codConta, @numAsiento, @anio, @mes, @fechaAsiento,
                 @descripcion, @balanceado, @usuario, @modulo, @notas, @referencia);";

            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, new
            {
                tipoAsiento = request.asiento.tipo_asiento.ToUpper(),
                codConta = request.asiento.cod_contabilidad,
                numAsiento = request.asiento.num_asiento,
                anio = request.asiento.anio,
                mes = request.asiento.mes,
                fechaAsiento = request.asiento.fecha_asiento,
                descripcion = request.asiento.descripcion,
                balanceado = request.balanceado ? "S" : "N",
                usuario = usuario.Trim().ToUpper(),
                modulo = vModulo,
                notas = (request.asiento.notas ?? string.Empty).Trim(),
                referencia = FxTruncar(request.asiento.referencia, 200)
            });
        }

        /// <summary>
        /// Elimina el detalle de un asiento contable para luego insertar el nuevo detalle actualizado
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="tipoAsiento"></param>
        /// <param name="numAsiento"></param>
        /// <returns></returns>
        private ErrorDto EliminarDetalle(int codEmpresa, int codConta, string tipoAsiento, string numAsiento)
        {
            const string sql = @"delete from Cntx_Asientos_detalle
                where cod_contabilidad = @codConta
                  and tipo_asiento = @tipoAsiento
                  and num_asiento = @numAsiento;";

            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, new { codConta, tipoAsiento, numAsiento });
        }

        /// <summary>
        /// Inserta el detalle de un asiento contable, eliminando previamente el detalle existente en caso de edicion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto InsertarDetalle(int codEmpresa, CntXAsientoGuardarRequest request)
        {
            const string sql = @"insert into Cntx_Asientos_detalle
            (num_linea, tipo_asiento, num_asiento, cod_contabilidad, cod_cuenta,
             cod_unidad, cod_centro_costo, cod_divisa, tipo_cambio, documento,
             detalle, monto_debito, monto_credito)
            values
            (@numLinea, @tipoAsiento, @numAsiento, @codConta, @codCuenta,
             @codUnidad, @codCentroCosto, @codDivisa, @tipoCambio, @documento,
             @detalle, @montoDebito, @montoCredito);";

            var lineas = request.detalle
                .Where(x => !string.IsNullOrWhiteSpace(x.cod_cuenta))
                .Select((x, index) => new
                {
                    numLinea = index + 1,
                    tipoAsiento = request.asiento.tipo_asiento.ToUpper(),
                    numAsiento = request.asiento.num_asiento,
                    codConta = request.asiento.cod_contabilidad,
                    codCuenta = x.cod_cuenta,
                    codUnidad = x.cod_unidad,
                    codCentroCosto = x.cod_centro_costo,
                    codDivisa = x.cod_divisa,
                    tipoCambio = x.tc,
                    documento = x.documento ?? string.Empty,
                    detalle = x.detalle ?? string.Empty,
                    montoDebito = x.monto_debito,
                    montoCredito = x.monto_credito
                });

            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, lineas);
        }

        /// <summary>
        /// Verifica que la cuenta contable exista y acepte movimientos para poder ser utilizada en el detalle del asiento contable
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="strCuenta"></param>
        /// <returns></returns>
        public bool FxVerificaCuenta(int codEmpresa, int codContabilidad, string strCuenta)
        {
            const string query = @"select isnull(count(*), 0)
                from CntX_Cuentas 
                where cod_contabilidad = @codConta 
                  and cod_cuenta = @codCuenta 
                  and acepta_movimientos = 1;";

            int existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, query, 0, 
                new
                {
                    codConta = codContabilidad,
                    codCuenta = strCuenta
                }).Result;

            return existe > 0;
        }

        /// <summary>
        /// Verifica que el asiento contable cumpla con las reglas de negocio para su creacion o modificacion, validando tanto la informacion del asiento como de sus lineas de detalle
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="edita"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto FxVerificarAsiento(int codEmpresa, bool edita, CntXAsientoGuardarRequest request)
        {
            try
            {
                var errores = new List<string>();

                VerificarTipoAsiento(codEmpresa, request, errores);
                var divisaLocal = ObtenerDivisaLocal(codEmpresa, request.asiento.cod_contabilidad);

                if (!edita)
                {
                    VerificarExistenciaAsiento(codEmpresa, request, errores);
                }

                if (!request.asiento.fecha_asiento.HasValue)
                {
                    errores.Add("La fecha del asiento es requerida.");
                }
                else
                {
                    var fecha = request.asiento.fecha_asiento.Value;

                    if (fecha.Month != request.asiento.mes)
                    {
                        errores.Add("El mes del período no coincide con la fecha del asiento.");
                    }

                    if (fecha.Year != request.asiento.anio)
                    {
                        errores.Add("El año del período no coincide con la fecha del asiento.");
                    }
                }

                VerificarDetalleAsiento(codEmpresa, request, divisaLocal, errores);

                if (errores.Count > 0)
                {
                    return new ErrorDto
                    {
                        Code = -2,
                        Description = string.Join(Environment.NewLine, errores.Select(x => $"- {x}"))
                    };
                }

                return new ErrorDto
                {
                    Code = 0,
                    Description = string.Empty
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        /// <summary>
        /// Verifica que el tipo de asiento 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="errores"></param>
        private void VerificarTipoAsiento(int codEmpresa, CntXAsientoGuardarRequest request, List<string> errores)
        {
            var existeTipoAsiento = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa,
                @"select count(*) from CntX_Tipos_Asientos
                where cod_contabilidad = @codConta and tipo_asiento = @tipoAsiento;", 0,
                new
                {
                    codConta = request.asiento.cod_contabilidad,
                    tipoAsiento = request.asiento.tipo_asiento
                }
            ).Result;

            if (existeTipoAsiento == 0)
            {
                errores.Add("El tipo de asiento indicado no existe.");
            }
        }

        /// <summary>
        /// Obtiene la divisa local 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <returns></returns>
        private string ObtenerDivisaLocal(int codEmpresa, int codConta)
        {
            var divisaLocal = DbHelper.ExecuteSingleQuery<string>(_portalDb, codEmpresa,
                @"select top 1 COD_DIVISA from CNTX_DIVISAS 
                where DIVISA_LOCAL = 1 and COD_CONTABILIDAD = @codConta;", string.Empty,
                new { codConta }
            ).Result;

            return (divisaLocal ?? string.Empty).Trim().ToUpperInvariant();
        }

        /// <summary>
        /// Verifica que no exista un asiento contable con el mismo número, tipo y contabilidad
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="errores"></param>
        private void VerificarExistenciaAsiento(int codEmpresa, CntXAsientoGuardarRequest request, List<string> errores)
        {
            var existeAsiento = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa,
                @"select count(*) from CntX_Asientos where cod_contabilidad = @codConta
                and tipo_asiento = @tipoAsiento  and num_asiento = @numAsiento;", 0,
                new
                {
                    codConta = request.asiento.cod_contabilidad,
                    tipoAsiento = request.asiento.tipo_asiento,
                    numAsiento = request.asiento.num_asiento
                }
            ).Result;

            if (existeAsiento > 0)
            {
                errores.Add("El asiento a registrar ya existe. Consúltelo para referencia o cambie el número actual.");
            }
        }

        /// <summary>
        /// Verifica que las líneas de detalle del asiento
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="divisaLocal"></param>
        /// <param name="errores"></param>
        private void VerificarDetalleAsiento(int codEmpresa, CntXAsientoGuardarRequest request, string divisaLocal, List<string> errores)
        {
            if (request.detalle == null || request.detalle.Count == 0)
            {
                errores.Add("El asiento no contiene líneas de detalle.");
                return;
            }
            for (int i = 0; i < request.detalle.Count; i++)
            {
                var linea = request.detalle[i];
                var numeroLinea = linea.num_linea > 0 ? linea.num_linea : i + 1;
                var codDivisaLinea = (linea.cod_divisa ?? string.Empty).Trim().ToUpperInvariant();

                VerificarCuentaLinea(codEmpresa, request, linea, errores);
                VerificarUnidadLinea(codEmpresa, request, linea, errores);
                VerificarCentroCostoLinea(codEmpresa, request, linea, errores);
                VerificarDivisaLinea(codEmpresa, request, codDivisaLinea, errores);

                if (!string.IsNullOrWhiteSpace(divisaLocal) &&
                    codDivisaLinea != divisaLocal &&
                    linea.tc == 1)
                {
                    errores.Add($"Línea {numeroLinea}: tipo de cambio incorrecto.");
                }
            }
        }

        /// <summary>
        /// Verifica que la cuenta contable 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="linea"></param>
        /// <param name="errores"></param>
        private void VerificarCuentaLinea(int codEmpresa, CntXAsientoGuardarRequest request, CntXAsientoDetalleData linea, List<string> errores)
        {
            var existeCuenta = FxVerificaCuenta(
                codEmpresa,
                request.asiento.cod_contabilidad,
                linea.cod_cuenta
            );
            if (!existeCuenta)
            {
                errores.Add($"La cuenta {linea.cod_cuenta} no existe o no acepta movimientos.");
            }
        }

        /// <summary>
        /// Verifica que la unidad de negocio sea valido
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="linea"></param>
        /// <param name="errores"></param>
        private void VerificarUnidadLinea(int codEmpresa, CntXAsientoGuardarRequest request, CntXAsientoDetalleData linea, List<string> errores)
        {
            var unidadExiste = _mCalculos.FxCntX_UnidadVerifica(
                codEmpresa,
                request.asiento.cod_contabilidad,
                linea.cod_unidad
            );
            if (!unidadExiste)
            {
                errores.Add($"La UNIDAD de negocio no es válida :{linea.cod_unidad} - No existe...");
            }
        }

        /// <summary>
        /// Verifica que el centro de costo sea valido
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="linea"></param>
        /// <param name="errores"></param>
        private void VerificarCentroCostoLinea(int codEmpresa, CntXAsientoGuardarRequest request, CntXAsientoDetalleData linea, List<string> errores)
        {
            var ccExiste = _mCalculos.FxCntX_CentroCostoVerifica(
                codEmpresa,
                request.asiento.cod_contabilidad,
                linea.cod_centro_costo,
                linea.cod_unidad
            );
            if (!ccExiste)
            {
                errores.Add($"El Centro de Costo no es válido y no puede ser utilizada por esta unidad: {linea.cod_centro_costo} - No existe...");
            }
        }

        /// <summary>
        /// Verifica que la divisa sea valida
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="codDivisaLinea"></param>
        /// <param name="errores"></param>
        private void VerificarDivisaLinea(int codEmpresa, CntXAsientoGuardarRequest request, string codDivisaLinea, List<string> errores)
        {
            var divisaExiste = _mCalculos.FxCntX_DivisaVerifica(
                codEmpresa,
                request.asiento.cod_contabilidad,
                codDivisaLinea
            );
            if (!divisaExiste)
            {
                errores.Add($"La DIVISA no es válida :  {codDivisaLinea} - No existe...");
            }
        }

        /// <summary>
        /// Verifica si los timestamp son iguales
        /// </summary>
        /// <param name="tsActual"></param>
        /// <param name="tsOriginal"></param>
        /// <returns></returns>
        private static bool TsSonIguales(byte[]? tsActual, byte[]? tsOriginal)
        {
            if (tsActual == null && tsOriginal == null)
                return true;

            if (tsActual == null || tsOriginal == null)
                return false;

            if (tsActual.Length != tsOriginal.Length)
                return false;

            for (int i = 0; i < tsActual.Length; i++)
            {
                if (tsActual[i] != tsOriginal[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Trunca un string al maximo de caracteres permitido por la base de datos 
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        private static string FxTruncar(string? valor, int maxLength)
        {
            if (string.IsNullOrEmpty(valor))
                return string.Empty;

            return valor.Length > maxLength
                ? valor.Substring(0, maxLength)
                : valor;
        }

        /// <summary>
        /// Registra un movimiento en la bitacora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _Bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
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