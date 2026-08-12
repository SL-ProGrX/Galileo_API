using System.Data;
using System.Data.Common;
using System.Globalization;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.DataBaseTier;
using Galileo_API.Models.ProGrX.General;

namespace Galileo_API.DataBaseTier.ProGrX.General
{
    public class FrmCcAnomaliasDB
    {
        private const int ModuloGeneral = 10;
        private const string TipoDocumentoNc = "NC";
        private const string TipoDocumentoNd = "ND";
        private const string ConceptoSaldosMenores = "CRD007";
        private const string ConceptoSaldosNegativos = "CRD008";
        private const string ParametroMontoSaldosMenores = "23";
        private const string ParametroMontoMoraMenor = "24";
        private const string ParametroMontoCtaDerivada = "24.1";
        private const string DivisaCol = "COL";

        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrxMain;
        private readonly MRecibos _mRecibos;
        private readonly MCntLinkDB _mCntLinkDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmCcAnomaliasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrxMain = new MProGrxMain(config);
            _mRecibos = new MRecibos(config);
            _mCntLinkDb = new MCntLinkDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene operaciones activas con saldo menor o igual al monto indicado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<CcAnomaliaCreditoItemDto>> CcAnomaliasSaldosMenores_Obtener(int codEmpresa, CcAnomaliaFiltroDto filtro)
        {
            const string sql = @"
                SELECT
                    R.codigo AS Codigo,
                    R.id_solicitud AS Id_Solicitud,
                    R.cedula AS Cedula,
                    S.nombre AS Nombre,
                    R.saldo AS Saldo,
                    R.opex AS Opex,
                    R.proceso AS Proceso,
                    R.estado AS Estado,
                    R.estadosol AS Estadosol,
                    I.descripcion AS Institucion,
                    C.descripcion AS LineaDesc,
                    ISNULL(D.descripcion, '') AS Destino
                FROM reg_creditos R
                INNER JOIN catalogo C
                    ON R.codigo = C.codigo
                   AND C.retencion = 'N'
                   AND C.poliza = 'N'
                INNER JOIN socios S
                    ON R.cedula = S.cedula
                INNER JOIN instituciones I
                    ON S.cod_institucion = I.cod_institucion
                LEFT JOIN catalogo_destinos D
                    ON R.cod_destino = D.cod_destino
                WHERE R.estado = 'A'
                  AND R.proceso = 'N'
                  AND R.saldo BETWEEN 0 AND @Monto
                  AND R.cod_divisa = 'COL'
                  AND (@Linea IS NULL OR R.codigo = @Linea)
                  AND (@Destino IS NULL OR R.cod_destino = @Destino)
                  AND (@Institucion IS NULL OR S.cod_institucion = @Institucion)
                ORDER BY
                    R.codigo,
                    R.id_solicitud;";

            return DbHelper.ExecuteListQuery<CcAnomaliaCreditoItemDto>(
                _portalDb,
                codEmpresa,
                sql,
                CrearParametrosAnomalia(filtro));
        }

        /// <summary>
        /// Obtiene el catálogo de créditos para filtros de anomalías.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CcAnomaliasCreditos_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    CODIGO AS item,
                    DESCRIPCION AS descripcion
                FROM CATALOGO
                WHERE LINEA_INTERNA = 1
                  AND RETENCION = 'N'
                  AND POLIZA = 'N'
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene el catálogo de destinos para filtros de anomalías.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CcAnomaliasDestinos_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    COD_DESTINO AS item,
                    DESCRIPCION AS descripcion
                FROM CATALOGO_DESTINOS
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene el catálogo de instituciones para filtros de anomalías.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CcAnomaliasInstituciones_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    COD_INSTITUCION AS item,
                    DESCRIPCION AS descripcion
                FROM INSTITUCIONES
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene operaciones con saldo negativo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<CcAnomaliaCreditoItemDto>> CcAnomaliasSaldosNegativos_Obtener(int codEmpresa, CcAnomaliaFiltroDto filtro)
        {
            const string sql = @"
                SELECT
                    R.codigo AS Codigo,
                    R.id_solicitud AS Id_Solicitud,
                    R.cedula AS Cedula,
                    S.nombre AS Nombre,
                    R.saldo AS Saldo,
                    R.opex AS Opex,
                    R.proceso AS Proceso,
                    R.estado AS Estado,
                    R.estadosol AS Estadosol,
                    I.descripcion AS Institucion,
                    C.descripcion AS LineaDesc,
                    ISNULL(D.descripcion, '') AS Destino
                FROM reg_creditos R
                INNER JOIN catalogo C
                    ON R.codigo = C.codigo
                   AND C.retencion = 'N'
                   AND C.poliza = 'N'
                INNER JOIN socios S
                    ON R.cedula = S.cedula
                INNER JOIN instituciones I
                    ON S.cod_institucion = I.cod_institucion
                LEFT JOIN catalogo_destinos D
                    ON R.cod_destino = D.cod_destino
                WHERE R.estado IN ('A', 'C')
                  AND R.proceso = 'N'
                  AND R.saldo < 0
                  AND R.cod_divisa = 'COL'
                  AND (@Linea IS NULL OR R.codigo = @Linea)
                  AND (@Destino IS NULL OR R.cod_destino = @Destino)
                  AND (@Institucion IS NULL OR S.cod_institucion = @Institucion)
                ORDER BY
                    R.codigo,
                    R.id_solicitud;";

            return DbHelper.ExecuteListQuery<CcAnomaliaCreditoItemDto>(
                _portalDb,
                codEmpresa,
                sql,
                CrearParametrosAnomalia(filtro));
        }

