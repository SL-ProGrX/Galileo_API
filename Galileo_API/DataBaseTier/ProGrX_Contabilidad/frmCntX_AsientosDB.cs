using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXAsientosDb
    {
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

        public ErrorDto<string?> CntXTiposAsientos_Descripcion_Obtener(int codEmpresa, int codConta, string tipoAsiento)
        {
            const string query = @"select top 1 rtrim(descripcion) 
                from CntX_Tipos_Asientos 
                where cod_contabilidad = @codConta
                  and tipo_asiento = @tipoAsiento;";

            return DbHelper.ExecuteSingleQuery<string>(_portalDb, codEmpresa, query, null, new { codConta, tipoAsiento });
        }

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

        public ErrorDto CntXAsientos_Guardar(int codEmpresa, string usuario, bool edita, CntXAsientoGuardarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            using var tran = conn.BeginTransaction();

            try
            {
                var periodoAbierto = _mCalculos.FxCntX_PeriodoVerifica(
                    codEmpresa,
                    request.asiento.cod_contabilidad,
                    request.asiento.anio,
                    request.asiento.mes
                );

                if (!periodoAbierto)
                {
                    tran.Rollback();
                    return new ErrorDto
                    {
                        Code = -1,
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

                    if (!TsSonIguales(concurrencia, request.asiento.ts))
                    {
                        tran.Rollback();
                        return new ErrorDto
                        {
                            Code = -1,
                            Description = "El asiento actual ha sido modificado por otro usuario o proceso."
                        };
                    }

                    var updateRows = ActualizarAsiento(conn, tran, usuario, request);
                    if (updateRows == 0)
                    {
                        tran.Rollback();
                        return new ErrorDto
                        {
                            Code = -1,
                            Description = "El asiento fue modificado por otro usuario o proceso."
                        };
                    }
                }
                else
                {
                    var existe = conn.QueryFirstOrDefault<int>(
                        @"select count(*)
                          from Cntx_Asientos
                          where cod_contabilidad = @codConta
                            and tipo_asiento = @tipoAsiento
                            and num_asiento = @numAsiento;",
                        new
                        {
                            codConta = request.asiento.cod_contabilidad,
                            tipoAsiento = request.asiento.tipo_asiento,
                            numAsiento = request.asiento.num_asiento
                        },
                        tran
                    );

                    if (existe > 0)
                    {
                        tran.Rollback();
                        return new ErrorDto
                        {
                            Code = -1,
                            Description = "El asiento a registrar ya existe."
                        };
                    }

                    InsertarAsiento(conn, tran, usuario, request);
                }

                EliminarDetalle(
                    conn,
                    tran,
                    request.asiento.cod_contabilidad,
                    request.asiento.tipo_asiento,
                    request.asiento.num_asiento
                );

                InsertarDetalle(conn, tran, request);

                tran.Commit();

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
                tran.Rollback();
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        public ErrorDto CntXAsientos_Eliminar(
            int codEmpresa, int codConta, string tipoAsiento, string numAsiento, byte[] ts, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            using var tran = conn.BeginTransaction();

            try
            {
                var concurrencia = _mCalculos.FxCntX_AsientoConcurrencia(
                    codEmpresa,
                    codConta,
                    numAsiento,
                    tipoAsiento
                );

                if (!TsSonIguales(concurrencia, ts))
                {
                    tran.Rollback();
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = "El asiento actual ha sido modificado por otro usuario o proceso."
                    };
                }

                conn.Execute(
                    @"delete from Cntx_Asientos_detalle
                      where cod_contabilidad = @codConta
                        and tipo_asiento = @tipoAsiento
                        and num_asiento = @numAsiento;",
                    new { codConta, tipoAsiento, numAsiento },
                    tran
                );

                var rows = conn.Execute(
                    @"delete from Cntx_Asientos
                      where cod_contabilidad = @codConta
                        and tipo_asiento = @tipoAsiento
                        and num_asiento = @numAsiento
                        and ts = @ts;",
                    new { codConta, tipoAsiento, numAsiento, ts },
                    tran
                );

                if (rows == 0)
                {
                    tran.Rollback();
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = "El asiento fue modificado por otro usuario o proceso."
                    };
                }

                tran.Commit();

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
                tran.Rollback();
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

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
                    documento = (request.documento ?? string.Empty).Length > 35
                        ? request.documento.Substring(0, 35)
                        : request.documento,
                    detalle = (request.detalle ?? string.Empty).Length > 100
                        ? request.detalle.Substring(0, 100)
                        : request.detalle,
                    referencia = (request.referencia ?? string.Empty).Length > 200
                        ? request.referencia.Substring(0, 200)
                        : request.referencia,
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

        private int ActualizarAsiento(
            System.Data.IDbConnection conn,
            System.Data.IDbTransaction tran,
            string usuario,
            CntXAsientoGuardarRequest request)
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

            return conn.Execute(sql, new
            {
                descripcion = request.asiento.descripcion,
                anio = request.asiento.anio,
                mes = request.asiento.mes,
                fechaAsiento = request.asiento.fecha_asiento,
                balanceado = request.balanceado ? "S" : "N",
                usuario = usuario.Trim().ToUpper(),
                notas = (request.asiento.notas ?? string.Empty).Trim(),
                referencia = LimitarTexto(request.asiento.referencia, 200),
                codConta = request.asiento.cod_contabilidad,
                tipoAsiento = request.asiento.tipo_asiento.ToUpper(),
                numAsiento = request.asiento.num_asiento,
                ts = request.asiento.ts
            }, tran);
        }

        private void InsertarAsiento(
            System.Data.IDbConnection conn,
            System.Data.IDbTransaction tran,
            string usuario,
            CntXAsientoGuardarRequest request)
        {
            const string sql = @"insert into Cntx_Asientos
                (tipo_asiento, cod_contabilidad, num_asiento, anio, mes, fecha_asiento,
                 descripcion, balanceado, user_crea, modulo, notas, referencia)
                values
                (@tipoAsiento, @codConta, @numAsiento, @anio, @mes, @fechaAsiento,
                 @descripcion, @balanceado, @usuario, @modulo, @notas, @referencia);";

            conn.Execute(sql, new
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
                referencia = LimitarTexto(request.asiento.referencia, 200)
            }, tran);
        }

        private void EliminarDetalle(
            System.Data.IDbConnection conn,
            System.Data.IDbTransaction tran,
            int codConta,
            string tipoAsiento,
            string numAsiento)
        {
            const string sql = @"delete from Cntx_Asientos_detalle
                where cod_contabilidad = @codConta
                  and tipo_asiento = @tipoAsiento
                  and num_asiento = @numAsiento;";

            conn.Execute(sql, new { codConta, tipoAsiento, numAsiento }, tran);
        }

        private void InsertarDetalle(
            System.Data.IDbConnection conn,
            System.Data.IDbTransaction tran,
            CntXAsientoGuardarRequest request)
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

            conn.Execute(sql, lineas, tran);
        }

        private bool TsSonIguales(byte[]? tsActual, byte[]? tsOriginal)
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

        private string LimitarTexto(string? valor, int max)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;

            return valor.Length > max ? valor.Substring(0, max) : valor;
        }

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