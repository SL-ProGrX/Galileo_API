using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo.Models.Security;


namespace Galileo.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasServiciosDB
    {
        private readonly PortalDB _portalDb;
        private readonly int vModulo = 5;
        private readonly MSecurityMainDb _Security_MainDB;

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
                string where = " WHERE C.cod_recaudador = @cod_recaudador ";
                string pagina = string.Empty;
                string paginacion = string.Empty;

                string filtroTexto = (filtros?.filtro ?? string.Empty).Trim();

                if (!string.IsNullOrWhiteSpace(filtroTexto))
                {
                    where += " AND (C.cod_servicio LIKE @like OR C.Descripcion LIKE @like) ";
                }

                if (filtros?.pagina != null)
                {
                    pagina = " OFFSET " + filtros.pagina + " ROWS ";
                    paginacion = " FETCH NEXT " + (filtros.paginacion != 0 ? filtros.paginacion : 10) + " ROWS ONLY ";
                }

                var qTotal = @"SELECT COUNT(*) 
                           FROM dbo.CAJAS_SERVICIOS C" + where;

                int total = cn.QueryFirstOrDefault<int>(
                    qTotal,
                    new
                    {
                        cod_recaudador = (cod_recaudador ?? string.Empty).Trim(),
                        like = "%" + filtroTexto + "%"
                    });

                resp.Result.total = total;

                var qDatos = @"
                SELECT
                      RTRIM(C.cod_servicio)            AS cod_servicio,
                      RTRIM(C.descripcion)            AS descripcion,
                      ISNULL(C.activo, 1)             AS activo,
                      RTRIM(C.cod_recaudador)         AS cod_recaudador
                FROM dbo.CAJAS_SERVICIOS C
                " + where + @"
                ORDER BY C.cod_servicio
                " + pagina + " " + paginacion;

                var enumerable = cn.Query<CajasServiciosConceptosData>(
                    qDatos,
                    new
                    {
                        cod_recaudador = (cod_recaudador ?? string.Empty).Trim(),
                        like = "%" + filtroTexto + "%"
                    });

                var lista = new List<CajasServiciosConceptosData>();
                foreach (var item in enumerable)
                {
                    lista.Add(item);
                }

                resp.Result.lista = lista;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result.total = 0;
                resp.Result.lista = new List<CajasServiciosConceptosData>();
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
            var resp = new ErrorDto<CajasServiciosConceptosData> { Code = 0, Description = "" };

            try
            {
                var cod = (cod_servicio ?? string.Empty).Trim();

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

                var query = @"
                SELECT TOP 1
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
                LEFT JOIN dbo.Cntx_Centro_Costos CC ON CC.cod_centro_costo   = C.cod_centro_costo
                " + where + @"
                " + order + ";";

                resp.Result = cn.QueryFirstOrDefault<CajasServiciosConceptosData>(
                    query,
                    new
                    {
                        cod_recaudador = (cod_recaudador ?? string.Empty).Trim(),
                        cod
                    });

                if (resp.Result == null)
                {
                    resp.Code = -2;
                    resp.Description = "No se encontraron más resultados.";
                }
                else
                {
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Error al desplazar el servicio del recaudador: " + ex.Message;
                resp.Result = null;
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
            var resp = new ErrorDto<CajasServiciosConceptosData> { Code = 0, Description = "" };

            try
            {
                var query = @"
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
                LEFT JOIN dbo.Cntx_Centro_Costos CC ON CC.cod_centro_costo   = C.cod_centro_costo
                WHERE C.cod_recaudador = @cod_recaudador
                  AND C.cod_servicio   = @cod;";

                resp.Result = cn.QueryFirstOrDefault<CajasServiciosConceptosData>(
                    query,
                    new
                    {
                        cod_recaudador = (cod_recaudador ?? string.Empty).Trim(),
                        cod = (cod_servicio ?? string.Empty).Trim()
                    });

                if (resp.Result == null)
                {
                    resp.Code = -2;
                    resp.Description = "Servicio no encontrado.";
                }
                else
                {
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Error al obtener el servicio del recaudador: " + ex.Message;
                resp.Result = null;
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
                        cod_recaudador = (cod_recaudador ?? string.Empty).Trim(),
                        cod = (cod_servicio ?? string.Empty).Trim().ToUpper()
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

            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                if (string.IsNullOrWhiteSpace(servicio.cod_recaudador))
                {
                    resp.Code = -2;
                    resp.Description = "Debe indicar el recaudador.";
                    return resp;
                }

                if (string.IsNullOrWhiteSpace(servicio.cod_servicio))
                {
                    resp.Code = -2;
                    resp.Description = "Debe indicar el código del servicio.";
                    return resp;
                }

                if (string.IsNullOrWhiteSpace(servicio.descripcion))
                {
                    resp.Code = -2;
                    resp.Description = "Debe indicar la descripción del servicio.";
                    return resp;
                }

                const string qExiste = @"
                SELECT ISNULL(COUNT(*),0)
                FROM dbo.CAJAS_SERVICIOS
                WHERE cod_recaudador = @cod_recaudador
                  AND cod_servicio   = @cod_servicio;";

                int existe = cn.QueryFirstOrDefault<int>(
                    qExiste,
                    new
                    {
                        cod_recaudador = (servicio.cod_recaudador ?? string.Empty).Trim(),
                        cod_servicio = (servicio.cod_servicio ?? string.Empty).Trim()
                    });

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
                resp.Code = -1;
                resp.Description = ex.Message;
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

            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

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

                cn.Execute(
                    qInsert,
                    new
                    {
                        cod_recaudador = (servicio.cod_recaudador ?? string.Empty).Trim(),
                        cod_servicio = (servicio.cod_servicio ?? string.Empty).Trim(),
                        descripcion = (servicio.descripcion ?? string.Empty).Trim(),
                        activo = servicio.activo,
                        contrato = (servicio.contrato ?? string.Empty).Trim(),
                        vence_fecha = servicio.vence_fecha ?? DateTime.Today,
                        vence_activo = servicio.vence_activo,
                        cod_cuenta = (servicio.cod_cuenta ?? string.Empty).Trim(),
                        cod_cuenta_comision = (servicio.cod_cuenta_comision ?? string.Empty).Trim(),
                        cod_cuenta_iv = (servicio.cod_cuenta_iv ?? string.Empty).Trim(),
                        cod_concepto = (servicio.cod_concepto ?? string.Empty).Trim(),
                        intercambio = servicio.intercambio,
                        cod_unidad = (servicio.cod_unidad ?? string.Empty).Trim(),
                        cod_centro_costo = (servicio.cod_centro_costo ?? string.Empty).Trim(),
                        valor_transito_valida = servicio.valor_transito_valida,
                        genera_factura = servicio.genera_factura,
                        cabys = (servicio.cabys ?? string.Empty).Trim(),
                        usuario = usuario ?? string.Empty
                    });

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    "Registra - WEB",
                    $"Servicio: {servicio.cod_servicio} Recaudador: {servicio.cod_recaudador}");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
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

            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

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

                cn.Execute(
                    qUpdate,
                    new
                    {
                        cod_recaudador = (servicio.cod_recaudador ?? string.Empty).Trim(),
                        cod_servicio = (servicio.cod_servicio ?? string.Empty).Trim(),
                        descripcion = (servicio.descripcion ?? string.Empty).Trim(),
                        activo = servicio.activo,
                        contrato = (servicio.contrato ?? string.Empty).Trim(),
                        vence_fecha = servicio.vence_fecha ?? DateTime.Today,
                        vence_activo = servicio.vence_activo,
                        cod_cuenta = (servicio.cod_cuenta ?? string.Empty).Trim(),
                        cod_cuenta_comision = (servicio.cod_cuenta_comision ?? string.Empty).Trim(),
                        cod_cuenta_iv = (servicio.cod_cuenta_iv ?? string.Empty).Trim(),
                        cod_concepto = (servicio.cod_concepto ?? string.Empty).Trim(),
                        intercambio = servicio.intercambio,
                        cod_unidad = (servicio.cod_unidad ?? string.Empty).Trim(),
                        cod_centro_costo = (servicio.cod_centro_costo ?? string.Empty).Trim(),
                        valor_transito_valida = servicio.valor_transito_valida,
                        genera_factura = servicio.genera_factura,
                        cabys = (servicio.cabys ?? string.Empty).Trim(),
                        usuario = usuario ?? string.Empty
                    });

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    "Modifica - WEB",
                    $"Servicio: {servicio.cod_servicio} Recaudador: {servicio.cod_recaudador}");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
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
                string where = " WHERE 1 = 1 ";
                string pagina = string.Empty;
                string paginacion = string.Empty;

                var filtroTexto = (filtros?.filtro ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(filtroTexto))
                {
                    where += " AND ( CABYS.Cod_ByS LIKE @like OR CABYS.Descripcion LIKE @like ) ";
                }

                if (filtros?.pagina != null)
                {
                    pagina = " OFFSET " + filtros.pagina + " ROWS ";
                    paginacion = " FETCH NEXT " + (filtros.paginacion != 0 ? filtros.paginacion : 30) + " ROWS ONLY ";
                }

                string sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                string sortDir = filtros?.sortOrder == 1 ? "DESC" : "ASC";

                string orderBy;
                switch (sortField)
                {
                    case "descripcion":
                        orderBy = $" ORDER BY CABYS.Descripcion {sortDir} ";
                        break;
                    case "item":
                    case "cod_bys":
                        orderBy = $" ORDER BY CABYS.Cod_ByS {sortDir} ";
                        break;
                    default:
                        orderBy = " ORDER BY CABYS.Cod_ByS ASC ";
                        break;
                }

                var qTotal = @"
                    SELECT COUNT(*)
                    FROM vINV_Cabys CABYS
                " + where;

                int total = cn.QueryFirstOrDefault<int>(
                    qTotal,
                    new
                    {
                        like = "%" + filtroTexto + "%"
                    });

                resp.Result.total = total;

                var qDatos = @"
                    SELECT
                        RTRIM(CABYS.Cod_ByS)      AS item,
                        RTRIM(CABYS.Descripcion) AS descripcion
                    FROM vINV_Cabys CABYS
                " + where + orderBy + pagina + " " + paginacion;

                var enumerable = cn.Query<DropDownListaGenericaModel>(
                    qDatos,
                    new
                    {
                        like = "%" + filtroTexto + "%"
                    });

                resp.Result.lista = enumerable.ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Error al obtener CABYS: " + ex.Message;
                resp.Result.total = 0;
                resp.Result.lista = new List<DropDownListaGenericaModel>();
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

            var resp = new ErrorDto<List<CajasServiciosComisionesData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CajasServiciosComisionesData>()
            };

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

                var enumerable = cn.Query<CajasServiciosComisionesData>(
                    q,
                    new
                    {
                        cod_recaudador = (cod_recaudador ?? string.Empty).Trim(),
                        cod_servicio = (cod_servicio ?? string.Empty).Trim()
                    });

                var lista = new List<CajasServiciosComisionesData>();
                foreach (var item in enumerable)
                {
                    lista.Add(item);
                }

                resp.Result = lista;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = new List<CajasServiciosComisionesData>();
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

            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                if (string.IsNullOrWhiteSpace(rango.cod_recaudador))
                {
                    resp.Code = -2;
                    resp.Description = "Debe indicar el recaudador.";
                    return resp;
                }

                if (string.IsNullOrWhiteSpace(rango.cod_servicio))
                {
                    resp.Code = -2;
                    resp.Description = "Debe indicar el servicio.";
                    return resp;
                }

                const string qExiste = @"
                SELECT ISNULL(COUNT(*),0)
                FROM dbo.CAJAS_SERVICIOS_RANGOS
                WHERE cod_recaudador = @cod_recaudador
                  AND cod_servicio   = @cod_servicio
                  AND linea          = @linea;";

                int existe = cn.ExecuteScalar<int>(qExiste, new
                {
                    cod_recaudador = rango.cod_recaudador,
                    cod_servicio = rango.cod_servicio,
                    linea = rango.linea
                });

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
                resp.Code = -1;
                resp.Description = ex.Message;
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

            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                if (rango.linea <= 0)
                {
                    const string qNextLinea = @"
                    SELECT ISNULL(MAX(linea),0) + 1
                    FROM dbo.CAJAS_SERVICIOS_RANGOS
                    WHERE cod_recaudador = @cod_recaudador
                      AND cod_servicio   = @cod_servicio;";

                    rango.linea = cn.ExecuteScalar<int>(qNextLinea, new
                    {
                        cod_recaudador = rango.cod_recaudador,
                        cod_servicio = rango.cod_servicio
                    });
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

                cn.Execute(qInsert, new
                {
                    cod_recaudador = rango.cod_recaudador,
                    cod_servicio = rango.cod_servicio,
                    linea = rango.linea,
                    monto_inicio = rango.monto_inicial,
                    monto_corte = rango.monto_corte,
                    monto_minimo_comision = rango.monto_minimo_comision,
                    porcentaje_comision = rango.porcentaje_comision,
                    porcentaje_imp_ventas = rango.porcentaje_imp_ventas
                });

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    "Registra - WEB",
                    $"Cajas Servicios - Rango comisión Línea: {rango.linea} Serv.: {rango.cod_servicio} Recaudador: {rango.cod_recaudador}");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
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

            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

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

                cn.Execute(qUpdate, new
                {
                    cod_recaudador = rango.cod_recaudador,
                    cod_servicio = rango.cod_servicio,
                    linea = rango.linea,
                    monto_inicio = rango.monto_inicial,
                    monto_corte = rango.monto_corte,
                    monto_minimo_comision = rango.monto_minimo_comision,
                    porcentaje_comision = rango.porcentaje_comision,
                    porcentaje_imp_ventas = rango.porcentaje_imp_ventas
                });

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    "Modifica - WEB",
                    $"Cajas Servicios - Rango comisión Línea: {rango.linea} Serv.: {rango.cod_servicio} Recaudador: {rango.cod_recaudador}");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
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

            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                const string qDelete = @"
                DELETE FROM dbo.CAJAS_SERVICIOS_RANGOS
                WHERE cod_recaudador = @cod_recaudador
                  AND cod_servicio   = @cod_servicio
                  AND linea          = @linea;";

                cn.Execute(qDelete, new
                {
                    cod_recaudador = (cod_recaudador ?? string.Empty).Trim(),
                    cod_servicio = (cod_servicio ?? string.Empty).Trim(),
                    linea
                });

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    "Elimina - WEB",
                    $"Cajas Servicios - Rango comisión Línea: {linea} Serv.: {cod_servicio} Recaudador: {cod_recaudador}");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
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

            var resp = new ErrorDto<List<CajasServiciosCajasVinculadasData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CajasServiciosCajasVinculadasData>()
            };

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

                var listaTmp = cn.Query<CajasServiciosCajasVinculadasData>(
                    q,
                    new
                    {
                        cod_recaudador = (cod_recaudador ?? string.Empty).Trim(),
                        cod_servicio = (cod_servicio ?? string.Empty).Trim()
                    });

                var lista = new List<CajasServiciosCajasVinculadasData>();
                foreach (var item in listaTmp)
                {
                    lista.Add(item);
                }

                resp.Result = lista;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = new List<CajasServiciosCajasVinculadasData>();
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

            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                const string qExiste = @"
            SELECT ISNULL(COUNT(1), 0)
            FROM dbo.CAJAS_SERVICIOS_ASIGNADOS
            WHERE cod_recaudador = @cod_recaudador
              AND cod_servicio   = @cod_servicio
              AND cod_caja       = @cod_caja;";

                int existe = cn.ExecuteScalar<int>(
                    qExiste,
                    new
                    {
                        cod_recaudador = (cod_recaudador ?? string.Empty).Trim(),
                        cod_servicio = (cod_servicio ?? string.Empty).Trim(),
                        cod_caja = (cod_caja ?? string.Empty).Trim()
                    });

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
                            new
                            {
                                cod_recaudador = (cod_recaudador ?? string.Empty).Trim(),
                                cod_servicio = (cod_servicio ?? string.Empty).Trim(),
                                cod_caja = (cod_caja ?? string.Empty).Trim(),
                                usuario = usuario ?? string.Empty
                            });

                        RegistrarBitacora(
                            CodEmpresa,
                            usuario,
                            "Registra - WEB",
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
                            new
                            {
                                cod_recaudador = (cod_recaudador ?? string.Empty).Trim(),
                                cod_servicio = (cod_servicio ?? string.Empty).Trim(),
                                cod_caja = (cod_caja ?? string.Empty).Trim()
                            });

                        RegistrarBitacora(
                            CodEmpresa,
                            usuario,
                            "Elimina - WEB",
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
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
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