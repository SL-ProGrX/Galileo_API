using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models;

namespace Galileo_API.DataBaseTier
{
    public class MCntXCalculosDb
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _mSecurityMain;
        private const int vModulo = 20;

        private sealed class CntXBalanceCuadradoData
        {
            public decimal debitos { get; set; } = 0;
            public decimal creditos { get; set; } = 0;
        }

        private sealed class CntXCalculosAsientoBorraData
        {
            public CntXCalculosAsientoBorraData(
                DateTime? fecha_aplicado,
                string? modulo,
                DateTime? fecha_autoriza)
            {
                this.fecha_aplicado = fecha_aplicado;
                this.modulo = modulo ?? string.Empty;
                this.fecha_autoriza = fecha_autoriza;
            }

            public DateTime? fecha_aplicado { get; }

            public string modulo { get; } = string.Empty;

            public DateTime? fecha_autoriza { get; }
        }

        public MCntXCalculosDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _mSecurityMain = new MSecurityMainDb(config);
        }

        public bool FxCntX_UnidadVerifica(int codEmpresa, int codConta, string pUnidad)
        {
            string sql = @"select isnull(count(*),0)
                   from CntX_Unidades
                   where cod_contabilidad = @CodConta
                     and cod_unidad = @Unidad";

            var parametros = new
            {
                CodConta = codConta,
                Unidad = pUnidad
            };

            int existe = DbHelper.ExecuteSingleQuery<int>(_portalDB, codEmpresa, sql, 0, parametros).Result;

            return existe > 0;
        }

        public bool FxCntX_CentroCostoVerifica(int codEmpresa, int codConta, string pCentroCosto, string pUnidad = "")
        {
            if (string.IsNullOrWhiteSpace(pCentroCosto))
            {
                return true;
            }

            string sql = @"select isnull(count(*),0)
                   from CntX_Centro_Costos
                   where cod_contabilidad = @CodConta
                     and cod_centro_costo = @CentroCosto";

            if (!string.IsNullOrWhiteSpace(pUnidad))
            {
                sql += @" and cod_centro_costo in (
                    select cod_centro_costo
                    from cntx_unidades_cc
                    where cod_contabilidad = @CodConta
                      and cod_unidad = @Unidad
                 )";
            }

            var parametros = new
            {
                CodConta = codConta,
                CentroCosto = pCentroCosto,
                Unidad = pUnidad
            };

            int existe = DbHelper.ExecuteSingleQuery<int>(_portalDB, codEmpresa, sql, 0, parametros).Result;

            return existe > 0;
        }

        public bool FxCntX_DivisaVerifica(int codEmpresa, int codConta, string pDivisa)
        {
            string sql = @"select isnull(count(*),0)
                   from CntX_Divisas
                   where cod_contabilidad = @CodConta
                     and cod_divisa = @Divisa";

            var parametros = new
            {
                CodConta = codConta,
                Divisa = pDivisa
            };

            int existe = DbHelper.ExecuteSingleQuery<int>(_portalDB, codEmpresa, sql, 0, parametros).Result;

            return existe > 0;
        }

        public bool FxCntX_PeriodoVerifica(int codEmpresa, int codConta, int pAnion, int pMes)
        {
            string sql = @"select isnull(count(*),0) as existe
                           from CntX_Periodos
                           where anio = @Anio
                             and mes = @Mes
                             and cod_contabilidad = @CodConta
                             and estado = 'P'";

            var parametros = new
            {
                Anio = pAnion,
                Mes = pMes,
                CodConta = codConta
            };

            int existe = DbHelper.ExecuteSingleQuery<int>(_portalDB, codEmpresa, sql, 0, parametros).Result;
            return existe > 0;
        }

        public byte[]? FxCntX_AsientoConcurrencia(int codEmpresa, int codConta, string numAsiento, string tipoAsiento)
        {
            string sql = @"select ts
                   from Cntx_Asientos
                   where cod_contabilidad = @CodConta
                     and num_asiento = @NumAsiento
                     and tipo_asiento = @TipoAsiento";

            var parametros = new
            {
                CodConta = codConta,
                NumAsiento = numAsiento,
                TipoAsiento = tipoAsiento
            };

            return DbHelper.ExecuteSingleQuery<byte[]?>(
                _portalDB,
                codEmpresa,
                sql,
                null,
                parametros).Result;
        }

        public string FxCntX_AsientoConcurrenciaHex(int codEmpresa, int codConta, string numAsiento, string tipoAsiento)
        {
            byte[]? ts = FxCntX_AsientoConcurrencia(codEmpresa, codConta, numAsiento, tipoAsiento);
            return FxTsToHex(ts);
        }

        public static string FxCntX_PeriodoDesc(int pAnio, int pMes)
        {
            return pMes switch
            {
                1 => $"ENERO DE {pAnio}",
                2 => $"FEBRERO DE {pAnio}",
                3 => $"MARZO DE {pAnio}",
                4 => $"ABRIL DE {pAnio}",
                5 => $"MAYO DE {pAnio}",
                6 => $"JUNIO DE {pAnio}",
                7 => $"JULIO DE {pAnio}",
                8 => $"AGOSTO DE {pAnio}",
                9 => $"SETIEMBRE DE {pAnio}",
                10 => $"OCTUBRE DE {pAnio}",
                11 => $"NOVIEMBRE DE {pAnio}",
                12 => $"DICIEMBRE DE {pAnio}",
                13 => $"CIERRE FISCAL {pAnio}",
                _ => string.Empty
            };
        }

        public bool FxCntX_MesFiscal(int codEmpresa, int codConta, int pAnio, int pMes)
        {
            string sql = @"select isnull(count(*),0) as Existe
                           from CntX_Asientos
                           where Anio = @Anio
                             and Mes = @Mes
                             and cod_Contabilidad = @CodConta
                             and Tipo_Asiento = 'CF'
                             and fecha_aplicado is not null";

            var parametros = new
            {
                Anio = pAnio,
                Mes = pMes,
                CodConta = codConta
            };

            int existe = DbHelper.ExecuteSingleQuery<int>(_portalDB, codEmpresa, sql, 0, parametros).Result;
            return existe > 0;
        }

        public bool FxCntX_BalanceCuadrado(int codEmpresa, int codConta, int pAnio, int pMes, string pUnidad = "")
        {
            string sql = @"
            select
                isnull(sum(abs(M.total_debitos)), 0) as debitos,
                isnull(sum(abs(M.total_creditos)), 0) as creditos
            from CntX_Mov_Cuentas_Detallado M
            inner join CntX_Cuentas C
                on M.cod_contabilidad = C.cod_contabilidad
               and M.cod_cuenta = C.cod_cuenta
               and C.cuenta_madre = ''
            where M.cod_contabilidad = @CodConta
              and M.anio = @Anio
              and M.mes = @Mes
              and (@Unidad = '' or M.cod_unidad = @Unidad);";

            var data = DbHelper.WithConn(_portalDB, codEmpresa, conn =>
                conn.QueryFirstOrDefault<CntXBalanceCuadradoData>(
                    sql,
                    new
                    {
                        CodConta = codConta,
                        Anio = pAnio,
                        Mes = pMes,
                        Unidad = pUnidad ?? string.Empty
                    }) ?? new CntXBalanceCuadradoData());

            if (data.Code != 0)
            {
                throw new InvalidOperationException(data.Description ?? "No fue posible validar si el balance est&aacute; cuadrado.");
            }

            decimal debitos = data.Result?.debitos ?? 0;
            decimal creditos = data.Result?.creditos ?? 0;

            return debitos - creditos == 0;
        }

        public ErrorDto SbCntX_RestructuraMovimientosRSM(int codEmpresa, CntXCalculosRestructuraRequest request)
        {
            return DbHelper.ExecuteNonQuery(
                _portalDB,
                codEmpresa,
                "exec spCntX_BalanceRestructura @Contabilidad, @Anio, @Mes, @RevisionTotal",
                new
                {
                    Contabilidad = request.cod_contabilidad,
                    Anio = request.anio,
                    Mes = request.mes,
                    RevisionTotal = request.revision_total
                });
        }

        public ErrorDto<CntXCalculosUtilidadDto> SbCntX_Utilidad(int codEmpresa, int codConta, int pAnio, int pMes)
        {
            var result = DbHelper.WithConn(_portalDB, codEmpresa, conn =>
                conn.QueryFirstOrDefault<CntXCalculosUtilidadDto>(
                    "exec spCntX_EstadoUtilidad @Contabilidad, @Anio, @Mes",
                    new
                    {
                        Contabilidad = codConta,
                        Anio = pAnio,
                        Mes = pMes
                    }) ?? new CntXCalculosUtilidadDto());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new CntXCalculosUtilidadDto())
                : DbHelper.CreateErrorResponse<CntXCalculosUtilidadDto>(
                    result.Description ?? "No fue posible obtener la utilidad del periodo.",
                    result.Code ?? -1,
                    new CntXCalculosUtilidadDto());
        }

        private bool FxExisteMovimiento(int codEmpresa, int codConta, int pAnio, int pMes, string pCuenta, string pUnidad, string pCentroCosto = "")
        {
            string sql = @"
                select isnull(count(*), 0)
                from CntX_Mov_Cuentas_Detallado
                where cod_contabilidad = @CodConta
                  and anio = @Anio
                  and mes = @Mes
                  and cod_cuenta = @Cuenta
                  and cod_unidad = @Unidad
                  and cod_centro_costo = @CentroCosto;";

            int existe = DbHelper.ExecuteSingleQuery<int>(
                _portalDB,
                codEmpresa,
                sql,
                0,
                new
                {
                    CodConta = codConta,
                    Anio = pAnio,
                    Mes = pMes,
                    Cuenta = pCuenta,
                    Unidad = pUnidad,
                    CentroCosto = pCentroCosto ?? string.Empty
                }).Result;

            return existe > 0;
        }

        private ErrorDto SbCntX_Asiento_GuardaMovimiento(
            int codEmpresa,
            CntXAsientoMovimientoData movimiento)
        {
            return DbHelper.ExecuteNonQuery(
                _portalDB,
                codEmpresa,
                @"exec spCntX_AsientoGuardaMov
                    @Contabilidad,
                    @Anio,
                    @Mes,
                    @Cuenta,
                    @Debito,
                    @Credito,
                    @Unidad,
                    @CentroCosto,
                    @Divisa,
                    @TipoCambio",
                new
                {
                    Contabilidad = movimiento.cod_contabilidad,
                    Anio = movimiento.anio,
                    Mes = movimiento.mes,
                    Cuenta = movimiento.cuenta,
                    Debito = movimiento.debito,
                    Credito = movimiento.credito,
                    Unidad = movimiento.unidad,
                    CentroCosto = movimiento.centro_costo ?? string.Empty,
                    Divisa = movimiento.divisa,
                    TipoCambio = movimiento.tipo_cambio
                });
        }

        public ErrorDto SbCntX_Asiento_Mayorizar(int codEmpresa, CntXCalculosAsientoProcesoRequest request)
        {
            string sqlValida = @"
                select top 1 count(*)
                from Cntx_Asientos Asi
                left join Cntx_Asientos_Detalle Ad
                    on Asi.Cod_Contabilidad = Ad.Cod_Contabilidad
                   and Asi.Tipo_Asiento = Ad.Tipo_Asiento
                   and Asi.Num_Asiento = Ad.Num_Asiento
                where Asi.cod_contabilidad = @CodConta
                  and Asi.tipo_asiento = @TipoAsiento
                  and Asi.num_asiento = @NumAsiento
                  and Asi.fecha_aplicado is null
                  and Asi.balanceado = 'S'
                group by Asi.Tipo_Asiento,
                     Asi.Num_Asiento,
                     Asi.Fecha_Asiento,
                     Asi.TS
                having sum(Ad.Monto_Debito) - sum(Ad.Monto_Credito) = 0;";

            var existe = DbHelper.WithConn(_portalDB, codEmpresa, conn =>
                conn.QueryFirstOrDefault<int?>(
                    sqlValida,
                    new
                    {
                        CodConta = request.cod_contabilidad,
                        TipoAsiento = request.tipo_asiento,
                        NumAsiento = request.num_asiento
                    }) ?? 0);

            if (existe.Code != 0)
            {
                return DbHelper.ErrorResponse(existe.Description ?? "No fue posible validar el asiento.");
            }

            if (existe.Result <= 0)
            {
                return DbHelper.ErrorResponse("No se encontr&oacute; un asiento pendiente y balanceado para mayorizar.", -2);
            }

            var result = DbHelper.ExecuteNonQuery(
                _portalDB,
                codEmpresa,
                "exec spCntX_AsientoMayoriza @Contabilidad, @Usuario, @Tipo, @Numero",
                new
                {
                    Contabilidad = request.cod_contabilidad,
                    Usuario = request.usuario,
                    Tipo = request.tipo_asiento,
                    Numero = request.num_asiento
                });

            return result;
        }

        public ErrorDto SbCntX_Asiento_Reversion(int codEmpresa, CntXCalculosAsientoProcesoRequest request)
        {
            var result = DbHelper.ExecuteNonQuery(
                _portalDB,
                codEmpresa,
                "exec spCntX_AsientoReversa @Contabilidad, @Usuario, @Tipo, @Numero",
                new
                {
                    Contabilidad = request.cod_contabilidad,
                    Usuario = request.usuario,
                    Tipo = request.tipo_asiento,
                    Numero = request.num_asiento
                });

            return result;
        }

        public ErrorDto SbCntX_AsientoBorra(
            int codEmpresa,
            CntXCalculosAsientoBorraRequest request)
        {
            const string sqlPeriodo = @"
            select isnull(count(*), 0)
            from CntX_Periodos
            where cod_contabilidad = @CodConta
              and anio = @Anio
              and mes = @Mes
              and estado = 'P';";

            var consultaPeriodo = DbHelper.WithConn(
                _portalDB,
                codEmpresa,
                conn => conn.QueryFirstOrDefault<int?>(
                    sqlPeriodo,
                    new
                    {
                        CodConta = request.cod_contabilidad,
                        Anio = request.anio,
                        Mes = request.mes
                    }));

            if (consultaPeriodo.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    consultaPeriodo.Description ??
                    "No fue posible consultar el estado del periodo.",
                    consultaPeriodo.Code ?? -1);
            }

            if ((consultaPeriodo.Result ?? 0) == 0)
            {
                return DbHelper.ErrorResponse(
                    "El asiento no puede ser borrado porque el periodo ya fue cerrado.",
                    -2);
            }

            const string sqlAsiento = @"
            select top 1
                fecha_aplicado,
                isnull(modulo, '') as modulo,
                fecha_autoriza
            from Cntx_Asientos
            where cod_contabilidad = @CodConta
              and tipo_asiento = @TipoAsiento
              and num_asiento = @NumAsiento;";

            var consultaAsiento = DbHelper.WithConn(
                _portalDB,
                codEmpresa,
                conn => conn.QueryFirstOrDefault<CntXCalculosAsientoBorraData>(
                    sqlAsiento,
                    new
                    {
                        CodConta = request.cod_contabilidad,
                        TipoAsiento = request.tipo_asiento,
                        NumAsiento = request.num_asiento
                    }));

            if (consultaAsiento.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    consultaAsiento.Description ??
                    "No fue posible consultar el asiento.",
                    consultaAsiento.Code ?? -1);
            }

            CntXCalculosAsientoBorraData? asiento = consultaAsiento.Result;

            if (asiento is null)
            {
                return DbHelper.ErrorResponse(
                    "No se encontr&oacute; el asiento indicado.",
                    -2);
            }

            if (!string.Equals(
                    asiento.modulo.Trim(),
                    "20",
                    StringComparison.OrdinalIgnoreCase) &&
                asiento.fecha_autoriza is null)
            {
                return DbHelper.ErrorResponse(
                    "No se pueden borrar asientos for&aacute;neos, solo se pueden modificar.",
                    -2);
            }

            if (asiento.fecha_aplicado.HasValue)
            {
                var reversa = SbCntX_Asiento_Reversion(
                    codEmpresa,
                    new CntXCalculosAsientoProcesoRequest
                    {
                        cod_contabilidad = request.cod_contabilidad,
                        usuario = request.usuario,
                        tipo_asiento = request.tipo_asiento,
                        num_asiento = request.num_asiento
                    });

                if (reversa.Code != 0)
                {
                    return reversa;
                }
            }

            var parametros = new
            {
                CodConta = request.cod_contabilidad,
                TipoAsiento = request.tipo_asiento,
                NumAsiento = request.num_asiento
            };

            var deleteDetalle = DbHelper.ExecuteNonQuery(
                _portalDB,
                codEmpresa,
                @"delete Cntx_Asientos_Detalle
              where cod_contabilidad = @CodConta
                and tipo_asiento = @TipoAsiento
                and num_asiento = @NumAsiento",
                parametros);

            if (deleteDetalle.Code != 0)
            {
                return deleteDetalle;
            }

            var deleteAsiento = DbHelper.ExecuteNonQuery(
                _portalDB,
                codEmpresa,
                @"delete Cntx_Asientos
              where cod_contabilidad = @CodConta
                and tipo_asiento = @TipoAsiento
                and num_asiento = @NumAsiento",
                parametros);

            if (deleteAsiento.Code != 0)
            {
                return deleteAsiento;
            }

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Elimina - WEB",
                $"Asiento : {request.tipo_asiento}-{request.num_asiento} " +
                $"Conta.{request.cod_contabilidad}");

            return deleteAsiento;
        }

        private string FxCuentaFinalRng(int codEmpresa, int codConta, string vCuentaI, string vCuentaC)
        {
            string sql = "select dbo.fxCntX_CuentaFinalRango(@CodConta, @CuentaInicio, @CuentaCorte) as Cuenta;";

            return DbHelper.ExecuteSingleQuery<string>(
                _portalDB,
                codEmpresa,
                sql,
                string.Empty,
                new
                {
                    CodConta = codConta,
                    CuentaInicio = vCuentaI,
                    CuentaCorte = vCuentaC
                }).Result ?? string.Empty;
        }

        public ErrorDto SbCntX_MovimientoCuentas(int codEmpresa, CntXCalculosMovimientoCuentasRequest request)
        {
            string cuentaCorte = FxCuentaFinalRng(
                codEmpresa,
                request.cod_contabilidad,
                request.cuenta_inicio ?? string.Empty,
                request.cuenta_corte ?? string.Empty);

            return DbHelper.ExecuteNonQuery(
                _portalDB,
                codEmpresa,
                @"exec spCntX_Analitico_Rsm
                    @Contabilidad,
                    @Usuario,
                    @FechaInicio,
                    @FechaCorte,
                    @CuentaInicio,
                    @CuentaCorte,
                    @MovimientoEnCero,
                    @Unidad,
                    @CentroCosto,
                    @DivisaOrigen,
                    @Pendientes",
                new
                {
                    Contabilidad = request.cod_contabilidad,
                    Usuario = request.usuario,
                    FechaInicio = request.fecha_desde.Date,
                    FechaCorte = request.fecha_hasta.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                    CuentaInicio = request.cuenta_inicio ?? string.Empty,
                    CuentaCorte = cuentaCorte,
                    MovimientoEnCero = request.mov_en_cero,
                    Unidad = request.unidad ?? "0x0",
                    CentroCosto = request.centro_costo ?? "0x0",
                    DivisaOrigen = request.divisa_origen,
                    Pendientes = request.pendientes
                });
        }

        public ErrorDto SbCntX_PeriodoCierre(int codEmpresa, CntXCalculosPeriodoProcesoRequest request)
        {
            return DbHelper.ExecuteNonQuery(
                _portalDB,
                codEmpresa,
                "exec spCntX_Periodo_Cierre @Contabilidad, @Anio, @Mes, @Usuario",
                new
                {
                    Contabilidad = request.cod_contabilidad,
                    Anio = request.anio,
                    Mes = request.mes,
                    Usuario = request.usuario
                });
        }

        public ErrorDto SbCntX_CierreFiscal(int codEmpresa, CntXCalculosPeriodoProcesoRequest request)
        {
            return DbHelper.ExecuteNonQuery(
                _portalDB,
                codEmpresa,
                "exec spCntX_Cierre_Fiscal_Asientos @Contabilidad, @Anio, @Mes, @Usuario",
                new
                {
                    Contabilidad = request.cod_contabilidad,
                    Anio = request.anio,
                    Mes = request.mes,
                    Usuario = request.usuario
                });
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _mSecurityMain.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private static string FxTsToHex(byte[]? ts)
        {
            return ts is null || ts.Length == 0
                ? string.Empty
                : "0x" + Convert.ToHexString(ts);
        }
    }
}