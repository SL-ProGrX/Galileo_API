using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo.Models.Security;
using Microsoft.Extensions.Configuration;

namespace Galileo.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasServiciosDB
    {
        private readonly PortalDB _portalDb;
        private readonly int vModulo = 5;
        private readonly MSecurityMainDb _Security_MainDB;

        private const string MensajeOk = "Ok";
        private const string MovimientoRegistraWeb = "Registra - WEB";
        private const string MovimientoModificaWeb = "Modifica - WEB";
        private const string MovimientoEliminaWeb = "Elimina - WEB";

        private const string ServicioDetalleSelect = @"
                SELECT
                      RTRIM(C.cod_recaudador)                  AS cod_recaudador,
                      RTRIM(C.cod_servicio)                    AS cod_servicio,
                      ISNULL(RTRIM(C.descripcion),'')          AS descripcion,
                      ISNULL(C.activo,1)                       AS activo,
                      ISNULL(RTRIM(C.contrato),'')             AS contrato,
                      C.vence_fecha                            AS vence_fecha,
                      ISNULL(C.vende_activo,1)                 AS vence_activo,
                      ISNULL(RTRIM(C.cod_concepto),'')         AS cod_concepto,
                      ISNULL(RTRIM(Con.descripcion),'')        AS concepto_desc,
                      ISNULL(C.Intercambio,0)                  AS intercambio,
                      ISNULL(C.VALOR_TRANSITO_VALIDA,0)        AS valor_transito_valida,
                      ISNULL(C.GENERA_FACTURA,0)               AS genera_factura,
                      ISNULL(RTRIM(C.CABYS),'')                AS cabys,
                      ISNULL(RTRIM(C.cod_unidad),'')           AS cod_unidad,
                      ISNULL(RTRIM(C.cod_centro_costo),'')     AS cod_centro_costo,
                      ISNULL(RTRIM(C.cod_cuenta),'')           AS cod_cuenta,
                      ISNULL(RTRIM(C.cod_cuenta_comision),'')  AS cod_cuenta_comision,
                      ISNULL(RTRIM(C.cod_cuenta_iv),'')        AS cod_cuenta_iv,
                      ISNULL(RTRIM(CC.descripcion),'')         AS centro_costo_desc,
                      ISNULL(RTRIM(U.descripcion),'')          AS unidad_desc,
                      ISNULL(RTRIM(Cta.descripcion),'')        AS cuenta_desc,
                      ISNULL(RTRIM(CtaCom.descripcion),'')     AS cuenta_comision_desc,
                      ISNULL(RTRIM(CtaIv.descripcion),'')      AS cuenta_iv_desc,
                      ISNULL(C.REGISTRO_USUARIO,'')            AS registro_usuario,
                      ISNULL(CONVERT(varchar(19), C.REGISTRO_FECHA,120),'') AS registro_fecha,
                      ISNULL(C.MODIFICA_USUARIO,'')            AS modifica_usuario,
                      ISNULL(CONVERT(varchar(19), C.MODIFICA_FECHA,120),'') AS modifica_fecha
                FROM dbo.CAJAS_SERVICIOS C
                LEFT JOIN dbo.SIF_CONCEPTOS Con     ON Con.COD_CONCEPTO      = C.cod_concepto
                LEFT JOIN dbo.CntX_Cuentas Cta      ON C.cod_cuenta          = Cta.Cod_Cuenta
                LEFT JOIN dbo.CntX_Cuentas CtaCom   ON C.cod_cuenta_comision = CtaCom.Cod_Cuenta
                LEFT JOIN dbo.CntX_Cuentas CtaIv    ON C.cod_cuenta_iv       = CtaIv.Cod_Cuenta
                LEFT JOIN dbo.Cntx_Unidades U       ON U.cod_unidad          = C.cod_unidad
                LEFT JOIN dbo.Cntx_Centro_Costos CC ON CC.cod_centro_costo   = C.cod_centro_costo";

        public FrmCajasServiciosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config ?? throw new ArgumentNullException(nameof(config)));
            _Security_MainDB = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Lista de recaudadores
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Servicios_Recaudadores_DropDown_Obtener(int CodEmpresa)
        {
            const string q = @"
            SELECT
                  RTRIM(cod_recaudador) AS item,
                  RTRIM(descripcion)   AS descripcion
            FROM dbo.CAJAS_RECAUDADOR
            WHERE ACTIVO = 1
            ORDER BY cod_recaudador;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                q);
        }

        /// <summary>
        /// Lista de servicios del recaudador (para el buscador/F4 del código de concepto)
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_recaudador"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CajasServiciosConceptosLista> Cajas_Servicios_Conceptos_Lista_Obtener(int CodEmpresa, string cod_recaudador, FiltrosLazyLoadData filtros)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var resp = new ErrorDto<CajasServiciosConceptosLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new CajasServiciosConceptosLista()
            };

            try
            {
                string where = ConstruirWhereConceptos(filtros, out string filtroTexto);
                var (pagina, paginacion) = ConstruirPaginacion(filtros, 10);

                int total = cn.QueryFirstOrDefault<int>(
                    @"SELECT COUNT(*)
                      FROM dbo.CAJAS_SERVICIOS C" + where,
                    CrearParametrosConceptos(cod_recaudador, filtroTexto));

                resp.Result.total = total;

                resp.Result.lista = cn.Query<CajasServiciosConceptosData>(
                    @"
                SELECT
                      RTRIM(C.cod_servicio)     AS cod_servicio,
                      RTRIM(C.descripcion)      AS descripcion,
                      ISNULL(C.activo, 1)       AS activo,
                      RTRIM(C.cod_recaudador)   AS cod_recaudador
                FROM dbo.CAJAS_SERVICIOS C
                " + where + @"
                ORDER BY C.cod_servicio
                " + pagina + " " + paginacion,
                    CrearParametrosConceptos(cod_recaudador, filtroTexto)).ToList();
            }
            catch (Exception ex)
            {
                AsignarError(resp, ex, new CajasServiciosConceptosLista
                {
                    total = 0,
                    lista = new List<CajasServiciosConceptosData>()
                });
            }

            return resp;
        }

        /// <summary>
        /// Navegación (scroll) entre conceptos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_recaudador"></param>
        /// <param name="scroll"></param>
        /// <param name="cod_servicio"></param>
        /// <returns></returns>
        public ErrorDto<CajasServiciosConceptosData> Cajas_Servicios_Conceptos_Scroll(int CodEmpresa, string cod_recaudador, int scroll, string? cod_servicio)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = CrearRespuesta<CajasServiciosConceptosData>();

            try
            {
                var cod = TextoSeguro(cod_servicio);

                string where = " WHERE C.cod_recaudador = @cod_recaudador ";

                if (!string.IsNullOrEmpty(cod))
                {
                    if (scroll == 1)
                    {
                        where += " AND C.cod_servicio > @cod ";
                    }
                    else
                    {
                        where += " AND C.cod_servicio < @cod ";
                    }
                }

                string order = scroll == 1
                    ? " ORDER BY C.cod_servicio ASC "
                    : " ORDER BY C.cod_servicio DESC ";

                var query = ServicioDetalleSelect + @"
                " + where + @"
                " + order + ";";

                resp.Result = cn.QueryFirstOrDefault<CajasServiciosConceptosData>(
                    query,
                    new
                    {
                        cod_recaudador = TextoSeguro(cod_recaudador),
                        cod
                    });

                if (resp.Result == null)
                {
                    resp.Code = -2;
                    resp.Description = "No se encontraron más resultados.";
                }
                else
                {
                    resp.Description = MensajeOk;
                }
            }
            catch (Exception ex)
            {
                AsignarError(resp, ex, null, "Error al desplazar el servicio del recaudador: ");
            }

            return resp;
        }

        /// <summary>
        /// Obtiene la información completa de un servicio específico del recaudador.
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_recaudador"></param>
        /// <param name="cod_servicio"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CajasServiciosConceptosData> Cajas_Servicios_Conceptos_Obtener(int CodEmpresa, string cod_recaudador, string cod_servicio)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = CrearRespuesta<CajasServiciosConceptosData>();

            try
            {
                var query = ServicioDetalleSelect + @"
                WHERE C.cod_recaudador = @cod_recaudador
                  AND C.cod_servicio   = @cod;";

                resp.Result = cn.QueryFirstOrDefault<CajasServiciosConceptosData>(
                    query,
                    new
                    {
                        cod_recaudador = TextoSeguro(cod_recaudador),
                        cod = TextoSeguro(cod_servicio)
                    });

                if (resp.Result == null)
                {
                    resp.Code = -2;
                    resp.Description = "Servicio no encontrado.";
                }
                else
                {
                    resp.Description = MensajeOk;
                }
            }
            catch (Exception ex)
            {
                AsignarError(resp, ex, null, "Error al obtener el servicio del recaudador: ");
            }

            return resp;
        }

        /// <summary>
        /// Obtiene una lista de conceptos.
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Servicios_Conceptos_DropDown_Obtener(int CodEmpresa)
        {
            const string q = @"
                SELECT
                      RTRIM(COD_CONCEPTO) AS item,
                      RTRIM(DESCRIPCION) AS descripcion
                FROM dbo.SIF_CONCEPTOS
                WHERE ACTIVO = 1
                  AND COD_CONCEPTO LIKE 'CAJ%'
                ORDER BY COD_CONCEPTO;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                q);
        }

        /// <summary>
        /// Verifica si el código de servicio está libre u ocupado para ese recaudador.
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_recaudador"></param>
        /// <param name="cod_servicio"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Cajas_Servicios_Conceptos_Existe_Obtener(int CodEmpresa, string cod_recaudador, string cod_servicio)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "" };

            try
            {
                const string q = @"
                SELECT COUNT(1)
                FROM dbo.CAJAS_SERVICIOS
                WHERE cod_recaudador = @cod_recaudador
                  AND UPPER(cod_servicio) = @cod;";

                int n = cn.QueryFirstOrDefault<int>(
                    q,
                    new
                    {
                        cod_recaudador = TextoSeguro(cod_recaudador),
                        cod = TextoSeguro(cod_servicio).ToUpperInvariant()
                    });

                if (n == 0)
                {
                    resp.Code = 0;
                    resp.Description = "Concepto: Libre!";
                }
                else
                {
                    resp.Code = -2;
                    resp.Description = "Concepto: Ocupado!";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Guarda (insert/update) un servicio del recaudador.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="servicio"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Cajas_Servicios_Conceptos_Guardar(int CodEmpresa, string usuario, CajasServiciosConceptosData servicio)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var resp = CrearRespuesta();

            try
            {
                var validacion = ValidarServicio(servicio);
                if (validacion != null)
                {
                    return validacion;
                }

                int existe = ObtenerExisteServicio(cn, servicio.cod_recaudador, servicio.cod_servicio);

                if (servicio.isNew)
                {
                    if (existe > 0)
                    {
                        resp.Code = -2;
                        resp.Description = $"El servicio {servicio.cod_servicio} ya existe para el recaudador {servicio.cod_recaudador}.";
                        return resp;
                    }

                    resp = Cajas_Servicios_Conceptos_Insertar(CodEmpresa, usuario, servicio);
                }
                else
                {
                    if (existe == 0)
                    {
                        resp.Code = -2;
                        resp.Description = $"El servicio {servicio.cod_servicio} no existe para el recaudador {servicio.cod_recaudador}.";
                        return resp;
                    }

                    resp = Cajas_Servicios_Conceptos_Actualizar(CodEmpresa, usuario, servicio);
                }
            }
            catch (Exception ex)
            {
                AsignarError(resp, ex);
            }

            return resp;
        }

        /// <summary>
        /// Inserta un nuevo servicio del recaudador.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="servicio"></param> 
        /// </summary>
        /// <returns></returns>
        private ErrorDto Cajas_Servicios_Conceptos_Insertar(int CodEmpresa, string usuario, CajasServiciosConceptosData servicio)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var resp = CrearRespuesta();

            try
            {
                const string qInsert = @"
                INSERT INTO dbo.CAJAS_SERVICIOS
                    (cod_recaudador,
                     cod_servicio,
                     descripcion,
                     activo,
                     contrato,
                     vence_fecha,
                     vende_activo,
                     cod_cuenta,
                     cod_cuenta_comision,
                     cod_cuenta_iv,
                     cod_concepto,
                     Intercambio,
                     cod_unidad,
                     cod_centro_costo,
                     VALOR_TRANSITO_VALIDA,
                     GENERA_FACTURA,
                     CABYS,
                     REGISTRO_USUARIO,
                     REGISTRO_FECHA)
                VALUES
                    (@cod_recaudador,
                     @cod_servicio,
                     @descripcion,
                     @activo,
                     @contrato,
                     @vence_fecha,
                     @vence_activo,
                     @cod_cuenta,
                     @cod_cuenta_comision,
                     @cod_cuenta_iv,
                     @cod_concepto,
                     @intercambio,
                     @cod_unidad,
                     @cod_centro_costo,
                     @valor_transito_valida,
                     @genera_factura,
                     @cabys,
                     @usuario,
                     dbo.MyGetdate());";

                cn.Execute(qInsert, CrearParametrosServicio(servicio, usuario));

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    MovimientoRegistraWeb,
                    $"Servicio: {servicio.cod_servicio} Recaudador: {servicio.cod_recaudador}");
            }
            catch (Exception ex)
            {
                AsignarError(resp, ex);
            }

            return resp;
        }

        /// <summary>
        /// Actualiza un servicio existente del recaudador.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="servicio"></param>
        /// </summary>
        /// <returns></returns>
        private ErrorDto Cajas_Servicios_Conceptos_Actualizar(int CodEmpresa, string usuario, CajasServiciosConceptosData servicio)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var resp = CrearRespuesta();

            try
            {
                const string qUpdate = @"
                UPDATE dbo.CAJAS_SERVICIOS
                SET descripcion          = @descripcion,
                    activo               = @activo,
                    contrato             = @contrato,
                    vence_fecha          = @vence_fecha,
                    vende_activo         = @vence_activo,
                    cod_cuenta           = @cod_cuenta,
                    cod_cuenta_comision  = @cod_cuenta_comision,
                    cod_cuenta_iv        = @cod_cuenta_iv,
                    cod_concepto         = @cod_concepto,
                    Intercambio          = @intercambio,
                    cod_unidad           = @cod_unidad,
                    cod_centro_costo     = @cod_centro_costo,
                    VALOR_TRANSITO_VALIDA= @valor_transito_valida,
                    GENERA_FACTURA       = @genera_factura,
                    CABYS                = @cabys,
                    MODIFICA_USUARIO     = @usuario,
                    MODIFICA_FECHA       = dbo.MyGetdate()
                WHERE cod_recaudador     = @cod_recaudador
                  AND cod_servicio       = @cod_servicio;";

                cn.Execute(qUpdate, CrearParametrosServicio(servicio, usuario));

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    MovimientoModificaWeb,
                    $"Servicio: {servicio.cod_servicio} Recaudador: {servicio.cod_recaudador}");
            }
            catch (Exception ex)
            {
                AsignarError(resp, ex);
            }

            return resp;
        }

        /// <summary>
        /// Obtiene una lista de codigos cabys.
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CajasServiciosCabysLista> Cajas_Servicios_Cabys_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var resp = new ErrorDto<CajasServiciosCabysLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new CajasServiciosCabysLista()
            };

            try
            {
                string where = ConstruirWhereCabys(filtros, out string filtroTexto);
                var (pagina, paginacion) = ConstruirPaginacion(filtros, 30);
                string orderBy = ConstruirOrderByCabys(filtros);

                resp.Result.total = cn.QueryFirstOrDefault<int>(
                    @"
                    SELECT COUNT(*)
                    FROM vINV_Cabys CABYS
                " + where,
                    CrearParametroLike(filtroTexto));

                resp.Result.lista = cn.Query<DropDownListaGenericaModel>(
                    @"
                    SELECT
                        RTRIM(CABYS.Cod_ByS)      AS item,
                        RTRIM(CABYS.Descripcion) AS descripcion
                    FROM vINV_Cabys CABYS
                " + where + orderBy + pagina + " " + paginacion,
                    CrearParametroLike(filtroTexto)).ToList();
            }
            catch (Exception ex)
            {
                AsignarError(resp, ex, new CajasServiciosCabysLista
                {
                    total = 0,
                    lista = new List<DropDownListaGenericaModel>()
                }, "Error al obtener CABYS: ");
            }

            return resp;
        }

        /// <summary>
        /// Lista todos los rangos de comisión de un servicio de un recaudador.
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_recaudador"></param>
        /// <param name="cod_servicio"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<CajasServiciosComisionesData>> Cajas_Servicios_Comisiones_Lista_Obtener(int CodEmpresa, string cod_recaudador, string cod_servicio)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var resp = CrearRespuesta(new List<CajasServiciosComisionesData>());

            try
            {
                const string q = @"
                SELECT
                    RTRIM(cod_recaudador)                AS cod_recaudador,
                    RTRIM(cod_servicio)                  AS cod_servicio,
                    linea                                AS linea,
                    ISNULL(monto_inicio,0)              AS monto_inicial,
                    ISNULL(monto_corte,0)               AS monto_corte,
                    ISNULL(comision_mnt_minimo,0)       AS monto_minimo_comision,
                    ISNULL(comision_porcentaje,0)       AS porcentaje_comision,
                    ISNULL(iv_porcentaje,0)             AS porcentaje_imp_ventas,
                    ''                                  AS usuario,
                    CAST(NULL AS datetime)              AS fecha
                FROM dbo.CAJAS_SERVICIOS_RANGOS
                WHERE cod_recaudador = @cod_recaudador
                  AND cod_servicio   = @cod_servicio
                ORDER BY linea;";

                resp.Result = cn.Query<CajasServiciosComisionesData>(
                    q,
                    CrearParametrosClaveServicio(cod_recaudador, cod_servicio)).ToList();
            }
            catch (Exception ex)
            {
                AsignarError(resp, ex, new List<CajasServiciosComisionesData>());
            }

            return resp;
        }

        /// <summary>
        /// Guarda (insert/update) un rango de comisión.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="rango"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Cajas_Servicios_Comisiones_Guardar(int CodEmpresa, string usuario, CajasServiciosComisionesData rango)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var resp = CrearRespuesta();

            try
            {
                var validacion = ValidarRango(rango);
                if (validacion != null)
                {
                    return validacion;
                }

                int existe = ObtenerExisteComision(cn, rango.cod_recaudador, rango.cod_servicio, rango.linea);

                if (rango.isNew)
                {
                    if (existe > 0)
                    {
                        resp.Code = -2;
                        resp.Description = $"La línea {rango.linea} ya existe para el servicio {rango.cod_servicio} del recaudador {rango.cod_recaudador}.";
                        return resp;
                    }

                    resp = Cajas_Servicios_Comisiones_Insertar(CodEmpresa, usuario, rango);
                }
                else
                {
                    if (existe == 0)
                    {
                        resp.Code = -2;
                        resp.Description = $"La línea {rango.linea} no existe para el servicio {rango.cod_servicio} del recaudador {rango.cod_recaudador}.";
                        return resp;
                    }

                    resp = Cajas_Servicios_Comisiones_Actualizar(CodEmpresa, usuario, rango);
                }
            }
            catch (Exception ex)
            {
                AsignarError(resp, ex);
            }

            return resp;
        }

        /// <summary>
        /// Inserta un nuevo rango de comisión.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="rango"></param>
        /// </summary>
        /// <returns></returns>
        private ErrorDto Cajas_Servicios_Comisiones_Insertar(int CodEmpresa, string usuario, CajasServiciosComisionesData rango)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var resp = CrearRespuesta();

            try
            {
                if (rango.linea <= 0)
                {
                    const string qNextLinea = @"
                    SELECT ISNULL(MAX(linea),0) + 1
                    FROM dbo.CAJAS_SERVICIOS_RANGOS
                    WHERE cod_recaudador = @cod_recaudador
                      AND cod_servicio   = @cod_servicio;";

                    rango.linea = cn.ExecuteScalar<int>(qNextLinea, CrearParametrosClaveServicio(rango.cod_recaudador, rango.cod_servicio));
                }

                const string qInsert = @"
                INSERT INTO dbo.CAJAS_SERVICIOS_RANGOS
                    (cod_recaudador,
                     cod_servicio,
                     linea,
                     monto_inicio,
                     monto_corte,
                     comision_mnt_minimo,
                     comision_porcentaje,
                     iv_porcentaje)
                VALUES
                    (@cod_recaudador,
                     @cod_servicio,
                     @linea,
                     @monto_inicio,
                     @monto_corte,
                     @monto_minimo_comision,
                     @porcentaje_comision,
                     @porcentaje_imp_ventas);";

                cn.Execute(qInsert, CrearParametrosComision(rango));

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    MovimientoRegistraWeb,
                    $"Cajas Servicios - Rango comisión Línea: {rango.linea} Serv.: {rango.cod_servicio} Recaudador: {rango.cod_recaudador}");
            }
            catch (Exception ex)
            {
                AsignarError(resp, ex);
            }

            return resp;
        }

        /// <summary>
        /// Actualiza un rango de comisión existente.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="rango"></param>
        /// </summary>
        /// <returns></returns>
        private ErrorDto Cajas_Servicios_Comisiones_Actualizar(int CodEmpresa, string usuario, CajasServiciosComisionesData rango)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var resp = CrearRespuesta();

            try
            {
                const string qUpdate = @"
                UPDATE dbo.CAJAS_SERVICIOS_RANGOS
                SET monto_inicio          = @monto_inicio,
                    monto_corte           = @monto_corte,
                    comision_mnt_minimo   = @monto_minimo_comision,
                    comision_porcentaje   = @porcentaje_comision,
                    iv_porcentaje         = @porcentaje_imp_ventas
                WHERE cod_recaudador = @cod_recaudador
                  AND cod_servicio   = @cod_servicio
                  AND linea          = @linea;";

                cn.Execute(qUpdate, CrearParametrosComision(rango));

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    MovimientoModificaWeb,
                    $"Cajas Servicios - Rango comisión Línea: {rango.linea} Serv.: {rango.cod_servicio} Recaudador: {rango.cod_recaudador}");
            }
            catch (Exception ex)
            {
                AsignarError(resp, ex);
            }

            return resp;
        }

        /// <summary>
        /// Elimina un rango de comisión.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_recaudador"></param>
        /// <param name="cod_servicio"></param>
        /// <param name="linea"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Cajas_Servicios_Comisiones_Eliminar(int CodEmpresa, string usuario, string cod_recaudador, string cod_servicio, int linea)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var resp = CrearRespuesta();

            try
            {
                const string qDelete = @"
                DELETE FROM dbo.CAJAS_SERVICIOS_RANGOS
                WHERE cod_recaudador = @cod_recaudador
                  AND cod_servicio   = @cod_servicio
                  AND linea          = @linea;";

                cn.Execute(qDelete, CrearParametrosComision(cod_recaudador, cod_servicio, linea));

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    MovimientoEliminaWeb,
                    $"Cajas Servicios - Rango comisión Línea: {linea} Serv.: {cod_servicio} Recaudador: {cod_recaudador}");
            }
            catch (Exception ex)
            {
                AsignarError(resp, ex);
            }

            return resp;
        }

        /// <summary>
        /// Lista de cajas con indicador de si están vinculadas al concepto.
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_recaudador"></param>
        /// <param name="cod_servicio"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<CajasServiciosCajasVinculadasData>> Cajas_Servicios_CajasVinculadas_Lista_Obtener(int CodEmpresa, string cod_recaudador, string cod_servicio)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var resp = CrearRespuesta(new List<CajasServiciosCajasVinculadasData>());

            try
            {
                const string q = @"
            SELECT
                @cod_servicio                                           AS concepto,
                C.COD_CAJA                                              AS cod_caja,
                RTRIM(ISNULL(C.DESCRIPCION, ''))                        AS desc_caja,
                CASE WHEN X.cod_caja IS NULL THEN CAST(0 AS smallint)
                     ELSE CAST(1 AS smallint) END                       AS asignada
            FROM dbo.CAJAS_DEFINICION C
            LEFT JOIN dbo.CAJAS_SERVICIOS_ASIGNADOS X
                   ON X.cod_caja       = C.COD_CAJA
                  AND X.cod_recaudador = @cod_recaudador
                  AND X.cod_servicio   = @cod_servicio
            ORDER BY C.DESCRIPCION;";

                resp.Result = cn.Query<CajasServiciosCajasVinculadasData>(
                    q,
                    CrearParametrosClaveServicio(cod_recaudador, cod_servicio)).ToList();
            }
            catch (Exception ex)
            {
                AsignarError(resp, ex, new List<CajasServiciosCajasVinculadasData>());
            }

            return resp;
        }

        /// <summary>
        /// Guarda las cajas vinculadas a un concepto.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_recaudador"></param>
        /// <param name="cod_servicio"></param>
        /// <param name="cod_caja"></param>
        /// <param name="asignada"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Cajas_Servicios_CajasVinculadas_Guardar(int CodEmpresa, string usuario, string cod_recaudador, string cod_servicio, string cod_caja, short asignada)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var resp = CrearRespuesta();

            try
            {
                int existe = ObtenerExisteCajaAsignada(cn, cod_recaudador, cod_servicio, cod_caja);

                if (asignada == 1)
                {
                    if (existe == 0)
                    {
                        const string qInsert = @"
                    INSERT INTO dbo.CAJAS_SERVICIOS_ASIGNADOS
                        (cod_recaudador, cod_servicio, cod_caja, registro_fecha, registro_usuario)
                    VALUES
                        (@cod_recaudador, @cod_servicio, @cod_caja, dbo.MyGetdate(), @usuario);";

                        cn.Execute(
                            qInsert,
                            CrearParametrosCajaAsignada(cod_recaudador, cod_servicio, cod_caja, usuario));

                        RegistrarBitacora(
                            CodEmpresa,
                            usuario,
                            MovimientoRegistraWeb,
                            $"Cajas_Servicios: Asigna caja {cod_caja} al servicio {cod_servicio} del recaudador {cod_recaudador}");
                    }
                    else
                    {
                        resp.Description = "La caja ya estaba asignada. No se realizaron cambios.";
                    }
                }
                else
                {
                    if (existe > 0)
                    {
                        const string qDelete = @"
                    DELETE FROM dbo.CAJAS_SERVICIOS_ASIGNADOS
                    WHERE cod_recaudador = @cod_recaudador
                      AND cod_servicio   = @cod_servicio
                      AND cod_caja       = @cod_caja;";

                        cn.Execute(
                            qDelete,
                            CrearParametrosCajaAsignada(cod_recaudador, cod_servicio, cod_caja));

                        RegistrarBitacora(
                            CodEmpresa,
                            usuario,
                            MovimientoEliminaWeb,
                            $"Cajas_Servicios: Quita caja {cod_caja} del servicio {cod_servicio} del recaudador {cod_recaudador}");
                    }
                    else
                    {
                        resp.Description = "La caja no estaba asignada. No se realizaron cambios.";
                    }
                }
            }
            catch (Exception ex)
            {
                AsignarError(resp, ex);
            }

            return resp;
        }

        private static object CrearParametrosClaveServicio(string? codRecaudador, string? codServicio)
        {
            return new
            {
                cod_recaudador = TextoSeguro(codRecaudador),
                cod_servicio = TextoSeguro(codServicio)
            };
        }

        private static object CrearParametrosConceptos(string? codRecaudador, string? filtroTexto)
        {
            return new
            {
                cod_recaudador = TextoSeguro(codRecaudador),
                like = CrearLike(filtroTexto)
            };
        }

        private static object CrearParametroLike(string? filtroTexto)
        {
            return new
            {
                like = CrearLike(filtroTexto)
            };
        }

        private static object CrearParametrosComision(CajasServiciosComisionesData rango)
        {
            return new
            {
                cod_recaudador = TextoSeguro(rango.cod_recaudador),
                cod_servicio = TextoSeguro(rango.cod_servicio),
                linea = rango.linea,
                monto_inicio = rango.monto_inicial,
                monto_corte = rango.monto_corte,
                monto_minimo_comision = rango.monto_minimo_comision,
                porcentaje_comision = rango.porcentaje_comision,
                porcentaje_imp_ventas = rango.porcentaje_imp_ventas
            };
        }

        private static object CrearParametrosComision(string? codRecaudador, string? codServicio, int linea)
        {
            return new
            {
                cod_recaudador = TextoSeguro(codRecaudador),
                cod_servicio = TextoSeguro(codServicio),
                linea
            };
        }

        private static object CrearParametrosCajaAsignada(string? codRecaudador, string? codServicio, string? codCaja, string? usuario = null)
        {
            return new
            {
                cod_recaudador = TextoSeguro(codRecaudador),
                cod_servicio = TextoSeguro(codServicio),
                cod_caja = TextoSeguro(codCaja),
                usuario = usuario ?? string.Empty
            };
        }

        private static string ConstruirWhereConceptos( FiltrosLazyLoadData filtros, out string filtroTexto)
        {
            string where = " WHERE C.cod_recaudador = @cod_recaudador ";
            filtroTexto = (filtros?.filtro ?? string.Empty).Trim();

            if (!string.IsNullOrWhiteSpace(filtroTexto))
            {
                where += " AND (C.cod_servicio LIKE @like OR C.Descripcion LIKE @like) ";
            }

            return where;
        }

        private static string ConstruirWhereCabys(FiltrosLazyLoadData filtros, out string filtroTexto)
        {
            string where = " WHERE 1 = 1 ";
            filtroTexto = (filtros?.filtro ?? string.Empty).Trim();

            if (!string.IsNullOrWhiteSpace(filtroTexto))
            {
                where += " AND ( CABYS.Cod_ByS LIKE @like OR CABYS.Descripcion LIKE @like ) ";
            }

            return where;
        }

        private static (string pagina, string paginacion) ConstruirPaginacion(FiltrosLazyLoadData filtros, int paginacionDefault)
        {
            if (filtros?.pagina == null)
            {
                return (string.Empty, string.Empty);
            }

            string pagina = " OFFSET " + filtros.pagina + " ROWS ";
            string paginacion = " FETCH NEXT " + (filtros.paginacion != 0 ? filtros.paginacion : paginacionDefault) + " ROWS ONLY ";
            return (pagina, paginacion);
        }

        private static string ConstruirOrderByCabys(FiltrosLazyLoadData filtros)
        {
            string sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
            string sortDir = filtros?.sortOrder == 1 ? "DESC" : "ASC";

            return sortField switch
            {
                "descripcion" => $" ORDER BY CABYS.Descripcion {sortDir} ",
                "item" or "cod_bys" => $" ORDER BY CABYS.Cod_ByS {sortDir} ",
                _ => " ORDER BY CABYS.Cod_ByS ASC "
            };
        }

        private static ErrorDto? ValidarServicio(CajasServiciosConceptosData servicio)
        {
            if (string.IsNullOrWhiteSpace(servicio.cod_recaudador))
            {
                return CrearRespuestaError("Debe indicar el recaudador.");
            }

            if (string.IsNullOrWhiteSpace(servicio.cod_servicio))
            {
                return CrearRespuestaError("Debe indicar el código del servicio.");
            }

            if (string.IsNullOrWhiteSpace(servicio.descripcion))
            {
                return CrearRespuestaError("Debe indicar la descripción del servicio.");
            }

            return null;
        }

        private static ErrorDto? ValidarRango(CajasServiciosComisionesData rango)
        {
            if (string.IsNullOrWhiteSpace(rango.cod_recaudador))
            {
                return CrearRespuestaError("Debe indicar el recaudador.");
            }

            if (string.IsNullOrWhiteSpace(rango.cod_servicio))
            {
                return CrearRespuestaError("Debe indicar el servicio.");
            }

            return null;
        }

        private static ErrorDto CrearRespuesta()
        {
            return new ErrorDto
            {
                Code = 0,
                Description = MensajeOk
            };
        }

        private static ErrorDto<T> CrearRespuesta<T>(T? result = default)
        {
            return new ErrorDto<T>
            {
                Code = 0,
                Description = string.Empty,
                Result = result
            };
        }

        private static ErrorDto CrearRespuestaError(string descripcion)
        {
            return new ErrorDto
            {
                Code = -2,
                Description = descripcion
            };
        }

        private static void AsignarError(ErrorDto resp, Exception ex, string prefijo = "")
        {
            resp.Code = -1;
            resp.Description = prefijo + ex.Message;
        }

        private static void AsignarError<T>(ErrorDto<T> resp, Exception ex, T? result, string prefijo = "")
        {
            resp.Code = -1;
            resp.Description = prefijo + ex.Message;
            resp.Result = result;
        }

        private static int ObtenerExisteServicio(System.Data.IDbConnection cn, string? codRecaudador, string? codServicio)
        {
            const string qExiste = @"
                SELECT ISNULL(COUNT(*),0)
                FROM dbo.CAJAS_SERVICIOS
                WHERE cod_recaudador = @cod_recaudador
                  AND cod_servicio   = @cod_servicio;";

            return cn.QueryFirstOrDefault<int>(qExiste, CrearParametrosClaveServicio(codRecaudador, codServicio));
        }

        private static int ObtenerExisteComision(System.Data.IDbConnection cn, string? codRecaudador, string? codServicio, int linea)
        {
            const string qExiste = @"
                SELECT ISNULL(COUNT(*),0)
                FROM dbo.CAJAS_SERVICIOS_RANGOS
                WHERE cod_recaudador = @cod_recaudador
                  AND cod_servicio   = @cod_servicio
                  AND linea          = @linea;";

            return cn.ExecuteScalar<int>(qExiste, CrearParametrosComision(codRecaudador, codServicio, linea));
        }

        private static int ObtenerExisteCajaAsignada(System.Data.IDbConnection cn, string? codRecaudador, string? codServicio, string? codCaja)
        {
            const string qExiste = @"
            SELECT ISNULL(COUNT(1), 0)
            FROM dbo.CAJAS_SERVICIOS_ASIGNADOS
            WHERE cod_recaudador = @cod_recaudador
              AND cod_servicio   = @cod_servicio
              AND cod_caja       = @cod_caja;";

            return cn.ExecuteScalar<int>(qExiste, CrearParametrosCajaAsignada(codRecaudador, codServicio, codCaja));
        }

        private static string CrearLike(string? texto)
        {
            return "%" + (texto ?? string.Empty).Trim() + "%";
        }

        private static DynamicParameters CrearParametrosServicio(CajasServiciosConceptosData servicio, string? usuario)
        {
            var parametros = new DynamicParameters();
            parametros.Add("@cod_recaudador", TextoSeguro(servicio.cod_recaudador));
            parametros.Add("@cod_servicio", TextoSeguro(servicio.cod_servicio));
            parametros.Add("@descripcion", TextoSeguro(servicio.descripcion));
            parametros.Add("@activo", servicio.activo);
            parametros.Add("@contrato", TextoSeguro(servicio.contrato));
            parametros.Add("@vence_fecha", servicio.vence_fecha ?? DateTime.Today);
            parametros.Add("@vence_activo", servicio.vence_activo);
            parametros.Add("@cod_cuenta", TextoSeguro(servicio.cod_cuenta));
            parametros.Add("@cod_cuenta_comision", TextoSeguro(servicio.cod_cuenta_comision));
            parametros.Add("@cod_cuenta_iv", TextoSeguro(servicio.cod_cuenta_iv));
            parametros.Add("@cod_concepto", TextoSeguro(servicio.cod_concepto));
            parametros.Add("@intercambio", servicio.intercambio);
            parametros.Add("@cod_unidad", TextoSeguro(servicio.cod_unidad));
            parametros.Add("@cod_centro_costo", TextoSeguro(servicio.cod_centro_costo));
            parametros.Add("@valor_transito_valida", servicio.valor_transito_valida);
            parametros.Add("@genera_factura", servicio.genera_factura);
            parametros.Add("@cabys", TextoSeguro(servicio.cabys));
            parametros.Add("@usuario", usuario ?? string.Empty);
            return parametros;
        }

        private static string TextoSeguro(string? valor)
        {
            return (valor ?? string.Empty).Trim();
        }

        private void RegistrarBitacora(int codEmpresa, string? usuario, string movimiento, string detalleMovimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario ?? string.Empty,
                Modulo = vModulo,
                Movimiento = movimiento,
                DetalleMovimiento = detalleMovimiento
            });
        }
    }
}