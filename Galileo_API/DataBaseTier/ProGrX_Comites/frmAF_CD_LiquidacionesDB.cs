using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Comites;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdLiquidacionesDb
    {
        private readonly PortalDB _portalDb;
        private readonly MRecibos _mRecibos;
        private readonly MProGrxMain _mProGrx;

        public FrmAfCdLiquidacionesDb(IConfiguration config)
            : this(new PortalDB(config), new MRecibos(config), new MProGrxMain(config))
        {
        }

        public FrmAfCdLiquidacionesDb(PortalDB portalDb, MRecibos mRecibos, MProGrxMain mProGrx)
        {
            _portalDb = portalDb;
            _mRecibos = mRecibos;
            _mProGrx = mProGrx;
        }

        /// <summary>
        /// Obtiene lista de comites
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AfCdComites_Lista_Obtener(int codEmpresa)
        {
            string query = @"select C.COD_COMITE as item,CM.DESCRIPCION from AFI_CD_CUENTAS C 
            inner join AFI_CD_COMITES CM on c.COD_COMITE = CM.COD_COMITE and C.ESTADO ='T' ";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene descripción de un comite especifico
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codComite"></param>
        /// <returns></returns>
        public ErrorDto<string?> AfCdComite_Descripcion_Obtener(int codEmpresa, string codComite)
        {
            string query = @"select DESCRIPCION from AFI_CD_COMITES where COD_COMITE = @Comite";
            return DbHelper.ExecuteSingleQuery<string>(_portalDb, codEmpresa, query, null,
                new { Comite = codComite });
        }

        /// <summary>
        /// Obtiene número de liquidaciones pendientes para un comite especifico
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codComite"></param>
        /// <returns></returns>
        public ErrorDto<int> AfCdLiquidaciones_Pendientes_Obtener(int codEmpresa, string codComite)
        {
            string query = @"select count(COD_COMITE)as Cuenta 
            from AFI_CD_CUENTAS where estado='T' and COD_COMITE= @Comite";
            return DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, query, 0,
                new { Comite = codComite });
        }

        /// <summary>
        /// Obtiene lista de operaciones para un comite especifico
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codComite"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCdOperacionData>> AfCdOperaciones_Lista_Obtener(int codEmpresa, string codComite)
        {
            string query = @"select 
                C.NOPERACION as noperacion,
                C.ACTIVA_FECHA as activa_fecha,
                DATEDIFF(DAY, C.ACTIVA_FECHA, GETDATE()) as dias_pendientes,
                ISNULL(CA.MONTO, 0) as monto,
                A.DESCRIPCION as actividad,
                case C.ESTADO
                    when 'T' then 'Trasladado'
                    when 'A' then 'Activo'
                    else 'Liquidado'
                end as estado,
                case C.TIPO
                    when 'T' then 'Transferencia'
                    else 'Cheque'
                end as desembolso,
                C.REGISTRO_USUARIO as registro_usuario,
                Tes.FECHA_EMISION as fecha_emision
            from dbo.AFI_CD_CUENTAS C
            inner join AFI_CD_CUENTAS_ACTIVIDADES CA 
                on C.NOPERACION = CA.NOPERACION
            inner join AFI_CD_ACTIVIDADES A 
                on CA.COD_ACTIVIDAD = A.COD_ACTIVIDAD
            left join TES_TRANSACCIONES Tes 
                on C.TESORERIA_NSOLICITUD = Tes.NSOLICITUD
            where C.COD_COMITE = @Comite
              and C.ESTADO = 'T'
              and C.PROCESO = 'T'";
            return DbHelper.ExecuteListQuery<AfCdOperacionData>(_portalDb, codEmpresa, query, 
                new { Comite = codComite });
        }

        /// <summary>
        /// Obtiene lista de operaciones detalladas para un comite especifico
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codComite"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCdOperacionData>> AfCdOperaciones_Detallar_Obtener(int codEmpresa, string codComite)
        {
            string query = @"select 
                C.NOPERACION as noperacion,
                datediff(day, C.REGISTRO_FECHA, getdate()) as dias_pendientes,
                sum(CA.MONTO) as monto
            from dbo.AFI_CD_CUENTAS C
            inner join AFI_CD_CUENTAS_ACTIVIDADES CA 
                on C.NOPERACION = CA.NOPERACION
            where C.COD_COMITE = @Comite
              and C.ESTADO = 'T'
              and C.PROCESO = 'D'
            group by C.NOPERACION, C.REGISTRO_FECHA
            order by C.NOPERACION desc";

            return DbHelper.ExecuteListQuery<AfCdOperacionData>(
                _portalDb,
                codEmpresa,
                query,
                new { Comite = codComite }
            );
        }

        /// <summary>
        /// Obtiene historial de operaciones para un comite especifico
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codComite"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCdOperacionHistoricoData>> AfCdOperaciones_Historico_Obtener(int codEmpresa, string codComite)
        {
            string query = @"select 
                C.NOPERACION as operacion,
                C.NOTAS as notas,
                C.LIQUIDA_FECHA as liquida_fecha,
                C.ACTIVA_FECHA as activa_fecha,
                Tes.FECHA_EMISION as fecha_emision,
                CA.MONTO as monto,
                A.DESCRIPCION as actividad,
                C.TESORERIA_FECHA as tesoreria_fecha,
                C.TESORERIA_NSOLICITUD as tesoreria_nsolicitud,
                case C.ESTADO 
                    when 'T' then 'Trasladado' 
                    when 'A' then 'Activo' 
                    else 'Liquidado' 
                end as estado,
                case C.APRUEBA 
                    when 'J' then 'Junta Directiva' 
                    when 'O' then 'Oficina Regional' 
                    else 'Director Zona' 
                end as aprueba,
                case C.TIPO 
                    when 'T' then 'Transferencia' 
                    else 'Cheque' 
                end as desembolso,
                C.REGISTRO_FECHA as registro_fecha,
                C.REGISTRO_USUARIO as registro_usuario,
                Tes.Beneficiario as tesoreria_beneficiario,
                Tes.Codigo as tesoreria_codigo
            from dbo.AFI_CD_CUENTAS C
            inner join AFI_CD_CUENTAS_ACTIVIDADES CA 
                on C.NOPERACION = CA.NOPERACION
            inner join AFI_CD_ACTIVIDADES A 
                on CA.COD_ACTIVIDAD = A.COD_ACTIVIDAD
            left join TES_TRANSACCIONES Tes 
                on C.TESORERIA_NSOLICITUD = Tes.NSOLICITUD
            where C.COD_COMITE = @Comite
            order by C.REGISTRO_FECHA desc";

            return DbHelper.ExecuteListQuery<AfCdOperacionHistoricoData>(
                _portalDb, codEmpresa, query,
                new { Comite = codComite }
            );
        }

        /// <summary>
        /// Obtiene lista de facturas para una operacion especifica
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCdFacturaData>> AfCdFacturas_Obtener(int codEmpresa, int operacion)
        {
            string query = @"select 
                NOPERACION, 
                CAST(CASE WHEN LTRIM(RTRIM(ISNULL(DEPOSITO, '0'))) = '1' THEN 1 ELSE 0 END AS bit) as deposito,
                NDOCUMENTO as ndocumento,
                FECHA_DOCUMENTO as fecha_documento,
                DETALLE as detalle,
                MONTO as monto
            from AFI_CD_DETALLE_LIQUIDACION
            where NOPERACION = @Operacion";

            return DbHelper.ExecuteListQuery<AfCdFacturaData>(
                _portalDb,
                codEmpresa,
                query,
                new { Operacion = operacion }
            );
        }

        /// <summary>
        /// Guarda el detalle de una liquidacion (factura)
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AfCdDetalleLiquidacion_Guardar(int codEmpresa, string usuario, AfCdFacturaData request)
        {
            request.ndocumento = request.ndocumento?.Trim() ?? string.Empty;
            request.detalle = request.detalle?.Trim() ?? string.Empty;

            var existe = ExisteFactura(codEmpresa, request.noperacion, request.ndocumento);

            var resp = existe
                ? ActualizarFactura(codEmpresa, usuario, request)
                : InsertarFactura(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = "Factura guardada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Elimina una factura de la liquidacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="documento"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AfCdDetalleLiquidacion_Eliminar(int codEmpresa, int operacion, string documento, string usuario)
        {
            const string sqlDelete = @"
            DELETE FROM AFI_CD_DETALLE_LIQUIDACION
            WHERE NOPERACION = @NOperacion
              AND NDOCUMENTO = @NDocumento;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    NOperacion = operacion,
                    NDocumento = documento.Trim()
                }
            );
        }

        /// <summary>
        /// Obtiene los montos de una liquidacion especifica
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<AfCdDetalleLiquidacionMontosData> AfCdDetalleLiquidacion_Montos_Obtener(int codEmpresa, int operacion)
        {
            const string sqlMontoTotal = @"
                SELECT ISNULL(Monto, 0)
                FROM AFI_CD_CUENTAS
                WHERE NOPERACION = @NOperacion;";

            const string sqlMontoFacturas = @"
                SELECT ISNULL(SUM(MONTO), 0)
                FROM AFI_CD_DETALLE_LIQUIDACION
                WHERE NOPERACION = @NOperacion;";

            var montoTotalResp = DbHelper.ExecuteSingleQuery<decimal>(
                _portalDb,  codEmpresa, sqlMontoTotal, 0m,
                new
                {
                    NOperacion = operacion
                }
            );

            if (montoTotalResp.Code < 0)
            {
                return new ErrorDto<AfCdDetalleLiquidacionMontosData>
                {
                    Code = montoTotalResp.Code,
                    Description = montoTotalResp.Description,
                    Result = null
                };
            }

            var montoFacturasResp = DbHelper.ExecuteSingleQuery<decimal>(
                _portalDb, codEmpresa, sqlMontoFacturas, 0m,
                new
                {
                    NOperacion = operacion
                }
            );

            if (montoFacturasResp.Code < 0)
            {
                return new ErrorDto<AfCdDetalleLiquidacionMontosData>
                {
                    Code = montoFacturasResp.Code,
                    Description = montoFacturasResp.Description,
                    Result = null
                };
            }

            var montoTotal = montoTotalResp.Result;
            var montoFacturas = montoFacturasResp.Result;
            var saldo = montoTotal - montoFacturas;

            return new ErrorDto<AfCdDetalleLiquidacionMontosData>
            {
                Code = 0,
                Description = "Ok",
                Result = new AfCdDetalleLiquidacionMontosData
                {
                    total = montoTotal,
                    totalFactura = montoFacturas,
                    diferencia = saldo
                }
            };
        }

        /// <summary>
        /// Aplica la liquidacion de una operacion especifica
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto AfCdLiquidacion_Detallar(int codEmpresa, int operacion)
        {
            const string sqlLiqDet = @"update afi_cd_cuentas set PROCESO = 'D' 
                where NOPERACION = @NOperacion;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlLiqDet,
                new
                {
                    NOperacion = operacion
                }
            );
        }

        /// <summary>
        /// Liquida una operacion especifica
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="usuario"></param>
        /// <param name="notas"></param>
        /// <returns></returns>
        public ErrorDto<object> AfCdLiquidacionOperacion_Liquidar(int codEmpresa, int operacion, string usuario, string notas)
        {
            string gOficinaTitular = _mProGrx.sbSifParametrosInicializa(codEmpresa, usuario).Result.GOficinaTitular ?? "";
            const string sqlLiq = @"exec spAFI_CD_AsientoLiquidacion @NOperacion, @Usuario, @Oficina, @Notas;";

            var resp = DbHelper.ExecuteSingleQuery<dynamic>(
                _portalDb, codEmpresa, sqlLiq, null,
                new
                {
                    NOperacion = operacion,
                    Usuario = usuario,
                    Oficina = gOficinaTitular,
                    Notas = notas
                }
            );
            if (resp.Code < 0)
            {
                return new ErrorDto<object>
                {
                    Code = -1,
                    Description = resp.Description,
                    Result = null
                };
            }
            var data = resp.Result;
            if (data == null)
            {
                return new ErrorDto<object>
                {
                    Code = -1,
                    Description = "Error al liquidar, no se encontraron registros.",
                    Result = null
                };
            }
            return DbHelper.CreateOkResponse<object>(data);
        }

        /// <summary>
        /// Imprime el recibo historico de una liquidacion especifica
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="col"></param>
        /// <param name="opRef"></param>
        /// <param name="codigoComite"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<object> AfCdLiquidacion_Historico_Imprimir(int codEmpresa, int col, string opRef, string codigoComite, string usuario)
        {
            string vTipoDoc;
            string vNumDoc = opRef?.Trim() ?? string.Empty;

            if (col == 1)
            {
                vTipoDoc = "CD.CxC";
            }
            else
            {
                vTipoDoc = "CD.Liq";

                const string sql = @"
                    SELECT cod_Transaccion
                    FROM sif_transacciones
                    WHERE Tipo_Documento = @TipoDocumento
                      AND Referencia_01 = @Referencia01
                      AND Referencia_02 = @Referencia02;";

                var resp = DbHelper.ExecuteSingleQuery<string>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    string.Empty,
                    new
                    {
                        TipoDocumento = vTipoDoc,
                        Referencia01 = codigoComite?.Trim() ?? string.Empty,
                        Referencia02 = opRef?.Trim() ?? string.Empty
                    }
                );

                if (resp.Code < 0)
                {
                    return new ErrorDto<object>
                    {
                        Code = -1,
                        Description = resp.Description,
                        Result = null
                    };
                }

                if (!string.IsNullOrWhiteSpace(resp.Result))
                {
                    vNumDoc = resp.Result.Trim();
                }
            }
            return sbImprimeRecibo(codEmpresa, vNumDoc, vTipoDoc, usuario);
        }

        /// <summary>
        /// Actualiza el saldo de una liquidacion especifica
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto AfCdLiquidacion_Saldo_Actualizar(int codEmpresa, int operacion)
        {
            const string sqlSaldo = @"
            SELECT ISNULL(C.Monto - SUM(DL.MONTO), 0) AS Saldo
            FROM dbo.AFI_CD_CUENTAS C
            INNER JOIN AFI_CD_DETALLE_LIQUIDACION DL
                ON C.NOPERACION = DL.NOPERACION
            WHERE C.NOPERACION = @NOperacion
            GROUP BY C.Monto;";

            var saldoResp = DbHelper.ExecuteSingleQuery<decimal>(
                _portalDb, codEmpresa, sqlSaldo, 0m,
                new
                {
                    NOperacion = operacion
                }
            );

            if (saldoResp.Code < 0)
            {
                return new ErrorDto
                {
                    Code = saldoResp.Code,
                    Description = saldoResp.Description
                };
            }

            const string sqlUpdate = @"
            UPDATE AFI_CD_CUENTAS
            SET
                SALDO = @Saldo,
                ESTADO = 'L'
            WHERE NOPERACION = @NOperacion;";

            var updateResp = DbHelper.ExecuteNonQuery(
                _portalDb, codEmpresa, sqlUpdate,
                new
                {
                    Saldo = saldoResp.Result,
                    NOperacion = operacion
                }
            );

            if (updateResp.Code < 0)
            {
                return new ErrorDto
                {
                    Code = updateResp.Code,
                    Description = updateResp.Description
                };
            }

            return new ErrorDto
            {
                Code = 0,
                Description = "Saldo actualizado satisfactoriamente..."
            };
        }

        /// <summary>
        /// Actualiza la informacion de una factura 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto ActualizarFactura(int codEmpresa, string usuario, AfCdFacturaData request)
        {
            const string sqlUpdate = @"
            UPDATE AFI_CD_DETALLE_LIQUIDACION
            SET
                DETALLE = @Detalle,
                FECHA_DOCUMENTO = @FechaDocumento,
                MONTO = @Monto,
                REGISTRO_FECHA = GETDATE(),
                REGISTRO_USUARIO = @Usuario
            WHERE NOPERACION = @NOperacion
              AND NDOCUMENTO = @NDocumento;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    NOperacion = request.noperacion,
                    NDocumento = request.ndocumento,
                    Detalle = request.detalle,
                    FechaDocumento = request.fecha_documento,
                    Monto = request.monto,
                    Usuario = usuario
                }
            );
        }

        /// <summary>
        /// Agrega informacion de una nueva factura
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto InsertarFactura(int codEmpresa, string usuario, AfCdFacturaData request)
        {
            const string sqlInsert = @"
            INSERT INTO AFI_CD_DETALLE_LIQUIDACION
            (
                NOPERACION,
                NDOCUMENTO,
                DEPOSITO,
                DETALLE,
                FECHA_DOCUMENTO,
                MONTO,
                REGISTRO_FECHA,
                REGISTRO_USUARIO
            )
            VALUES
            (
                @NOperacion,
                @NDocumento,
                @Deposito,
                @Detalle,
                @FechaDocumento,
                @Monto,
                GETDATE(),
                @Usuario
            );";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    NOperacion = request.noperacion,
                    NDocumento = request.ndocumento,
                    Deposito = request.deposito ? "1" : "0",
                    Detalle = request.detalle,
                    FechaDocumento = request.fecha_documento,
                    Monto = request.monto,
                    Usuario = usuario
                }
            );
        }

        /// <summary>
        /// Valida si la factura existe
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="nOperacion"></param>
        /// <param name="nDocumento"></param>
        /// <returns></returns>
        private bool ExisteFactura(int codEmpresa, int nOperacion, string nDocumento)
        {
            const string sqlExiste = @"
            SELECT ISNULL(COUNT(*), 0)
            FROM AFI_CD_DETALLE_LIQUIDACION
            WHERE NOPERACION = @NOperacion
              AND NDOCUMENTO = @NDocumento;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    NOperacion = nOperacion,
                    NDocumento = nDocumento.Trim()
                }
            );

            return resp.Result > 0;
        }

        /// <summary>
        /// Llama a sbImprimeRecibo de mRecibos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pDocumento"></param>
        /// <param name="pTipo"></param>
        /// <param name="Usuario"></param>
        /// <param name="pReImprime"></param>
        /// <returns></returns>
        private ErrorDto<object> sbImprimeRecibo(int CodEmpresa, string pDocumento, string pTipo, string Usuario, bool pReImprime = false)
        {
            return _mRecibos.sbImprimeRecibo(CodEmpresa, pDocumento, pTipo, Usuario, pReImprime);
        }
    }
}