        /// <summary>
        /// Obtiene operaciones con mora financiera menor o igual al monto indicado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<CcAnomaliaCreditoItemDto>> CcAnomaliasMoraMenor_Obtener(int codEmpresa, CcAnomaliaFiltroDto filtro)
        {
            const string sql = @"
                SELECT
                    R.codigo AS Codigo,
                    R.id_solicitud AS Id_Solicitud,
                    R.cedula AS Cedula,
                    S.nombre AS Nombre,
                    R.saldo AS Saldo,
                    R.opex AS Opex,
                    R.proceso AS Proceso,
                    R.estado AS Estado,
                    NULL AS Estadosol,
                    (
                        M.intc
                        + M.intm
                        + M.amortiza
                        + M.cargo
                    ) AS MoraFinanciera,
                    I.descripcion AS Institucion,
                    C.descripcion AS LineaDesc,
                    ISNULL(D.descripcion, '') AS Destino
                FROM reg_creditos R
                INNER JOIN catalogo C
                    ON R.codigo = C.codigo
                   AND C.retencion = 'N'
                   AND C.poliza = 'N'
                INNER JOIN morosidad M
                    ON R.id_solicitud = M.id_solicitud
                   AND M.estado = 'A'
                   AND R.cod_divisa = 'COL'
                INNER JOIN socios S
                    ON R.cedula = S.cedula
                INNER JOIN instituciones I
                    ON S.cod_institucion = I.cod_institucion
                LEFT JOIN catalogo_destinos D
                    ON R.cod_destino = D.cod_destino
                WHERE R.estado = 'A'
                  AND R.proceso <> 'J'
                  AND (
                        M.intc
                        + M.intm
                        + M.amortiza
                        + M.cargo
                      ) BETWEEN 0 AND @Monto
                  AND (@Linea IS NULL OR R.codigo = @Linea)
                  AND (@Destino IS NULL OR R.cod_destino = @Destino)
                  AND (@Institucion IS NULL OR S.cod_institucion = @Institucion)
                ORDER BY
                    R.codigo,
                    R.id_solicitud;";

            return DbHelper.ExecuteListQuery<CcAnomaliaCreditoItemDto>(
                _portalDb,
                codEmpresa,
                sql,
                CrearParametrosAnomalia(filtro));
        }

        /// <summary>
        /// Obtiene operaciones con cuenta derivada menor al monto indicado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<CcAnomaliaCtaDerivadaItemDto>> CcAnomaliasCtaDerivadaMenor_Obtener(int codEmpresa, CcAnomaliaCtaDerivadaFiltroDto filtro)
        {
            // VB6 sbCtaDerivada_Consulta: fxCrdParametro("24.1"), default 50 como el SP.
            var mntCancela = ObtenerParametroDecimal(codEmpresa, ParametroMontoCtaDerivada);
            if (mntCancela <= 0)
            {
                mntCancela = filtro?.Monto > 0 ? filtro.Monto.Value : 50m;
            }

            const string sql = @"
                SELECT
                    V.id_solicitud AS Id_Solicitud,
                    V.codigo AS Codigo,
                    R.cedula AS Cedula,
                    S.nombre AS Nombre,
                    R.saldo AS Saldo,
                    V.num_cuota AS Num_Cuota,
                    (
                        V.intcor
                        + V.intmor
                        + V.cargos
                        + V.poliza
                        + V.principal
                    ) AS Monto,
                    C.descripcion AS Descripcion
                FROM crd_operacion_transac V
                INNER JOIN catalogo C
                    ON V.codigo = C.codigo
                   AND C.retencion = 'N'
                   AND C.poliza = 'N'
                   AND C.linea_interna = 1
                INNER JOIN reg_creditos R
                    ON V.id_solicitud = R.id_solicitud
                INNER JOIN crd_garantia_tipos GT
                    ON R.garantia = GT.garantia
                INNER JOIN socios S
                    ON R.cedula = S.cedula
                WHERE V.num_cuota_madre > 0
                  AND R.estado = 'A'
                  AND R.proceso <> 'J'
                  AND V.estado = 'A'
                  AND V.num_cuota <> 0
                  AND (
                        V.intcor
                        + V.intmor
                        + V.cargos
                        + V.poliza
                        + V.principal
                      ) < @Monto
                  AND R.cod_divisa = 'COL'
                ORDER BY
                    V.id_solicitud;";

            return DbHelper.ExecuteListQuery<CcAnomaliaCtaDerivadaItemDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { Monto = mntCancela });
        }

        /// <summary>
        /// Obtiene la cuenta del parámetro de créditos (equivalente VB6 fxCrdParametro) con máscara y descripción.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="parametro">Código en CRD_PARAMETROS (p. ej. "22").</param>
        /// <returns></returns>
        public ErrorDto<CcAnomaliaCuentaOpcionDto?> CcAnomaliasCuentaOpcion_Obtener(int codEmpresa, string parametro)
        {
            const string sql = @"
                SELECT TOP 1
                    RTRIM(P.valor) AS Cod_Cuenta,
                    ISNULL(NULLIF(RTRIM(C.cod_cuenta_mask), ''), RTRIM(P.valor)) AS Cuenta_Mask,
                    ISNULL(RTRIM(C.descripcion), '') AS Descripcion
                FROM CRD_PARAMETROS P
                LEFT JOIN vCNTX_CUENTAS_LOCAL C
                    ON C.cod_cuenta = REPLACE(RTRIM(P.valor), '-', '')
                WHERE P.cod_parametro = @Parametro;";

            return DbHelper.ExecuteSingleQuery<CcAnomaliaCuentaOpcionDto?>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new { Parametro = (parametro ?? string.Empty).Trim() });
        }

        /// <summary>
        /// Corrige saldos menores (VB6 sbCorrigeSaldoMenor): genera NC, aplica abonos y asientos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CcAnomaliaSaldosMenoresCorregirResultado> CcAnomaliasSaldosMenores_Corregir(
            int codEmpresa,
            CcAnomaliaSaldosMenoresCorregirRequest request)
        {
            if (request is null)
            {
                return CrearErrorCorregir("Debe indicar los datos de la corrección.");
            }

            var usuario = (request.Usuario ?? string.Empty).Trim();
            var cuentaUi = (request.Cuenta ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return CrearErrorCorregir("Debe indicar el usuario.");
            }

            if (string.IsNullOrWhiteSpace(cuentaUi))
            {
                return CrearErrorCorregir("Cuenta Contable no es válida, revisar!");
            }

            var pCuenta = _mCntLinkDb.fxgCntCuentaFormato(codEmpresa, false, cuentaUi, 0);
            if (string.IsNullOrWhiteSpace(pCuenta) || !_mCntLinkDb.fxgCntCuentaValida(codEmpresa, pCuenta))
            {
                return CrearErrorCorregir("Cuenta Contable no es válida, revisar!");
            }

            var globalesResp = _mProGrxMain.sbSifParametrosInicializa(codEmpresa, usuario);
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return CrearErrorCorregir(
                    globalesResp.Description ?? "No fue posible obtener Globales.");
            }

            var globales = globalesResp.Result;
            var pMonto = ObtenerParametroDecimal(codEmpresa, ParametroMontoSaldosMenores);
            if (pMonto <= 0)
            {
                return CrearErrorCorregir(
                    "No se encontró el monto del parámetro 23 (saldos menores).");
            }

            try
            {
                var lngNumero = _mRecibos.FxDocumentoConsecutivo(codEmpresa, TipoDocumentoNc);
                if (lngNumero <= 0)
                {
                    return CrearErrorCorregir("No fue posible obtener el consecutivo de la Nota de Crédito.");
                }

                using var conn = _portalDb.CreateConnection(codEmpresa);
                conn.Open();
                using var tx = conn.BeginTransaction();

                var oficina = ObtenerOficinaOmision(conn, tx);
                var creditos = ObtenerCreditosSaldosMenores(
                    conn,
                    tx,
                    new CcAnomaliaFiltroDto
                    {
                        Monto = pMonto,
                        Linea = request.Linea,
                        Destino = request.Destino,
                        Institucion = request.Institucion
                    });

                if (creditos.Count == 0)
                {
                    tx.Rollback();
                    return CrearErrorCorregir(
                        "No existen datos para Ajustar, consulte nuevamente.!!!");
                }

                InsertarDocumentoNc(
                    conn,
                    tx,
                    lngNumero,
                    usuario,
                    globales.GOficinaTitular ?? string.Empty,
                    pMonto);

                decimal curTotalSaldos = 0m;

                foreach (var credito in creditos)
                {
                    var saldoAbs = Math.Abs(credito.Saldo);
                    curTotalSaldos += saldoAbs;
                    AplicarCreditoSaldoMenor(
                        conn,
                        tx,
                        globales,
                        credito,
                        usuario,
                        lngNumero,
                        saldoAbs);
                }

                InsertarAsiento(
                    conn,
                    tx,
                    TipoDocumentoNc,
                    lngNumero,
                    curTotalSaldos,
                    "D",
                    DivisaCol,
                    1m,
                    globales.GEnlace,
                    oficina.Cod_Unidad,
                    oficina.Cod_Centro_Costo,
                    pCuenta,
                    string.Empty,
                    string.Empty);

                tx.Commit();

                _securityMainDb.Bitacora(
                    new BitacoraInsertarDto
                    {
                        EmpresaId = codEmpresa,
                        Usuario = usuario,
                        Movimiento = "Aplica",
                        DetalleMovimiento =
                            $"Elimina Saldos Menores a {pMonto}c :{curTotalSaldos}",
                        Modulo = ModuloGeneral
                    });

                return DbHelper.CreateOkResponse(
                    new CcAnomaliaSaldosMenoresCorregirResultado
                    {
                        Mensaje =
                            $"Corrección aplicada con Nota de Crédito #{lngNumero}. " +
                            $"Casos: {creditos.Count}. Total: {curTotalSaldos:N2}.",
                        Tipo_Documento = TipoDocumentoNc,
                        Numero_Documento = lngNumero,
                        Total_Corregido = curTotalSaldos,
                        Casos = creditos.Count
                    });
            }
            catch (DbException ex)
            {
                return CrearErrorCorregir(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return CrearErrorCorregir(ex.Message);
            }
        }

        /// <summary>
        /// Corrige saldos negativos (VB6 sbCorrigeSaldoNegativo): genera ND, anula abonos y asientos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CcAnomaliaSaldosNegativosCorregirResultado> CcAnomaliasSaldosNegativos_Corregir(
            int codEmpresa,
            CcAnomaliaSaldosNegativosCorregirRequest request)
        {
            if (request is null)
            {
                return CrearErrorCorregirNegativos("Debe indicar los datos de la corrección.");
            }

            var usuario = (request.Usuario ?? string.Empty).Trim();
            var cuentaUi = (request.Cuenta ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return CrearErrorCorregirNegativos("Debe indicar el usuario.");
            }

            if (string.IsNullOrWhiteSpace(cuentaUi))
            {
                return CrearErrorCorregirNegativos("Cuenta Contable no es válida, revisar!");
            }

            var pCuenta = _mCntLinkDb.fxgCntCuentaFormato(codEmpresa, false, cuentaUi, 0);
            if (string.IsNullOrWhiteSpace(pCuenta) || !_mCntLinkDb.fxgCntCuentaValida(codEmpresa, pCuenta))
            {
                return CrearErrorCorregirNegativos("Cuenta Contable no es válida, revisar!");
            }

            var globalesResp = _mProGrxMain.sbSifParametrosInicializa(codEmpresa, usuario);
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return CrearErrorCorregirNegativos(
                    globalesResp.Description ?? "No fue posible obtener Globales.");
            }

            var globales = globalesResp.Result;

            try
            {
                var lngNumero = _mRecibos.FxDocumentoConsecutivo(codEmpresa, TipoDocumentoNd);
                if (lngNumero <= 0)
                {
                    return CrearErrorCorregirNegativos(
                        "No fue posible obtener el consecutivo de la Nota de Débito.");
                }

                using var conn = _portalDb.CreateConnection(codEmpresa);
                conn.Open();
                using var tx = conn.BeginTransaction();

                var creditos = ObtenerCreditosSaldosNegativos(
                    conn,
                    tx,
                    new CcAnomaliaFiltroDto
                    {
                        Linea = request.Linea,
                        Destino = request.Destino,
                        Institucion = request.Institucion
                    });

                if (creditos.Count == 0)
                {
                    tx.Rollback();
                    return CrearErrorCorregirNegativos(
                        "No existen datos para Ajustar, consulte nuevamente.!!!");
                }

                InsertarDocumentoNd(
                    conn,
                    tx,
                    lngNumero,
                    usuario,
                    globales.GOficinaTitular ?? string.Empty);

                var vFecha = conn.ExecuteScalar<DateTime>(
                    "SELECT dbo.MyGetdate();",
                    transaction: tx);

                decimal curTotalSaldos = 0m;

                foreach (var credito in creditos)
                {
                    var saldoAbs = Math.Abs(credito.Saldo);
                    curTotalSaldos += saldoAbs;

                    if (globales.SysPlanPagos == 1)
                    {
                        conn.Execute(
                            @"exec spCrdPlanPagoAnulaAbono
                                @Operacion,
                                @Concepto,
                                @Usuario,
                                @TipoDoc,
                                @Documento,
                                1,
                                0,
                                0,
                                @Amortizacion,
                                0,
                                0,
                                @Fecha,
                                '';",
                            new
                            {
                                Operacion = credito.Id_Solicitud,
                                Concepto = ConceptoSaldosNegativos,
                                Usuario = usuario,
                                TipoDoc = TipoDocumentoNd,
                                Documento = lngNumero,
                                Amortizacion = saldoAbs,
                                Fecha = vFecha
                            },
                            tx);
                    }
                    else
                    {
                        // VB6: AMORTIZA = AMORTIZA + saldo (saldo negativo tal cual)
                        conn.Execute(
                            @"UPDATE reg_creditos
                                 SET SALDO = 0,
                                     AMORTIZA = AMORTIZA + @Saldo,
                                     estado = 'C'
                               WHERE id_solicitud = @Operacion;",
                            new
                            {
                                Operacion = credito.Id_Solicitud,
                                credito.Saldo
                            },
                            tx);

                        conn.Execute(
                            @"INSERT INTO CREDITOS_DT
                                (CODIGO, ID_SOLICITUD, CUOTA, ABONO, INTCP, AMORTIZA,
                                 FECHAS, FECHAP, TCON, NCON)
                              VALUES
                                (@Codigo, @Operacion, 0, @SaldoAbs, 0, @SaldoAbs,
                                 @Fecha, @FechaCredito, @TipoDoc, @Documento);",
                            new
                            {
                                credito.Codigo,
                                Operacion = credito.Id_Solicitud,
                                SaldoAbs = saldoAbs,
                                Fecha = vFecha,
                                FechaCredito = globales.GlngFechaCR,
                                TipoDoc = TipoDocumentoNd,
                                Documento = lngNumero
                            },
                            tx);
                    }

                    var cuentas = ObtenerCuentasOperacion(conn, tx, credito.Id_Solicitud);
                    InsertarAsiento(
                        conn,
                        tx,
                        TipoDocumentoNd,
                        lngNumero,
                        saldoAbs,
                        "D",
                        cuentas.Cod_Divisa,
                        1m,
                        globales.GEnlace,
                        cuentas.Cod_Unidad,
                        cuentas.Cod_Centro_Costo,
                        cuentas.CtaAmortiza,
                        credito.Id_Solicitud.ToString(),
                        credito.Codigo);
                }

                // Contrapartida general: Crédito con unidad/CC de Globales (gOficinaUnidad / gOficinaCentroCosto)
                InsertarAsiento(
                    conn,
                    tx,
                    TipoDocumentoNd,
                    lngNumero,
                    curTotalSaldos,
                    "C",
                    DivisaCol,
                    1m,
                    globales.GEnlace,
                    globales.GOficinaUnidad ?? string.Empty,
                    globales.GOficinaCentroCosto ?? string.Empty,
                    pCuenta,
                    string.Empty,
                    string.Empty);

                tx.Commit();

                _securityMainDb.Bitacora(
                    new BitacoraInsertarDto
                    {
                        EmpresaId = codEmpresa,
                        Usuario = usuario,
                        Movimiento = "Aplica",
                        DetalleMovimiento =
                            $"Anulacion a Saldos Negativos total:{curTotalSaldos}",
                        Modulo = ModuloGeneral
                    });

                return DbHelper.CreateOkResponse(
                    new CcAnomaliaSaldosNegativosCorregirResultado
                    {
                        Mensaje =
                            $"Saldos Negativos Anulados Satisfactoriamente con Nota de Debito #{lngNumero}",
                        Tipo_Documento = TipoDocumentoNd,
                        Numero_Documento = lngNumero,
                        Total_Corregido = curTotalSaldos,
                        Casos = creditos.Count
                    });
            }
            catch (DbException ex)
            {
                return CrearErrorCorregirNegativos(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return CrearErrorCorregirNegativos(ex.Message);
            }
        }

        /// <summary>
        /// Corrige mora menor (VB6 sbCorrigeMora): elimina MOROSIDAD_CARGOS y MOROSIDAD.
        /// Solo aplica cuando SysPlanPagos = 0.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CcAnomaliaMoraMenorCorregirResultado> CcAnomaliasMoraMenor_Corregir(
            int codEmpresa,
            CcAnomaliaMoraMenorCorregirRequest request)
        {
            if (request is null)
            {
                return CrearErrorCorregirMora("Debe indicar los datos de la corrección.");
            }

            var usuario = (request.Usuario ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return CrearErrorCorregirMora("Debe indicar el usuario.");
            }

            var globalesResp = _mProGrxMain.sbSifParametrosInicializa(codEmpresa, usuario);
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return CrearErrorCorregirMora(
                    globalesResp.Description ?? "No fue posible obtener Globales.");
            }

            if (globalesResp.Result.SysPlanPagos != 0)
            {
                return CrearErrorCorregirMora(
                    "Esta Opción No Aplica con el Modelo de Plan de Pagos!");
            }

            var pMonto = ObtenerParametroDecimal(codEmpresa, ParametroMontoMoraMenor);

            try
            {
                using var conn = _portalDb.CreateConnection(codEmpresa);
                conn.Open();
                using var tx = conn.BeginTransaction();

                var parametros = CrearParametrosAnomalia(
                    pMonto,
                    request.Linea,
                    request.Destino,
                    request.Institucion);

                const string sqlCargos = @"
                    DELETE Morosidad_Cargos
                    WHERE id_Moro IN
                    (
                        SELECT M.id_Moro
                        FROM reg_creditos R
                        INNER JOIN catalogo C
                            ON R.codigo = C.codigo
                           AND C.retencion = 'N'
                           AND C.poliza = 'N'
                        INNER JOIN Morosidad M
                            ON R.id_solicitud = M.id_solicitud
                           AND M.estado = 'A'
                        INNER JOIN Socios S
                            ON R.cedula = S.cedula
                        WHERE R.estado = 'A'
                          AND R.proceso <> 'J'
                          AND R.cod_Divisa = @Divisa
                          AND (M.intc + M.intm + M.amortiza) BETWEEN 0 AND @Monto
                          AND (@Linea IS NULL OR R.Codigo = @Linea)
                          AND (@Destino IS NULL OR R.cod_destino = @Destino)
                          AND (@Institucion IS NULL OR S.cod_institucion = @Institucion)
                    );";

                conn.Execute(sqlCargos, parametros, tx);

                const string sqlMora = @"
                    DELETE M
                    FROM reg_creditos R
                    INNER JOIN catalogo C
                        ON R.codigo = C.codigo
                       AND C.retencion = 'N'
                       AND C.poliza = 'N'
                    INNER JOIN Morosidad M
                        ON R.id_solicitud = M.id_solicitud
                       AND M.estado = 'A'
                    INNER JOIN Socios S
                        ON R.cedula = S.cedula
                    WHERE R.estado = 'A'
                      AND R.proceso <> 'J'
                      AND R.cod_Divisa = @Divisa
                      AND (M.intc + M.intm + M.amortiza) BETWEEN 0 AND @Monto
                      AND (@Linea IS NULL OR R.Codigo = @Linea)
                      AND (@Destino IS NULL OR R.cod_destino = @Destino)
                      AND (@Institucion IS NULL OR S.cod_institucion = @Institucion);";

                conn.Execute(sqlMora, parametros, tx);

                tx.Commit();

                return DbHelper.CreateOkResponse(
                    new CcAnomaliaMoraMenorCorregirResultado
                    {
                        Mensaje =
                            $"Mora Menor a {pMonto:0.00} Eliminada Satisfactoriamente...",
                        Monto_Limite = pMonto
                    });
            }
            catch (DbException ex)
            {
                return CrearErrorCorregirMora(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return CrearErrorCorregirMora(ex.Message);
            }
        }

        /// <summary>
        /// Corrige cta. derivada menor (VB6 sbCtaDerivada_Corrige).
        /// Delega toda la lógica a spSys_Creditos_Clean_Ctas_Menores.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CcAnomaliaCtaDerivadaCorregirResultado> CcAnomaliasCtaDerivadaMenor_Corregir(
            int codEmpresa,
            CcAnomaliaCtaDerivadaCorregirRequest request)
        {
            if (request is null)
            {
                return CrearErrorCorregirCtaDerivada("Debe indicar los datos de la corrección.");
            }

            var usuario = (request.Usuario ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return CrearErrorCorregirCtaDerivada("Debe indicar el usuario.");
            }

            try
            {
                using var conn = _portalDb.CreateConnection(codEmpresa);
                conn.Open();

                // VB6: Call ConectionExecute("exec spSys_Creditos_Clean_Ctas_Menores '" & Usuario & "'")
                // El SP retorna: SELECT @TipoDoc AS TipoDoc, @NumDoc AS NumDoc
                var spResult = conn.QueryFirstOrDefault<CcAnomaliaCtaDerivadaSpDto>(
                    "exec spSys_Creditos_Clean_Ctas_Menores @Usuario",
                    new { Usuario = usuario });

                if (spResult is null || string.IsNullOrWhiteSpace(spResult.NumDoc))
                {
                    return CrearErrorCorregirCtaDerivada(
                        "No fue posible aplicar las cuotas derivadas.");
                }

                var tipoDoc = (spResult.TipoDoc ?? string.Empty).Trim();
                var numDoc = spResult.NumDoc.Trim();

                return DbHelper.CreateOkResponse(
                    new CcAnomaliaCtaDerivadaCorregirResultado
                    {
                        Mensaje = "Cuotas Derivadas Aplicadas...",
                        Tipo_Documento = tipoDoc,
                        Numero_Documento = numDoc
                    });
            }
            catch (DbException ex)
            {
                return CrearErrorCorregirCtaDerivada(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return CrearErrorCorregirCtaDerivada(ex.Message);
            }
        }

        /// <summary>
        /// Lee el valor numérico de un parámetro de créditos (VB6 fxCrdParametro).
        /// </summary>
        private decimal ObtenerParametroDecimal(int codEmpresa, string codParametro)
        {
            const string sql = @"
                SELECT TOP 1 RTRIM(valor) AS valor
                FROM CRD_PARAMETROS
                WHERE cod_parametro = @Parametro;";

            var valor = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                sql,
                string.Empty,
                new { Parametro = codParametro }).Result;

            if (string.IsNullOrWhiteSpace(valor))
            {
                return 0m;
            }

            return decimal.TryParse(
                valor.Trim(),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var monto)
                ? monto
                : 0m;
        }

        /// <summary>
        /// Aplica el abono de un crédito en corrección de saldos menores.
        /// </summary>
        private static void AplicarCreditoSaldoMenor(
            IDbConnection conn,
            IDbTransaction tx,
            Globales globales,
            CcAnomaliaCreditoItemDto credito,
            string usuario,
            long lngNumero,
            decimal saldoAbs)
        {
            if (globales.SysPlanPagos == 1)
            {
                conn.Execute(
                    @"exec spCrdPlanPagoAbonoEC
                        @Operacion,@Concepto,@Usuario,@TipoDoc,@Documento,
                        0,0,@Saldo,0,dbo.MyGetdate(),'',1;",
                    new
                    {
                        Operacion = credito.Id_Solicitud,
                        Concepto = ConceptoSaldosMenores,
                        Usuario = usuario,
                        TipoDoc = TipoDocumentoNc,
                        Documento = lngNumero,
                        Saldo = saldoAbs
                    },
                    tx);
            }
            else
            {
                conn.Execute(
                    @"UPDATE reg_creditos
                         SET SALDO = 0,
                             AMORTIZA = AMORTIZA + @Saldo,
                             estado = 'C'
                       WHERE id_solicitud = @Operacion
                         AND estado = 'A';",
                    new
                    {
                        Operacion = credito.Id_Solicitud,
                        credito.Saldo
                    },
                    tx);

                conn.Execute(
                    @"INSERT INTO CREDITOS_DT
                        (CODIGO, ID_SOLICITUD, CUOTA, ABONO, INTCP, AMORTIZA,
                         FECHAS, FECHAP, TCON, NCON)
                      VALUES
                        (@Codigo, @Operacion, 0, @SaldoAbs, 0, @SaldoAbs,
                         dbo.MyGetdate(), @FechaCredito, @TipoDoc, @Documento);",
                    new
                    {
                        credito.Codigo,
                        Operacion = credito.Id_Solicitud,
                        SaldoAbs = saldoAbs,
                        FechaCredito = globales.GlngFechaCR,
                        TipoDoc = TipoDocumentoNc,
                        Documento = lngNumero
                    },
                    tx);
            }

            var cuentas = ObtenerCuentasOperacion(conn, tx, credito.Id_Solicitud);
            InsertarAsiento(
                conn,
                tx,
                TipoDocumentoNc,
                lngNumero,
                saldoAbs,
                "C",
                cuentas.Cod_Divisa,
                1m,
                globales.GEnlace,
                cuentas.Cod_Unidad,
                cuentas.Cod_Centro_Costo,
                cuentas.CtaAmortiza,
                credito.Id_Solicitud.ToString(),
                credito.Codigo);
        }

        /// <summary>
        /// Obtiene unidad y centro de costo de la oficina por omisión.
        /// </summary>
        private static CcAnomaliaOficinaOmisionDto ObtenerOficinaOmision(
            IDbConnection conn,
            IDbTransaction tx)
        {
            const string sql = @"
                SELECT TOP 1
                    ISNULL(RTRIM(cod_unidad), '') AS Cod_Unidad,
                    ISNULL(RTRIM(cod_centro_costo), '') AS Cod_Centro_Costo
                FROM sif_oficinas
                WHERE ESTADO = 1
                  AND OFICINA_OMISION = 1;";

            return conn.QueryFirstOrDefault<CcAnomaliaOficinaOmisionDto>(sql, transaction: tx)
                ?? new CcAnomaliaOficinaOmisionDto();
        }

        /// <summary>
        /// Consulta los créditos a corregir (no usa la grilla visible; replica el SELECT del VB6).
        /// </summary>
        private static List<CcAnomaliaCreditoItemDto> ObtenerCreditosSaldosMenores(
            IDbConnection conn,
            IDbTransaction tx,
            CcAnomaliaFiltroDto filtro)
        {
            const string sql = @"
                SELECT
                    R.codigo AS Codigo,
                    R.id_solicitud AS Id_Solicitud,
                    R.cedula AS Cedula,
                    R.saldo AS Saldo,
                    R.opex AS Opex,
                    R.proceso AS Proceso
                FROM reg_creditos R
                INNER JOIN Catalogo C
                    ON R.codigo = C.codigo
                INNER JOIN Socios S
                    ON R.cedula = S.cedula
                WHERE R.estado = 'A'
                  AND R.saldo BETWEEN 0 AND @Monto
                  AND R.proceso = 'N'
                  AND C.retencion = 'N'
                  AND C.poliza = 'N'
                  AND R.cod_Divisa = @Divisa
                  AND (@Linea IS NULL OR R.Codigo = @Linea)
                  AND (@Destino IS NULL OR R.cod_destino = @Destino)
                  AND (@Institucion IS NULL OR S.cod_institucion = @Institucion)
                ORDER BY R.codigo;";

            return conn.Query<CcAnomaliaCreditoItemDto>(
                sql,
                CrearParametrosAnomalia(filtro),
                tx).AsList();
        }

        /// <summary>
        /// Inserta el encabezado de la Nota de Crédito en SIF_TRANSACCIONES.
        /// </summary>
        private static void InsertarDocumentoNc(
            IDbConnection conn,
            IDbTransaction tx,
            long numeroDocumento,
            string usuario,
            string oficinaTitular,
            decimal montoLimite)
        {
            conn.Execute(
                @"INSERT INTO SIF_TRANSACCIONES
                    (COD_TRANSACCION, TIPO_DOCUMENTO, REGISTRO_FECHA, REGISTRO_USUARIO,
                     Cliente_IDENTIFICACION, CLIENTE_NOMBRE, cod_concepto, monto, estado,
                     Referencia_01, Referencia_02, Referencia_03, cod_oficina,
                     linea1, linea2, linea3, linea4, linea5, linea6, linea7, linea8, linea9, linea10,
                     detalle, documento)
                  VALUES
                    (@Documento, @Tipo, dbo.MyGetDate(), @Usuario,
                     '', @ClienteNombre, @Concepto, 0, 'P',
                     '', '', '', @Oficina,
                     '', @Linea2, '', '', '', '', '', '', @Linea9, '',
                     '', '');",
                new
                {
                    Documento = numeroDocumento,
                    Tipo = TipoDocumentoNc,
                    Usuario = usuario,
                    ClienteNombre = "APLICACIÓN GENERAL",
                    Concepto = ConceptoSaldosMenores,
                    Oficina = oficinaTitular,
                    Linea2 = $"Corrige Saldos Menor a :{montoLimite:0.##}",
                    Linea9 = $"Usuario           {usuario}"
                },
                tx);
        }

        /// <summary>
        /// Inserta el encabezado de la Nota de Débito en SIF_TRANSACCIONES.
        /// </summary>
        private static void InsertarDocumentoNd(
            IDbConnection conn,
            IDbTransaction tx,
            long numeroDocumento,
            string usuario,
            string oficinaTitular)
        {
            conn.Execute(
                @"INSERT INTO SIF_TRANSACCIONES
                    (COD_TRANSACCION, TIPO_DOCUMENTO, REGISTRO_FECHA, REGISTRO_USUARIO,
                     Cliente_IDENTIFICACION, CLIENTE_NOMBRE, cod_concepto, monto, estado,
                     Referencia_01, Referencia_02, Referencia_03, cod_oficina,
                     linea1, linea2, linea3, linea4, linea5, linea6, linea7, linea8, linea9, linea10,
                     detalle, documento)
                  VALUES
                    (@Documento, @Tipo, dbo.MyGetDate(), @Usuario,
                     '', @ClienteNombre, @Concepto, 0, 'P',
                     '', '', '', @Oficina,
                     '', @Linea2, '', '', '', '', '', '', @Linea9, '',
                     '', '');",
                new
                {
                    Documento = numeroDocumento,
                    Tipo = TipoDocumentoNd,
                    Usuario = usuario,
                    ClienteNombre = "APLICACIÓN GENERAL",
                    Concepto = ConceptoSaldosNegativos,
                    Oficina = oficinaTitular,
                    Linea2 = "Corrige Saldos Negativos",
                    Linea9 = $"Usuario           {usuario}"
                },
                tx);
        }

        /// <summary>
        /// Consulta los créditos con saldo negativo a corregir (réplica SELECT VB6 sbCorrigeSaldoNegativo).
        /// </summary>
        private static List<CcAnomaliaCreditoItemDto> ObtenerCreditosSaldosNegativos(
            IDbConnection conn,
            IDbTransaction tx,
            CcAnomaliaFiltroDto filtro)
        {
            const string sql = @"
                SELECT
                    R.codigo AS Codigo,
                    R.id_solicitud AS Id_Solicitud,
                    R.cedula AS Cedula,
                    R.saldo AS Saldo,
                    R.opex AS Opex,
                    R.proceso AS Proceso
                FROM reg_creditos R
                INNER JOIN Catalogo C
                    ON R.codigo = C.codigo
                   AND C.Poliza = 'N'
                   AND C.retencion = 'N'
                INNER JOIN Socios S
                    ON R.cedula = S.cedula
                WHERE R.estado IN ('A','C','N')
                  AND R.saldo < 0
                  AND C.retencion = 'N'
                  AND C.poliza = 'N'
                  AND R.cod_Divisa = @Divisa
                  AND (@Linea IS NULL OR R.Codigo = @Linea)
                  AND (@Destino IS NULL OR R.cod_destino = @Destino)
                  AND (@Institucion IS NULL OR S.cod_institucion = @Institucion)
                ORDER BY R.codigo;";

            return conn.Query<CcAnomaliaCreditoItemDto>(
                sql,
                CrearParametrosAnomalia(filtro),
                tx).AsList();
        }

        /// <summary>
        /// Obtiene cuentas contables de la operación (spCrdOperacionCtas).
        /// </summary>
        private static CcAnomaliaOperacionCtasDto ObtenerCuentasOperacion(
            IDbConnection conn,
            IDbTransaction tx,
            int operacion)
            => conn.QueryFirstOrDefault<CcAnomaliaOperacionCtasDto>(
                "exec spCrdOperacionCtas @Operacion",
                new { Operacion = operacion },
                tx)
                ?? throw new InvalidOperationException(
                    $"No fue posible obtener las cuentas de la operación {operacion}.");

        /// <summary>
        /// Registra una línea contable con spSIFDocsAsiento.
        /// </summary>
        private static void InsertarAsiento(
            IDbConnection conn,
            IDbTransaction tx,
            string tipoDocumento,
            long documento,
            decimal monto,
            string debeHaber,
            string divisa,
            decimal tipoCambio,
            int enlace,
            string unidad,
            string centroCosto,
            string cuenta,
            string operacion,
            string codigo)
        {
            if (string.IsNullOrWhiteSpace(cuenta))
            {
                throw new InvalidOperationException(
                    "No se encontró una cuenta contable válida para el asiento.");
            }

            conn.Execute(
                @"exec spSIFDocsAsiento
                    @Tipo, @Documento, @Monto, @DebeHaber, @Divisa, @TipoCambio,
                    @Enlace, @Unidad, @CentroCosto, @Cuenta, @Operacion, @Codigo, '';",
                new
                {
                    Tipo = tipoDocumento,
                    Documento = documento,
                    Monto = monto,
                    DebeHaber = debeHaber,
                    Divisa = divisa,
                    TipoCambio = tipoCambio,
                    Enlace = enlace,
                    Unidad = unidad ?? string.Empty,
                    CentroCosto = centroCosto ?? string.Empty,
                    Cuenta = cuenta,
                    Operacion = operacion ?? string.Empty,
                    Codigo = codigo ?? string.Empty
                },
                tx);
        }

        private static ErrorDto<CcAnomaliaSaldosMenoresCorregirResultado> CrearErrorCorregir(
            string mensaje)
            => DbHelper.CreateErrorResponse(
                mensaje,
                -2,
                new CcAnomaliaSaldosMenoresCorregirResultado());

        private static ErrorDto<CcAnomaliaSaldosNegativosCorregirResultado> CrearErrorCorregirNegativos(
            string mensaje)
            => DbHelper.CreateErrorResponse(
                mensaje,
                -2,
                new CcAnomaliaSaldosNegativosCorregirResultado());

        private static ErrorDto<CcAnomaliaMoraMenorCorregirResultado> CrearErrorCorregirMora(
            string mensaje)
            => DbHelper.CreateErrorResponse(
                mensaje,
                -2,
                new CcAnomaliaMoraMenorCorregirResultado());

        private static ErrorDto<CcAnomaliaCtaDerivadaCorregirResultado> CrearErrorCorregirCtaDerivada(
            string mensaje)
            => DbHelper.CreateErrorResponse(
                mensaje,
                -2,
                new CcAnomaliaCtaDerivadaCorregirResultado());

        private static object CrearParametrosAnomalia(CcAnomaliaFiltroDto? filtro)
        {
            return CrearParametrosAnomalia(
                filtro?.Monto ?? 0m,
                filtro?.Linea,
                filtro?.Destino,
                filtro?.Institucion);
        }

        private static object CrearParametrosAnomalia(
            decimal monto,
            string? linea,
            string? destino,
            int? institucion)
        {
            return new
            {
                Monto = monto,
                Divisa = DivisaCol,
                Linea = string.IsNullOrWhiteSpace(linea) ? null : linea.Trim(),
                Destino = string.IsNullOrWhiteSpace(destino) ? null : destino.Trim(),
                Institucion = institucion
            };
        }
    }
}
